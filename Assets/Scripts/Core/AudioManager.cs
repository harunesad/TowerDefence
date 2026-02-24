using UnityEngine;
using System.Collections.Generic;

namespace TowerDefence.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int sfxPoolSize = 10;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        private List<AudioSource> sfxPool = new List<AudioSource>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Add(source);
            }
        }

        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null) return;
            if (bgmSource.clip == clip) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.pitch = pitch;
                source.volume = volume;
                source.PlayOneShot(clip);
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in sfxPool)
            {
                if (!source.isPlaying) return source;
            }

            // Gerekirse havuzu büyüt
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            sfxPool.Add(newSource);
            return newSource;
        }
    }
}
