using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;
using System.Collections.Generic;
using UnityEngine.InputSystem;

using TowerDefence.Combat;
using TowerDefence.UI;

namespace TowerDefence.Grid
{
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [SerializeField] private List<TowerData> allTowers;
        public List<TowerData> AllTowers => allTowers;

        private TowerSlot activeSlot;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            // Yeni Input Sistemi kontrolü
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // UI üzerinden tıklama yapılıyorsa (buton vb.) Raycast atma
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                HandleMouseClick();
            }
        }

        private void HandleMouseClick()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                TowerSlot slot = hit.collider.GetComponent<TowerSlot>();
                if (slot != null)
                {
                    slot.HandleClick();
                }
            }
        }

        public void StartPlacementAtSlot(TowerSlot slot)
        {
            // Eğer daha önce başka bir slot aktifse onun menüsünü kapatabilirsin
            activeSlot = slot;
        }

        public void SelectTower(TowerData data)
        {
            if (activeSlot == null) return;

            if (CurrencyManager.Instance.TrySpendCurrency(data.side, data.cost))
            {
                activeSlot.PlaceTower(data.prefab, data);
                
                if (VFXManager.Instance != null)
                    VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, activeSlot.GetPlacementPosition(), Quaternion.identity);

                CloseSelection();
            }
            else
            {
                Debug.Log("Not enough currency!");
            }
        }

        public void CloseSelection()
        {
            activeSlot = null;
        }
    }
}
