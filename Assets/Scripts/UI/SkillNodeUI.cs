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

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSFX;
        [SerializeField] private AudioClip errorSFX;

        private void Start()
        {
            if (skillData != null) SetupUI(skillData);
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        public void SetupUI(SkillNodeData data)
        {
            skillData = data;
            iconImage.sprite = data.icon;
            costText.text = data.karmaCost.ToString();
            RefreshStatus();
        }

        public void RefreshStatus()
        {
            bool isUnlocked = MetaProgressionManager.Instance.IsSkillUnlocked(skillData.skillID);
            
            purchasedOverlay.gameObject.SetActive(isUnlocked);
            buyButton.interactable = !isUnlocked;

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
            
            // Eğer puanı yetmiyorsa veya önkoşul sağlanmamışsa buton pasif olabilir
            if (!isUnlocked)
            {
                bool canAfford = MetaProgressionManager.Instance.GetTotalKarma() >= skillData.karmaCost;
                buyButton.interactable = requirementsMet && canAfford;
            }
        }

        private void OnBuyClicked()
        {
            if (MetaProgressionManager.Instance.TryUnlockSkill(skillData))
            {
                if (AudioManager.Instance != null && unlockSFX != null)
                    AudioManager.Instance.PlaySFX(unlockSFX);

                // UI'ı bir üst seviyede yenilemek daha iyi olabilir
                SendMessageUpwards("UpdateAllNodes", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if (AudioManager.Instance != null && errorSFX != null)
                    AudioManager.Instance.PlaySFX(errorSFX);
            }
        }

        public SkillNodeData GetSkillData() => skillData;
    }
}
