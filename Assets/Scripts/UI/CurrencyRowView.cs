using TMPro;
using UnityEngine;

namespace TowerDefence.UI
{
    public static class CurrencyRowView
    {
        public static void Apply(Transform host, int karma, int crystals, bool showPlus)
        {
            if (host == null) return;
            Transform row = host.Find("CurrencyRow");
            if (row == null) return;

            ApplyGroup(row.Find("KarmaGroup"), karma, showPlus);
            ApplyGroup(row.Find("CrystalGroup"), crystals, showPlus);
        }

        private static void ApplyGroup(Transform group, int amount, bool showPlus)
        {
            if (group == null) return;
            group.gameObject.SetActive(amount > 0);
            if (amount <= 0) return;

            TextMeshProUGUI value = group.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (value != null)
                value.text = showPlus ? $"+{amount}" : amount.ToString();
        }
    }
}
