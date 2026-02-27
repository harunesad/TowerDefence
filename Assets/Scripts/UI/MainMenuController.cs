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
        [SerializeField] private GameObject sideSelectionPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button skillTreeButton;
        [SerializeField] private Button quitButton;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);

        private GameObject currentActivePanel;

        private void Start()
        {
            // İlk açılışta tüm panelleri hazırla (CanvasGroup alpha = 0 yapabiliriz)
            InitPanel(mainMenuPanel);
            InitPanel(levelSelectPanel);
            InitPanel(skillTreePanel);
            InitPanel(sideSelectionPanel);

            // Buton olaylarını otomatik bağla
            if (playButton != null) playButton.onClick.AddListener(ShowLevelSelect);
            if (skillTreeButton != null) skillTreeButton.onClick.AddListener(ShowSkillTree);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            // İlk paneli aç
            currentActivePanel = mainMenuPanel;
            mainMenuPanel.SetActive(true);
            AnimatePanelIn(mainMenuPanel);
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

        private void AnimatePanelIn(GameObject panel)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
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
