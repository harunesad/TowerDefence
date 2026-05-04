using UnityEngine;
using TMPro;
using TowerDefence.Core;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI totalKarmaText;
        [SerializeField] private TextMeshProUGUI feedbackText; // Yeni: Hata/Bilgi mesajı alanı
        [SerializeField] private List<SkillNodeUI> allNodes;
        [SerializeField] private Button backButton;

        private float feedbackTimer;

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowMainMenu();
                });
            }
        }

        private void OnEnable()
        {
            UpdateAllNodes();
        }

        private void Update()
        {
            if (feedbackTimer > 0)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0 && feedbackText != null)
                {
                    feedbackText.text = "";
                }
            }
        }

        public void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
                feedbackTimer = 3f; // 3 saniye sonra silinir
            }
        }

        public void UpdateAllNodes()
        {
            // Toplam Karma miktarını güncelle
            if (totalKarmaText != null)
            {
                totalKarmaText.text = $"Karma: {MetaProgressionManager.Instance.GetTotalKarma()}";
            }

            // Tüm düğümleri yenile
            foreach (var node in allNodes)
            {
                if (node != null) node.RefreshStatus();
            }
        }

        // Oyunun başka yerlerinden Karma kazanıldığında UI'ı yenilemek için
        public void AddTestKarma(int amount)
        {
            MetaProgressionManager.Instance.AddKarma(amount);
            UpdateAllNodes();
        }
    }
}
