using UnityEngine;

namespace TowerDefence.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private float gridSize = 1f;
        [SerializeField] private LayerMask placementLayer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private System.Collections.Generic.Dictionary<Vector2Int, bool> occupiedCells = new System.Collections.Generic.Dictionary<Vector2Int, bool>();

        public Vector3 GetNearestPointOnGrid(Vector3 position)
        {
            float x = Mathf.Round(position.x / gridSize) * gridSize;
            float z = Mathf.Round(position.z / gridSize) * gridSize;
            return new Vector3(x, 0, z);
        }

        public Vector2Int WorldToGrid(Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / gridSize);
            int z = Mathf.RoundToInt(position.z / gridSize);
            return new Vector2Int(x, z);
        }

        public bool IsPlaceable(Vector3 position)
        {
            Vector2Int gridPos = WorldToGrid(position);
            return !occupiedCells.ContainsKey(gridPos) || !occupiedCells[gridPos];
        }

        public void OccupyCell(Vector3 position, bool occupied = true)
        {
            Vector2Int gridPos = WorldToGrid(position);
            occupiedCells[gridPos] = occupied;
        }
    }
}
