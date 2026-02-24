using UnityEngine;
using UnityEngine.UI;

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

        private void Start()
        {
            // Buton olaylarını otomatik bağla
            if (playButton != null) playButton.onClick.AddListener(ShowLevelSelect);
            if (skillTreeButton != null) skillTreeButton.onClick.AddListener(ShowSkillTree);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            DeactivateAllPanels();
            mainMenuPanel.SetActive(true);
        }

        public void ShowLevelSelect()
        {
            DeactivateAllPanels();
            levelSelectPanel.SetActive(true);
        }

        public void ShowSkillTree()
        {
            DeactivateAllPanels();
            skillTreePanel.SetActive(true);
        }

        public void ShowSideSelection()
        {
            DeactivateAllPanels();
            sideSelectionPanel.SetActive(true);
        }

        private void DeactivateAllPanels()
        {
            mainMenuPanel.SetActive(false);
            levelSelectPanel.SetActive(false);
            skillTreePanel.SetActive(false);
            sideSelectionPanel.SetActive(false);
        }

        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Quit Game");
        }
    }
}
