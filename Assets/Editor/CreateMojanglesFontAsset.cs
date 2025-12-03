using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class CreateMojanglesFontAsset
{
    [MenuItem("Tools/Create Mojangles TMP Font Asset")]
    public static void CreateFontAsset()
    {
        // Find the .otf file
        string[] guids = AssetDatabase.FindAssets("mojangles t:Font");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "mojangles.otf not found! Please make sure it's imported into Unity.", "OK");
            return;
        }

        string fontPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);

        if (sourceFont == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to load mojangles font!", "OK");
            return;
        }

        // Create directory for TMP font assets if it doesn't exist
        string tmpFontPath = "Assets/Resources/Fonts/TMP";
        if (!Directory.Exists(tmpFontPath))
        {
            Directory.CreateDirectory(tmpFontPath);
        }

        // Create the TMP_FontAsset
        string outputPath = tmpFontPath + "/Mojangles SDF.asset";

        // Use TMPro_FontAssetCreatorWindow to create the font asset programmatically
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);

        if (fontAsset != null)
        {
            AssetDatabase.CreateAsset(fontAsset, outputPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success",
                "Mojangles TMP Font Asset created successfully at:\n" + outputPath +
                "\n\nYou can now use the 'Apply Mojangles Font to All Text' tool.",
                "OK");

            // Select the created asset
            Selection.activeObject = fontAsset;
            EditorGUIUtility.PingObject(fontAsset);
        }
        else
        {
            EditorUtility.DisplayDialog("Error",
                "Failed to create font asset. Please create it manually:\n\n" +
                "1. Window > TextMeshPro > Font Asset Creator\n" +
                "2. Select mojangles font\n" +
                "3. Click Generate Font Atlas\n" +
                "4. Save to Assets/Resources/Fonts/TMP/Mojangles SDF",
                "OK");
        }
    }
}
