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

        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 1f;
        public float SFXVolume { get; private set; } = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                
                if (bgmSource == null)
                {
                    bgmSource = gameObject.AddComponent<AudioSource>();
                    bgmSource.loop = true;
                }
                
                LoadSettings();
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            ApplyVolumes();
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            ApplyVolumes();
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            ApplyVolumes();
        }

        public void SetSFXVolume(float value)
        {
            SFXVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null)
                bgmSource.volume = MusicVolume * MasterVolume;
            
            foreach (var source in sfxPool)
            {
                if (source != null)
                    source.volume = SFXVolume * MasterVolume;
            }
            PlayerPrefs.Save();
        }

        private void InitializePool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = SFXVolume * MasterVolume;
                sfxPool.Add(source);
            }
        }

        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null) return;
            if (bgmSource.clip == clip) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = MusicVolume * MasterVolume;
            bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.pitch = pitch;
                source.volume = SFXVolume * MasterVolume * volumeMultiplier;
                source.PlayOneShot(clip);
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in sfxPool)
            {
                if (!source.isPlaying) return source;
            }

            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.volume = SFXVolume * MasterVolume;
            sfxPool.Add(newSource);
            return newSource;
        }
    }
}
