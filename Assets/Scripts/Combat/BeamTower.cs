using UnityEngine;
using TowerDefence.Interfaces;
using TowerDefence.Core;

namespace TowerDefence.Combat
{
    public class BeamTower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tower baseTower;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject impactEffect;

        [Header("Beam Settings")]
        [SerializeField] private float damagePerSecond = 50f;
        [SerializeField] private float beamWidth = 0.2f;

        private Transform target;

        private void Awake()
        {
            if (baseTower == null) baseTower = GetComponent<Tower>();
            ResolveFirePoint();
            ResolveLineRenderer();
        }

        private void ResolveFirePoint()
        {
            if (firePoint != null) return;

            Transform visuals = transform.Find("Visuals");
            if (visuals != null && visuals.childCount > 0)
            {
                Transform firstVisual = visuals.GetChild(0);
                Transform fp = firstVisual.Find("FirePoint");
                if (fp != null)
                {
                    firePoint = fp;
                    return;
                }
            }

            firePoint = transform.Find("FirePoint");
            if (firePoint == null) firePoint = transform;
        }

        private void ResolveLineRenderer()
        {
            if (lineRenderer != null) return;

            GameObject beamGO = new GameObject("BeamRenderer");
            beamGO.transform.SetParent(transform);
            beamGO.transform.localPosition = Vector3.zero;
            lineRenderer = beamGO.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.cyan;
            lineRenderer.endColor = Color.cyan;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat)
            {
                DisableBeam();
                return;
            }

            if (baseTower != null)
            {
                target = baseTower.GetCurrentTarget();
            }

            UpdateBeam();
        }

        private void UpdateBeam()
        {
            if (target == null)
            {
                DisableBeam();
                return;
            }

            if (!lineRenderer.enabled) lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, target.position);

            // Hasar Uygulama
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            // Efektler
            if (impactEffect != null)
            {
                impactEffect.transform.position = target.position;
                impactEffect.transform.LookAt(firePoint.position);
            }
        }

        private void DisableBeam()
        {
            if (lineRenderer.enabled) lineRenderer.enabled = false;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
