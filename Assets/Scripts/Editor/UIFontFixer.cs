using UnityEngine;
using UnityEditor;
using TMPro;

namespace TowerDefence.Editor
{
    public static class UIFontFixer
    {
        public static void FixAllFonts()
        {
            Debug.Log("[UIFontFixer] Loading Cinzel fonts...");
            
            TMP_FontAsset regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Cinzel-Regular SDF.asset");
            TMP_FontAsset boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Cinzel-Bold SDF.asset");
            TMP_FontAsset blackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Cinzel-Black SDF.asset");

            if (regularFont == null || boldFont == null)
            {
                Debug.LogError("[UIFontFixer] Could not find Cinzel fonts in Assets/Fonts! Please ensure they are named exactly 'Cinzel-Regular SDF.asset' and 'Cinzel-Bold SDF.asset'.");
                return;
            }

            string[] searchPaths = new string[] { "Assets/Prefabs", "Assets/Scenes" };
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", searchPaths);

            int modifiedPrefabs = 0;
            int modifiedTexts = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool isModified = false;
                
                // Get all TextMeshProUGUI components in the prefab (including inactive)
                TextMeshProUGUI[] texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                
                foreach (TextMeshProUGUI tmp in texts)
                {
                    TMP_FontAsset targetFont = DetermineFont(tmp.gameObject.name, tmp.text, regularFont, boldFont, blackFont);
                    
                    if (tmp.font != targetFont)
                    {
                        tmp.font = targetFont;
                        isModified = true;
                        modifiedTexts++;
                    }
                }

                // Get all TextMeshPro (3D Text) components in the prefab
                TextMeshPro[] text3Ds = prefab.GetComponentsInChildren<TextMeshPro>(true);
                foreach (TextMeshPro tmp3D in text3Ds)
                {
                    TMP_FontAsset targetFont = DetermineFont(tmp3D.gameObject.name, tmp3D.text, regularFont, boldFont, blackFont);
                    
                    if (tmp3D.font != targetFont)
                    {
                        tmp3D.font = targetFont;
                        isModified = true;
                        modifiedTexts++;
                    }
                }

                if (isModified)
                {
                    EditorUtility.SetDirty(prefab);
                    modifiedPrefabs++;
                }
            }

            // Ayrıca mevcut açık sahnede (Scene) olanları da güncelle
            TextMeshProUGUI[] sceneTexts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tmp in sceneTexts)
            {
                TMP_FontAsset targetFont = DetermineFont(tmp.gameObject.name, tmp.text, regularFont, boldFont, blackFont);
                if (tmp.font != targetFont)
                {
                    Undo.RecordObject(tmp, "Fix Font");
                    tmp.font = targetFont;
                    EditorUtility.SetDirty(tmp);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(tmp);
                    modifiedTexts++;
                }
            }
            
            TextMeshPro[] scene3DTexts = Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tmp3D in scene3DTexts)
            {
                TMP_FontAsset targetFont = DetermineFont(tmp3D.gameObject.name, tmp3D.text, regularFont, boldFont, blackFont);
                if (tmp3D.font != targetFont)
                {
                    Undo.RecordObject(tmp3D, "Fix Font");
                    tmp3D.font = targetFont;
                    EditorUtility.SetDirty(tmp3D);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(tmp3D);
                    modifiedTexts++;
                }
            }

            if (sceneTexts.Length > 0 || scene3DTexts.Length > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[UIFontFixer] Font repair complete! Applied to {modifiedTexts} text elements across {modifiedPrefabs} prefabs and the current scene.");
        }

        private static TMP_FontAsset DetermineFont(string objectName, string textContent, TMP_FontAsset regular, TMP_FontAsset bold, TMP_FontAsset black)
        {
            string lowerName = objectName.ToLowerInvariant();
            string lowerText = textContent?.ToLowerInvariant() ?? "";

            // Siyah/Kalın Başlıklar
            if (lowerName.Contains("title") || lowerName.Contains("header") || lowerName.Contains("banner") || lowerName.Contains("wave") || lowerName.Contains("victory") || lowerName.Contains("defeat"))
            {
                return black != null ? black : bold;
            }

            // Bold / Vurgulu Metinler (Sayılar, Fiyatlar, İsimler, vb.)
            if (lowerName.Contains("name") || lowerName.Contains("cost") || lowerName.Contains("price") || 
                lowerName.Contains("damage") || lowerName.Contains("health") || lowerName.Contains("level") ||
                lowerName.Contains("amount") || lowerName.Contains("value") || lowerName.Contains("stat") ||
                lowerName.Contains("btn") || lowerName.Contains("button"))
            {
                return bold;
            }

            // Text içeriği tamamen sayıysa büyük ihtimalle cost veya stat'tır, bold yapalım.
            if (!string.IsNullOrEmpty(lowerText))
            {
                if (int.TryParse(lowerText, out _))
                {
                    return bold;
                }
            }

            // Geri kalan her şey (Açıklamalar, normal metinler) için Regular
            return regular;
        }
    }
}
