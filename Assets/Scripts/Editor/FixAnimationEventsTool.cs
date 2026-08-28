using UnityEngine;
using UnityEditor;
using TowerDefence.Combat;

namespace TowerDefence.Editor
{
    public class FixAnimationEventsTool
    {
        [MenuItem("Tower Defence/🛠️ Utilities/Fix Animation Event Handlers")]
        public static void FixMissingHandlers()
        {
            string[] searchFolders = { "Assets/Prefabs/Gameplay/Units", "Assets/Prefabs/Gameplay/Heroes" };
            string[] guids = AssetDatabase.FindAssets("t:GameObject", searchFolders);
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                // Prefab'ı içeriğiyle beraber doğru şekilde belleğe al
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);

                if (contentsRoot != null && contentsRoot.GetComponent<Unit>() != null)
                {
                    bool isModified = false;
                    Animator[] animators = contentsRoot.GetComponentsInChildren<Animator>(true);
                    
                    foreach (var anim in animators)
                    {
                        if (anim.GetComponent<AnimationEventHandler>() == null)
                        {
                            anim.gameObject.AddComponent<AnimationEventHandler>();
                            isModified = true;
                            count++;
                        }
                    }

                    // Eğer değişiklik yapıldıysa diske kaydet
                    if (isModified)
                    {
                        PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
                        Debug.Log($"Fixed and Saved: {contentsRoot.name}");
                    }
                }
                
                // Prefab'ı bellekten çıkar
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Fix complete! Added AnimationEventHandler to {count} animator(s).");
        }
    }
}
