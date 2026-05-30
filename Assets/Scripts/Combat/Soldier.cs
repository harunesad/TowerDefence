using UnityEngine;
using TowerDefence.Combat;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    public class Soldier : Unit
    {
        [Header("Soldier Settings")]
        public float searchRadius = 8f;
        
        private Vector3 rallyPoint;
        private bool hasRallyPoint;
        private BarracksTower myBarracks;

        public void SetBarracks(BarracksTower barracks)
        {
            myBarracks = barracks;
        }

        public void SetRallyPoint(Vector3 point)
        {
            rallyPoint = point;
            hasRallyPoint = true;
        }

        protected override void Update()
        {
            if (IsDead) return;
            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            // 1. Düşman Taraması (Sadece birini engellemiyorsak)
            if (CanBlockMore())
            {
                SearchAndBlockEnemy();
            }

            // 2. Savaş ve Hareket Mantığı (Base sınıf kovalama ve saldırıyı halleder)
            base.Update();

            // 3. Eğer engellemiyorsak ve savaşmıyorsak Rally Point'e dön
            if (!IsBlocked && !IsAttacking() && GetTarget() == null && hasRallyPoint)
            {
                MoveToRallyPoint();
            }
        }

        private void SearchAndBlockEnemy()
        {
            // Düşman katmanını belirle (Side.Light isek Dark olanlara (7) bak)
            int enemyLayer = (GetSide() == Side.Light) ? (1 << 7) : (1 << 6);
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, enemyLayer);

            foreach (var hit in hits)
            {
                Unit enemy = hit.GetComponent<Unit>();
                // Sadece ölmemiş ve başka biri tarafından engellenmemiş düşmanları engelle
                if (enemy != null && !enemy.IsDead && !enemy.IsBlocked)
                {
                    Debug.Log($"<color=green>[SOLDIER]</color> {gameObject.name} engaged {enemy.gameObject.name}!");
                    StartBlocking(enemy);
                    break; 
                }
            }
        }

        private void MoveToRallyPoint()
        {
            Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatRally = new Vector3(rallyPoint.x, 0, rallyPoint.z);
            float dist = Vector3.Distance(flatPos, flatRally);
            
            if (dist > 0.2f)
            {
                // Unit.cs'deki MoveTowardsTarget ve HandleRotation metodlarını kullan
                MoveTowardsTarget(rallyPoint);
                HandleRotation(rallyPoint);
                
                if (animator != null) animator.SetBool("IsMoving", true);
            }
            else
            {
                if (animator != null) animator.SetBool("IsMoving", false);
            }
        }
    }
}
