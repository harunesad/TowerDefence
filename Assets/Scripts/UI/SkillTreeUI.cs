using UnityEngine;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Data;
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
        [Header("Tab Buttons")]
        [SerializeField] private Button btnLight;
        [SerializeField] private Button btnDark;
        [SerializeField] private Button btnNeutral;
        [SerializeField] private Button btnPassives;
        [SerializeField] private Button btnSpells;

        private void Awake()
        {
            if (btnLight != null) btnLight.onClick.AddListener(() => SetSide(0));
            if (btnDark != null) btnDark.onClick.AddListener(() => SetSide(1));
            if (btnNeutral != null) btnNeutral.onClick.AddListener(() => SetSide(2));
            
            if (btnPassives != null) btnPassives.onClick.AddListener(() => SetShowSpells(false));
            if (btnSpells != null) btnSpells.onClick.AddListener(() => SetShowSpells(true));
        }

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowMainMenu();
                });
            }
            
            // Başlangıç seçimi
            SetSide(0);
            SetShowSpells(false);
        }

        private void UpdateTabVisuals()
        {
            // Taraf buton renkleri
            if (btnLight != null) btnLight.image.color = currentSide == Side.Light ? Color.yellow : Color.white;
            if (btnDark != null) btnDark.image.color = currentSide == Side.Dark ? Color.yellow : Color.white;
            if (btnNeutral != null) btnNeutral.image.color = currentSide == Side.Neutral ? Color.yellow : Color.white;

            // Kategori buton renkleri
            if (btnPassives != null) btnPassives.image.color = !showSpells ? Color.cyan : Color.white;
            if (btnSpells != null) btnSpells.image.color = showSpells ? Color.cyan : Color.white;
        }

        [Header("Feedback")]
        private float feedbackTimer;

        public void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
                feedbackTimer = 3f;
            }
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

        [Header("Tab Settings")]
        private Side currentSide = Side.Light;
        private bool showSpells = false;

        private void OnEnable()
        {
            UpdateAllNodes();
            FilterNodes();
        }

        public void SetSide(int sideIndex)
        {
            currentSide = (Side)sideIndex;
            UpdateTabVisuals();
            FilterNodes();
        }

        public void SetShowSpells(bool value)
        {
            showSpells = value;
            UpdateTabVisuals();
            FilterNodes();
        }

        public void FilterNodes()
        {
            foreach (var node in allNodes)
            {
                if (node == null) continue;
                
                var data = node.GetSkillData();
                if (data == null)
                {
                    node.gameObject.SetActive(false);
                    continue;
                }

                bool sideMatch = (data.side == currentSide);
                bool categoryMatch = showSpells 
                    ? (data.upgradeType == UpgradeType.UnlockSpell)
                    : (data.upgradeType != UpgradeType.UnlockSpell);

                node.gameObject.SetActive(sideMatch && categoryMatch);
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
            
            FilterNodes();
        }

        // Oyunun başka yerlerinden Karma kazanıldığında UI'ı yenilemek için
        public void AddTestKarma(int amount)
        {
            MetaProgressionManager.Instance.AddKarma(amount);
            UpdateAllNodes();
        }
    }
}
