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

            if (impactEffect == null)
            {
#if UNITY_EDITOR
                GameObject sparkPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hovl Studio/Magic effects pack/Prefabs/Sparks/Sparks blue.prefab");
                if (sparkPrefab != null)
                {
                    impactEffect = Instantiate(sparkPrefab, transform);
                    impactEffect.SetActive(false);
                }
#endif
            }
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
                if (!impactEffect.activeSelf) impactEffect.SetActive(true);
                impactEffect.transform.position = target.position;
                impactEffect.transform.LookAt(firePoint.position);
            }
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
