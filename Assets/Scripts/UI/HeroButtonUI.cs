using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Combat;

namespace TowerDefence.UI
{
    public class HeroButtonUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image respawnOverlay;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image selectionFrame;

        private int slotIndex = -1;
        private HeroUnit linkedHero;

        public void Setup(int index)
        {
            slotIndex = index;

            // Buton ile seçim özelliği kaldırıldı (Kullanıcı doğrudan 3D karakterin üzerine tıklayacak)
            if (selectButton != null)
            {
                selectButton.interactable = false;
            }

            RefreshHeroData();
            SetEmpty(!HasEquippedHero());
        }

        public void RefreshFromManager()
        {
            RefreshHeroData();
            SetEmpty(!HasEquippedHero());
        }

        private bool HasEquippedHero()
        {
            return HeroManager.Instance != null &&
                   HeroManager.Instance.GetEquippedHeroData(slotIndex) != null;
        }

        private void RefreshHeroData()
        {
            if (HeroManager.Instance == null) return;

            var data = HeroManager.Instance.GetEquippedHeroData(slotIndex);
            if (data != null && iconImage != null)
                iconImage.sprite = data.GetIcon();
        }

        private void OnEnable()
        {
            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.OnHeroSpawned += OnHeroSpawned;
                HeroManager.Instance.OnHeroDied += OnHeroDied;
                HeroManager.Instance.OnHeroRespawnTimerUpdated += OnRespawnTimerUpdated;
                HeroManager.Instance.OnHeroSelectionChanged += OnHeroSelectionChanged;
            }
        }

        private void OnDisable()
        {
            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.OnHeroSpawned -= OnHeroSpawned;
                HeroManager.Instance.OnHeroDied -= OnHeroDied;
                HeroManager.Instance.OnHeroRespawnTimerUpdated -= OnRespawnTimerUpdated;
                HeroManager.Instance.OnHeroSelectionChanged -= OnHeroSelectionChanged;
            }
        }

        private void Update()
        {
            if (linkedHero != null && !linkedHero.IsDead && healthSlider != null)
            {
                healthSlider.gameObject.SetActive(true);
                float maxHp = linkedHero.GetMaxHealth();
                healthSlider.value = maxHp > 0 ? linkedHero.GetHealth() / maxHp : 0f;
            }
        }

        private void OnHeroSpawned(int index, HeroUnit hero)
        {
            if (index != slotIndex) return;
            linkedHero = hero;
            if (respawnOverlay != null) respawnOverlay.gameObject.SetActive(false);
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (healthSlider != null) healthSlider.gameObject.SetActive(true);
        }

        private void OnHeroDied(int index)
        {
            if (index != slotIndex) return;
            linkedHero = null;
            if (respawnOverlay != null) respawnOverlay.gameObject.SetActive(true);
            if (timerText != null) timerText.gameObject.SetActive(true);
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);
            UpdateSelectionFrame(false);
        }

        private void OnRespawnTimerUpdated(int index, float current, float max)
        {
            if (index != slotIndex) return;
            if (respawnOverlay != null) respawnOverlay.fillAmount = current / max;
            if (timerText != null) timerText.text = $"{Mathf.CeilToInt(current)}s";
        }

        private void OnHeroSelectionChanged(HeroUnit hero)
        {
            UpdateSelectionFrame(linkedHero != null && hero == linkedHero);
        }

        private void UpdateSelectionFrame(bool active)
        {
            if (selectionFrame != null)
                selectionFrame.gameObject.SetActive(active);
        }

        private void OnButtonClicked()
        {
            if (HeroManager.Instance != null)
                HeroManager.Instance.SelectHeroSlot(slotIndex);
        }

        private void SetEmpty(bool empty)
        {
            gameObject.SetActive(!empty);
            if (empty)
            {
                linkedHero = null;
                if (healthSlider != null) healthSlider.gameObject.SetActive(false);
                if (respawnOverlay != null) respawnOverlay.gameObject.SetActive(false);
                if (timerText != null) timerText.gameObject.SetActive(false);
                UpdateSelectionFrame(false);
            }
        }
    }
}
