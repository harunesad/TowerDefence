using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private AudioClip clickSound;
        static AudioClip shared;

        private void Awake()
        {
            if (clickSound != null)
                shared = clickSound;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            Button button = GetComponent<Button>();
            if (button == null || !button.IsInteractable()) return;
            PlayClip(clickSound);
        }

        public void SetSound(AudioClip clip)
        {
            clickSound = clip;
            if (clip != null) shared = clip;
        }

        public static void Attach(Button button)
        {
            if (button == null) return;
            UIButtonSound sound = button.GetComponent<UIButtonSound>();
            if (sound == null) sound = button.gameObject.AddComponent<UIButtonSound>();
            if (sound.clickSound == null) sound.SetSound(SharedClip());
        }

        public static void Play()
        {
            PlayClip(null);
        }

        static AudioClip SharedClip()
        {
            if (shared != null) return shared;
            return AudioManager.UiClick;
        }

        static void PlayClip(AudioClip clip)
        {
            if (clip == null) clip = SharedClip();
            if (clip == null || AudioManager.Instance == null) return;
            AudioManager.Instance.PlaySFX(clip, 1f, Random.Range(0.9f, 1.1f));
        }
    }
}
