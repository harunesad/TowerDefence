using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Core;

namespace TowerDefence.Combat
{
    public class CorruptionManager : MonoBehaviour
    {
        public static CorruptionManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float speedMultiplier = 1.3f;
        [SerializeField] private LayerMask corruptionLayer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            InvokeRepeating(nameof(TriggerCorruptionVFX), 1f, 2f);
        }

        private void TriggerCorruptionVFX()
        {
            if (VFXManager.Instance == null) return;

            // Logic representing all corruption points/zones in the scene.
            // Example: Spawns effects above all active Corruption Nodes.
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("CorruptionNode");
            foreach (var node in nodes)
            {
                VFXManager.Instance.SpawnVFX(VFXType.CorruptionPulse, node.transform.position, Quaternion.identity);
            }
        }

        public bool IsPositionCorrupted(Vector3 position)
        {
            // Checks whether a "Corruption Node" exists at that position
            return Physics.CheckSphere(position, 1f, corruptionLayer);
        }

        public float GetSpeedModifier(Vector3 position)
        {
            return IsPositionCorrupted(position) ? speedMultiplier : 1f;
        }
    }
}
