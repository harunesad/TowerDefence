using UnityEngine;
using System.Collections.Generic;

namespace TowerDefence.Combat
{
    public class PathWaypoints : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        [Header("Spline Settings")]
        [SerializeField] private bool useSpline = true;
        [SerializeField] private int splineResolution = 10;
        
        [HideInInspector] public List<Vector3> curveHandles = new List<Vector3>();
        private List<Vector3> cachedPathPoints;

        public List<Transform> GetWaypoints() => waypoints;

        public List<Vector3> GetPathPoints()
        {
            if (cachedPathPoints == null || cachedPathPoints.Count == 0 || !Application.isPlaying)
            {
                GeneratePathPoints();
            }
            return cachedPathPoints;
        }

        public void GeneratePathPoints()
        {
            cachedPathPoints = new List<Vector3>();
            if (waypoints == null || waypoints.Count == 0) return;
            
            // Eğer yeterli handle yoksa listeyi eşitle
            while (curveHandles.Count < waypoints.Count - 1)
            {
                curveHandles.Add(Vector3.zero);
            }
            
            if (!useSpline || waypoints.Count < 2)
            {
                foreach (var wp in waypoints)
                {
                    if (wp != null) cachedPathPoints.Add(wp.position);
                }
                return;
            }

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] == null || waypoints[i+1] == null) continue;

                Vector3 p0 = waypoints[i].position;
                Vector3 p2 = waypoints[i + 1].position;
                
                // Handle hiç ayarlanmamışsa (Vector3.zero) otomatik orta nokta yap
                Vector3 p1 = curveHandles[i];
                if (p1 == Vector3.zero) 
                {
                    p1 = (p0 + p2) / 2f;
                    curveHandles[i] = p1;
                }

                for (int j = 0; j < splineResolution; j++)
                {
                    float t = j / (float)splineResolution;
                    cachedPathPoints.Add(GetQuadraticBezier(t, p0, p1, p2));
                }
            }
            
            Transform lastWp = waypoints[waypoints.Count - 1];
            if (lastWp != null)
                cachedPathPoints.Add(lastWp.position);
        }

        private Vector3 GetQuadraticBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            Vector3 p = uu * p0; // (1-t)^2 * P0
            p += 2 * u * t * p1; // 2(1-t)t * P1
            p += tt * p2;        // t^2 * P2
            return p;
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            // Kontrol noktalarını (orijinal waypoints) çiz
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, (i == waypoints.Count - 1) ? 0.5f : 0.4f);
                }
            }

            // Kavisli veya düz yolu çiz
            GeneratePathPoints();
            if (cachedPathPoints != null && cachedPathPoints.Count > 1)
            {
                Gizmos.color = useSpline ? Color.magenta : Color.yellow;
                for (int i = 0; i < cachedPathPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(cachedPathPoints[i], cachedPathPoints[i+1]);
                }
            }
        }
    }
}
