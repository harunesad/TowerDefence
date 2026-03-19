using UnityEngine;
using UnityEditor;
using System.IO;

namespace TowerDefence.EditorTools
{
    public class IconGenerator : EditorWindow
    {
        private GameObject targetObject;
        private Vector2 iconSize = new Vector2(2048, 2048);
        private Color backgroundColor = new Color(0, 0, 0, 0); // Transparent by default
        
        // Camera Settings
        private Vector3 cameraPositionOffset = new Vector3(0, 0, -10f);
        private Vector3 cameraRotation = new Vector3(0, 0, 0);
        private float cameraOrthographicSize = 3f;
        private bool useOrthographic = false;

        [MenuItem("Tower Defence/🛠️ Utilities/🖼️ Icon Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<IconGenerator>("Icon Generator");
            window.minSize = new Vector2(400, 450);
            window.Show();
        }

        private void OnEnable()
        {
            if (Selection.activeGameObject != null)
            {
                targetObject = Selection.activeGameObject;
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                targetObject = Selection.activeGameObject;
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("🎨 3D to 2D UI Icon Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Sahnedeki bir objeyi seçerek şeffaf arka planlı yüksek kaliteli PNG ikonunu oluşturabilirsiniz. Oluşturulan ikon otomatik olarak Sprite formatına çevrilir.", MessageType.Info);
            EditorGUILayout.Space();

            targetObject = (GameObject)EditorGUILayout.ObjectField("Hedef Obje (Target)", targetObject, typeof(GameObject), true);
            
            EditorGUILayout.Space();
            GUILayout.Label("Görsel Ayarları", EditorStyles.boldLabel);
            iconSize = EditorGUILayout.Vector2Field("İkon Çözünürlüğü (px)", iconSize);
            backgroundColor = EditorGUILayout.ColorField("Arka Plan Rengi", backgroundColor);

            EditorGUILayout.Space();
            GUILayout.Label("Kamera Ayarları", EditorStyles.boldLabel);
            useOrthographic = EditorGUILayout.Toggle("Ortografik (İzometrik) Görünüm", useOrthographic);
            
            if (useOrthographic)
            {
                cameraOrthographicSize = EditorGUILayout.FloatField("Ortografik Boyut", cameraOrthographicSize);
            }
            
            cameraPositionOffset = EditorGUILayout.Vector3Field("Kamera Pozisyon Offseti", cameraPositionOffset);
            cameraRotation = EditorGUILayout.Vector3Field("Kamera Dönüş Açısı (Euler)", cameraRotation);

            EditorGUILayout.Space();
            if (GUILayout.Button("📸 İkonu Oluştur ve Kaydet", GUILayout.Height(40)))
            {
                if (targetObject == null)
                {
                    EditorUtility.DisplayDialog("Hata", "Lütfen sahneden veya hiyerarşiden bir hedef obje seçin!", "Tamam");
                    return;
                }
                
                CaptureIcon();
            }
        }

        private void CaptureIcon()
        {
            // 1. Kamera Kurulumu
            GameObject camGO = new GameObject("IconRenderCam_Temp");
            Camera cam = camGO.AddComponent<Camera>();
            
            cam.transform.position = targetObject.transform.position + cameraPositionOffset;
            cam.transform.rotation = Quaternion.Euler(cameraRotation);
            
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            cam.orthographic = useOrthographic;
            cam.orthographicSize = cameraOrthographicSize;
            
            // 2. Render Texture Ayarları
            int resWidth = (int)iconSize.x;
            int resHeight = (int)iconSize.y;
            
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            rt.antiAliasing = 8; // Yüksek kalite için
            cam.targetTexture = rt;
            
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
            
            // 3. Render İşlemi
            cam.Render();
            
            // 4. Pikselleri Oku
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            screenShot.Apply();
            
            // 5. Temizlik
            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);
            DestroyImmediate(camGO);
            
            // 6. PNG Olarak Kaydet
            byte[] bytes = screenShot.EncodeToPNG();
            
            string folderPath = Application.dataPath + "/Data/Icons";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            string cleanName = targetObject.name.Replace("(Clone)", "").Trim();
            string relativePath = $"Assets/Data/Icons/{cleanName}_Icon.png";
            string absolutePath = $"{Application.dataPath}/Data/Icons/{cleanName}_Icon.png";
            
            File.WriteAllBytes(absolutePath, bytes);
            
            // 7. Unity Asset Veritabanını Yenile ve Sprite Formatına Çevir
            AssetDatabase.Refresh();
            
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(relativePath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false; // UI ikonlarında mipmap gereksizdir, netliği bozar
                importer.SaveAndReimport();
            }
            
            EditorUtility.DisplayDialog("Başarılı!", $"İkon başarıyla oluşturuldu ve {resWidth}x{resHeight} UI Sprite formatına çevrildi!\n\nDosya Yolu: {relativePath}", "Harika!");
        }
    }
}
