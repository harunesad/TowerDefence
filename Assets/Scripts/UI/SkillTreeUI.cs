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
        [SerializeField] private List<SkillNodeUI> allNodes;
        [SerializeField] private Button backButton;

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
