using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace TowerDefence.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private GameObject heroShopPanel;
        [SerializeField] private GameObject sideSelectionPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button skillTreeButton;
        [SerializeField] private Button heroesButton;
        [SerializeField] private Button quitButton;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);

        private GameObject currentActivePanel;

        private void Start()
        {
            // Referans yoksa isimle bul (prefab bağlantısı kopsa bile çalışır)
            if (mainMenuPanel   == null) mainMenuPanel   = FindChild("MainMenuPanel");
            if (levelSelectPanel == null) levelSelectPanel = FindChild("LevelSelectionPanel");
            if (skillTreePanel   == null) skillTreePanel   = FindChild("SkillTreePanel");

            if (playButton      == null) playButton      = FindChildButton("PlayButton");
            if (skillTreeButton == null) skillTreeButton = FindChildButton("SkillTreeButton");
            if (quitButton      == null) quitButton      = FindChildButton("QuitButton");

            // Tüm panelleri kapat
            InitPanel(mainMenuPanel);
            InitPanel(levelSelectPanel);
            InitPanel(skillTreePanel);
            InitPanel(heroShopPanel);
            InitPanel(sideSelectionPanel);

            // Buton olaylarını bağla
            if (playButton      != null) playButton.onClick.AddListener(ShowLevelSelect);
            if (skillTreeButton != null) skillTreeButton.onClick.AddListener(ShowSkillTree);
            if (heroesButton    != null) heroesButton.onClick.AddListener(ShowHeroShop);
            if (quitButton      != null) quitButton.onClick.AddListener(QuitGame);

            // Ana menüyü hemen görünür olarak aç (fade yok, anında)
            if (mainMenuPanel != null)
            {
                currentActivePanel = mainMenuPanel;
                mainMenuPanel.SetActive(true);
                AnimatePanelIn(mainMenuPanel, instant: true);
            }
        }

        private GameObject FindChild(string childName)
        {
            Transform t = transform.Find(childName);
            return t != null ? t.gameObject : null;
        }

        private Button FindChildButton(string path)
        {
            Transform t = transform.Find("MainMenuPanel/ButtonContainer/" + path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private void InitPanel(GameObject panel)
        {
            if (panel == null) return;
            panel.SetActive(false);
            
            // Eğer CanvasGroup yoksa ekle (opsiyonel ama önerilir)
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group == null) group = panel.AddComponent<CanvasGroup>();
            group.alpha = 0;
            panel.transform.localScale = startScale;
        }

        public void ShowMainMenu() => TransitionToPanel(mainMenuPanel);
        public void ShowLevelSelect() => TransitionToPanel(levelSelectPanel);
        public void ShowSkillTree() => TransitionToPanel(skillTreePanel);
        public void ShowHeroShop() => TransitionToPanel(heroShopPanel);
        public void ShowSideSelection() => TransitionToPanel(sideSelectionPanel);

        private void TransitionToPanel(GameObject targetPanel)
        {
            if (targetPanel == null || targetPanel == currentActivePanel) return;

            // Mevcut paneli kapat
            if (currentActivePanel != null)
            {
                GameObject panelToClose = currentActivePanel;
                CanvasGroup group = panelToClose.GetComponent<CanvasGroup>();
                
                group.DOFade(0, fadeDuration).SetUpdate(true);
                panelToClose.transform.DOScale(startScale, fadeDuration).SetUpdate(true).OnComplete(() => {
                    panelToClose.SetActive(false);
                });
            }

            // Yeni paneli aç
            currentActivePanel = targetPanel;
            targetPanel.SetActive(true);
            AnimatePanelIn(targetPanel);
        }

        private void AnimatePanelIn(GameObject panel, bool instant = false)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group == null) group = panel.AddComponent<CanvasGroup>();

            if (instant)
            {
                group.alpha = 1f;
                panel.transform.localScale = Vector3.one;
                return;
            }

            group.alpha = 0;
            panel.transform.localScale = startScale;
            group.DOFade(1, fadeDuration).SetUpdate(true);
            panel.transform.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Quit Game");
        }
    }
}
