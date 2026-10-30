using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence.Data
{
    [CreateAssetMenu(fileName = "LoadingTipsData", menuName = "Tower Defence/UI/Loading Tips Data", order = 100)]
    public class LoadingTipsData : ScriptableObject
    {
        [Header("Loading Screen Tips")]
        [Tooltip("Sahneler arası geçişte alt kısımda rastgele gösterilecek ipuçları")]
        [TextArea(2, 5)]
        public List<string> tips = new List<string>()
        {
            "Tip: Upgrade your towers to deal more damage!",
            "Tip: Place towers at choke points for maximum effectiveness.",
            "Tip: Use spells wisely — they have long cooldowns.",
            "Tip: Different tower types are strong against different enemies.",
            "Tip: Don't forget to check the Skill Tree for permanent upgrades!",
            "Tip: Watch ads for extra Fortune Wheel spins.",
            "Tip: Heroes can turn the tide of battle — equip them wisely!",
            "Tip: Earn Karma by completing levels to unlock new skills.",
            "Tip: Try harder difficulties for greater challenges and rewards!",
            "Tip: Daily rewards reset every day — don't miss them!"
        };

        public string GetRandomTip()
        {
            if (tips == null || tips.Count == 0)
                return "Tip: Plan your defence carefully!";

            return tips[Random.Range(0, tips.Count)];
        }
    }
}
