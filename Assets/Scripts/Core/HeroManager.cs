using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Combat;
using TowerDefence.Data;
using UnityEngine.InputSystem;

namespace TowerDefence.Core
{
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        [Header("Hero Config")]
        [SerializeField] private List<HeroData> allHeroes = new List<HeroData>();
        [SerializeField] private float globalRespawnCooldown = 15f;
        [SerializeField] private float spawnSpread = 2.5f;



        private readonly HeroUnit[] activeHeroes = new HeroUnit[2];
        private readonly HeroData[] equippedHeroData = new HeroData[2];
        private readonly float[] respawnTimers = new float[2];
        private readonly Vector3[] lastMovePoints = new Vector3[2];

        private HeroUnit selectedHero;
        private int selectedSlotIndex = -1;
        private bool awaitingMoveCommand;

        public event System.Action<int, float, float> OnHeroRespawnTimerUpdated;
        public event System.Action<int, HeroUnit> OnHeroSpawned;
        public event System.Action<int> OnHeroDied;
        public event System.Action<HeroUnit> OnHeroSelectionChanged;

        public HeroData GetEquippedHeroData(int slot) => (slot >= 0 && slot < 2) ? equippedHeroData[slot] : null;
        public int GetEquippedHeroCount()
        {
            int count = 0;
            for (int i = 0; i < 2; i++)
                if (equippedHeroData[i] != null) count++;
            return count;
        }
        public HeroUnit GetActiveHero(int slot) => (slot >= 0 && slot < 2) ? activeHeroes[slot] : null;
        public bool IsHeroDead(int slot) => respawnTimers[slot] > 0;
        public HeroUnit SelectedHero => selectedHero;
        public bool IsAwaitingMoveCommand => awaitingMoveCommand;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void InitializeForLevel()
        {
            ClearAllHeroes();
            LoadEquippedHeroesFromSave();

            for (int i = 0; i < 2; i++)
            {
                respawnTimers[i] = 0f;
                lastMovePoints[i] = GetDefaultSpawnPoint() + GetSpawnOffset(i);
                if (equippedHeroData[i] != null)
                    SpawnHero(i);
            }
        }

        private void Update()
        {
            for (int i = 0; i < 2; i++)
            {
                if (respawnTimers[i] <= 0) continue;

                respawnTimers[i] -= Time.deltaTime;
                OnHeroRespawnTimerUpdated?.Invoke(i, respawnTimers[i], globalRespawnCooldown);

                if (respawnTimers[i] <= 0)
                {
                    respawnTimers[i] = 0f;
                    SpawnHero(i);
                }
            }

            HandleInput();
        }

        private void HandleInput()
        {
            if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;
            if (Camera.main == null) return;

            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(pointerPos);

            // 1. ÖNCELİK: 3D dünyada Hero tıklamasını kontrol et (UI engellemesinden bağımsız)
            RaycastHit[] hits = Physics.RaycastAll(ray, 250f);
            // Mesafeye göre sırala — en yakın collider'ı önce kontrol et
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                HeroUnit clickedHero = hit.collider.GetComponentInParent<HeroUnit>();
                if (clickedHero != null && !clickedHero.IsDead && IsManagedHero(clickedHero))
                {
                    Debug.Log($"[HeroManager] Hero clicked: {clickedHero.gameObject.name}");
                    SelectHeroInstance(clickedHero);
                    return;
                }
            }

            // 2. Seçili hero varsa zemine tıklayarak hareket ettir (UI engellemesinden bağımsız)
            if (awaitingMoveCommand && selectedHero != null && !selectedHero.IsDead)
            {
                if (TryGetGroundPoint(ray, out Vector3 groundPoint))
                {
                    Debug.Log($"[HeroManager] Moving hero to: {groundPoint}");
                    MoveSelectedHeroTo(groundPoint);
                    return;
                }
            }

