using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SideSelectionUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button lightSideButton;
        [SerializeField] private Button darkSideButton;
        [SerializeField] private Button backButton;

        private void Start()
        {
            lightSideButton.onClick.AddListener(() => OnSideSelected(Side.Light));
            darkSideButton.onClick.AddListener(() => OnSideSelected(Side.Dark));

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowLevelSelect();
                });
            }
        }

        private void OnSideSelected(Side side)
        {
            SideController.Instance.SetPlayerSide(side);
            Debug.Log($"Side Selection UI: {side} selected.");
            
            // Taraf seçildikten sonra seviye yüklenir
            CampaignManager.Instance.LoadSelectedLevel();
        }
    }
}
