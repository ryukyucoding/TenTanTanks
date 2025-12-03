using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public class ApplyFontToAll : EditorWindow
{
    private TMP_FontAsset targetFont;

    [MenuItem("Tools/Apply Mojangles Font to All Text")]
    public static void ShowWindow()
    {
        GetWindow<ApplyFontToAll>("Apply Font to All");
    }

    void OnGUI()
    {
        GUILayout.Label("Apply Font to All TextMeshPro Components", EditorStyles.boldLabel);

        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Target Font:", targetFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Apply to All Prefabs and Scenes"))
        {
            if (targetFont == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a font first!", "OK");
                return;
            }

            ApplyFontToAllText();
        }

        if (GUILayout.Button("Find Mojangles Font"))
        {
            FindMojanglesFont();
        }
    }

    void FindMojanglesFont()
    {
        string[] guids = AssetDatabase.FindAssets("mojangles t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            Debug.Log("Found Mojangles font at: " + path);
        }
        else
        {
            Debug.LogWarning("Mojangles font asset not found. Please create a TextMeshPro font asset from the .otf file first.");
        }
    }

    void ApplyFontToAllText()
    {
        int count = 0;

        // Apply to all prefabs
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                bool modified = false;
                TextMeshProUGUI[] tmpComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

                foreach (TextMeshProUGUI tmp in tmpComponents)
                {
                    tmp.font = targetFont;
                    modified = true;
                    count++;
                }

                if (modified)
                {
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        // Apply to all scenes
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (string guid in sceneGUIDs)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

            GameObject[] rootObjects = scene.GetRootGameObjects();
            bool sceneModified = false;

            foreach (GameObject root in rootObjects)
            {
                TextMeshProUGUI[] tmpComponents = root.GetComponentsInChildren<TextMeshProUGUI>(true);

                foreach (TextMeshProUGUI tmp in tmpComponents)
                {
                    tmp.font = targetFont;
                    EditorUtility.SetDirty(tmp);
                    sceneModified = true;
                    count++;
                }
            }

            if (sceneModified)
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            }

            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Complete", $"Applied font to {count} TextMeshPro components!", "OK");
        Debug.Log($"Applied font to {count} TextMeshPro components.");
    }
}
