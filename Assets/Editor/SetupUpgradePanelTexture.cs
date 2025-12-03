using UnityEngine;
using UnityEditor;

public class SetupUpgradePanelTexture
{
    [MenuItem("Tools/Setup upgrade_points Texture Import Settings")]
    public static void SetupTextureImportSettings()
    {
        string[] guids = AssetDatabase.FindAssets("upgrade_points t:Texture2D");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error",
                "upgrade_points.png not found in the project!\n\n" +
                "Please make sure the file is in Assets/UI/Images/",
                "OK");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        Debug.Log("Found texture at: " + path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            // Set texture to Sprite (2D and UI)
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;

            // Apply the changes
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            EditorUtility.DisplayDialog("Success",
                "Texture import settings configured successfully!\n\n" +
                "Path: " + path + "\n\n" +
                "Settings:\n" +
                "- Texture Type: Sprite (2D and UI)\n" +
                "- Sprite Mode: Single\n" +
                "- Pixels Per Unit: 100\n\n" +
                "You can now use the 'Apply Upgrade Panel Image' tool.",
                "OK");

            // Select the asset
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        else
        {
            EditorUtility.DisplayDialog("Error",
                "Failed to get TextureImporter for the file!",
                "OK");
        }
    }
}
