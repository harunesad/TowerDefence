using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    /// <summary>
    /// Aura Tower bileşeni: Yakındaki dost kulelere hasar ve ateş hızı buffu uygular.
    /// Aura alanı zemin spriteı ile görselleştirilir, etkilenen kuleler turkuaz parlaklıkla belirtilir.
    /// </summary>
    [RequireComponent(typeof(Tower))]
    public class AuraTower : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer auraSprite;   // Zemin halkası (Inspector'dan atanır veya otomatik oluşturulur)
        [SerializeField] private Color auraColor = new Color(0.2f, 0.7f, 1f, 0.25f);

        private Tower myTower;
        private TowerData myData;
        private float radius;
        private float damageBonus;
        private float fireRateBonus;
        private Side mySide;

        // Şu an buffluyor olduğumuz kuleler
        private readonly HashSet<Tower> buffedTowers = new HashSet<Tower>();

        private void Awake()
        {
            myTower = GetComponent<Tower>();
        }

        private void Start()
        {
            myData = myTower.GetTowerData();
            if (myData == null || !myData.isAuraTower)
            {
                enabled = false;
                return;
            }

            radius = myData.auraRadius;
            damageBonus = myData.auraDamageBonus;
            fireRateBonus = myData.auraFireRateBonus;
            mySide = myTower.GetSide();

            SetupAuraSprite();
        }

        private void SetupAuraSprite()
        {
            if (auraSprite == null)
            {
                // Otomatik oluştur
                GameObject spriteGO = new GameObject("AuraRadius");
                spriteGO.transform.SetParent(transform);
                spriteGO.transform.localPosition = Vector3.zero;
                spriteGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Yere yatır
                auraSprite = spriteGO.AddComponent<SpriteRenderer>();
                auraSprite.sprite = CreateCircleSprite();
                auraSprite.sortingOrder = -1;
            }

            auraSprite.color = auraColor;

            // Radius'a göre ölçekle (Sprite 1 Unity Unit = 1 dünya birimi varsayımı)
            float scale = radius * 2f;
            auraSprite.transform.localScale = new Vector3(scale, scale, 1f);
        }

        /// <summary>Prosedürel yumuşak çember sprite oluşturur (dışa gidildikçe şeffaflaşır).</summary>
        private Sprite CreateCircleSprite()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerR = size / 2f;
            float innerR = outerR * 0.55f;   // İç kısım şeffaf
            float edgeW  = outerR * 0.18f;   // Kenar kalınlığı

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);

                    float alpha;
                    if (dist > outerR)
                    {
                        alpha = 0f;
                    }
                    else if (dist < innerR)
                    {
                        // İç merkez: çok hafif dolgu
                        alpha = 0.04f;
                    }
                    else
                    {
                        // Kenar bandı: yumuşak geçiş
                        float t = (dist - innerR) / (outerR - innerR);
                        alpha = Mathf.SmoothStep(0f, 0.85f, t) * Mathf.SmoothStep(0f, 0.85f, 1f - (dist - outerR + edgeW) / edgeW);
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void Update()
        {
            if (PhaseManager.Instance == null) return;
            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            RefreshBuffedTowers();
        }

        private void RefreshBuffedTowers()
        {
            // Aura içindeki aynı taraf kulelerini bul
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            HashSet<Tower> currentInRange = new HashSet<Tower>();

            foreach (Collider col in hits)
            {
                // Kendi Tower bileşenimizi atlıyoruz
                Tower t = col.GetComponent<Tower>();
                if (t == null || t == myTower) continue;
                if (t.GetSide() != mySide) continue;

                currentInRange.Add(t);
                if (!buffedTowers.Contains(t))
                {
                    t.ApplyAuraBuff(damageBonus, fireRateBonus);
                    buffedTowers.Add(t);
                }
            }

            // Aralıktan çıkanların buffını kaldır
            List<Tower> toRemove = new List<Tower>();
            foreach (Tower t in buffedTowers)
            {
                if (!currentInRange.Contains(t))
                {
                    if (t != null) t.RemoveAuraBuff();
                    toRemove.Add(t);
                }
            }
            foreach (Tower t in toRemove) buffedTowers.Remove(t);
        }

        private void OnDestroy()
        {
            // Kule yıkılınca tüm buffly temizle
            foreach (Tower t in buffedTowers)
            {
                if (t != null) t.RemoveAuraBuff();
            }
            buffedTowers.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (myData != null)
            {
                Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, myData.auraRadius);
            }
        }
    }
}
