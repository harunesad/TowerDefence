using UnityEngine;

namespace TowerDefence.Core
{
    [CreateAssetMenu(fileName = "TutorialArt", menuName = "Tower Defence/Tutorial Art")]
    public class TutorialArt : ScriptableObject
    {
        public Sprite card;
        public Sprite button;
        public Sprite next;
        public Sprite mira;
    }
}
