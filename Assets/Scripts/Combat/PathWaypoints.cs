using UnityEngine;
using System.Collections.Generic;

namespace TowerDefence.Combat
{
    public class PathWaypoints : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        public List<Transform> GetWaypoints() => waypoints;

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count < 2) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                }
            }
            Gizmos.DrawSphere(waypoints[waypoints.Count - 1].position, 0.3f);
        }
    }
}
