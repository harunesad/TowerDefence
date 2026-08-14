using UnityEngine;

namespace TowerDefence.Combat
{
    public class FallingArrow : MonoBehaviour
    {
        [SerializeField] private float fallSpeed = 20f;
        [SerializeField] private float destroyY = 0.2f;

        private void Update()
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            if (transform.position.y <= destroyY)
            {
                Destroy(gameObject);
            }
        }
    }
}
