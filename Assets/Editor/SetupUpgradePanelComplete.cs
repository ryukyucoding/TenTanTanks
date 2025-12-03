using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;

public class SetupUpgradePanelComplete : EditorWindow
{
    [Header("Panel Settings")]
    private Vector2 panelSize = new Vector2(400, 300);
    private Vector2 panelPosition = new Vector2(200, 150);
    private float panelScale = 1.0f;
    private Sprite panelSprite;

    [Header("Text Settings")]
    private Vector2 titleTextPosition = new Vector2(0, 120);
    private Vector2 titleTextSize = new Vector2(350, 60);
    private float titleTextFontSize = 32f;

    private Vector2 button1TextPosition = new Vector2(0, 40);
    private Vector2 button2TextPosition = new Vector2(0, -20);
    private Vector2 button3TextPosition = new Vector2(0, -80);
    private Vector2 buttonTextSize = new Vector2(350, 50);
    private float buttonTextFontSize = 24f;

    [Header("Font")]
    private TMP_FontAsset targetFont;

    private Vector2 scrollPosition;

    [MenuItem("Tools/Setup Upgrade Panel (Complete)")]
    public static void ShowWindow()
    {
        GetWindow<SetupUpgradePanelComplete>("Setup Upgrade Panel");
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Complete Upgrade Panel Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Panel Settings
        EditorGUILayout.LabelField("Panel Settings", EditorStyles.boldLabel);
        panelSprite = (Sprite)EditorGUILayout.ObjectField("Panel Sprite:", panelSprite, typeof(Sprite), false);
        panelSize = EditorGUILayout.Vector2Field("Panel Size:", panelSize);
        panelPosition = EditorGUILayout.Vector2Field("Panel Position:", panelPosition);
        panelScale = EditorGUILayout.Slider("Panel Scale:", panelScale, 0.5f, 2.0f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // Text Settings
        EditorGUILayout.LabelField("Text Settings", EditorStyles.boldLabel);
        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font:", targetFont, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Title Text (Upgrade Points)", EditorStyles.boldLabel);
        titleTextPosition = EditorGUILayout.Vector2Field("Position:", titleTextPosition);
        titleTextSize = EditorGUILayout.Vector2Field("Size:", titleTextSize);
        titleTextFontSize = EditorGUILayout.FloatField("Font Size:", titleTextFontSize);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Button Texts", EditorStyles.boldLabel);
        button1TextPosition = EditorGUILayout.Vector2Field("Button 1 Position:", button1TextPosition);
        button2TextPosition = EditorGUILayout.Vector2Field("Button 2 Position:", button2TextPosition);
        button3TextPosition = EditorGUILayout.Vector2Field("Button 3 Position:", button3TextPosition);
        buttonTextSize = EditorGUILayout.Vector2Field("Button Text Size:", buttonTextSize);
        buttonTextFontSize = EditorGUILayout.FloatField("Button Font Size:", buttonTextFontSize);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // Quick Setup Buttons
        EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find Panel Sprite"))
        {
            FindUpgradePanelSprite();
        }
        if (GUILayout.Button("Find Mojangles Font"))
        {
            FindMojanglesFont();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Preset Buttons
        EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Use Default Layout"))
        {
            UseDefaultLayout();
        }
        if (GUILayout.Button("Use Centered Layout"))
        {
            UseCenteredLayout();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // Apply Buttons
        EditorGUILayout.HelpBox(
            "This will update:\n" +
            "• Panel sprite and size\n" +
            "• All text positions and font sizes\n" +
            "• Font for all texts\n" +
            "in the selected scenes.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply to All Level Scenes", GUILayout.Height(40)))
        {
            if (ValidateSettings())
            {
                ApplyToAllScenes();
            }
        }

        if (GUILayout.Button("Apply to Current Scene Only"))
        {
            if (ValidateSettings())
            {
                ApplyToCurrentScene();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    void UseDefaultLayout()
    {
        panelSize = new Vector2(400, 300);
        panelPosition = new Vector2(200, 150);
        panelScale = 1.0f;

        titleTextPosition = new Vector2(0, 120);
        titleTextSize = new Vector2(350, 60);
        titleTextFontSize = 32f;

        button1TextPosition = new Vector2(0, 40);
        button2TextPosition = new Vector2(0, -20);
        button3TextPosition = new Vector2(0, -80);
        buttonTextSize = new Vector2(350, 50);
        buttonTextFontSize = 24f;

        Debug.Log("Applied default layout preset");
    }

    void UseCenteredLayout()
    {
        panelSize = new Vector2(450, 350);
        panelPosition = new Vector2(225, 175);
        panelScale = 1.0f;

        titleTextPosition = new Vector2(0, 140);
        titleTextSize = new Vector2(400, 60);
        titleTextFontSize = 36f;

        button1TextPosition = new Vector2(0, 50);
        button2TextPosition = new Vector2(0, -20);
        button3TextPosition = new Vector2(0, -90);
        buttonTextSize = new Vector2(380, 55);
        buttonTextFontSize = 26f;

        Debug.Log("Applied centered layout preset");
    }

    void FindUpgradePanelSprite()
    {
        string[] guids = AssetDatabase.FindAssets("upgrade_points t:Sprite");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object obj in sprites)
            {
                if (obj is Sprite sprite)
                {
                    panelSprite = sprite;
                    Debug.Log("Found upgrade_points sprite at: " + path);
                    return;
                }
            }
        }
        Debug.LogWarning("upgrade_points.png sprite not found!");
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
            Debug.LogWarning("Mojangles font not found! Please create it first using 'Tools > Create Mojangles TMP Font Asset'");
        }
    }

    bool ValidateSettings()
    {
        if (panelSprite == null)
        {
            EditorUtility.DisplayDialog("Missing Sprite",
                "Please assign a panel sprite or click 'Find Panel Sprite'", "OK");
            return false;
        }

        if (targetFont == null)
        {
            EditorUtility.DisplayDialog("Missing Font",
                "Please assign a font or click 'Find Mojangles Font'", "OK");
            return false;
        }

        return true;
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

            if (UpdateScene(scene))
            {
                EditorSceneManager.SaveScene(scene);
                updatedCount++;
            }

            EditorSceneManager.CloseScene(scene, true);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Complete",
            $"Successfully updated {updatedCount} scenes!",
            "OK");
    }

