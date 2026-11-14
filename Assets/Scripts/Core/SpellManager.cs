using UnityEngine;
using System.Collections.Generic;
using CartoonFX;
using TowerDefence.Combat;
using TowerDefence.Interfaces;

namespace TowerDefence.Core
{
    public class SpellManager : MonoBehaviour
    {
        public static SpellManager Instance { get; private set; }

        public Data.SpellData SelectedSpell { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SelectSpell(Data.SpellData spell)
        {
            SelectedSpell = spell;
            Debug.Log($"Spell Selected: {spell.spellName}. Click to cast!");
        }

        public void ClearSelectedSpell() => SelectedSpell = null;

        private Dictionary<string, float> cooldowns = new Dictionary<string, float>();
        private HashSet<string> usedOneTimeSpells = new HashSet<string>();

        public bool HasBeenUsed(Data.SpellData spell)
        {
            return spell.isOneTimeUse && usedOneTimeSpells.Contains(spell.spellID);
        }

        public bool CanCast(Data.SpellData spell)
        {
            if (HasBeenUsed(spell)) return false;

            if (cooldowns.ContainsKey(spell.spellID))
            {
                if (Time.time < cooldowns[spell.spellID]) return false;
            }

            // Para kontrolü
            return CurrencyManager.Instance.CanAfford(SideController.Instance.GetPlayerSide(), spell.manaCost);
        }

        public void CastSpell(Data.SpellData spell, Vector3 targetPos)
        {
            if (!CanCast(spell)) return;
            if (spell.spellType == Data.SpellType.Reinforcement && spell.unitPrefabToSpawn == null)
                return;

            Side payer = SideController.Instance.GetPlayerSide();
            if (!CurrencyManager.Instance.TrySpendCurrency(payer, spell.manaCost))
                return;
            TowerDefence.UI.CurrencyFloatUI.Show(targetPos, -spell.manaCost, payer);
            
            // Eğer tek seferlikse kaydet
            if (spell.isOneTimeUse)
            {
                usedOneTimeSpells.Add(spell.spellID);
            }
            else
            {
                // Cooldown başlat
                cooldowns[spell.spellID] = Time.time + spell.cooldown;
            }

            // Yazılım Mantığını Uygula
            ExecuteSpellEffect(spell, targetPos);

            PlaySpellVfx(spell, targetPos);
            if (AudioManager.Instance != null && spell.castSFX != null) AudioManager.Instance.PlaySFX(spell.castSFX);
        }

        public GameObject PlaySpellVfx(Data.SpellData spell, Vector3 targetPos)
        {
            if (spell == null || spell.spellVFXPrefab == null) return null;

            Vector3 vfxPos = spell.spellType == Data.SpellType.GoldBoost
                ? PlayerBasePosition(targetPos)
                : targetPos;
            GameObject vfx = Instantiate(spell.spellVFXPrefab, vfxPos, Quaternion.identity);
            CFXR_Effect[] clearEffects = vfx.GetComponentsInChildren<CFXR_Effect>(true);
            for (int i = 0; i < clearEffects.Length; i++)
                clearEffects[i].clearBehavior = CFXR_Effect.ClearBehavior.None;
            if (IsOneShotSpell(spell))
                StopLoop(vfx);
            Destroy(vfx, SpellVfxLifetime(spell));
            return vfx;
        }

        public static float SpellVfxLifetime(Data.SpellData spell)
        {
            if (spell == null) return 0f;
            return Mathf.Max(3f, spell.duration + 1f);
        }

        private static bool IsOneShotSpell(Data.SpellData spell)
        {
            if (spell == null) return false;
            if (spell.spellType == Data.SpellType.GoldBoost || spell.spellType == Data.SpellType.Reinforcement)
                return true;
            return spell.spellID == "Spell_Neutral_Earthquake"
                || spell.spellID == "Spell_Dark_Rift"
                || spell.spellID == "Spell_Light_Thunder_1"
                || spell.spellID == "Spell_Light_Thunder_2";
        }

        private static void StopLoop(GameObject vfx)
        {
            ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem.MainModule main = systems[i].main;
                main.loop = false;
            }
        }

