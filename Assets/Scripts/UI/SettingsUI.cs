using UnityEngine;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject backdrop;

        private void Start()
        {
            if (Core.AudioManager.Instance == null)
            {
                Debug.LogWarning("SettingsUI: AudioManager not found!");
                return;
            }

            // Init sliders with current values
            if (masterSlider != null)
            {
                masterSlider.value = Core.AudioManager.Instance.MasterVolume;
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicSlider != null)
            {
                musicSlider.value = Core.AudioManager.Instance.MusicVolume;
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = Core.AudioManager.Instance.SFXVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        private void Awake()
        {
            if (backdrop == null && transform.parent != null)
            {
                Transform bd = transform.parent.Find("SettingsBackdrop");
                if (bd != null) backdrop = bd.gameObject;
            }

            if (backdrop != null)
            {
                Button bb = backdrop.GetComponent<Button>();
                if (bb == null) bb = backdrop.AddComponent<Button>();
                bb.onClick.AddListener(Close);
            }
        }

        private void OnMasterVolumeChanged(float val)
        {
            if (Core.AudioManager.Instance != null)
                Core.AudioManager.Instance.SetMasterVolume(val);
        }

        private void OnMusicVolumeChanged(float val)
        {
            if (Core.AudioManager.Instance != null)
                Core.AudioManager.Instance.SetMusicVolume(val);
        }

        private void OnSFXVolumeChanged(float val)
        {
            if (Core.AudioManager.Instance != null)
                Core.AudioManager.Instance.SetSFXVolume(val);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            if (backdrop != null) backdrop.SetActive(true);
            
            // Re-sync values just in case
            if (Core.AudioManager.Instance != null)
            {
                if (masterSlider != null) masterSlider.SetValueWithoutNotify(Core.AudioManager.Instance.MasterVolume);
                if (musicSlider != null) musicSlider.SetValueWithoutNotify(Core.AudioManager.Instance.MusicVolume);
                if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(Core.AudioManager.Instance.SFXVolume);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (backdrop != null) backdrop.SetActive(false);
        }
    }
}
