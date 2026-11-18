using UnityEngine;
using System.Collections.Generic;

namespace TowerDefence.Data
{
    [System.Serializable]
    public class WaveUnitGroup
    {
        public UnitData unitData;
        public int count;
        public float spawnInterval = 1f;
        public float groupDelay = 0f; // Delay before starting the group
        public int spawnerIndex = 0; // Which spawner will spawn this group?
    }

    [CreateAssetMenu(fileName = "New Wave Data", menuName = "Tower Defence/Level/Wave Data")]
    public class WaveData : ScriptableObject
    {
        public List<WaveUnitGroup> unitGroups;
        public float timeBeforeNextWave = 10f;
    }
}
