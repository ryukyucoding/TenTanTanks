using UnityEngine;
using UnityEngine.InputSystem;
using WheelUpgradeSystem;
using TMPro;

/// <summary>
/// ENGLISH UPGRADE SYSTEM WITH CUSTOM FONT
/// Simple English dialog, no emojis, clear descriptions
/// Y = YES (only way to close), detailed upgrade descriptions, custom font styling
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;

    [Header("Custom Font Settings")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private int titleFontSize = 42;
    [SerializeField] private int descriptionFontSize = 24;
    [SerializeField] private int instructionFontSize = 32;

    [Header("Enhanced UI")]
    [SerializeField] private GameObject confirmationDialog;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private enum UpgradeState
    {
        Hidden,
        SelectingUpgrade,
        ConfirmingUpgrade
    }

    private UpgradeState currentState = UpgradeState.Hidden;
    private WheelUpgradeOption selectedUpgrade;
    private EnhancedTransitionMover enhancedTransitionMover;
    private Transform wheelContainer;

    private Canvas dialogCanvas;
    private GameObject dialogPanel;
    private TextMeshProUGUI dialogTitle;
    private TextMeshProUGUI dialogDescription;
    private TextMeshProUGUI dialogInstruction;
    private UnityEngine.UI.Image dialogBackground;

    void Start()
    {
        DebugLog("Enhanced English Upgrade System Started");

        LoadCustomFont();

        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();
        if (tankUpgradeSystem == null)
            tankUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();
        if (enhancedTransitionMover == null)
            enhancedTransitionMover = FindFirstObjectByType<EnhancedTransitionMover>();

        if (upgradeCanvas == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            foreach (var canvas in canvases)
            {
                if (canvas.name.ToLower().Contains("upgrade") ||
                    canvas.name.ToLower().Contains("wheel"))
                {
                    upgradeCanvas = canvas;
                    break;
                }
            }
        }

        CreateEnhancedDialogWithCustomFont();
        DisableWheelCloser();

        DebugLog("Components found:");
        DebugLog("  - UpgradeWheelUI: " + (upgradeWheelUI != null ? "Found" : "Missing"));
        DebugLog("  - Custom Font: " + (customFont != null ? "Loaded (" + customFont.name + ")" : "Missing"));
        DebugLog("  - TankUpgradeSystem: " + (tankUpgradeSystem != null ? "Found" : "Missing"));
    }

    private void LoadCustomFont()
    {
        if (customFont == null)
        {
            customFont = Resources.Load<TMP_FontAsset>("Fonts/MinecraftTen-VGORe SDF");

            if (customFont == null)
            {
                string[] fontPaths = {
                    "MinecraftTen-VGORe SDF",
                    "Fonts/MinecraftTen-VGORe SDF",
                    "Assets/Fonts/MinecraftTen-VGORe SDF"
                };

                foreach (string path in fontPaths)
                {
                    customFont = Resources.Load<TMP_FontAsset>(path);
                    if (customFont != null)
                    {
                        DebugLog("Found custom font at: " + path);
                        break;
                    }
                }
            }

            if (customFont == null)
            {
                DebugLog("Custom font not found, using default font");
                var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                if (allFonts.Length > 0)
                {
                    customFont = allFonts[0];
                    DebugLog("Using fallback font: " + customFont.name);
                }
            }
            else
            {
                DebugLog("Loaded custom font: " + customFont.name);
            }
        }
    }

    void Update()
    {
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (currentState == UpgradeState.ConfirmingUpgrade)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                DebugLog("Y pressed - confirming upgrade");
                ConfirmUpgrade();
            }
            else if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                DebugLog("N pressed - canceling upgrade");
                CancelUpgrade();
            }
        }
    }

    private void DisableWheelCloser()
    {
        var wheelCloser = FindFirstObjectByType<UpgradeWheelCloser>();
        if (wheelCloser != null)
        {
            wheelCloser.enabled = false;
            DebugLog("Disabled UpgradeWheelCloser");
        }
    }

    public void ShowUpgradePanel(string transitionType)
    {
        DebugLog("ShowUpgradePanel called with: " + transitionType);
        ShowUpgradeWheel();
    }

    private void ShowUpgradeWheel()
    {
        DebugLog("Showing Upgrade Wheel");

        currentState = UpgradeState.SelectingUpgrade;

        ActivateWheelComponents();
        FixAllScaleIssues();

        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.SetTransitionMode(1, "");
            upgradeWheelUI.ShowWheelForTransition();
        }

        DebugLog("Wheel visible - waiting for selection");
    }

    private void ActivateWheelComponents()
    {
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.gameObject.SetActive(true);
            DebugLog("Activated UpgradeWheelUI");
        }

        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(true);
            DebugLog("Activated UpgradeCanvas");
        }

        if (upgradeWheelUI != null)
        {
            string[] containerNames = { "UpgradeWheelContainer", "WheelContainer", "Wheel Container" };

            foreach (string name in containerNames)
            {
                wheelContainer = upgradeWheelUI.transform.Find(name);
                if (wheelContainer != null)
                {
                    wheelContainer.gameObject.SetActive(true);
                    DebugLog("Activated wheel container: " + wheelContainer.name);
                    break;
                }
            }
        }
    }

    private void FixAllScaleIssues()
    {
        if (upgradeWheelUI == null) return;

        DebugLog("Fixing Scale Issues");
        upgradeWheelUI.transform.localScale = Vector3.one;

        if (wheelContainer != null)
            wheelContainer.localScale = Vector3.one;

        FixChildScales(upgradeWheelUI.transform);
        DebugLog("Scale issues fixed");
    }

    private void FixChildScales(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.localScale != Vector3.one)
            {
                child.localScale = Vector3.one;
                DebugLog("Fixed scale: " + child.name);
            }

            FixChildScales(child);
        }
    }

    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        DebugLog("Upgrade Selected: " + upgrade.upgradeName);

        selectedUpgrade = upgrade;
        currentState = UpgradeState.ConfirmingUpgrade;

        ShowDetailedConfirmationWithCustomFont(upgrade);
        DebugLog("Waiting for Y/N input");
    }

    private void ShowDetailedConfirmationWithCustomFont(WheelUpgradeOption upgrade)
    {
        if (dialogPanel == null) return;

        string detailedDescription = GetDetailedUpgradeDescription(upgrade.upgradeName);

        if (dialogTitle != null)
        {
            dialogTitle.text = "Tank Upgrade: " + upgrade.upgradeName;
            ApplyCustomFontStyling(dialogTitle, titleFontSize, Color.yellow);
        }

        if (dialogDescription != null)
        {
            dialogDescription.text = detailedDescription;
            ApplyCustomFontStyling(dialogDescription, descriptionFontSize, Color.white);
        }

        if (dialogInstruction != null)
        {
            dialogInstruction.text = "Press Y to Confirm | Press N to Cancel";
            ApplyCustomFontStyling(dialogInstruction, instructionFontSize, Color.cyan);
        }

        dialogPanel.SetActive(true);
        DebugLog("Showing dialog for: " + upgrade.upgradeName);
    }

    private void ApplyCustomFontStyling(TextMeshProUGUI textComponent, int fontSize, Color color)
    {
        if (textComponent == null) return;

        if (customFont != null)
        {
            textComponent.font = customFont;
        }

        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.richText = true;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.outlineWidth = 0.2f;
        textComponent.outlineColor = Color.black;
    }

    private string GetDetailedUpgradeDescription(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "heavy":
                return "<size=28><color=orange>Heavy Tank Upgrade</color></size>\n\n" +
                       "CANNON CHANGES:\n" +
                       "- Cannon becomes bigger and thicker\n" +
                       "- Bullets are much more powerful\n" +
                       "- Slower firing speed\n" +
                       "- Perfect for armored enemies\n\n" +
                       "This upgrade focuses on raw firepower.";

            case "rapid":
                return "<size=28><color=lightblue>Rapid Fire Upgrade</color></size>\n\n" +
                       "CANNON CHANGES:\n" +
                       "- Cannon becomes smaller and thinner\n" +
                       "- Bullets are smaller but fire rapidly\n" +
                       "- Much faster firing speed\n" +
                       "- Great against many enemies\n\n" +
                       "This upgrade focuses on attack speed.";

            case "balanced":
                return "<size=28><color=purple>Balanced Upgrade</color></size>\n\n" +
                       "CANNON CHANGES:\n" +
                       "- Cannon size stays the same\n" +
                       "- Bullet power remains unchanged\n" +
                       "- You get a second cannon\n" +
                       "- Both cannons fire together\n\n" +
                       "This upgrade focuses on multiple attacks.";

            default:
                return "<size=28>" + upgradeName + " Upgrade</size>\n\n" +
                       "This upgrade will improve your tank.\n" +
                       "Check the upgrade details for more info.";
        }
    }

    private void ConfirmUpgrade()
    {
        DebugLog("UPGRADE CONFIRMED: " + selectedUpgrade.upgradeName);

        HideDialog();

        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog("Applied upgrade: " + selectedUpgrade.upgradeName);
        }

        HideWheelCompletely();
        ResumeGameplay();

        DebugLog("UPGRADE PROCESS COMPLETE");
    }

    private void CancelUpgrade()
    {
        DebugLog("Upgrade Canceled");

        selectedUpgrade = null;
        currentState = UpgradeState.SelectingUpgrade;

        HideDialog();
        DebugLog("Returned to upgrade selection");
    }

    private void HideWheelCompletely()
    {
        DebugLog("HIDING WHEEL COMPLETELY");

        currentState = UpgradeState.Hidden;

        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.HideWheel();
            upgradeWheelUI.gameObject.SetActive(false);
        }

        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(false);
        }

        if (wheelContainer != null)
        {
            wheelContainer.gameObject.SetActive(false);
        }

        HideDialog();
        DebugLog("HIDE COMPLETE");
    }

    private void ResumeGameplay()
    {
        if (enhancedTransitionMover != null)
        {
            DebugLog("Notifying EnhancedTransitionMover to resume");
            enhancedTransitionMover.ResumeMovement();
        }
    }

    #region Enhanced Dialog with Custom Font

    private void CreateEnhancedDialogWithCustomFont()
    {
        DebugLog("Creating enhanced dialog with custom font...");

        GameObject dialogCanvasGO = new GameObject("CustomFontConfirmationCanvas");
        dialogCanvas = dialogCanvasGO.AddComponent<Canvas>();
        dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dialogCanvas.sortingOrder = 2000;

        var canvasScaler = dialogCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        dialogCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        dialogPanel = new GameObject("CustomFontConfirmationPanel");
        dialogPanel.transform.SetParent(dialogCanvas.transform, false);

        var panelImage = dialogPanel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.05f, 0.97f);

        var panelRect = dialogPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(900, 600);
        panelRect.anchoredPosition = Vector2.zero;

        var outline = dialogPanel.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(4, 4);

        var shadow = dialogPanel.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(8, -8);

        CreateCustomFontText("CustomTitle", dialogPanel, new Vector2(0, 200), titleFontSize, Color.yellow, out dialogTitle);
        CreateCustomFontText("CustomDescription", dialogPanel, Vector2.zero, descriptionFontSize, Color.white, out dialogDescription);
        CreateCustomFontText("CustomInstruction", dialogPanel, new Vector2(0, -220), instructionFontSize, Color.cyan, out dialogInstruction);

        HideDialog();
        DebugLog("Created enhanced dialog with custom font");
    }

    private void CreateCustomFontText(string name, GameObject parent, Vector2 position, int fontSize, Color color, out TextMeshProUGUI textComponent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);

        textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = "";

        if (customFont != null)
        {
            textComponent.font = customFont;
        }

        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.richText = true;
        textComponent.outlineWidth = 0.3f;
        textComponent.outlineColor = Color.black;

        var textRect = textComponent.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(850, fontSize == descriptionFontSize ? 350 : 80);
        textRect.anchoredPosition = position;
    }

    private void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }

    #endregion

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log("[EnglishUpgrade] " + message);
        }
    }

    public bool IsUpgradeInProgress()
    {
        return currentState != UpgradeState.Hidden;
    }

    [ContextMenu("Test Custom Font")]
    public void TestCustomFont()
    {
        DebugLog("Custom Font Test: " + (customFont != null ? customFont.name : "NOT LOADED"));
    }

    [ContextMenu("Test Heavy with Custom Font")]
    public void TestHeavyWithCustomFont()
    {
        var testUpgrade = new WheelUpgradeOption("Heavy", "Test Heavy", 1);
        OnUpgradeSelected(testUpgrade);
    }
}