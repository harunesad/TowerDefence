using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.VFX
{
    public class PooledVFX : MonoBehaviour
    {
        [SerializeField] private VFXType type;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private bool autoReturnByTime = true;

        private void OnEnable()
        {
            if (!autoReturnByTime) return;
            if (VFXManager.Instance == null || transform.parent != VFXManager.Instance.transform)
                return;
            Invoke(nameof(ReturnToPool), lifeTime);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        public void ReturnToPool()
        {
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.ReturnVFX(type, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
