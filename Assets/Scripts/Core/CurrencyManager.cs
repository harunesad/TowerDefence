using UnityEngine;
using System;

namespace TowerDefence.Core
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        public event Action<Side, int> OnCurrencyChanged;

        [Header("Starting Values")]
        [SerializeField] private int startingGold = 100;
        [SerializeField] private int startingSoul = 100;

        private int gold;
        private int soul;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            gold = startingGold;
            soul = startingSoul;
        }

        private void Start()
        {
            OnCurrencyChanged?.Invoke(Side.Light, gold);
            OnCurrencyChanged?.Invoke(Side.Dark, soul);
        }

        public bool CanAfford(Side side, int amount)
        {
            return GetCurrency(side) >= amount;
        }

        public void AddCurrency(Side side, int amount)
        {
            if (side == Side.Light) gold += amount;
            else if (side == Side.Dark) soul += amount;
            
            OnCurrencyChanged?.Invoke(side, GetCurrency(side));
            Debug.Log($"{side} Currency Added: {amount}. New Total: {GetCurrency(side)}");
        }

        public bool TrySpendCurrency(Side side, int amount)
        {
            if (CanAfford(side, amount))
            {
                if (side == Side.Light) gold -= amount;
                else if (side == Side.Dark) soul -= amount;

                OnCurrencyChanged?.Invoke(side, GetCurrency(side));
                Debug.Log($"{side} Currency Spent: {amount}. Remaining: {GetCurrency(side)}");
                return true;
            }
            
            return false;
        }

        public int GetCurrency(Side side)
        {
            return side == Side.Light ? gold : soul;
        }
    }
}
