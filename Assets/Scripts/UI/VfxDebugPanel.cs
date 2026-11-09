using System.Collections;
using System.Collections.Generic;
using CartoonFX;
using TMPro;
using TowerDefence.Combat;
using TowerDefence.Core;
using TowerDefence.Data;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TowerDefence.UI
{
    /// <summary>
    /// Editor-only list of every VFX the match actually plays. Each entry uses the same call as gameplay.
    /// </summary>
    public class VfxDebugPanel : MonoBehaviour
    {
        class Entry
        {
            public string label;
            public System.Func<Vector3, IEnumerator> play;
        }

        readonly List<Entry> entries = new List<Entry>();
        TextMeshProUGUI nameLabel;
        TextMeshProUGUI allLabel;
        int index;
        bool playingAll;
        Coroutine session;
        GameObject lastSpawned;
        GameObject lastPooled;
        readonly List<GameObject> lastExtras = new List<GameObject>();
        VFXType lastPooledType;

        const int GroundMask = (1 << 0) | (1 << 8);

        void Awake()
        {
            BuildCatalog();
            BuildUi();
            RefreshLabel();
        }

        void BuildCatalog()
        {
            AddPooled("Light Impact", VFXType.LightImpact, 2.5f);
            AddPooled("Dark Impact", VFXType.DarkImpact, 1.2f);
            AddPooled("Unit Spawn", VFXType.UnitSpawn, 2.2f);
            AddPooled("Unit Death", VFXType.UnitDeath, 5.7f);
            AddPooled("Slow", VFXType.SlowEffect, 1.3f);
            AddPooled("Upgrade Sparkle", VFXType.UpgradeSparkle, 2.5f);
            AddPooled("Tower Death", VFXType.SpellEarthquake, 3.7f);
            AddSmoke("Area Smoke r2", 2f);
            AddSmoke("Area Smoke r5", 5f);
            AddSmoke("Area Smoke r8", 8f);

#if UNITY_EDITOR
            UnitData dragon = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/Data/Units/Bone_Dragon.asset");
            if (dragon != null && dragon.breathVfxPrefab != null)
            {
                GameObject breath = dragon.breathVfxPrefab;
                float scale = dragon.breathVfxScale > 0.01f ? dragon.breathVfxScale : 0.55f;
                entries.Add(new Entry { label = "Bone Dragon Breath", play = pos => PlayBreath(breath, scale, pos) });
            }

            GameObject spark = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hovl Studio/Magic effects pack/Prefabs/Sparks/Sparks blue.prefab");
            if (spark != null)
                entries.Add(new Entry { label = "Beam Spark", play = pos => PlayOneShot(spark, pos, 1.5f, 1f) });

            AddHero("Rally Heal", "Assets/Prefabs/HeroVFX/RallyHeal.prefab", 2f, false);
            AddHero("Holy Shield", "Assets/Prefabs/HeroVFX/HolyShield.prefab", 2f, false);
            GameObject rain = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HeroVFX/ArrowRain.prefab");
            GameObject arrow = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/Projectiles/Arrow.prefab");
            if (rain != null)
                entries.Add(new Entry { label = "Arrow Rain", play = pos => PlayArrowRain(rain, arrow, pos) });
            AddHero("Swift Strike", "Assets/Prefabs/HeroVFX/SwiftStrike.prefab", 1f, true);
            AddHero("Fortify Taunt", "Assets/Prefabs/HeroVFX/FortifyTaunt.prefab", 2f, false);
            AddHero("Solar Smite", "Assets/Prefabs/HeroVFX/SolarSmite.prefab", 1f, true);
            AddHero("Life Drain", "Assets/Prefabs/HeroVFX/LifeDrain.prefab", 2f, false);
            AddHero("Soul Execute", "Assets/Prefabs/HeroVFX/SoulExecute.prefab", 1.5f, true);
            AddHero("Ground Slam", "Assets/Prefabs/HeroVFX/GroundSlam.prefab", 2f, false);
            AddHero("Plague Cloud", "Assets/Prefabs/HeroVFX/PlagueCloud.prefab", 2f, false);
            AddHero("Bone Armor", "Assets/Prefabs/HeroVFX/BoneArmor.prefab", 2f, false);
            AddHero("Shadow Step", "Assets/Prefabs/HeroVFX/ShadowStep.prefab", 1f, false);

            AddSpell("Thunderstrike", "Assets/Data/Spells/Spell_Light_Thunder_1.asset");
            AddSpell("Greater Thunderstrike", "Assets/Data/Spells/Spell_Light_Thunder_2.asset");
            AddSpell("Earthquake", "Assets/Data/Spells/Spell_Neutral_Earthquake.asset");
            AddSpell("Abyssal Rift", "Assets/Data/Spells/Spell_Dark_Rift.asset");
            AddSpell("Gold Rush", "Assets/Data/Spells/Spell_Neutral_Gold.asset");
            AddSpell("Reinforcements", "Assets/Data/Spells/Spell_Light_Reinforce_1.asset");
            AddSpell("Royal Guards", "Assets/Data/Spells/Spell_Light_Reinforce_2.asset");
            AddSpell("Shadow Freeze", "Assets/Data/Spells/Spell_Dark_Freeze.asset");
            AddSpell("Divine Shield", "Assets/Data/Spells/Spell_Light_Shield.asset");
            AddSpell("Holy Blessing", "Assets/Data/Spells/Spell_Light_Blessing.asset");
            AddSpell("Bloodlust", "Assets/Data/Spells/Spell_Dark_Bloodlust.asset");
            AddSpell("Plague Rain", "Assets/Data/Spells/Spell_Dark_PlagueRain.asset");
#endif
        }

        void AddPooled(string label, VFXType type, float life)
        {
            entries.Add(new Entry { label = label, play = pos => PlayPooled(type, life, pos) });
        }

        void AddSmoke(string label, float radius)
        {
            entries.Add(new Entry { label = label, play = pos => PlaySmoke(radius, pos) });
        }

#if UNITY_EDITOR
        void AddHero(string label, string path, float delay, bool oneShot)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;
            entries.Add(new Entry { label = label, play = pos => PlayHero(prefab, delay, oneShot, pos) });
        }

        void AddSpell(string label, string path)
        {
            SpellData spell = AssetDatabase.LoadAssetAtPath<SpellData>(path);
            if (spell == null) return;
            entries.Add(new Entry { label = label, play = pos => PlaySpell(spell, pos) });
        }
#endif

        void BuildUi()
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-15f, -128f);
            rt.sizeDelta = new Vector2(340f, 78f);

            nameLabel = MakeText("Name", new Vector2(0f, -2f), new Vector2(340f, 28f), 16);

            MakeButton("Prev", "Prev", new Vector2(0f, -36f), new Vector2(70f, 36f), () => Step(-1), out _);
            MakeButton("Play", "Play", new Vector2(76f, -36f), new Vector2(70f, 36f), PlayCurrent, out _);
            MakeButton("Next", "Next", new Vector2(152f, -36f), new Vector2(70f, 36f), () => Step(1), out _);
            MakeButton("All", "All", new Vector2(228f, -36f), new Vector2(112f, 36f), ToggleAll, out allLabel);

            transform.SetAsLastSibling();
        }

        TextMeshProUGUI MakeText(string name, Vector2 pos, Vector2 size, float fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        void MakeButton(string name, string text, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction click, out TextMeshProUGUI label)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.35f, 0.55f, 0.9f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);

            label = MakeText(name + "Label", Vector2.zero, size, 16);
            label.transform.SetParent(go.transform, false);
            RectTransform labelRt = label.rectTransform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            label.alignment = TextAlignmentOptions.Center;
            label.text = text;
        }

        void RefreshLabel()
        {
            if (nameLabel == null) return;
            if (entries.Count == 0)
            {
                nameLabel.text = "No VFX";
                return;
            }
            nameLabel.text = (index + 1) + "/" + entries.Count + "  " + entries[index].label;
        }

        void Step(int dir)
        {
            StopSession();
            if (entries.Count == 0) return;
            index = (index + dir + entries.Count) % entries.Count;
            RefreshLabel();
        }

        void PlayCurrent()
        {
            StopSession();
            if (entries.Count == 0) return;
            session = StartCoroutine(PlayOne());
        }

        void ToggleAll()
        {
            if (playingAll)
            {
                StopSession();
                return;
            }
            if (entries.Count == 0) return;
            session = StartCoroutine(PlayAll());
        }

        void StopSession()
        {
            playingAll = false;
            if (session != null)
            {
                StopCoroutine(session);
                session = null;
            }
            ReleaseLast();
            if (allLabel != null) allLabel.text = "All";
        }

        IEnumerator PlayOne()
        {
            RefreshLabel();
            yield return entries[index].play(PreviewPoint());
            ReleaseLast();
            session = null;
        }

        IEnumerator PlayAll()
        {
            playingAll = true;
            if (allLabel != null) allLabel.text = "Stop";
            for (int i = 0; i < entries.Count; i++)
            {
                if (!playingAll) yield break;
                index = i;
                RefreshLabel();
                yield return entries[i].play(PreviewPoint());
            }
            playingAll = false;
            if (allLabel != null) allLabel.text = "All";
            ReleaseLast();
            session = null;
        }

        IEnumerator PlayPooled(VFXType type, float life, Vector3 pos)
        {
            ReleaseLast();
            if (VFXManager.Instance == null) yield break;
            lastPooled = VFXManager.Instance.SpawnVFX(type, pos, Quaternion.identity);
            lastPooledType = type;
            yield return new WaitForSeconds(life + 0.15f);
        }

        IEnumerator PlaySmoke(float radius, Vector3 pos)
        {
            ReleaseLast();
            if (VFXManager.Instance == null) yield break;
            lastSpawned = VFXManager.Instance.SpawnAreaSmoke(pos, radius);
            yield return new WaitForSeconds(2.4f);
        }

        IEnumerator PlayBreath(GameObject prefab, float scale, Vector3 pos)
        {
            ReleaseLast();
            Vector3 origin = pos + Vector3.up * 1.2f;
            Vector3 aim = origin + FlatForward() * 4f;
            Vector3 dir = aim - origin;
            GameObject vfx = Instantiate(prefab, origin, Quaternion.LookRotation(dir.normalized));
            vfx.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

            ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem.MainModule main = systems[i].main;
                main.playOnAwake = false;
                main.loop = false;
                systems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            CFXR_Effect[] clearEffects = vfx.GetComponentsInChildren<CFXR_Effect>(true);
            for (int i = 0; i < clearEffects.Length; i++)
                clearEffects[i].clearBehavior = CFXR_Effect.ClearBehavior.None;

            for (int i = 0; i < systems.Length; i++)
                systems[i].Play(false);

            lastSpawned = vfx;
            yield return new WaitForSeconds(1.3f);
        }

        IEnumerator PlayOneShot(GameObject prefab, Vector3 pos, float life, float scale)
        {
            ReleaseLast();
            GameObject vfx = Instantiate(prefab, pos, Quaternion.LookRotation(FlatForward()));
            vfx.transform.localScale = Vector3.one * scale;
            PlayParticlesOnce(vfx);
            lastSpawned = vfx;
            yield return new WaitForSeconds(life);
        }

        IEnumerator PlayArrowRain(GameObject rainPrefab, GameObject arrowPrefab, Vector3 pos)
        {
            ReleaseLast();
            GameObject vfx = Instantiate(rainPrefab, pos, Quaternion.identity);
            lastSpawned = vfx;
            if (arrowPrefab != null)
            {
                const float radius = 4f;
                const int arrowCount = 8;
                for (int i = 0; i < arrowCount; i++)
                {
                    Vector2 circle = Random.insideUnitCircle * radius;
                    Vector3 spawnPos = new Vector3(pos.x + circle.x, 12f, pos.z + circle.y);
                    Quaternion spawnRot = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
                    GameObject arrow = Instantiate(arrowPrefab, spawnPos, spawnRot);
                    Projectile projectile = arrow.GetComponent<Projectile>();
                    if (projectile != null) Destroy(projectile);
                    arrow.AddComponent<FallingArrow>();
                    lastExtras.Add(arrow);
                }
            }
            yield return HeroUnit.PlayAbilityVfx(vfx, 2f, false);
        }

        IEnumerator PlayHero(GameObject prefab, float delay, bool oneShot, Vector3 pos)
        {
            ReleaseLast();
            GameObject vfx = Instantiate(prefab, pos, Quaternion.identity);
            lastSpawned = vfx;
            yield return HeroUnit.PlayAbilityVfx(vfx, delay, oneShot);
        }

        IEnumerator PlaySpell(SpellData spell, Vector3 pos)
        {
            ReleaseLast();
            if (SpellManager.Instance == null) yield break;
            lastSpawned = SpellManager.Instance.PlaySpellVfx(spell, pos);
            yield return new WaitForSeconds(SpellManager.SpellVfxLifetime(spell));
        }

        static void PlayParticlesOnce(GameObject vfx)
        {
            ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem.MainModule main = systems[i].main;
                main.playOnAwake = false;
                main.loop = false;
                systems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            for (int i = 0; i < systems.Length; i++)
                systems[i].Play(false);
        }

        void ReleaseLast()
        {
            if (lastPooled != null)
            {
                if (lastPooled.activeSelf && VFXManager.Instance != null)
                    VFXManager.Instance.ReturnVFX(lastPooledType, lastPooled);
                lastPooled = null;
            }
            if (lastSpawned != null)
            {
                Destroy(lastSpawned);
                lastSpawned = null;
            }
            for (int i = 0; i < lastExtras.Count; i++)
            {
                if (lastExtras[i] != null)
                    Destroy(lastExtras[i]);
            }
            lastExtras.Clear();
        }

        static Vector3 PreviewPoint()
        {
            Camera cam = Camera.main;
            if (cam == null) return new Vector3(0f, 0.3f, 0f);
            Vector3 ahead = cam.transform.position + cam.transform.forward * 12f;
            Vector3 probe = new Vector3(ahead.x, 80f, ahead.z);
            if (Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 160f, GroundMask, QueryTriggerInteraction.Ignore))
                return hit.point + Vector3.up * 0.3f;
            return new Vector3(ahead.x, 0.3f, ahead.z);
        }

        static Vector3 FlatForward()
        {
            Camera cam = Camera.main;
            Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            return forward.normalized;
        }
    }
}
