using UnityEngine;
using UnityEngine.InputSystem;
using WheelUpgradeSystem;
using TMPro;

/// <summary>
/// COMPREHENSIVE FIX FOR DIVIDER LINES AND FONT ISSUES
/// Fixes secondary divider scaling and ensures proper font application
/// ENHANCED WITH TANK TRANSFORMATION PERSISTENCE
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
    [SerializeField] private bool enableScaleDebug = true; // Extra debug for scale issues

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

    void Start()
    {
        DebugLog("Enhanced English Upgrade System with Comprehensive Fixes Started");

        // Load custom font with better detection
        LoadCustomFontWithFallback();

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

        CreateEnhancedDialogWithBetterFont();
        DisableWheelCloser();

        DebugLog("Components Status:");
        DebugLog("  - UpgradeWheelUI: " + (upgradeWheelUI != null ? "Found" : "Missing"));
        DebugLog("  - Custom Font: " + (customFont != null ? "Loaded (" + customFont.name + ")" : "Missing"));
        DebugLog("  - TankUpgradeSystem: " + (tankUpgradeSystem != null ? "Found" : "Missing"));
    }

    /// <summary>
    /// Enhanced font loading with multiple fallback strategies
    /// </summary>
    private void LoadCustomFontWithFallback()
    {
        if (customFont != null)
        {
            DebugLog("Custom font already assigned: " + customFont.name);
            return;
        }

        DebugLog("Loading custom font...");

        // Strategy 1: Direct Resources load
        customFont = Resources.Load<TMP_FontAsset>("Fonts/MinecraftTen-VGORe SDF");

        if (customFont != null)
        {
            DebugLog("FONT LOADED SUCCESSFULLY: " + customFont.name);
        }
        else
        {
            DebugLog("WARNING: No TMP font found - dialog will use default");
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
        FixAllScaleIssuesComprehensive(); // Enhanced scale fixing

        if (upgradeWheelUI != null)
        {
            // ✅ 修正：檢查當前是否已經設定為 transition mode，如果是則不要覆蓋
            bool isAlreadyInTransitionMode = upgradeWheelUI.IsInTransitionMode(); // 需要暴露這個屬性
            int currentAllowedTier = upgradeWheelUI.GetTransitionAllowedTier(); // 需要暴露這個屬性
            string currentParentUpgrade = upgradeWheelUI.GetTransitionParentUpgrade(); // 需要暴露這個屬性

            if (isAlreadyInTransitionMode)
            {
                DebugLog($"✅ Already in transition mode: Tier {currentAllowedTier}, Parent: '{currentParentUpgrade}' - preserving settings");
                // 不重新設定 SetTransitionMode，直接顯示
            }
            else
            {
                DebugLog("Setting to default Tier 1 transition mode");
                upgradeWheelUI.SetTransitionMode(1, "");
            }

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

    /// <summary>
    /// COMPREHENSIVE scale fixing that handles all divider types
    /// </summary>
    private void FixAllScaleIssuesComprehensive()
    {
        if (upgradeWheelUI == null)
        {
            DebugLog("Cannot fix scales - upgradeWheelUI is null");
            return;
        }

        DebugLog("=== COMPREHENSIVE SCALE FIXING ===");

        // Fix main UI scale
        upgradeWheelUI.transform.localScale = Vector3.one;
        if (enableScaleDebug) DebugLog("Reset UpgradeWheelUI scale");

        // Fix wheel container scale
        if (wheelContainer != null)
        {
            wheelContainer.localScale = Vector3.one;
            if (enableScaleDebug) DebugLog("Reset wheel container scale");
        }

        // Fix ALL children with comprehensive pattern matching
        FixAllChildScalesComprehensive(upgradeWheelUI.transform, 0);

        DebugLog("=== COMPREHENSIVE SCALE FIXING COMPLETE ===");
    }

    /// <summary>
    /// Enhanced recursive scale fixing with specific divider detection
    /// </summary>
    private void FixAllChildScalesComprehensive(Transform parent, int depth)
    {
        if (depth > 5) return; // Prevent infinite recursion

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            string childName = child.name.ToLower();

            // IMPROVED: More specific detection to EXCLUDE divider lines and UI elements
            bool isDividerLine = childName.Contains("division") ||
                               childName.Contains("divider") ||
                               childName.Contains("line") ||
                               childName.Contains("primarydivider") ||
                               childName.Contains("secondarydivider");

            // SKIP divider lines completely to preserve their intended size
            if (isDividerLine)
            {
                if (enableScaleDebug)
                {
                    DebugLog($"SKIPPING divider line: {child.name} - preserving original scale: {child.localScale}");
                }

                // Still recursively check children of dividers, but don't change the divider itself
                FixAllChildScalesComprehensive(child, depth + 1);
                continue; // Skip this child's scale modification
            }

            // Only fix non-divider elements that have incorrect scale
            if (child.localScale != Vector3.one)
            {
                Vector3 oldScale = child.localScale;
                child.localScale = Vector3.one;

                if (enableScaleDebug)
                {
                    DebugLog($"Fixed scale for: {child.name} (was: {oldScale})");
                }

                // Special handling for UI Images (potential dividers we missed)
                var image = child.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    // Reset any transform modifications that might cause line extension
                    var rectTransform = child.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // Don't modify size, just ensure scale is correct
                        rectTransform.localScale = Vector3.one;

                        if (enableScaleDebug)
                        {
                            DebugLog("Reset RectTransform scale for image: " + child.name);
                        }
                    }
                }
            }

            // Recursively fix children
            FixAllChildScalesComprehensive(child, depth + 1);
        }
    }

    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        DebugLog("Upgrade Selected: " + upgrade.upgradeName);

        selectedUpgrade = upgrade;
        currentState = UpgradeState.ConfirmingUpgrade;

        ShowDetailedConfirmationWithBetterFont(upgrade);
        DebugLog("Waiting for Y/N input");
    }

    /// <summary>
    /// Enhanced dialog display with better font handling
    /// </summary>
    private void ShowDetailedConfirmationWithBetterFont(WheelUpgradeOption upgrade)
    {
        if (dialogPanel == null)
        {
            DebugLog("Dialog panel is null - cannot show confirmation");
            return;
        }

        string detailedDescription = GetDetailedUpgradeDescription(upgrade.upgradeName);

        // Update all text with proper font application
        if (dialogTitle != null)
        {
            dialogTitle.text = "Tank Upgrade: " + upgrade.upgradeName;
            ApplyCustomFontWithVerification(dialogTitle, titleFontSize, Color.yellow, "Title");
        }

        if (dialogDescription != null)
        {
            dialogDescription.text = detailedDescription;
            ApplyCustomFontWithVerification(dialogDescription, descriptionFontSize, Color.white, "Description");
        }

        if (dialogInstruction != null)
        {
            dialogInstruction.text = "Press Y to Confirm | Press N to Cancel";
            ApplyCustomFontWithVerification(dialogInstruction, instructionFontSize, Color.cyan, "Instruction");
        }

        dialogPanel.SetActive(true);
        DebugLog("Dialog shown for: " + upgrade.upgradeName);

        // Verify font application after a frame
        Invoke("VerifyFontApplication", 0.1f);
    }

    /// <summary>
    /// Apply font with verification and logging
    /// </summary>
    private void ApplyCustomFontWithVerification(TextMeshProUGUI textComponent, int fontSize, Color color, string componentName)
    {
        if (textComponent == null)
        {
            DebugLog("Cannot apply font to " + componentName + " - component is null");
            return;
        }

        // Apply custom font
        if (customFont != null)
        {
            textComponent.font = customFont;
            DebugLog("Applied custom font to " + componentName + ": " + customFont.name);
        }
        else
        {
            DebugLog("WARNING: No custom font available for " + componentName);
        }

        // Apply other properties
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.richText = true;
        textComponent.alignment = TextAlignmentOptions.Center;

        // Add outline for better readability
        textComponent.outlineWidth = 0.2f;
        textComponent.outlineColor = Color.black;

        // Force refresh the text component
        textComponent.ForceMeshUpdate();

        DebugLog("Font application complete for " + componentName);
    }

    /// <summary>
    /// Verify that fonts were applied correctly
    /// </summary>
    private void VerifyFontApplication()
    {
        DebugLog("=== FONT APPLICATION VERIFICATION ===");

        if (dialogTitle != null)
        {
            DebugLog("Title font: " + (dialogTitle.font != null ? dialogTitle.font.name : "NULL"));
        }

        if (dialogDescription != null)
        {
            DebugLog("Description font: " + (dialogDescription.font != null ? dialogDescription.font.name : "NULL"));
        }

        if (dialogInstruction != null)
        {
            DebugLog("Instruction font: " + (dialogInstruction.font != null ? dialogInstruction.font.name : "NULL"));
        }

        DebugLog("=== VERIFICATION COMPLETE ===");
    }

    private string GetDetailedUpgradeDescription(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "heavy":
                return "<size=28><color=orange>Heavy Tank Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- Cannon becomes bigger\n</size>" +
                       "<size=28>- Bullets are bigger, easier to hit target\n</size>" +
                       "<size=28>- Slower firing speed</size>";

            case "rapid":
                return "<size=28><color=lightblue>Rapid Fire Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- Cannon becomes smaller\n</size>" +
                       "<size=28>- Bullets are smaller\n</size>" +
                       "<size=28>- Much faster firing speed</size>";

            case "balanced":
                return "<size=28><color=purple>Balanced Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- Cannon size, bullet size, firing rate stays the same\n</size>" +
                       "<size=28>- You get a second cannon at the same direction\n</size>" +
                       "<size=28>- Both cannons fire together</size>";

            case "armorpiercing":
                return "<size=28><color=purple>Armor Piercing Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- A total of three HEAVY cannons\n</size>" +
                       "<size=28>- All in the same direction\n</size>" +
                       "<size=28>- Gives powerful and concentrated attack.</size>";

            case "superheavy":
                return "<size=28><color=purple>Super Heavy Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- A total of three HEAVY cannons\n</size>" +
                       "<size=28>- Three cannons at 0, 120, 240 degree\n</size>" +
                       "<size=28>- Can take care of more directions at the same time.</size>";

            case "burst":
                return "<size=28><color=purple>Burst Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- A total of three RAPID cannons\n</size>" +
                       "<size=28>- All in the same direction</size>" +
                       "<size=28>- Gives fast and concentrated attack.</size>";

            case "machinegun":
                return "<size=28><color=purple>Machine Gun Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- A total of three RAPID cannons\n</size>" +
                       "<size=28>- Three cannons at 0, 120, 240 degree</size>" +
                       "<size=28>- Can take care of more directions at the same time.</size>";

            case "tactical":
                return "<size=28><color=purple>Tactical Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- A total of FOUR cannons\n</size>" +
                       "<size=28>- 2 in front, 2 in back</size>" +
                       "<size=28>- A mix of bullet size, speed, directions and concentration.</size>";

            case "versatile":
                return "<size=28><color=purple>Versatile Upgrade</color></size>\n\n" +
                       "<size=28>CANNON CHANGES:\n</size>" +
                       "<size=28>- A total of FOUR cannons\n</size>" +
                       "<size=28>- One in each side of the tank</size>" +
                       "<size=28>- Take care of even more directions with medium bullet size</size>";

            default:
                return "<size=28>" + upgradeName + " Upgrade</size>\n\n" +
                       "This upgrade will improve your tank.\n" +
                       "Check the upgrade details for more info.";
        }
    }

    /// <summary>
    /// ENHANCED ConfirmUpgrade with Tank Transformation Persistence
    /// </summary>
    private void ConfirmUpgrade()
    {
        DebugLog("UPGRADE CONFIRMED: " + selectedUpgrade.upgradeName);

        HideDialog();

        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog("Applied upgrade: " + selectedUpgrade.upgradeName);

            // ★★★ PERSISTENCE FIX: Save tank transformation for cross-scene persistence ★★★
            var playerDataManager = EnsurePlayerDataManager();
            if (playerDataManager != null)
            {
                playerDataManager.SaveTankTransformation(selectedUpgrade.upgradeName);
                DebugLog($"✓ Saved tank transformation to PlayerDataManager: {selectedUpgrade.upgradeName}");
            }
            else
            {
                DebugLog("ERROR: Could not create or find PlayerDataManager! Tank transformation will not persist.");
            }
        }

        HideWheelCompletely();
        ResumeGameplay();

        DebugLog("UPGRADE PROCESS COMPLETE");
    }

    /// <summary>
    /// Ensure PlayerDataManager exists, create if missing
    /// </summary>
    private PlayerDataManager EnsurePlayerDataManager()
    {
        // Try to find existing instance first
        if (PlayerDataManager.Instance != null)
        {
            return PlayerDataManager.Instance;
        }

        DebugLog("PlayerDataManager.Instance is null - attempting to find or create...");

        // Try to find existing PlayerDataManager in scene
        var existing = FindFirstObjectByType<PlayerDataManager>();
        if (existing != null)
        {
            DebugLog("Found existing PlayerDataManager in scene");
            return existing;
        }

        // Create new PlayerDataManager if none exists
        DebugLog("Creating new PlayerDataManager...");
        GameObject pdmGO = new GameObject("PlayerDataManager");
        var pdm = pdmGO.AddComponent<PlayerDataManager>();

        DebugLog("✓ Created PlayerDataManager successfully");
        return pdm;
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

    #region Enhanced Dialog Creation

    /// <summary>
    /// Create dialog with better font handling and verification
    /// </summary>
    private void CreateEnhancedDialogWithBetterFont()
    {
        DebugLog("Creating dialog with enhanced font handling...");

        GameObject dialogCanvasGO = new GameObject("EnhancedConfirmationCanvas");
        dialogCanvas = dialogCanvasGO.AddComponent<Canvas>();
        dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dialogCanvas.sortingOrder = 2000;

        var canvasScaler = dialogCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        dialogCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        dialogPanel = new GameObject("EnhancedConfirmationPanel");
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

        CreateBetterFontText("EnhancedTitle", dialogPanel, new Vector2(0, 200), titleFontSize, Color.yellow, out dialogTitle);
        CreateBetterFontText("EnhancedDescription", dialogPanel, Vector2.zero, descriptionFontSize, Color.white, out dialogDescription);
        CreateBetterFontText("EnhancedInstruction", dialogPanel, new Vector2(0, -220), instructionFontSize, Color.cyan, out dialogInstruction);

        HideDialog();
        DebugLog("Enhanced dialog creation complete");
    }

    private void CreateBetterFontText(string name, GameObject parent, Vector2 position, int fontSize, Color color, out TextMeshProUGUI textComponent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);

        textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = "";

        // Apply font immediately during creation
        if (customFont != null)
        {
            textComponent.font = customFont;
            DebugLog("Applied font during creation: " + customFont.name + " to " + name);
        }
        else
        {
            DebugLog("WARNING: No custom font available during creation of " + name);
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
            Debug.Log("[ComprehensiveFix] " + message);
        }
    }

    public bool IsUpgradeInProgress()
    {
        return currentState != UpgradeState.Hidden;
    }

    [ContextMenu("Test Font Loading")]
    public void TestFontLoading()
    {
        LoadCustomFontWithFallback();
    }

    [ContextMenu("Test Scale Fixing")]
    public void TestScaleFixing()
    {
        FixAllScaleIssuesComprehensive();
    }

    [ContextMenu("Test Font Verification")]
    public void TestFontVerification()
    {
        VerifyFontApplication();
    }
}