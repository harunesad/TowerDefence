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
        [SerializeField] private Transform rowUpper; // Üst satır (3 kule için)
        [SerializeField] private Transform rowLower; // Alt satır (2 kule için)

        private TowerData[] allTowers;
        private TowerSlot currentActiveSlot;

        private void Awake()
        {
            gameObject.SetActive(false); // Başlangıçta gizle
        }

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Satırları temizle
            if (rowUpper != null) foreach (Transform child in rowUpper) Destroy(child.gameObject);
            if (rowLower != null) foreach (Transform child in rowLower) Destroy(child.gameObject);

            // Kule verilerini al
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.AllTowers != null)
                allTowers = TowerPlacementManager.Instance.AllTowers.ToArray();
            else
                allTowers = new TowerData[0];

            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            Debug.Log($"TowerSelectionUI: Found {allTowers.Length} total towers. PlayerSide: {playerSide}");

            int buttonsCreated = 0;
            foreach (TowerData tower in allTowers)
            {
                if (tower.side != playerSide) continue;

                // Hedef satırı belirle (İlk 3 kule üstte, sonrakiler altta)
                Transform targetRow = (buttonsCreated < 3) ? rowUpper : rowLower;
                if (targetRow == null) continue;

                GameObject buttonGO = Instantiate(towerButtonPrefab, targetRow);
                Button button = buttonGO.GetComponent<Button>();
                
                Image icon = buttonGO.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null) icon.sprite = tower.icon;

                // Maliyet yazısını güncelle
                TextMeshProUGUI costText = buttonGO.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
                if (costText != null) costText.text = tower.cost.ToString();

                button.onClick.AddListener(() => OnTowerButtonClicked(tower));
                buttonsCreated++;
            }
        }

        public void ShowForSlot(TowerSlot slot)
        {
            // TOGGLE Mantığı: Eğer zaten bu slot için açıksa, Kapat.
            if (gameObject.activeSelf && currentActiveSlot == slot)
            {
                Hide();
                return;
            }

            currentActiveSlot = slot;
            gameObject.SetActive(true);

            // Direkt dünya konumuna taşı (Yuvanın biraz üzerine)
            transform.position = slot.transform.position + new Vector3(0, 2.5f, 0);
        }

        private void Update()
        {
            // Menü açıksa her zaman kameraya bakmalı (Billboarding)
            if (gameObject.activeSelf && Camera.main != null)
            {
                // Tüm panellerin kameraya tam paralel ve aynı açıda durması için (Daha temiz görünüm)
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnTowerButtonClicked(TowerData tower)
        {
            TowerPlacementManager.Instance.SelectTower(tower);
        }
    }
}