        private void ExecuteSpellEffect(Data.SpellData spell, Vector3 targetPos)
        {
            switch (spell.spellType)
            {
                case Data.SpellType.Thunderstrike:
                    ApplyAoEDamage(targetPos, spell.radius, spell.power);
                    if (spell.spellID == "Spell_Neutral_Earthquake")
                        ApplyAoEStatus(targetPos, spell.radius, StatusEffectType.Stun, spell.duration > 0f ? spell.duration : 2f, 1f);
                    else if (spell.spellID == "Spell_Dark_PlagueRain")
                        ApplyAoEStatus(targetPos, spell.radius, StatusEffectType.Poison, spell.duration > 0f ? spell.duration : 5f, spell.power * 0.25f);
                    break;
                case Data.SpellType.Freeze:
                    ApplyAoEFreeze(targetPos, spell.radius, spell.duration > 0 ? spell.duration : spell.power);
                    break;
                case Data.SpellType.Reinforcement:
                    int spawnCount = Mathf.RoundToInt(spell.power);
                    if (spawnCount < 1) spawnCount = spell.unitSpawnCount;
                    SpawnReinforcements(targetPos, spell.unitPrefabToSpawn, spawnCount);
                    break;
                case Data.SpellType.GoldBoost:
                    int goldAmt = (int)spell.power;
                    Side boosted = SideController.Instance.GetPlayerSide();
                    CurrencyManager.Instance.AddCurrency(boosted, goldAmt);
                    TowerDefence.UI.CurrencyFloatUI.Show(PlayerBasePosition(targetPos), goldAmt, boosted);
                    break;
                case Data.SpellType.Shield:
                    ApplyAoEShield(targetPos, spell.radius, spell.duration > 0 ? spell.duration : spell.power);
                    break;
                case Data.SpellType.Buff:
                    ApplyAoEBuff(targetPos, spell.radius, spell.power, spell.duration > 0 ? spell.duration : 5f);
                    break;
            }
        }

        private void ApplyAoEStatus(Vector3 center, float radius, StatusEffectType type, float duration, float power)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            HashSet<Unit> units = new HashSet<Unit>();
            foreach (var col in colliders)
            {
                Unit unit = col.GetComponent<Unit>() ?? col.GetComponentInParent<Unit>();
                if (unit != null && !unit.IsDead && unit.GetSide() != playerSide && units.Add(unit))
                    unit.AddStatusEffect(type, duration, power);
            }
        }

