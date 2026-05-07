using UnityEngine;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Core;
using TowerDefence.Grid;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace TowerDefence.UI
{
    public class TowerSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject towerButtonPrefab;
        [SerializeField] private Transform rowUpper; // Üst satır (3 buton için)
        [SerializeField] private Transform rowLower; // Alt satır (2 buton için)

        private TowerSlot currentActiveSlot;

        private void Awake()
        {
            gameObject.SetActive(false); // Başlangıçta gizle
        }

        private void OnEnable()
        {
            // Taraf veya altın değiştiğinde menüyü güncelle
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged += HandleSideChanged;
            
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
        }

        private void OnDisable()
        {
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged -= HandleSideChanged;

            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        private void HandleCurrencyChanged(Side side, int amount)
        {
            if (gameObject.activeSelf) InitializeUI();
        }

        private void HandleSideChanged(Side side)
        {
            if (gameObject.activeSelf) InitializeUI();
        }

        private void InitializeUI()
        {
            // Satırları temizle
            if (rowUpper != null) foreach (Transform child in rowUpper) Destroy(child.gameObject);
            if (rowLower != null) foreach (Transform child in rowLower) Destroy(child.gameObject);

            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            
            // Managers check
            List<TowerData> allTowers = TowerPlacementManager.Instance != null ? TowerPlacementManager.Instance.AllTowers : new List<TowerData>();

            // FALLBACK: Eğer manager boşsa her şeyi bul
            if (allTowers == null || allTowers.Count == 0)
            {
                allTowers = new List<TowerData>(Resources.FindObjectsOfTypeAll<TowerData>());
                Debug.LogWarning($"[CORE-UI] Manager empty. Local search found {allTowers.Count} towers.");
            }

            Debug.Log($"[CORE-UI] Populating TOWERS ONLY for side: {playerSide}. Pool size: {allTowers.Count}");

            int buttonsCreated = 0;
            HashSet<string> addedNames = new HashSet<string>();

            // SADECE KULELERİ GÖSTER
            foreach (TowerData tower in allTowers)
            {
                if (tower == null) continue;
                TowerData chosen = null;

                if (tower.side == playerSide) chosen = tower;
                else if (tower.enemyCounterpart != null && tower.enemyCounterpart.side == playerSide) chosen = tower.enemyCounterpart;

                if (chosen != null && chosen.isBaseTower && !addedNames.Contains(chosen.towerName))
                {
                    // Her satırda 3 buton olacak şekilde dağıt (Toplam 6 buton desteği)
                    Transform targetRow = (buttonsCreated < 3) ? rowUpper : rowLower;
                    CreateSelectionButton(chosen.icon, chosen.cost, () => OnTowerButtonClicked(chosen), targetRow);
                    
                    addedNames.Add(chosen.towerName);
                    buttonsCreated++;
                    if (buttonsCreated >= 6) break; 
                }
            }

            if (buttonsCreated == 0)
            {
                Debug.LogWarning($"[CORE-UI] No TOWERS found for side {playerSide}.");
            }
        }

        private void CreateSelectionButton(Sprite icon, int cost, UnityEngine.Events.UnityAction onClick, Transform parent)
        {
            if (parent == null || towerButtonPrefab == null) return;

            GameObject buttonGO = Instantiate(towerButtonPrefab, parent);
            Button button = buttonGO.GetComponent<Button>();
            
            Image iconImg = buttonGO.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null) iconImg.sprite = icon;

            TextMeshProUGUI costText = buttonGO.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
            if (costText != null) costText.text = cost.ToString();

            button.onClick.AddListener(onClick);

            // Dinamik altın kontrolü
            Side side = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            button.interactable = CurrencyManager.Instance.CanAfford(side, cost);
        }

        private static float lastShowTime = -1f;

        public void ShowForSlot(TowerSlot slot)
        {
            if (Time.time - lastShowTime < 0.2f) return;
            lastShowTime = Time.time;

            InitializeUI();

            if (gameObject.activeSelf && currentActiveSlot == slot)
            {
                Hide();
                return;
            }

            currentActiveSlot = slot;
            gameObject.SetActive(true);

            // Standart Ölçek
            transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);
            transform.position = slot.transform.position + new Vector3(0, 3.5f, 0);
        }

        private void Update()
        {
            if (gameObject.activeSelf && Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            currentActiveSlot = null;
        }

        private void OnTowerButtonClicked(TowerData tower)
        {
            if (TowerPlacementManager.Instance != null && currentActiveSlot != null)
            {
                TowerPlacementManager.Instance.StartPlacementAtSlot(currentActiveSlot);
                TowerPlacementManager.Instance.SelectTower(tower);
                Hide();
            }
        }

        private void OnUnitButtonClicked(UnitData unit)
        {
            // Bu metod artık seçim panelinde (popup) kullanılmıyor, üniteler alt barda.
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpendCurrency(unit.side, unit.spawnCost))
            {
                Spawner.SpawnPlayerUnits(unit);
                Hide();
            }
        }
    }
}
