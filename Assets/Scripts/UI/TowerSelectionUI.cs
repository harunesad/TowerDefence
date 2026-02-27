using UnityEngine;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Core;
using TowerDefence.Grid;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class TowerSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject towerButtonPrefab;
        [SerializeField] private Transform container;

        private TowerData[] allTowers;
        private RectTransform rectTransform;
        private Canvas parentCanvas;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            gameObject.SetActive(false); // Başlangıçta gizle
        }

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Konteynerdaki eski butonları temizle (eğer varsa)
            foreach (Transform child in container) Destroy(child.gameObject);

            // Tüm kule verilerini Resources'tan otomatik yükle
            allTowers = Resources.LoadAll<TowerData>("Data/Towers");

            Side playerSide = SideController.Instance.GetPlayerSide();

            foreach (TowerData tower in allTowers)
            {
                if (tower.side != playerSide) continue;

                GameObject buttonGO = Instantiate(towerButtonPrefab, container);
                Button button = buttonGO.GetComponent<Button>();
                
                Image icon = buttonGO.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null) icon.sprite = tower.icon;

                button.onClick.AddListener(() => OnTowerButtonClicked(tower));
            }
        }

        public void ShowForSlot(TowerSlot slot)
        {
            gameObject.SetActive(true);

            // Direkt dünya konumuna taşı (Yuvanın biraz üzerine)
            transform.position = slot.transform.position + new Vector3(0, 2.5f, 0);
        }

        private void Update()
        {
            // Menü açıksa her zaman kameraya bakmalı (Billboarding)
            if (gameObject.activeSelf && Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
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
