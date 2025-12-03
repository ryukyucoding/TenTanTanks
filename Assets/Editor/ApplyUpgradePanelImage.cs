using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class ApplyUpgradePanelImage : EditorWindow
{
    private Sprite upgradePanelSprite;

    [MenuItem("Tools/Apply Upgrade Panel Image")]
    public static void ShowWindow()
    {
        GetWindow<ApplyUpgradePanelImage>("Apply Upgrade Panel Image");
    }

    void OnGUI()
    {
        GUILayout.Label("Apply Upgrade Panel Image to All Levels", EditorStyles.boldLabel);

        upgradePanelSprite = (Sprite)EditorGUILayout.ObjectField("Upgrade Panel Sprite:", upgradePanelSprite, typeof(Sprite), false);

        if (GUILayout.Button("Find upgrade_points.png"))
        {
            FindUpgradePanelSprite();
        }

        if (GUILayout.Button("Apply to All Level Scenes"))
        {
            if (upgradePanelSprite == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a sprite first!", "OK");
                return;
            }

            ApplyToAllScenes();
        }
    }

    void FindUpgradePanelSprite()
    {
        string[] guids = AssetDatabase.FindAssets("upgrade_points t:Sprite");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            // Load all sprites from the texture
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object obj in sprites)
            {
                if (obj is Sprite sprite)
                {
                    upgradePanelSprite = sprite;
                    Debug.Log("Found upgrade_points sprite at: " + path);
                    return;
                }
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error",
                "upgrade_points.png not found!\n\nPlease make sure:\n" +
                "1. The file is in Assets/UI/Images/\n" +
                "2. Texture Type is set to 'Sprite (2D and UI)'\n" +
                "3. Click Apply in the import settings",
                "OK");
        }
    }

    void ApplyToAllScenes()
    {
        int updatedCount = 0;
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity",
            "Assets/Scenes/Level4.unity",
            "Assets/Scenes/Level5.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            bool sceneModified = false;

            // Find all GameObjects with UpgradeUI component
            UpgradeUI[] upgradeUIs = FindObjectsByType<UpgradeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (UpgradeUI upgradeUI in upgradeUIs)
            {
                // Get the upgradePanel GameObject through reflection
                var upgradeUIType = upgradeUI.GetType();
                var upgradePanelField = upgradeUIType.GetField("upgradePanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (upgradePanelField != null)
                {
                    GameObject upgradePanel = upgradePanelField.GetValue(upgradeUI) as GameObject;

                    if (upgradePanel != null)
                    {
                        Image panelImage = upgradePanel.GetComponent<Image>();

                        if (panelImage != null)
                        {
                            Undo.RecordObject(panelImage, "Update Upgrade Panel Sprite");
                            panelImage.sprite = upgradePanelSprite;
                            panelImage.type = Image.Type.Sliced; // Use sliced mode for better scaling
                            panelImage.color = Color.white; // Reset to full opacity

                            EditorUtility.SetDirty(panelImage);
                            sceneModified = true;
                            updatedCount++;

                            Debug.Log($"Updated UpgradePanel in {scene.name}: {upgradePanel.name}");
                        }
                    }
                }
            }

            if (sceneModified)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Saved changes to {scene.name}");
            }

            EditorSceneManager.CloseScene(scene, true);
        }

        EditorUtility.DisplayDialog("Complete",
            $"Updated {updatedCount} upgrade panels across all level scenes!",
            "OK");
    }
}
