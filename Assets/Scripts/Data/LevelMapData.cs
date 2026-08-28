using UnityEngine;
using System.Collections.Generic;

namespace TowerDefence.Data
{
    [CreateAssetMenu(fileName = "New Level Map", menuName = "Tower Defence/Level/Level Map")]
    public class LevelMapData : ScriptableObject
    {
        public string mapName;
        public Sprite mapBackground;
        public List<LevelData> levels = new List<LevelData>();
    }
}