            // 3. UI üzerindeyse deselect işlemini engelle
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            // 4. Boş alana tıklandıysa seçimi kaldır
            if (selectedHero != null)
            {
                DeselectHero();
            }
        }

        public void SelectHeroSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 2) return;
            if (IsHeroDead(slotIndex)) return;

            HeroUnit hero = activeHeroes[slotIndex];
            if (hero == null || hero.IsDead) return;

            SelectHeroInstance(hero, slotIndex);
        }

        private void SelectHeroInstance(HeroUnit hero, int slotIndex = -1)
        {
            if (slotIndex < 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (activeHeroes[i] == hero)
                    {
                        slotIndex = i;
                        break;
                    }
                }
            }

            if (slotIndex < 0) return;

            DeselectHero();

            selectedHero = hero;
            selectedSlotIndex = slotIndex;
            awaitingMoveCommand = true;

            for (int i = 0; i < 2; i++)
            {
                if (activeHeroes[i] == null || activeHeroes[i].IsDead) continue;
                activeHeroes[i].SetSelectionState(activeHeroes[i] == hero
                    ? HeroSelectionState.Active
                    : HeroSelectionState.Passive);
            }

            OnHeroSelectionChanged?.Invoke(hero);
        }

        public void DeselectHero()
        {
            for (int i = 0; i < 2; i++)
            {
                if (activeHeroes[i] != null && !activeHeroes[i].IsDead)
                    activeHeroes[i].SetSelectionState(HeroSelectionState.Passive);
            }

            selectedHero = null;
            selectedSlotIndex = -1;
            awaitingMoveCommand = false;
            OnHeroSelectionChanged?.Invoke(null);
        }

        private void MoveSelectedHeroTo(Vector3 point)
        {
            if (selectedHero == null || selectedSlotIndex < 0) return;

            lastMovePoints[selectedSlotIndex] = point;
            selectedHero.SetMoveDestination(point);

            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, point, Quaternion.identity);

            DeselectHero();
        }

        private bool TryGetGroundPoint(Ray ray, out Vector3 point)
        {
            point = Vector3.zero;
            int groundMask = LayerMask.GetMask("Default", "Path", "Ground");

            if (Physics.Raycast(ray, out RaycastHit hit, 250f, groundMask))
            {
                point = hit.point;
                return true;
            }

            // Yedek: herhangi bir collider (kule/yol dışı zemin)
            if (Physics.Raycast(ray, out hit, 250f))
            {
                if (hit.collider.GetComponentInParent<HeroUnit>() != null)
                    return false;

                point = hit.point;
                return true;
            }

            return false;
        }

        private void LoadEquippedHeroesFromSave()
        {
            equippedHeroData[0] = null;
            equippedHeroData[1] = null;

            if (MetaProgressionManager.Instance == null) return;

            var equippedIDs = MetaProgressionManager.Instance.GetEquippedHeroIDs();
            int slot = 0;
            foreach (string heroID in equippedIDs)
            {
                if (slot >= 2) break;
                HeroData data = MetaProgressionManager.Instance.GetHeroByID(heroID);
                if (data == null || data.unitData == null) continue;
                equippedHeroData[slot] = data;
                slot++;
            }
        }

        private void SpawnHero(int slot)
        {
            HeroData data = equippedHeroData[slot];
            if (data == null || data.unitData == null || data.unitData.prefab == null) return;

            Vector3 spawnPos = lastMovePoints[slot] != Vector3.zero
                ? lastMovePoints[slot]
                : GetDefaultSpawnPoint() + GetSpawnOffset(slot);

            // Eğer kahraman zaten sahnedeyse yeniden oluşturma, respawn et
            if (activeHeroes[slot] != null)
            {
                activeHeroes[slot].Respawn(spawnPos);
                activeHeroes[slot].SetMoveDestination(spawnPos);
                OnHeroSpawned?.Invoke(slot, activeHeroes[slot]);
                return;
            }

            GameObject heroGO = Instantiate(data.unitData.prefab, spawnPos, Quaternion.identity);
            heroGO.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            heroGO.transform.position = new Vector3(spawnPos.x, 0.2f, spawnPos.z);
            
            HeroUnit hero = heroGO.GetComponent<HeroUnit>();
            if (hero == null)
                hero = heroGO.AddComponent<HeroUnit>();

            float statMult = MetaProgressionManager.Instance != null
                ? MetaProgressionManager.Instance.GetHeroStatMultiplier(data.heroID)
                : 1f;

            hero.Initialize(data.unitData, statMult);
            hero.SetupHero(data, globalRespawnCooldown, deadHero => OnHeroDiedInScene(slot));
            hero.SetMoveDestination(spawnPos);

            activeHeroes[slot] = hero;
            OnHeroSpawned?.Invoke(slot, hero);
        }

        private void OnHeroDiedInScene(int slot)
        {
            if (selectedHero == activeHeroes[slot])
                DeselectHero();

            // activeHeroes[slot] = null; // Nesneyi koruyoruz, null yapma!
            respawnTimers[slot] = globalRespawnCooldown;
            OnHeroDied?.Invoke(slot);
        }

        private bool IsManagedHero(HeroUnit hero)
        {
            return activeHeroes[0] == hero || activeHeroes[1] == hero;
        }

        private Vector3 GetDefaultSpawnPoint()
        {
            // Tüm yolları bul ve en uzun (en çok waypoint'e sahip) olanı seç
            PathWaypoints[] allPaths = Object.FindObjectsByType<PathWaypoints>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            PathWaypoints mainPath = null;
            int maxWps = 0;

            foreach (var path in allPaths)
            {
                var wps = path.GetWaypoints();
                if (wps != null && wps.Count > maxWps)
                {
                    maxWps = wps.Count;
                    mainPath = path;
                }
            }

            if (mainPath != null)
            {
                var waypoints = mainPath.GetWaypoints();
                if (waypoints != null && waypoints.Count > 0)
                {
                    // Özel Durum: Sadece 2 waypoint varsa, ikisinin tam ortasını hesapla
                    if (waypoints.Count == 2 && waypoints[0] != null && waypoints[1] != null)
                    {
                        Debug.Log($"[HeroManager] Spawning heroes at mathematical middle of 2-point path '{mainPath.name}'");
                        return (waypoints[0].position + waypoints[1].position) * 0.5f;
                    }

                    // Genel Durum: 2'den fazla waypoint varsa, orta noktadaki waypoint'i seç
                    int midIndex = waypoints.Count / 2;
                    if (waypoints[midIndex] != null)
                    {
                        Debug.Log($"[HeroManager] Spawning heroes at middle waypoint {midIndex} of path '{mainPath.name}'");
                        return waypoints[midIndex].position;
                    }
                }
            }

            // Yedek: Eğer yol bulunamazsa player spawner'ı kullan
            foreach (var spawner in Spawner.AllSpawners)
            {
                if (spawner != null && spawner.isPlayerSpawner)
                    return spawner.transform.position;
            }

            // Son çare: Sahne merkezi veya sıfır noktası
            return Vector3.zero;
        }

        private Vector3 GetSpawnOffset(int slot)
        {
            return slot == 0 ? new Vector3(-spawnSpread, 0f, 0f) : new Vector3(spawnSpread, 0f, 0f);
        }

        private void ClearAllHeroes()
        {
            DeselectHero();
            for (int i = 0; i < 2; i++)
            {
                if (activeHeroes[i] != null)
                    Destroy(activeHeroes[i].gameObject);
                activeHeroes[i] = null;
                respawnTimers[i] = 0f;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
