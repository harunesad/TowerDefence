using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence.Combat
{
    public class TowerObstacle : MonoBehaviour
    {
        public static readonly List<TowerObstacle> All = new List<TowerObstacle>();

        public float Radius { get; set; }

        public Vector3 Center => transform.position;

        private void OnEnable()
        {
            if (!All.Contains(this))
                All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }
    }
}
