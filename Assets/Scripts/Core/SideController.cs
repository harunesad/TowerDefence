using UnityEngine;
using System;

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

        public event Action<Side> OnSideChanged;

        [SerializeField] private Side playerSide;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetPlayerSide(Side side)
        {
            playerSide = side;
            Debug.Log($"Player chose: {side} side");
            OnSideChanged?.Invoke(side);
        }

        public Side GetPlayerSide() => playerSide;
        
        public Side GetOpponentSide() 
        {
            return playerSide == Side.Light ? Side.Dark : Side.Light;
        }
    }
}
