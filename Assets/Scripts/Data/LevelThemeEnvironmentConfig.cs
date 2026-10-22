using UnityEngine;
using TowerDefence.Data;

namespace TowerDefence.Data
{
    [CreateAssetMenu(fileName = "LevelThemeEnvironmentConfig", menuName = "Tower Defence/Config/Level Theme Environment Config")]
    public class LevelThemeEnvironmentConfig : ScriptableObject
    {
        public enum BackgroundMode
        {
            SolidColor,
            SkyboxMaterial
        }

        [Header("Mode")]
        [Tooltip("SolidColor: Kamera arka plan rengi kullanılır. SkyboxMaterial: Klasik skybox kullanılır.")]
        public BackgroundMode backgroundMode = BackgroundMode.SolidColor;

        [Header("Theme Solid Colors (Mode = SolidColor)")]
        [Tooltip("Forest seviyeleri için kamera arka plan rengi")]
        public Color forestColor = new Color(0.18f, 0.28f, 0.22f, 1f); // Doğal yumuşak orman yeşili/tonu

        [Tooltip("Desert seviyeleri için kamera arka plan rengi")]
        public Color desertColor = new Color(0.38f, 0.30f, 0.20f, 1f); // Çöl kumu / sıcak toprak tonu

        [Tooltip("Snow seviyeleri için kamera arka plan rengi")]
        public Color snowColor = new Color(0.24f, 0.32f, 0.40f, 1f); // Soğuk kış mavisi/grisi

        [Tooltip("Lava seviyeleri için kamera arka plan rengi")]
        public Color lavaColor = new Color(0.28f, 0.12f, 0.10f, 1f); // Koyu lav / kızıl kül tonu

        [Tooltip("Underworld seviyeleri için kamera arka plan rengi")]
        public Color underworldColor = new Color(0.14f, 0.10f, 0.22f, 1f); // Mistik yeraltı moru/karanlık

        [Tooltip("Belirlenmemiş temalar için varsayılan renk")]
        public Color defaultColor = new Color(0.15f, 0.18f, 0.22f, 1f);

        [Header("Theme Skyboxes (Mode = SkyboxMaterial)")]
        public Material forestSkybox;
        public Material desertSkybox;
        public Material snowSkybox;
        public Material lavaSkybox;
        public Material underworldSkybox;
        public Material defaultSkybox;

        public Color GetColorForTheme(LevelTheme theme)
        {
            switch (theme)
            {
                case LevelTheme.Forest: return forestColor;
                case LevelTheme.Desert: return desertColor;
                case LevelTheme.Snow: return snowColor;
                case LevelTheme.Lava: return lavaColor;
                case LevelTheme.Underworld: return underworldColor;
                default: return defaultColor;
            }
        }

        public Material GetSkyboxForTheme(LevelTheme theme)
        {
            Material mat = null;
            switch (theme)
            {
                case LevelTheme.Forest: mat = forestSkybox; break;
                case LevelTheme.Desert: mat = desertSkybox; break;
                case LevelTheme.Snow: mat = snowSkybox; break;
                case LevelTheme.Lava: mat = lavaSkybox; break;
                case LevelTheme.Underworld: mat = underworldSkybox; break;
            }
            return mat != null ? mat : defaultSkybox;
        }
    }
}
