using UnityEngine;
using System.Collections.Generic;
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

        public bool CanCast(Data.SpellData spell)
        {
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

            // Maliyeti düş
            CurrencyManager.Instance.TrySpendCurrency(SideController.Instance.GetPlayerSide(), spell.manaCost);
            
            // Cooldown başlat
            cooldowns[spell.spellID] = Time.time + spell.cooldown;

            // Yazılım Mantığını Uygula
            ExecuteSpellEffect(spell, targetPos);

            // VFX & SFX
            if (spell.spellVFXPrefab != null)
            {
                // Instantiate/Destroy mantığıyla VFX
                GameObject vfx = Instantiate(spell.spellVFXPrefab, targetPos, Quaternion.identity);
                Destroy(vfx, 3f); // 3 saniye sonra temizle
            }
            if (AudioManager.Instance != null && spell.castSFX != null) AudioManager.Instance.PlaySFX(spell.castSFX);
        }

        private void ExecuteSpellEffect(Data.SpellData spell, Vector3 targetPos)
        {
            switch (spell.spellType)
            {
                case Data.SpellType.Thunderstrike:
                    ApplyAoEDamage(targetPos, spell.radius, spell.power);
                    break;
                case Data.SpellType.Freeze:
                    ApplyAoEFreeze(targetPos, spell.radius, spell.duration > 0 ? spell.duration : spell.power);
                    break;
                case Data.SpellType.Reinforcement:
                    SpawnReinforcements(targetPos, spell.unitPrefabToSpawn, spell.unitSpawnCount);
                    break;
                case Data.SpellType.GoldBoost:
                    CurrencyManager.Instance.AddCurrency(SideController.Instance.GetPlayerSide(), (int)spell.power);
                    break;
                case Data.SpellType.Shield:
                    ApplyAoEShield(targetPos, spell.radius, spell.duration > 0 ? spell.duration : spell.power);
                    break;
                case Data.SpellType.Buff:
                    ApplyAoEBuff(targetPos, spell.radius, spell.power, spell.duration > 0 ? spell.duration : 5f);
                    break;
            }
        }

        private void ApplyAoEShield(Vector3 center, float radius, float duration)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            foreach (var col in colliders)
            {
                Tower tower = col.GetComponent<Tower>();
                if (tower == null) tower = col.GetComponentInParent<Tower>();
                
                if (tower != null && tower.GetSide() == SideController.Instance.GetPlayerSide())
                {
                    tower.SetInvulnerable(duration);
                }
            }
        }

        private void ApplyAoEBuff(Vector3 center, float radius, float multiplier, float duration)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            foreach (var col in colliders)
            {
                Tower tower = col.GetComponent<Tower>();
                if (tower == null) tower = col.GetComponentInParent<Tower>();

                if (tower != null && tower.GetSide() == SideController.Instance.GetPlayerSide())
                {
                    tower.ApplyTempBuff(multiplier, duration);
                }
            }
        }

        private void ApplyAoEDamage(Vector3 center, float radius, float damage)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            foreach (var col in colliders)
            {
                IDamageable hit = col.GetComponent<IDamageable>();
                if (hit != null && hit.GetSide() != SideController.Instance.GetPlayerSide())
                {
                    hit.TakeDamage(damage);
                }
            }
        }

        private void ApplyAoEFreeze(Vector3 center, float radius, float duration)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            foreach (var col in colliders)
            {
                Unit unit = col.GetComponent<Unit>();
                if (unit != null && unit.GetSide() != SideController.Instance.GetPlayerSide())
                {
                    unit.ApplySlow(0f, duration); // 0 hız = Donma
                }
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
                if (path == null || path.GetWaypoints().Count == 0) continue;
                // En yakın waypoint'i bul
                foreach (var wp in path.GetWaypoints())
                {
                    float dist = Vector3.Distance(center, wp.position);
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
                spawnPos.y = 0; // Zemine sabitle
                GameObject unitGO = Instantiate(prefab, spawnPos, Quaternion.identity);
                Unit unit = unitGO.GetComponent<Unit>();
                
                if (unit != null)
                {
                    // Oyuncu tarafı olduğunu varsayıyoruz. 
                    // StatMultiplier 1f.
                    // unit.Initialize(unit.unitData); // Eğer start'ta initialize olmuyorsa

                    if (nearestPath != null)
                    {
                        unit.SetPathAtNearestWaypoint(nearestPath, spawnPos);
                    }

                    // Reinforcement'lar kalıcı olmasın, 30 saniye sonra ölsünler (TakeDamage kullanarak güvenli ölüm)
                    StartCoroutine(KillReinforcementAfterTime(unit, 30f));
                }
            }
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
