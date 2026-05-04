using UnityEngine;
using System;
using TowerDefence.Data;

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
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            gold = startingGold;
            soul = startingSoul;
        }

        private void Start()
        {
            // İlk başlatma (Varsayılan)
            ResetToDefaults();
        }

        public void ResetToDefaults()
        {
            gold = startingGold;
            soul = startingSoul;
            ApplyMetaBonuses();
            OnCurrencyChanged?.Invoke(Side.Light, gold);
            OnCurrencyChanged?.Invoke(Side.Dark, soul);
        }

        public void InitializeFromLevel(LevelData level)
        {
            if (level == null) return;

            gold = level.startingCurrencyLight;
            soul = level.startingCurrencyDark;

            ApplyMetaBonuses();

            OnCurrencyChanged?.Invoke(Side.Light, gold);
            OnCurrencyChanged?.Invoke(Side.Dark, soul);
            
            Debug.Log($"CurrencyManager: Initialized from level {level.levelName}. Gold: {gold}, Soul: {soul}");
        }

        private void ApplyMetaBonuses()
        {
            if (MetaProgressionManager.Instance != null)
            {
                float lightMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.CurrencyStartBonus, Side.Light);
                float darkMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.CurrencyStartBonus, Side.Dark);

                gold = Mathf.RoundToInt(gold * lightMult);
                soul = Mathf.RoundToInt(soul * darkMult);
            }
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