    void ApplyToCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (UpdateScene(currentScene))
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorUtility.DisplayDialog("Complete",
                $"Successfully updated {currentScene.name}!",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error",
                "No upgrade panel found in current scene!",
                "OK");
        }
    }

    bool UpdateScene(Scene scene)
    {
        bool updated = false;

        UpgradeUI[] upgradeUIs = FindObjectsByType<UpgradeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (UpgradeUI upgradeUI in upgradeUIs)
        {
            if (upgradeUI.gameObject.scene != scene)
                continue;

            var upgradeUIType = upgradeUI.GetType();

            // Get upgradePanel
            var upgradePanelField = upgradeUIType.GetField("upgradePanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Get upgradePointsText
            var upgradePointsTextField = upgradeUIType.GetField("upgradePointsText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Get buttons
            var moveSpeedButtonField = upgradeUIType.GetField("moveSpeedButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bulletSpeedButtonField = upgradeUIType.GetField("bulletSpeedButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fireRateButtonField = upgradeUIType.GetField("fireRateButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (upgradePanelField != null)
            {
                GameObject upgradePanel = upgradePanelField.GetValue(upgradeUI) as GameObject;

                if (upgradePanel != null)
                {
                    // Update panel
                    UpdatePanel(upgradePanel);

                    // Update title text
                    if (upgradePointsTextField != null)
                    {
                        TextMeshProUGUI titleText = upgradePointsTextField.GetValue(upgradeUI) as TextMeshProUGUI;
                        if (titleText != null)
                        {
                            UpdateText(titleText, titleTextPosition, titleTextSize, titleTextFontSize);
                        }
                    }

                    // Update button texts
                    if (moveSpeedButtonField != null)
                    {
                        var button = moveSpeedButtonField.GetValue(upgradeUI);
                        UpdateButtonText(button, button1TextPosition, 0);
                    }

                    if (bulletSpeedButtonField != null)
                    {
                        var button = bulletSpeedButtonField.GetValue(upgradeUI);
                        UpdateButtonText(button, button2TextPosition, 1);
                    }

                    if (fireRateButtonField != null)
                    {
                        var button = fireRateButtonField.GetValue(upgradeUI);
                        UpdateButtonText(button, button3TextPosition, 2);
                    }

                    updated = true;
                    Debug.Log($"Updated UpgradePanel in {scene.name}");
                }
            }
        }

        return updated;
    }

    void UpdatePanel(GameObject panel)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Undo.RecordObject(rectTransform, "Update Panel Transform");
            rectTransform.sizeDelta = panelSize;
            rectTransform.anchoredPosition = panelPosition;
            rectTransform.localScale = Vector3.one * panelScale;
            EditorUtility.SetDirty(rectTransform);
        }

        Image image = panel.GetComponent<Image>();
        if (image != null)
        {
            Undo.RecordObject(image, "Update Panel Image");
            image.sprite = panelSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            EditorUtility.SetDirty(image);
        }
    }

    void UpdateText(TextMeshProUGUI text, Vector2 position, Vector2 size, float fontSize)
    {
        if (text == null) return;

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Undo.RecordObject(rectTransform, "Update Text Transform");
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            EditorUtility.SetDirty(rectTransform);
        }

        Undo.RecordObject(text, "Update Text Properties");
        text.font = targetFont;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        EditorUtility.SetDirty(text);
    }

    void UpdateButtonText(object button, Vector2 position, int buttonIndex)
    {
        if (button == null) return;

        var buttonType = button.GetType();
        var statNameTextField = buttonType.GetField("statNameText");

        if (statNameTextField != null)
        {
            TextMeshProUGUI text = statNameTextField.GetValue(button) as TextMeshProUGUI;
            if (text != null)
            {
                UpdateText(text, position, buttonTextSize, buttonTextFontSize);
            }
        }
    }
}
