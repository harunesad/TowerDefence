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
        [SerializeField] private GameObject impactSparkPrefab;

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
            if (lineRenderer == null)
            {
                GameObject beamGO = new GameObject("BeamRenderer");
                beamGO.transform.SetParent(transform);
                beamGO.transform.localPosition = Vector3.zero;
                lineRenderer = beamGO.AddComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = beamWidth;
                lineRenderer.endWidth = beamWidth * 0.7f;

                // Parlayan additive shader/materyal kullan
                Shader beamShader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                Material mat = new Material(beamShader);
                mat.EnableKeyword("_EMISSION");
                lineRenderer.material = mat;
                lineRenderer.startColor = new Color(0.2f, 0.9f, 1f, 1f);
                lineRenderer.endColor = new Color(0.8f, 0.2f, 1f, 0.9f);
                lineRenderer.useWorldSpace = true;
                lineRenderer.enabled = false;
            }

            if (impactEffect == null && impactSparkPrefab != null)
            {
                impactEffect = Instantiate(impactSparkPrefab, transform);
                impactEffect.SetActive(false);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.HasMatchEnded())
            {
                DisableBeam();
                return;
            }

            if (baseTower != null && baseTower.IsDisabled)
            {
                DisableBeam();
                return;
            }

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

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable == null) damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.IsDead)
            {
                DisableBeam();
                return;
            }

            bool beamWasOff = !lineRenderer.enabled;
            if (beamWasOff) lineRenderer.enabled = true;
            if (beamWasOff)
                PlayBeamStart();

            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, target.position);

            float dps = baseTower != null ? baseTower.GetCurrentDamage() : damagePerSecond;
            damageable.TakeDamage(dps * Time.deltaTime);

            // Efektler
            if (impactEffect != null)
            {
                if (!impactEffect.activeSelf) impactEffect.SetActive(true);
                impactEffect.transform.position = target.position;
                impactEffect.transform.LookAt(firePoint.position);
            }
        }

        private void PlayBeamStart()
        {
            if (baseTower == null || AudioManager.Instance == null) return;
            TowerDefence.Data.TowerData data = baseTower.GetTowerData();
            if (data != null && data.shootSFX != null)
                AudioManager.Instance.PlaySFX(data.shootSFX);
        }

        public void StopBeam()
        {
            target = null;
            DisableBeam();
        }

        private void DisableBeam()
        {
            if (lineRenderer != null && lineRenderer.enabled) lineRenderer.enabled = false;
            if (impactEffect != null && impactEffect.activeSelf) impactEffect.SetActive(false);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
