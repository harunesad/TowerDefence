using UnityEngine;

namespace TowerDefence.Combat
{
    /// <summary>
    /// Attach this to the child GameObject that holds the Animator.
    /// It forwards animation events (like OnAttackHit) up to the parent Unit script.
    /// </summary>
    public class AnimationEventHandler : MonoBehaviour
    {
        private Unit parentUnit;

        private void Awake()
        {
            parentUnit = GetComponentInParent<Unit>();
        }

        // Animation Event'ten çağrılır
        public void OnAttackHit()
        {
            if (parentUnit != null)
            {
                parentUnit.OnAttackHit();
            }
        }
    }
}
