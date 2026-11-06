using UnityEngine;

namespace TowerDefence.Core
{
    /// <summary>
    /// İlerleme kaydı MetaProgressionManager içindedir. Bu nesne ayrı bir kayıt dosyası yazmaz.
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