        private void ApplyAoEShield(Vector3 center, float radius, float duration)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            HashSet<Tower> towers = new HashSet<Tower>();
            HashSet<Unit> units = new HashSet<Unit>();
            foreach (var col in colliders)
            {
                Tower tower = col.GetComponent<Tower>() ?? col.GetComponentInParent<Tower>();
                if (tower != null && tower.GetSide() == playerSide && towers.Add(tower))
                    tower.SetInvulnerable(duration);

                Unit unit = col.GetComponent<Unit>() ?? col.GetComponentInParent<Unit>();
                if (unit != null && unit.GetSide() == playerSide && units.Add(unit))
                    unit.SetInvulnerable(duration);
            }
        }

        private void ApplyAoEBuff(Vector3 center, float radius, float multiplier, float duration)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            HashSet<Tower> towers = new HashSet<Tower>();
            HashSet<Unit> units = new HashSet<Unit>();
            foreach (var col in colliders)
            {
                Tower tower = col.GetComponent<Tower>() ?? col.GetComponentInParent<Tower>();
                if (tower != null && tower.GetSide() == playerSide && towers.Add(tower))
                    tower.ApplyTempBuff(multiplier, duration);

                Unit unit = col.GetComponent<Unit>() ?? col.GetComponentInParent<Unit>();
                if (unit != null && unit.GetSide() == playerSide && units.Add(unit))
                    unit.ApplyTempBuff(multiplier, duration);
            }
        }

        private void ApplyAoEDamage(Vector3 center, float radius, float damage)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            HashSet<Unit> units = new HashSet<Unit>();
            HashSet<Tower> towers = new HashSet<Tower>();
            HashSet<Base> bases = new HashSet<Base>();
            foreach (var col in colliders)
            {
                Unit unit = col.GetComponent<Unit>() ?? col.GetComponentInParent<Unit>();
                if (unit != null)
                {
                    if (unit.GetSide() != playerSide && units.Add(unit))
                        unit.TakeDamage(damage);
                    continue;
                }

                Tower tower = col.GetComponent<Tower>() ?? col.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    if (tower.GetSide() != playerSide && towers.Add(tower))
                        tower.TakeDamage(damage);
                    continue;
                }

                Base targetBase = col.GetComponent<Base>() ?? col.GetComponentInParent<Base>();
                if (targetBase != null && targetBase.GetSide() != playerSide && bases.Add(targetBase))
                    targetBase.TakeDamage(damage);
            }
        }

        private void ApplyAoEFreeze(Vector3 center, float radius, float duration)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            HashSet<Unit> units = new HashSet<Unit>();
            foreach (var col in colliders)
            {
                Unit unit = col.GetComponent<Unit>() ?? col.GetComponentInParent<Unit>();
                if (unit != null && unit.GetSide() != playerSide && units.Add(unit))
                    unit.ApplySlow(0f, duration);
            }
        }

        private void SpawnReinforcements(Vector3 center, GameObject prefab, int count)
        {
            if (prefab == null) return;
            
            PathWaypoints[] allPaths = FindObjectsByType<PathWaypoints>(FindObjectsSortMode.None);
            PathWaypoints nearestPath = null;
            float minDist = float.MaxValue;

            foreach (var path in allPaths)
            {
                if (path == null || path.GetPathPoints().Count == 0) continue;
                // En yakın waypoint'i bul
                foreach (var wp in path.GetPathPoints())
                {
                    float dist = Vector3.Distance(center, wp);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestPath = path;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = center + Random.insideUnitSphere * 1.5f;
                spawnPos.y = GroundHeight(spawnPos.x, spawnPos.z);
                GameObject unitGO = Instantiate(prefab, spawnPos, Quaternion.identity);
                Unit unit = unitGO.GetComponent<Unit>();
                
                if (unit != null)
                {
                    // Oyuncu tarafı olduğunu varsayıyoruz. 
                    // StatMultiplier 1f.
                    // unit.Initialize(unit.unitData); // Eğer start'ta initialize olmuyorsa

                    unit.SuppressKillReward();
                    if (nearestPath != null)
                    {
                        unit.SetPathAtNearestWaypoint(nearestPath, spawnPos);
                    }

                    // Reinforcement'lar kalıcı olmasın, 30 saniye sonra ölsünler (TakeDamage kullanarak güvenli ölüm)
                    StartCoroutine(KillReinforcementAfterTime(unit, 30f));
                }
            }
        }

        private static float GroundHeight(float x, float z)
        {
            int mask = LayerMask.GetMask("Default", "Path");
            Vector3 origin = new Vector3(x, 80f, z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 160f, mask, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            return 0.2f;
        }

        private static Vector3 PlayerBasePosition(Vector3 fallback)
        {
            if (SideController.Instance == null) return fallback;
            Side playerSide = SideController.Instance.GetPlayerSide();
            Base[] bases = FindObjectsByType<Base>(FindObjectsSortMode.None);
            for (int i = 0; i < bases.Length; i++)
            {
                if (bases[i] != null && bases[i].GetSide() == playerSide)
                    return bases[i].transform.position;
            }
            return fallback;
        }

        private System.Collections.IEnumerator KillReinforcementAfterTime(Unit unit, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (unit != null)
            {
                unit.TakeDamage(99999f); // Güvenli ölüm (ölüm animasyonu vb. tetiklenir)
            }
        }

        public float GetRemainingCooldown(string spellID)
        {
            if (!cooldowns.ContainsKey(spellID)) return 0;
            return Mathf.Max(0, cooldowns[spellID] - Time.time);
        }
    }
}
