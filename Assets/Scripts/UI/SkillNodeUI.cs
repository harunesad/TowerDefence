using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SkillNodeUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SkillNodeData skillData;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image lockedOverlay;
        [SerializeField] private Image purchasedOverlay;
        [SerializeField] private TextMeshProUGUI typeText; // Yeni: PASİF / AKTİF etiketi

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSFX;
        [SerializeField] private AudioClip errorSFX;

        private void Start()
        {
            if (skillData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            SetupUI(skillData);
            buyButton.onClick.AddListener(OnNodeClicked);
        }

        public void SetupUI(SkillNodeData data)
        {
            skillData = data;
            iconImage.sprite = data.icon;
            
            if (data.crystalCost > 0)
                costText.text = $"{data.karmaCost}/{data.crystalCost}";
            else
                costText.text = data.karmaCost.ToString();

            if (typeText != null)
            {
                bool isActive = data.upgradeType == UpgradeType.UnlockSpell;
                typeText.text = isActive ? "ACTIVE" : "PASSIVE";
                typeText.color = isActive ? new Color(1f, 0.8f, 0.2f) : Color.white; // Aktifler altın sarısı
            }

            RefreshStatus();
        }

        public void RefreshStatus()
        {
            if (skillData == null) return;

            bool isUnlocked = MetaProgressionManager.Instance.IsSkillUnlocked(skillData.skillID);
            
            purchasedOverlay.gameObject.SetActive(isUnlocked);
            // Buton her zaman tıklanabilir olmalı ki detay paneli açılsın
            buyButton.interactable = true; 

            // Önkoşul kontrolü
            bool requirementsMet = true;
            foreach (var req in skillData.requiredSkills)
            {
                if (!MetaProgressionManager.Instance.IsSkillUnlocked(req.skillID))
                {
                    requirementsMet = false;
                    break;
                }
            }

            lockedOverlay.gameObject.SetActive(!requirementsMet && !isUnlocked);
            
            if (!isUnlocked)
            {
                // Puan yetmiyorsa maliyet metnini kırmızı yap, yetiyorsa yeşil
                bool canAffordKarma = MetaProgressionManager.Instance.GetTotalKarma() >= skillData.karmaCost;
                bool canAffordCrystals = MetaProgressionManager.Instance.GetTotalCrystals() >= skillData.crystalCost;
                
                costText.color = (canAffordKarma && canAffordCrystals) ? new Color(0.6f, 1f, 0.6f) : Color.red;
            }
        }

        private void OnNodeClicked()
        {
            SkillTreeUI treeUI = GetComponentInParent<SkillTreeUI>();
            if (treeUI != null)
            {
                treeUI.OnNodeClicked(skillData);
            }
        }

        public SkillNodeData GetSkillData() => skillData;
    }
}
