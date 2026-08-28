using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private AudioClip clickSound;

        private void Awake()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(PlaySound);
            }
        }

        private void PlaySound()
        {
            if (AudioManager.Instance != null && clickSound != null)
            {
                AudioManager.Instance.PlaySFX(clickSound, 1f, Random.Range(0.9f, 1.1f));
            }
        }

        public void SetSound(AudioClip clip)
        {
            clickSound = clip;
        }
    }
}
