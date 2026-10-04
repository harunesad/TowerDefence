using UnityEditor;
using UnityEngine;
using TowerDefence.Combat;

namespace TowerDefence.Editor
{
    [CustomEditor(typeof(PathWaypoints))]
    public class PathWaypointsEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            PathWaypoints path = (PathWaypoints)target;

            var waypoints = path.GetWaypoints();
            if (waypoints == null || waypoints.Count < 2) return;

            // Listeleri senkronize etmek için oluşturmayı zorla
            path.GeneratePathPoints();

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] == null || waypoints[i + 1] == null) continue;

                Vector3 p0 = waypoints[i].position;
                Vector3 p2 = waypoints[i + 1].position;

                if (i >= path.curveHandles.Count) continue;

                Vector3 p1 = path.curveHandles[i];
                // Eğer handle varsayılan değerindeyse (örneğin ilk açılışta), iki noktanın ortasını al
                if (p1 == Vector3.zero) p1 = (p0 + p2) / 2f;

                // Handle çizgilerini çiz (Bezier Tanjantları gibi görünsün)
                Handles.color = Color.green;
                Handles.DrawDottedLine(p0, p1, 4f);
                Handles.DrawDottedLine(p1, p2, 4f);

                // Ortadaki handle noktası
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, p1, Quaternion.identity, 0.5f, EventType.Repaint);

                EditorGUI.BeginChangeCheck();

                // Kullanıcının tutup çekebileceği Pozisyon tutamacı (Move Tool)
                Vector3 newHandlePos = Handles.PositionHandle(p1, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(path, "Move Path Curve Handle");
                    path.curveHandles[i] = newHandlePos;
                    path.GeneratePathPoints(); // Kavisleri hemen güncelle
                    EditorUtility.SetDirty(path);
                }
            }
        }
    }
}
