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
            
            // Eğer önkoşul sağlanmamışsa veya zaten alınmışsa buton pasif olur.
            // Puan yetmiyorsa bile buton aktif kalsın ki kullanıcı tıklayıp hata sesini duyabilsin/uyarı görebilsin.
            if (!isUnlocked)
            {
                buyButton.interactable = requirementsMet;
                
                // Puan yetmiyorsa maliyet metnini kırmızı yap, yetiyorsa yeşil
                bool canAfford = MetaProgressionManager.Instance.GetTotalKarma() >= skillData.karmaCost;
                costText.color = canAfford ? new Color(0.6f, 1f, 0.6f) : Color.red;
            }
        }

        private void OnBuyClicked()
        {
            SkillTreeUI treeUI = GetComponentInParent<SkillTreeUI>();

            if (MetaProgressionManager.Instance.TryUnlockSkill(skillData))
            {
                if (AudioManager.Instance != null && unlockSFX != null)
                    AudioManager.Instance.PlaySFX(unlockSFX);

                if (treeUI != null) treeUI.ShowFeedback($"{skillData.skillName} Açıldı!", Color.green);

                // UI'ı bir üst seviyede yenilemek daha iyi olabilir
                SendMessageUpwards("UpdateAllNodes", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if (AudioManager.Instance != null && errorSFX != null)
                    AudioManager.Instance.PlaySFX(errorSFX);

                if (treeUI != null)
                {
                    // Hata tipini belirle
                    bool canAfford = MetaProgressionManager.Instance.GetTotalKarma() >= skillData.karmaCost;
                    string msg = canAfford ? "Önkoşul Sağlanmadı!" : "Yetersiz Karma!";
                    treeUI.ShowFeedback(msg, Color.red);
                }
            }
        }

        public SkillNodeData GetSkillData() => skillData;
    }
}
