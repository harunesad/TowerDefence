using System.IO;
using UnityEditor;
using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.EditorTools
{
    public static class TutorialResetMenu
    {
        [MenuItem("Tools/Tower Defence/Tutorialı Baştan Başlat")]
        public static void ResetTutorial()
        {
            string directory = Application.persistentDataPath;
            string[] files =
            {
                "meta_progression.json",
                "meta_progression.json.bak",
                "meta_progression.json.tmp",
                "tutorial_reset.flag"
            };

            if (Application.isPlaying && MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.ResetSave();
                if (TutorialDirector.Instance != null)
                    TutorialDirector.Instance.OnProgressReset();

                SceneLoader.EnsureInstance();
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene(1);

                Debug.Log("Kayıt silindi. Oyun yeni başlamış gibi ana menüye dönüyor.");
                return;
            }

            for (int i = 0; i < files.Length; i++)
            {
                string path = Path.Combine(directory, files[i]);
                if (File.Exists(path)) File.Delete(path);
            }

            Debug.Log("Kayıt dosyası silindi. Sonraki Play yeni oyun olur.");
        }
    }
}
