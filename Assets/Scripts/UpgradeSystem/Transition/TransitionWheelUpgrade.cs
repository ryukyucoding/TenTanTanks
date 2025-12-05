using UnityEngine;
using UnityEngine.InputSystem;
using WheelUpgradeSystem;
using UnityEngine.UI;

/// <summary>
/// FINAL IMPROVED KEYBOARD UPGRADE SYSTEM
/// Y = YES (only way to close), detailed upgrade descriptions, prominent dialog
/// No ESC, no click-to-close, clear upgrade explanations
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;

    [Header("Enhanced UI")]
    [SerializeField] private GameObject confirmationDialog; // Enhanced dialog

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // State management
    private enum UpgradeState
    {
        Hidden,           // No wheel showing
        SelectingUpgrade, // Wheel is showing, waiting for upgrade selection
        ConfirmingUpgrade // Upgrade selected, waiting for Y confirmation
    }

    private UpgradeState currentState = UpgradeState.Hidden;
    private WheelUpgradeOption selectedUpgrade;
    private EnhancedTransitionMover enhancedTransitionMover;
    private Transform wheelContainer;

    // Enhanced UI References
    private Canvas dialogCanvas;
    private GameObject dialogPanel;
    private Text dialogTitle;
    private Text dialogDescription;
    private Text dialogInstruction;
    private Image dialogBackground;

    void Start()
    {
        DebugLog("=== Enhanced Keyboard Upgrade System Started ===");

        // Auto-find components
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

        // Create enhanced confirmation dialog
        CreateEnhancedDialog();

        // IMPORTANT: Disable existing wheel closer to prevent click-to-close
        DisableWheelCloser();

        DebugLog($"Components found:");
        DebugLog($"  - UpgradeWheelUI: {(upgradeWheelUI != null ? "Y" : "N")}");
        DebugLog($"  - UpgradeCanvas: {(upgradeCanvas != null ? "Y" : "N")}");
        DebugLog($"  - TankUpgradeSystem: {(tankUpgradeSystem != null ? "Y" : "N")}");
    }

    void Update()
    {
        HandleKeyboardInput();
    }

    /// <summary>
    /// Handle keyboard input - ONLY Y key when confirming
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        // ONLY handle Y key when confirming upgrade
        if (currentState == UpgradeState.ConfirmingUpgrade)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                DebugLog("Y pressed - confirming upgrade");
                ConfirmUpgrade();
            }
            // N key to cancel (return to selection)
            else if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                DebugLog("N pressed - canceling upgrade");
                CancelUpgrade();
            }
        }

        // NO ESC KEY HANDLING - removed as requested
    }

    /// <summary>
    /// Disable existing wheel closer to prevent unwanted closing
    /// </summary>
    private void DisableWheelCloser()
    {
        var wheelCloser = FindFirstObjectByType<UpgradeWheelCloser>();
        if (wheelCloser != null)
        {
            wheelCloser.enabled = false;
            DebugLog("Disabled UpgradeWheelCloser to prevent click-to-close");
        }
    }

    /// <summary>
    /// Main method called by EnhancedTransitionMover
    /// </summary>
    public void ShowUpgradePanel(string transitionType)
    {
        DebugLog($"ShowUpgradePanel called with: {transitionType}");
        ShowUpgradeWheel();
    }

    /// <summary>
    /// Show the upgrade wheel
    /// </summary>
    private void ShowUpgradeWheel()
    {
        DebugLog("=== Showing Upgrade Wheel ===");

        currentState = UpgradeState.SelectingUpgrade;

        // Activate all necessary components
        ActivateWheelComponents();

        // Fix scaling issues
        FixAllScaleIssues();

        // Show wheel
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.SetTransitionMode(1, "");
            upgradeWheelUI.ShowWheelForTransition();
        }

        DebugLog("Wheel should now be visible - waiting for upgrade selection");
    }

    /// <summary>
    /// Activate all wheel components properly
    /// </summary>
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

        // Find and activate wheel container
        if (upgradeWheelUI != null)
        {
            string[] containerNames = { "UpgradeWheelContainer", "WheelContainer", "Wheel Container" };

            foreach (string name in containerNames)
            {
                wheelContainer = upgradeWheelUI.transform.Find(name);
                if (wheelContainer != null)
                {
                    wheelContainer.gameObject.SetActive(true);
                    DebugLog($"Activated wheel container: {wheelContainer.name}");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Fix all scaling issues
    /// </summary>
    private void FixAllScaleIssues()
    {
        if (upgradeWheelUI == null) return;

        DebugLog("=== Fixing Scale Issues ===");

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
                DebugLog($"Fixed scale: {child.name}");
            }

            FixChildScales(child);
        }
    }

    /// <summary>
    /// Called when upgrade is selected - show detailed confirmation
    /// </summary>
    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        DebugLog($"=== Upgrade Selected: {upgrade.upgradeName} ===");

        selectedUpgrade = upgrade;
        currentState = UpgradeState.ConfirmingUpgrade;

        // Show detailed confirmation dialog
        ShowDetailedConfirmation(upgrade);

        DebugLog("Waiting for Y/N input");
    }

    /// <summary>
    /// Show enhanced confirmation dialog with detailed descriptions
    /// </summary>
    private void ShowDetailedConfirmation(WheelUpgradeOption upgrade)
    {
        if (dialogPanel == null) return;

        // Get detailed description based on upgrade type
        string detailedDescription = GetDetailedUpgradeDescription(upgrade.upgradeName);

        // Update dialog content
        if (dialogTitle != null)
            dialogTitle.text = $"選擇升級: {upgrade.upgradeName}";

        if (dialogDescription != null)
            dialogDescription.text = detailedDescription;

        if (dialogInstruction != null)
            dialogInstruction.text = "按 Y 確認 | 按 N 取消";

        // Show dialog
        dialogPanel.SetActive(true);

        DebugLog($"Showing detailed confirmation for: {upgrade.upgradeName}");
    }

    /// <summary>
    /// Get detailed description for each upgrade type
    /// </summary>
    private string GetDetailedUpgradeDescription(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "heavy":
                return "重型升級\n\n" +
                       "- 砲管將變得更大更粗\n" +
                       "- 子彈威力大幅增加\n" +
                       "- 射擊速度較慢\n" +
                       "- 適合對付裝甲敵人\n\n" +
                       "這是一個專注於火力的升級選擇。";

            case "rapid":
                return "快速升級\n\n" +
                       "- 砲管將變得更小更細\n" +
                       "- 子彈較小但發射快速\n" +
                       "- 射擊速度大幅提升\n" +
                       "- 適合對付大量敵人\n\n" +
                       "這是一個專注於速度的升級選擇。";

            case "balanced":
                return "平衡升級\n\n" +
                       "- 砲管尺寸保持不變\n" +
                       "- 子彈威力保持相同\n" +
                       "- 獲得額外的第二門砲管\n" +
                       "- 雙砲管同時射擊\n\n" +
                       "這是一個專注於多重攻擊的升級選擇。";

            default:
                return $"{upgradeName} 升級\n\n" +
                       "這個升級將改善你的坦克性能。\n" +
                       "詳細效果請查看升級說明。";
        }
    }

    /// <summary>
    /// Y key pressed - confirm the upgrade
    /// </summary>
    private void ConfirmUpgrade()
    {
        DebugLog($"=== UPGRADE CONFIRMED: {selectedUpgrade.upgradeName} ===");

        // Hide confirmation dialog
        HideDialog();

        // Apply upgrade
        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog($"Applied upgrade: {selectedUpgrade.upgradeName}");
        }

        // Hide wheel completely (ONLY way to close)
        HideWheelCompletely();

        // Resume gameplay
        ResumeGameplay();

        DebugLog("=== UPGRADE PROCESS COMPLETE ===");
    }

    /// <summary>
    /// N key pressed - cancel upgrade and return to selection
    /// </summary>
    private void CancelUpgrade()
    {
        DebugLog("=== Upgrade Canceled ===");

        selectedUpgrade = null;
        currentState = UpgradeState.SelectingUpgrade;

        // Hide dialog and return to wheel selection
        HideDialog();

        DebugLog("Returned to upgrade selection");
    }

    /// <summary>
    /// Hide wheel completely - ONLY called when Y is pressed
    /// </summary>
    private void HideWheelCompletely()
    {
        DebugLog("=== HIDING WHEEL COMPLETELY (Y PRESSED) ===");

        currentState = UpgradeState.Hidden;

        // Hide everything
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

        DebugLog("=== HIDE COMPLETE (ONLY Y CAN CLOSE) ===");
    }

    /// <summary>
    /// Resume normal gameplay
    /// </summary>
    private void ResumeGameplay()
    {
        if (enhancedTransitionMover != null)
        {
            DebugLog("Notifying EnhancedTransitionMover to resume");
            enhancedTransitionMover.ResumeMovement();
        }
    }

    #region Enhanced Confirmation Dialog

    /// <summary>
    /// Create prominent, beautiful confirmation dialog
    /// </summary>
    private void CreateEnhancedDialog()
    {
        DebugLog("Creating enhanced confirmation dialog...");

        // Create dialog canvas
        GameObject dialogCanvasGO = new GameObject("ConfirmationDialogCanvas");
        dialogCanvas = dialogCanvasGO.AddComponent<Canvas>();
        dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dialogCanvas.sortingOrder = 2000; // Above everything else

        var canvasScaler = dialogCanvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        dialogCanvasGO.AddComponent<GraphicRaycaster>();

        // Create dialog panel
        dialogPanel = new GameObject("ConfirmationPanel");
        dialogPanel.transform.SetParent(dialogCanvas.transform, false);

        var panelImage = dialogPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Nearly opaque dark background

        var panelRect = dialogPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(800, 500);
        panelRect.anchoredPosition = Vector2.zero;

        // Add border
        var outline = dialogPanel.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3, 3);

        // Create title text
        CreateDialogText("ConfirmationTitle", dialogPanel, new Vector2(0, 150), 36, Color.yellow, TextAnchor.MiddleCenter, out dialogTitle);

        // Create description text  
        CreateDialogText("ConfirmationDescription", dialogPanel, Vector2.zero, 20, Color.white, TextAnchor.MiddleCenter, out dialogDescription);

        // Create instruction text
        CreateDialogText("ConfirmationInstruction", dialogPanel, new Vector2(0, -180), 28, Color.cyan, TextAnchor.MiddleCenter, out dialogInstruction);

        // Hide initially
        HideDialog();

        DebugLog("Created enhanced confirmation dialog");
    }

    private void CreateDialogText(string name, GameObject parent, Vector2 position, int fontSize, Color color, TextAnchor alignment, out Text textComponent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);

        textComponent = textGO.AddComponent<Text>();
        textComponent.text = "";
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = alignment;

        var textRect = textComponent.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(750, fontSize == 20 ? 300 : 60);
        textRect.anchoredPosition = position;

        // Add shadow for better readability
        var shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);
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
            Debug.Log($"[EnhancedUpgrade] {message}");
        }
    }

    // Public methods for compatibility
    public bool IsUpgradeInProgress()
    {
        return currentState != UpgradeState.Hidden;
    }

    // Testing methods
    [ContextMenu("Force Show Wheel")]
    public void ForceShowWheel()
    {
        ShowUpgradeWheel();
    }

    [ContextMenu("Test Heavy Description")]
    public void TestHeavyDescription()
    {
        var testUpgrade = new WheelUpgradeOption("Heavy", "Test Heavy", 1);
        OnUpgradeSelected(testUpgrade);
    }

    [ContextMenu("Test Rapid Description")]
    public void TestRapidDescription()
    {
        var testUpgrade = new WheelUpgradeOption("Rapid", "Test Rapid", 1);
        OnUpgradeSelected(testUpgrade);
    }

    [ContextMenu("Test Balanced Description")]
    public void TestBalancedDescription()
    {
        var testUpgrade = new WheelUpgradeOption("Balanced", "Test Balanced", 1);
        OnUpgradeSelected(testUpgrade);
    }
}