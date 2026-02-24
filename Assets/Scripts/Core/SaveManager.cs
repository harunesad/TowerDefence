using UnityEngine;
using System.IO;

namespace TowerDefence.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string savePath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Gelecekte genel oyun ayarlarını (ses seviyesi, grafik vb.) buradan kaydedebiliriz.
        // Şu an için MetaProgressionManager kendi kaydını yönetiyor ancak 
        // merkezi bir sistem olması ölçeklenebilirlik için iyidir.
        
        public string GetSavePath() => savePath;
    }
}
