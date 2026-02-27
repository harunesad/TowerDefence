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

    [CreateAssetMenu(fileName = "New Level Data", menuName = "Tower Defence/Level/Level Data")]
    public class LevelData : ScriptableObject
    {
        public string levelName;
        public int sceneIndex;
        public Sprite levelPreview;
        public LevelType levelType;
        [Range(1, 3)] public int difficulty = 1;
        public string levelID; // Kilit takibi için benzersiz ID

        [Header("Map Settings")]
        public GameObject mapPrefab;

        [Header("Waves")]
        public List<WaveData> waves;

        [Header("Starting Resources")]
        public int startingCurrencyLight = 200;
        public int startingCurrencyDark = 300;
    }
}
