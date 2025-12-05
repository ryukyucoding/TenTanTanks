using UnityEngine;
using UnityEngine.InputSystem;
using WheelUpgradeSystem;

/// <summary>
/// SIMPLE KEYBOARD-BASED UPGRADE SYSTEM
/// Y = YES (confirm upgrade), N = NO (cancel), ESC = EXIT everything
/// This replaces the confusing dialog system with clear keyboard controls
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;

    [Header("Simple UI")]
    [SerializeField] private GameObject instructionPanel; // We'll create this automatically

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // State management
    private enum UpgradeState
    {
        Hidden,           // No wheel showing
        SelectingUpgrade, // Wheel is showing, waiting for upgrade selection
        ConfirmingUpgrade // Upgrade selected, waiting for Y/N confirmation
    }

    private UpgradeState currentState = UpgradeState.Hidden;
    private WheelUpgradeOption selectedUpgrade;
    private EnhancedTransitionMover enhancedTransitionMover;
    private Transform wheelContainer;

    // UI References for simple instruction display
    private UnityEngine.UI.Text instructionText;
    private Canvas instructionCanvas;

    void Start()
    {
        DebugLog("=== Simple Keyboard Upgrade System Started ===");

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

        // Create simple instruction UI
        CreateInstructionUI();

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
    /// Handle all keyboard input for the upgrade system
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        switch (currentState)
        {
            case UpgradeState.SelectingUpgrade:
                // ESC = Exit wheel completely
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    DebugLog("ESC pressed - hiding wheel completely");
                    HideWheelCompletely();
                    ResumeGameplay();
                }
                break;

            case UpgradeState.ConfirmingUpgrade:
                // Y = YES, confirm upgrade
                if (Keyboard.current.yKey.wasPressedThisFrame)
                {
                    DebugLog("Y pressed - confirming upgrade");
                    ConfirmUpgrade();
                }
                // N = NO, cancel and return to wheel
                else if (Keyboard.current.nKey.wasPressedThisFrame)
                {
                    DebugLog("N pressed - canceling upgrade");
                    CancelUpgrade();
                }
                // ESC = Exit completely
                else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    DebugLog("ESC pressed - exiting completely");
                    HideWheelCompletely();
                    ResumeGameplay();
                }
                break;
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
    /// Show the upgrade wheel with proper setup
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

        // Show simple instruction
        ShowInstruction("Select an upgrade, then press ESC to exit");

        DebugLog("Wheel should now be visible");
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
    /// Fix all scaling issues that cause line problems
    /// </summary>
    private void FixAllScaleIssues()
    {
        if (upgradeWheelUI == null) return;

        DebugLog("=== Fixing Scale Issues ===");

        // Reset main scales
        upgradeWheelUI.transform.localScale = Vector3.one;

        if (wheelContainer != null)
            wheelContainer.localScale = Vector3.one;

        // Fix all children recursively
        FixChildScales(upgradeWheelUI.transform);

        DebugLog("Scale issues fixed");
    }

    private void FixChildScales(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // Reset scale if it's not (1,1,1)
            if (child.localScale != Vector3.one)
            {
                child.localScale = Vector3.one;
                DebugLog($"Fixed scale: {child.name}");
            }

            // Recursively fix children
            FixChildScales(child);
        }
    }

    /// <summary>
    /// Called when upgrade is selected (replaces dialog system)
    /// </summary>
    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        DebugLog($"=== Upgrade Selected: {upgrade.upgradeName} ===");

        selectedUpgrade = upgrade;
        currentState = UpgradeState.ConfirmingUpgrade;

        // Show keyboard confirmation instruction
        ShowInstruction($"Confirm '{upgrade.upgradeName}'? Y = YES, N = NO, ESC = EXIT");

        DebugLog("Waiting for Y/N/ESC input");
    }

    /// <summary>
    /// Y key pressed - confirm the upgrade
    /// </summary>
    private void ConfirmUpgrade()
    {
        DebugLog($"=== UPGRADE CONFIRMED: {selectedUpgrade.upgradeName} ===");

        // Apply upgrade
        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog($"Applied upgrade: {selectedUpgrade.upgradeName}");
        }

        // Hide everything
        HideWheelCompletely();

        // Resume gameplay
        ResumeGameplay();

        DebugLog("=== UPGRADE PROCESS COMPLETE ===");
    }

    /// <summary>
    /// N key pressed - cancel upgrade and return to wheel
    /// </summary>
    private void CancelUpgrade()
    {
        DebugLog("=== Upgrade Canceled ===");

        selectedUpgrade = null;
        currentState = UpgradeState.SelectingUpgrade;

        // Show wheel selection instruction again
        ShowInstruction("Select an upgrade, then press ESC to exit");

        DebugLog("Returned to upgrade selection");
    }

    /// <summary>
    /// Hide wheel completely with all background elements
    /// </summary>
    private void HideWheelCompletely()
    {
        DebugLog("=== HIDING WHEEL COMPLETELY ===");

        currentState = UpgradeState.Hidden;

        // Method 1: Standard hiding
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.HideWheel();
            DebugLog("Called upgradeWheelUI.HideWheel()");
        }

        // Method 2: Deactivate canvas (this should fix background issue)
        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(false);
            DebugLog("Deactivated upgrade canvas");
        }

        // Method 3: Deactivate wheel container
        if (wheelContainer != null)
        {
            wheelContainer.gameObject.SetActive(false);
            DebugLog("Deactivated wheel container");
        }

        // Method 4: Deactivate UI GameObject
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.gameObject.SetActive(false);
            DebugLog("Deactivated UpgradeWheelUI GameObject");
        }

        // Hide instruction
        HideInstruction();

        DebugLog("=== HIDE COMPLETE ===");
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

    #region Simple Instruction UI

    /// <summary>
    /// Create simple instruction UI that shows keyboard controls
    /// </summary>
    private void CreateInstructionUI()
    {
        // Create instruction canvas
        GameObject instructionCanvasGO = new GameObject("InstructionCanvas");
        instructionCanvas = instructionCanvasGO.AddComponent<Canvas>();
        instructionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        instructionCanvas.sortingOrder = 1000; // Make sure it's on top

        var canvasScaler = instructionCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        instructionCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create instruction panel
        instructionPanel = new GameObject("InstructionPanel");
        instructionPanel.transform.SetParent(instructionCanvas.transform, false);

        var panelImage = instructionPanel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black

        var panelRect = instructionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.1f);
        panelRect.anchorMax = new Vector2(0.5f, 0.1f);
        panelRect.sizeDelta = new Vector2(600, 100);
        panelRect.anchoredPosition = Vector2.zero;

        // Create instruction text
        GameObject instructionTextGO = new GameObject("InstructionText");
        instructionTextGO.transform.SetParent(instructionPanel.transform, false);

        instructionText = instructionTextGO.AddComponent<UnityEngine.UI.Text>();
        instructionText.text = "";
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.fontSize = 24;
        instructionText.color = Color.white;
        instructionText.alignment = TextAnchor.MiddleCenter;

        var textRect = instructionText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Hide initially
        HideInstruction();

        DebugLog("Created instruction UI");
    }

    private void ShowInstruction(string message)
    {
        if (instructionText != null && instructionPanel != null)
        {
            instructionText.text = message;
            instructionPanel.SetActive(true);
            DebugLog($"Instruction: {message}");
        }
    }

    private void HideInstruction()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }
    }

    #endregion

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SimpleUpgrade] {message}");
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

    [ContextMenu("Force Hide Wheel")]
    public void ForceHideWheel()
    {
        HideWheelCompletely();
    }

    [ContextMenu("Test Y Key")]
    public void TestYKey()
    {
        if (currentState == UpgradeState.ConfirmingUpgrade)
            ConfirmUpgrade();
    }

    [ContextMenu("Test N Key")]
    public void TestNKey()
    {
        if (currentState == UpgradeState.ConfirmingUpgrade)
            CancelUpgrade();
    }
}