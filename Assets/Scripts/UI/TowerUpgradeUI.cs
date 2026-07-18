using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Combat;
using TowerDefence.Data;
using TowerDefence.Core;
using TowerDefence.Grid;

namespace TowerDefence.UI
{
    public class TowerUpgradeUI : MonoBehaviour
    {
        // Singleton kaldırıldı, artık her kulede bir tane bağımsız var

        [Header("References")]
        public GameObject mainPanel;
        public TextMeshProUGUI towerNameText;
        public TextMeshProUGUI levelText;
        
        [Header("Normal Upgrade")]
        public GameObject normalUpgradeGroup;
        public Button upgradeButton;
        public TextMeshProUGUI upgradeCostText;

        [Header("Specialization")]
        public GameObject specializationGroup;
        public Button specAButton;
        public Button specBButton;
        public TextMeshProUGUI specACostText;
        public TextMeshProUGUI specBCostText;
        public Image specAIcon;
        public Image specBIcon;

        [Header("Sell")]
        public Button sellButton;
        public TextMeshProUGUI sellValueText;

        [Header("Targeting")]
        public Button priorityButton;
        public TextMeshProUGUI priorityText;

        private TowerSlot currentSlot;
        private Tower currentTower;

        private void OnEnable()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
        }

        private void OnDisable()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        private void HandleCurrencyChanged(Side side, int amount)
        {
            if (gameObject.activeSelf && currentTower != null)
            {
                RefreshUI();
            }
        }

        private void Start()
        {
            // Buton dinleyicilerini temizleyip yeniden bağlayarak prefab bağımsızlığını sağla
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

            specAButton.onClick.RemoveAllListeners();
            specAButton.onClick.AddListener(OnSpecAClicked);

            specBButton.onClick.RemoveAllListeners();
            specBButton.onClick.AddListener(OnSpecBClicked);

            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);

            if (priorityButton != null)
            {
                priorityButton.onClick.RemoveAllListeners();
                priorityButton.onClick.AddListener(OnPriorityClicked);
            }
        }

        private static float lastShowTime = -1f;

        public void Show(TowerSlot slot, Tower tower)
        {
            if (Time.time - lastShowTime < 0.2f) return;
            lastShowTime = Time.time;

            // Eğer aynı kuleye tekrar tıklandıysa ve panel açıksa, kapat (Toggle)
            if (currentSlot == slot && mainPanel.activeSelf)
            {
                Hide();
                return;
            }

            currentSlot = slot;
            currentTower = tower;
            mainPanel.SetActive(true);

            // Seçilen kulenin menzil halkasını göster (Range veya Kışla menzili)
            currentTower.SetRangeVisible(true);

            // World Space modunda olduğumuz için billboarding yapıyoruz
            transform.rotation = Camera.main.transform.rotation;

            RefreshUI();
        }

        public void RefreshUI()
        {
            if (currentTower == null) return;

            TowerData data = currentTower.GetTowerData();
            towerNameText.text = data.towerName;
            levelText.text = $"Level {currentTower.CurrentLevel}";

            int level = currentTower.CurrentLevel;
            var specs = currentTower.GetSpecializations();

            if (level >= 3 && specs != null && specs.Count >= 2)
            {
                // Elite Branching (Sadece Seviye 3'te görünür)
                normalUpgradeGroup.SetActive(false);
                specializationGroup.SetActive(true);

                specAIcon.sprite = specs[0].icon;
                specBIcon.sprite = specs[1].icon;
                specACostText.text = specs[0].cost.ToString();
                specBCostText.text = specs[1].cost.ToString();

                specAButton.interactable = CurrencyManager.Instance.CanAfford(data.side, specs[0].cost);
                specBButton.interactable = CurrencyManager.Instance.CanAfford(data.side, specs[1].cost);
            }
            else if (level < 3)
            {
                // Normal Upgrade (Seviye 1 ve 2 için)
                normalUpgradeGroup.SetActive(true);
                specializationGroup.SetActive(false);
                upgradeCostText.text = data.upgradeCost.ToString();
                upgradeButton.interactable = CurrencyManager.Instance.CanAfford(data.side, data.upgradeCost);
            }
            else
            {
                // Branşlaşma tanımlanmamış Max Level kuleler
                normalUpgradeGroup.SetActive(false);
                specializationGroup.SetActive(false);
            }

            sellValueText.text = (data.cost / 2).ToString();

            if (priorityText != null)
            {
                if (currentTower is BarracksTower)
                {
                    priorityText.text = "Rally Point";
                }
                else
                {
                    priorityText.text = $"Target: {currentTower.GetTargetingPriority()}";
                }
            }
        }

        public void OnUpgradeClicked()
        {
            if (currentTower == null) return;
            TowerData data = currentTower.GetTowerData();

            if (CurrencyManager.Instance.TrySpendCurrency(data.side, data.upgradeCost))
            {
                currentTower.Upgrade();
                RefreshUI();
            }
        }

        public void OnSpecAClicked() => Specialize(0);
        public void OnSpecBClicked() => Specialize(1);

        private void Specialize(int index)
        {
            var specs = currentTower.GetSpecializations();
            if (specs == null || index >= specs.Count) return;

            TowerData specData = specs[index];
            if (CurrencyManager.Instance.TrySpendCurrency(specData.side, specData.cost))
            {
                // Görsel değişim için slot üzerinden kuleyi tamamen yenile
                currentSlot.SpecializeTower(specData);
                
                if (VFXManager.Instance != null)
                    VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, currentSlot.GetPlacementPosition(), Quaternion.identity);

                Hide();
            }
        }

        public void OnSellClicked()
        {
            TowerData data = currentTower.GetTowerData();
            CurrencyManager.Instance.AddCurrency(data.side, data.cost / 2);
            currentSlot.ClearSlot();
            Hide();
        }

        public void OnPriorityClicked()
        {
            if (currentTower == null) return;

            if (currentTower is BarracksTower bt)
            {
                if (TowerPlacementManager.Instance != null)
                {
                    // Önce bir referans al, çünkü Hide() metodunda currentTower null yapılıyor
                    BarracksTower tempBarracks = bt;
                    Hide(); 
                    TowerPlacementManager.Instance.EnterRallyPlacementMode(tempBarracks);
                }
            }
            else
            {
                currentTower.CycleTargetingPriority();
                RefreshUI();
            }
        }

        public void Hide()
        {
            if (currentTower != null) currentTower.SetRangeVisible(false);
            
            mainPanel.SetActive(false);
            currentSlot = null;
            currentTower = null;
        }

        private void Update()
        {
            if (mainPanel.activeSelf && Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
