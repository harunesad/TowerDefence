using TMPro;
using UnityEngine;

namespace TowerDefence.UI
{
    public static class StatIcons
    {
        public static string Health => Icon("health");
        public static string Gold => Icon("gold");
        public static string Slow => Icon("slow");
        public static string Damage => Icon("damage");
        public static string Range => Icon("range");
        public static string Attack => Icon("attack");
        public static string Move => Icon("move");
        public static string Soldiers => Icon("soldiers");
        public static string Area => Icon("area");
        public static string Aura => Icon("aura");
        public static string Soul => Icon("soul");
        public static string Time => Icon("time");

        public static string Price(TowerDefence.Core.Side side)
        {
            if (side == TowerDefence.Core.Side.Dark) return Soul;
            if (side == TowerDefence.Core.Side.Light) return Gold;
            if (TowerDefence.Core.SideController.Instance != null &&
                TowerDefence.Core.SideController.Instance.GetPlayerSide() == TowerDefence.Core.Side.Dark)
                return Soul;
            return Gold;
        }

        static TMP_SpriteAsset asset;

        static string Icon(string name)
        {
            return "<color=#FFFFFF><sprite name=\"" + name + "\"></color>";
        }

        public static void Set(TMP_Text text, string body)
        {
            if (text == null) return;
            if (asset == null)
                asset = Resources.Load<TMP_SpriteAsset>("UI/StatIcons");
            text.richText = true;
            if (asset != null)
                text.spriteAsset = asset;
            text.text = body;
        }
    }
}
