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
        public int spawnerIndex = 0; // Hangi spawner bu grubu çıkaracak?
    }

    [CreateAssetMenu(fileName = "New Wave Data", menuName = "Tower Defence/Level/Wave Data")]
    public class WaveData : ScriptableObject
    {
        public List<WaveUnitGroup> unitGroups;
        public float timeBeforeNextWave = 10f;
    }
}
