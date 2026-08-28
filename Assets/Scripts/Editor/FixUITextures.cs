using UnityEditor;
using UnityEngine;

public class FixUITextures
{
    [MenuItem("Tower Defence/Fix UI Textures")]
    public static void FixTextures()
    {
        FixTexture("Assets/UI/Button_Normal.png", new Vector4(30, 30, 30, 30)); // 30px is safe for 70px+ tall buttons
        FixTexture("Assets/UI/Panel_Wood.png", new Vector4(80, 80, 80, 80)); // Safe for panels
        FixTexture("Assets/UI/Panel_Parchment.jpg", new Vector4(100, 100, 100, 100));
        FixTexture("Assets/UI/MainMenu_BG.jpg", Vector4.zero);
        FixTexture("Assets/UI/FortuneWheel_Base.png", Vector4.zero);
        FixTexture("Assets/UI/Icon_Gold.png", Vector4.zero);
        FixTexture("Assets/UI/Icon_Heart.png", Vector4.zero);
        FixTexture("Assets/UI/Icon_Skull.png", Vector4.zero);
        FixTexture("Assets/UI/Icon_Soul.png", Vector4.zero);
        
        Debug.Log("UI Textures fixed!");
    }

    private static void FixTexture(string path, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogWarning("Could not find texture at: " + path);
        }
    }
}
