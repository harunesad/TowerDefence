using UnityEngine;

namespace TowerDefence.Core
{
    /// <summary>
    /// Progression save is handled in MetaProgressionManager. This object does not write a separate save file.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

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
        }
    }
}
