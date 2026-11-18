using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    public enum LevelType
    {
        Maze,
        SplitPath,
        MultiEntrance,
        Bottleneck
    }

    public enum LevelTheme
    {
        Forest,
        Desert,
        Snow,
        Lava,
        Underworld
    }

    [System.Serializable]
    public class LevelPath
    {
        public List<Vector3> points = new List<Vector3>();
        public int spawnerIndex = 0;
    }

    [CreateAssetMenu(fileName = "New Level Data", menuName = "Tower Defence/Level/Level Data")]
    public class LevelData : ScriptableObject
    {
        public string levelName;
        public int sceneIndex;
        public LevelType levelType;
        [Range(1, 5)] public int difficulty = 1;
        public string levelID; // Unique ID for unlock tracking

        [Header("Map Generation Settings")]
        public LevelTheme theme;
        [Tooltip("Optional: Select if you want to assign a custom background color to this level instead of the theme color/skybox.")]
        public Color customBackgroundColor = Color.clear;
        [Tooltip("Optional: Assign if skybox mode is active and a custom skybox is desired for this level.")]
        public Material customSkybox;
        public List<LevelPath> paths = new List<LevelPath>();
        public List<Vector3> basePoints = new List<Vector3>();
        public List<Vector3> customSlotPositions = new List<Vector3>();
        public GameObject mapPrefab;

        [Header("Waves")]
        public List<WaveData> waves;

        [Header("Starting Resources")]
        public int startingCurrencyLight = 200;
        public int startingCurrencyDark = 300;
    }
}
