using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class AdjustUpgradePanelSize : EditorWindow
{
    private Vector2 panelSize = new Vector2(400, 300);
    private Vector2 panelPosition = new Vector2(80, 80);
    private float panelScale = 1.0f;

    [MenuItem("Tools/Adjust Upgrade Panel Size")]
    public static void ShowWindow()
    {
        GetWindow<AdjustUpgradePanelSize>("Adjust Upgrade Panel Size");
    }

    void OnGUI()
    {
        GUILayout.Label("Adjust Upgrade Panel Size & Position", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.Label("Current Settings (Level1):", EditorStyles.boldLabel);
        GUILayout.Label("Size: 200 x 150", EditorStyles.helpBox);
        GUILayout.Label("Position: (80, 80) from bottom-left", EditorStyles.helpBox);
        GUILayout.Label("Scale: 0.74", EditorStyles.helpBox);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        GUILayout.Label("New Settings:", EditorStyles.boldLabel);

        panelSize = EditorGUILayout.Vector2Field("Panel Size:", panelSize);
        panelPosition = EditorGUILayout.Vector2Field("Position (from bottom-left):", panelPosition);
        panelScale = EditorGUILayout.Slider("Scale:", panelScale, 0.5f, 2.0f);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Tips:\n" +
            "• Size: Width and Height in pixels\n" +
            "• Position: Distance from bottom-left corner\n" +
            "• Scale: Overall scaling factor\n" +
            "• Recommended size for the wooden panel: 400x300 or larger",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply to All Level Scenes", GUILayout.Height(40)))
        {
            ApplyToAllScenes();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply to Current Scene Only"))
        {
            ApplyToCurrentScene();
        }
    }

    void ApplyToAllScenes()
    {
        if (!EditorUtility.DisplayDialog("Confirm",
            $"Apply these settings to all level scenes?\n\n" +
            $"Size: {panelSize.x} x {panelSize.y}\n" +
            $"Position: ({panelPosition.x}, {panelPosition.y})\n" +
            $"Scale: {panelScale}",
            "Apply", "Cancel"))
        {
            return;
        }

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

            if (UpdateScenePanels(scene))
            {
                EditorSceneManager.SaveScene(scene);
                updatedCount++;
            }

            EditorSceneManager.CloseScene(scene, true);
        }

        EditorUtility.DisplayDialog("Complete",
            $"Updated upgrade panels in {updatedCount} scenes!",
            "OK");
    }

    void ApplyToCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (UpdateScenePanels(currentScene))
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorUtility.DisplayDialog("Complete",
                $"Updated upgrade panel in {currentScene.name}!",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error",
                "No upgrade panel found in current scene!",
                "OK");
        }
    }

    bool UpdateScenePanels(Scene scene)
    {
        bool updated = false;

        UpgradeUI[] upgradeUIs = FindObjectsByType<UpgradeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (UpgradeUI upgradeUI in upgradeUIs)
        {
            if (upgradeUI.gameObject.scene != scene)
                continue;

            var upgradeUIType = upgradeUI.GetType();
            var upgradePanelField = upgradeUIType.GetField("upgradePanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (upgradePanelField != null)
            {
                GameObject upgradePanel = upgradePanelField.GetValue(upgradeUI) as GameObject;

                if (upgradePanel != null)
                {
                    RectTransform rectTransform = upgradePanel.GetComponent<RectTransform>();

                    if (rectTransform != null)
                    {
                        Undo.RecordObject(rectTransform, "Adjust Upgrade Panel Size");

                        // Set size
                        rectTransform.sizeDelta = panelSize;

                        // Set position
                        rectTransform.anchoredPosition = panelPosition;

                        // Set scale
                        rectTransform.localScale = Vector3.one * panelScale;

                        EditorUtility.SetDirty(rectTransform);
                        updated = true;

                        Debug.Log($"Updated UpgradePanel in {scene.name}:");
                        Debug.Log($"  Size: {panelSize}");
                        Debug.Log($"  Position: {panelPosition}");
                        Debug.Log($"  Scale: {panelScale}");
                    }
                }
            }
        }

        return updated;
    }
}
