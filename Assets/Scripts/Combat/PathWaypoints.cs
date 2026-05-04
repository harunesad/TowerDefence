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

            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(waypoints[i].position, 0.4f);
                    Gizmos.color = Color.yellow;
                }
            }
            if (waypoints.Count > 0 && waypoints[waypoints.Count - 1] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(waypoints[waypoints.Count - 1].position, 0.5f);
            }
        }
    }
}
