using UnityEngine;

namespace TowerDefence.Core
{
    public enum Side
    {
        Light,
        Dark,
        Neutral
    }

    public class SideController : MonoBehaviour
    {
        public static SideController Instance { get; private set; }

        [SerializeField] private Side playerSide;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SetPlayerSide(Side side)
        {
            playerSide = side;
            Debug.Log($"Player chose: {side} side");
            
            // Tarafa özgü HUD ve kaynak ayarları burada yapılabilir
        }

        public Side GetPlayerSide() => playerSide;
        
        public Side GetOpponentSide() 
        {
            return playerSide == Side.Light ? Side.Dark : Side.Light;
        }
    }
}
