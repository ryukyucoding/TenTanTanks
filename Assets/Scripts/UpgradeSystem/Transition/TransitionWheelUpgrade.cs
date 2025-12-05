using UnityEngine;
using UnityEngine.SceneManagement;
using WheelUpgradeSystem;

/// <summary>
/// Manages the transition scene upgrade system with tank movement and wheel display
/// BALANCED version - shows wheel normally, force hides ONLY after YES clicked
/// Replace your existing script with this version
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Existing UI References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private GameObject transitionMover;  // Keep your existing references
    [SerializeField] private GameObject doubleheadModel; // Keep your existing references

    [Header("Tank Movement - NEW")]
    [SerializeField] private Transform playerTank;
    [SerializeField] private float tankMoveSpeed = 5f;
    [SerializeField] private Vector3 centerStopPosition = Vector3.zero;
    [SerializeField] private Vector3 finalExitPosition = new Vector3(10f, 0f, 0f);
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Upgrade System - NEW")]
    [SerializeField] private int upgradeLevel = 1; // Set to 1 for testing
    [SerializeField] private int upgradeTier = 1; // Which tier to show (1 or 2)

    [Header("Confirmation Dialog - NEW")]
    [SerializeField] private SimpleTransitionDialog confirmationDialog;

    [Header("Scene Transition - NEW")]
    [SerializeField] private string nextSceneName = "Level1";
    [SerializeField] private float delayBeforeSceneLoad = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // NEW: State management for tank movement and upgrade process
    private enum TransitionState
    {
        MovingToCenter,
        ShowingWheel,
        WaitingForConfirmation,
        MovingToExit,
        LoadingNextScene
    }

    private TransitionState currentState = TransitionState.MovingToCenter;
    private WheelUpgradeOption selectedUpgrade;
    private EnhancedTransitionMover enhancedTransitionMover; // Reference to the enhanced transition mover

    void Start()
    {
        // Initialize existing components (keep your existing logic)
        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();
        if (tankUpgradeSystem == null)
            tankUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();
        if (confirmationDialog == null)
            confirmationDialog = FindFirstObjectByType<SimpleTransitionDialog>();

        // NEW: Find EnhancedTransitionMover
        if (enhancedTransitionMover == null)
            enhancedTransitionMover = FindFirstObjectByType<EnhancedTransitionMover>();

        // NEW: Find player tank if not set
        if (playerTank == null)
        {
            GameObject tankGO = GameObject.FindGameObjectWithTag("Player");
            if (tankGO != null)
                playerTank = tankGO.transform;
        }

        // FIXED: Only fix scaling issues, don't hide anything on start
        FixWheelScalingOnly();

        // NEW: Check if this level should show upgrades
        CheckUpgradeCondition();

        DebugLog($"TransitionWheelUpgrade started. State: {currentState}");
    }

    void Update()
    {
        // NEW: Handle tank movement states
        switch (currentState)
        {
            case TransitionState.MovingToCenter:
                MoveTankToCenter();
                break;

            case TransitionState.MovingToExit:
                MoveTankToExit();
                break;
        }
    }

    // FIXED: Only fix scaling, don't hide anything
    private void FixWheelScalingOnly()
    {
        if (upgradeWheelUI != null)
        {
            // Find any divider lines or UI elements that might have scaling issues
            Transform wheelContainer = upgradeWheelUI.transform.Find("WheelContainer");
            if (wheelContainer == null)
            {
                // Try alternative names
                wheelContainer = upgradeWheelUI.transform.Find("Wheel Container");
                if (wheelContainer == null)
                {
                    // Try to find any child that looks like a container
                    for (int i = 0; i < upgradeWheelUI.transform.childCount; i++)
                    {
                        var child = upgradeWheelUI.transform.GetChild(i);
                        if (child.name.ToLower().Contains("wheel") || child.name.ToLower().Contains("container"))
                        {
                            wheelContainer = child;
                            break;
                        }
                    }
                }
            }

            if (wheelContainer != null)
            {
                // ONLY reset scale to 1,1,1 to prevent duplicate/scaled lines - DON'T HIDE
                wheelContainer.localScale = Vector3.one;
                DebugLog("Fixed wheel container scaling (without hiding)");

                // Look for divider lines and fix their scaling ONLY
                var allChildren = wheelContainer.GetComponentsInChildren<Transform>(true); // Include inactive
                foreach (var child in allChildren)
                {
                    if (child.name.ToLower().Contains("divider") ||
                        child.name.ToLower().Contains("line") ||
                        child.name.ToLower().Contains("separator"))
                    {
                        child.localScale = Vector3.one;
                        DebugLog($"Fixed scaling for: {child.name} (without hiding)");
                    }
                }
            }
            else
            {
                DebugLog("Warning: Could not find wheel container for scaling fix");
            }
        }
    }

    #region ShowUpgradePanel Methods - OPTIMIZED for EnhancedTransitionMover

    /// <summary>
    /// Show upgrade panel - MAIN METHOD called by EnhancedTransitionMover
    /// Handles transition type strings like "Level2To3", "Level4To5"
    /// </summary>
    public void ShowUpgradePanel(string transitionType)
    {
        DebugLog($"ShowUpgradePanel called by EnhancedTransitionMover with transitionType: {transitionType}");

        // Parse the transition type and set appropriate upgrade tier
        ParseTransitionType(transitionType);

        // Show the upgrade panel
        ShowUpgradePanelInternal();
    }

    /// <summary>
    /// Parse transition type string and set upgrade tier accordingly
    /// </summary>
    private void ParseTransitionType(string transitionType)
    {
        if (string.IsNullOrEmpty(transitionType))
        {
            DebugLog("Empty transition type, using default tier 1");
            upgradeTier = 1;
            return;
        }

        switch (transitionType.ToLower())
        {
            case "level2to3":
                upgradeTier = 1; // First tier upgrades
                upgradeLevel = 3;
                DebugLog("Level 2¡÷3 transition: Using Tier 1 upgrades");
                break;

            case "level4to5":
                upgradeTier = 2; // Second tier upgrades 
                upgradeLevel = 5;
                DebugLog("Level 4¡÷5 transition: Using Tier 2 upgrades");
                break;

            default:
                // For testing or unknown types, default to tier 1
                upgradeTier = 1;
                upgradeLevel = 1;
                DebugLog($"Unknown transition type '{transitionType}', defaulting to Tier 1");
                break;
        }
    }

    /// <summary>
    /// Show upgrade panel - no arguments (for backward compatibility)
    /// </summary>
    public void ShowUpgradePanel()
    {
        DebugLog("ShowUpgradePanel() called with no arguments");
        ShowUpgradePanelInternal();
    }

    /// <summary>
    /// Show upgrade panel - with int parameter (tier number)
    /// </summary>
    public void ShowUpgradePanel(int parameter)
    {
        DebugLog($"ShowUpgradePanel(int) called with parameter: {parameter}");

        // Use the parameter as upgrade tier if it's valid
        if (parameter >= 1 && parameter <= 2)
        {
            upgradeTier = parameter;
        }

        ShowUpgradePanelInternal();
    }

    /// <summary>
    /// Show upgrade panel - with bool parameter (for enable/disable)
    /// </summary>
    public void ShowUpgradePanel(bool parameter)
    {
        DebugLog($"ShowUpgradePanel(bool) called with parameter: {parameter}");

        if (parameter)
        {
            ShowUpgradePanelInternal();
        }
        else
        {
            DebugLog("ShowUpgradePanel called with false - skipping upgrade");
            ContinueToExit();
        }
    }

    /// <summary>
    /// Internal method that actually shows the upgrade panel
    /// </summary>
    private void ShowUpgradePanelInternal()
    {
        DebugLog($"ShowUpgradePanelInternal - Tier {upgradeTier}, Level {upgradeLevel}");

        // ONLY fix scaling before showing, don't hide anything
        FixWheelScalingOnly();

        // Force show the upgrade wheel
        currentState = TransitionState.ShowingWheel;
        ShowUpgradeWheel();
    }

    #endregion

    // NEW: Check if we should show upgrade wheel based on level
    private void CheckUpgradeCondition()
    {
        // For testing, always show at level 1
        // In actual game, check: upgradeLevel == 3 || upgradeLevel == 5
        bool shouldShowUpgrade = (upgradeLevel == 1) || (upgradeLevel == 3) || (upgradeLevel == 5);

        if (!shouldShowUpgrade)
        {
            // Skip to moving to exit
            currentState = TransitionState.MovingToExit;
            DebugLog($"No upgrade needed at level {upgradeLevel}, continuing to exit");
        }
        else
        {
            DebugLog($"Upgrade required at level {upgradeLevel}, moving to center");
        }
    }

    // NEW: Move tank towards center position and stop
    private void MoveTankToCenter()
    {
        if (playerTank == null) return;

        Vector3 direction = (centerStopPosition - playerTank.position).normalized;
        float distance = Vector3.Distance(playerTank.position, centerStopPosition);

        if (distance > stopDistance)
        {
            // Move tank towards center
            playerTank.position += direction * tankMoveSpeed * Time.deltaTime;

            // Rotate tank to face movement direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                playerTank.rotation = Quaternion.Slerp(playerTank.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }
        else
        {
            // Reached center, show upgrade wheel
            playerTank.position = centerStopPosition;
            ShowUpgradeWheel();
        }
    }

    // NEW: Show the upgrade wheel in transition mode
    private void ShowUpgradeWheel()
    {
        currentState = TransitionState.ShowingWheel;
        DebugLog($"Tank reached center, showing upgrade wheel (Tier {upgradeTier})");

        if (upgradeWheelUI != null)
        {
            // ONLY fix scaling before showing, don't hide anything
            FixWheelScalingOnly();

            // Set wheel to transition mode with correct tier
            upgradeWheelUI.SetTransitionMode(upgradeTier, "");
            upgradeWheelUI.ShowWheelForTransition();

            DebugLog("Wheel should now be visible");
        }
        else
        {
            Debug.LogError("UpgradeWheelUI not found! Cannot show upgrade wheel.");
            ContinueToExit();
        }
    }

    // NEW: Called by UpgradeWheelUI when an upgrade is selected
    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        DebugLog($"Upgrade selected: {upgrade.upgradeName}");
        selectedUpgrade = upgrade;
        currentState = TransitionState.WaitingForConfirmation;

        // Show confirmation dialog
        ShowConfirmationDialog(upgrade);
    }

    // NEW: Show confirmation dialog for the selected upgrade
    private void ShowConfirmationDialog(WheelUpgradeOption upgrade)
    {
        DebugLog($"Attempting to show confirmation dialog for: {upgrade.upgradeName}");

        if (confirmationDialog != null)
        {
            DebugLog("Using SimpleTransitionDialog for confirmation");
            confirmationDialog.ShowDialog(upgrade, OnUpgradeConfirmed, OnUpgradeCanceled);
        }
        else
        {
            // Try to find any dialog component
            var anyDialog = FindFirstObjectByType<SimpleTransitionDialog>();
            if (anyDialog != null)
            {
                DebugLog("Found SimpleTransitionDialog in scene, using it");
                confirmationDialog = anyDialog;
                confirmationDialog.ShowDialog(upgrade, OnUpgradeConfirmed, OnUpgradeCanceled);
            }
            else
            {
                Debug.LogWarning("No confirmation dialog found, auto-confirming upgrade");
                OnUpgradeConfirmed();
            }
        }
    }

    // FIXED: Called when player confirms the upgrade - ONLY HIDE HERE!
    private void OnUpgradeConfirmed()
    {
        DebugLog($"Upgrade confirmed: {selectedUpgrade?.upgradeName} - NOW HIDING WHEEL");

        // STEP 1: NOW we force hide the wheel (ONLY after YES clicked)
        ForceHideUpgradeWheelAfterConfirm();

        // STEP 2: Apply the upgrade
        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog($"Applied upgrade to tank: {selectedUpgrade.upgradeName}");
        }

        // STEP 3: Continue to exit
        ContinueToExit();
    }

    // FIXED: Called when player cancels the upgrade
    private void OnUpgradeCanceled()
    {
        DebugLog("Upgrade canceled, showing wheel again");

        // Reset to showing wheel
        currentState = TransitionState.ShowingWheel;
        selectedUpgrade = null;

        // Show wheel again (don't hide it)
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.ShowWheelForTransition();
        }
    }

    // FIXED: Force hide ONLY after YES is clicked
    private void ForceHideUpgradeWheelAfterConfirm()
    {
        DebugLog("ForceHideUpgradeWheelAfterConfirm - hiding because YES was clicked");

        // Method 1: Normal hide first
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.HideWheel();
            DebugLog("Called upgradeWheelUI.HideWheel()");

            // Wait a moment then force hide if normal hide didn't work
            Invoke(nameof(ForceHideIfStillVisible), 0.5f);
        }

        // Method 2: Hide the upgrade canvas
        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(false);
            DebugLog("Deactivated upgrade canvas");
        }
    }

    // FIXED: Backup force hide method (only if normal hide didn't work)
    private void ForceHideIfStillVisible()
    {
        // Check if wheel is still visible
        bool wheelStillVisible = false;

        if (upgradeWheelUI != null && upgradeWheelUI.gameObject.activeInHierarchy)
        {
            wheelStillVisible = true;
        }

        if (upgradeCanvas != null && upgradeCanvas.gameObject.activeInHierarchy)
        {
            wheelStillVisible = true;
        }

        if (wheelStillVisible)
        {
            DebugLog("Wheel still visible after normal hide - using force hide as backup");

            // Force hide everything upgrade-related
            if (upgradeWheelUI != null)
            {
                upgradeWheelUI.gameObject.SetActive(false);
            }

            if (upgradeCanvas != null)
            {
                upgradeCanvas.gameObject.SetActive(false);
            }

            // Find any remaining upgrade UI elements
            var upgradeObjects = FindObjectsOfType<Canvas>();
            foreach (var canvas in upgradeObjects)
            {
                if (canvas.name.ToLower().Contains("upgrade") || canvas.name.ToLower().Contains("wheel"))
                {
                    canvas.gameObject.SetActive(false);
                    DebugLog($"Force deactivated remaining canvas: {canvas.name}");
                }
            }
        }
        else
        {
            DebugLog("Wheel properly hidden with normal method - no force hide needed");
        }
    }

    // NEW: Continue tank movement to exit
    private void ContinueToExit()
    {
        DebugLog("Continuing tank movement to exit");
        currentState = TransitionState.MovingToExit;

        // Notify EnhancedTransitionMover that upgrade is complete
        if (enhancedTransitionMover != null)
        {
            DebugLog("Notifying EnhancedTransitionMover that upgrade is complete");
            enhancedTransitionMover.ResumeMovement();
        }
    }

    // NEW: Move tank towards exit position and load next scene
    private void MoveTankToExit()
    {
        if (playerTank == null) return;

        Vector3 direction = (finalExitPosition - playerTank.position).normalized;
        float distance = Vector3.Distance(playerTank.position, finalExitPosition);

        if (distance > stopDistance)
        {
            // Move tank towards exit
            playerTank.position += direction * tankMoveSpeed * Time.deltaTime;

            // Rotate tank to face movement direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                playerTank.rotation = Quaternion.Slerp(playerTank.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }
        else
        {
            // Reached exit, load next scene
            LoadNextScene();
        }
    }

    // NEW: Load the next scene
    private void LoadNextScene()
    {
        if (currentState != TransitionState.LoadingNextScene)
        {
            currentState = TransitionState.LoadingNextScene;
            DebugLog($"Loading next scene: {nextSceneName}");

            Invoke(nameof(DoSceneLoad), delayBeforeSceneLoad);
        }
    }

    private void DoSceneLoad()
    {
        // Use existing SceneTransitionManager.LoadSceneWithTransition method
        DebugLog($"Using SceneTransitionManager.LoadSceneWithTransition for: {nextSceneName}");
        SceneTransitionManager.LoadSceneWithTransition(nextSceneName);
    }

    // ADDED: Public method to check if upgrade is in progress
    /// <summary>
    /// Check if upgrade system is currently showing
    /// </summary>
    public bool IsUpgradeInProgress()
    {
        return currentState == TransitionState.ShowingWheel || currentState == TransitionState.WaitingForConfirmation;
    }

    // NEW: Debug logging helper
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TransitionWheelUpgrade] {message}");
        }
    }

    #region Context Menu Testing

    [ContextMenu("Skip Upgrade")]
    public void SkipUpgrade()
    {
        ForceHideUpgradeWheelAfterConfirm();
        ContinueToExit();
    }

    [ContextMenu("Force Show Wheel")]
    public void ForceShowWheel()
    {
        upgradeLevel = 1;
        currentState = TransitionState.MovingToCenter;
        CheckUpgradeCondition();
    }

    [ContextMenu("Test Upgrade Confirmation")]
    public void TestUpgradeConfirmation()
    {
        // Create a test upgrade
        var testUpgrade = new WheelUpgradeOption("TestUpgrade", "This is a test upgrade", 1);
        selectedUpgrade = testUpgrade;
        OnUpgradeConfirmed();
    }

    [ContextMenu("Force Hide Wheel (Emergency Only)")]
    public void EmergencyForceHideWheel()
    {
        ForceHideUpgradeWheelAfterConfirm();
    }

    [ContextMenu("Fix Wheel Scaling Only")]
    public void TestFixWheelScaling()
    {
        FixWheelScalingOnly();
    }

    [ContextMenu("Test Level2To3 Transition")]
    public void TestLevel2To3()
    {
        ShowUpgradePanel("Level2To3");
    }

    [ContextMenu("Test Level4To5 Transition")]
    public void TestLevel4To5()
    {
        ShowUpgradePanel("Level4To5");
    }

    [ContextMenu("Set Level 1 (Testing)")]
    public void SetTestingLevel1()
    {
        upgradeLevel = 1;
        upgradeTier = 1;
        DebugLog("Set to Level 1 for testing");
    }

    [ContextMenu("Set Level 3 (Tier 1 Upgrade)")]
    public void SetLevel3()
    {
        upgradeLevel = 3;
        upgradeTier = 1;
        DebugLog("Set to Level 3 for Tier 1 upgrades");
    }

    [ContextMenu("Set Level 5 (Tier 2 Upgrade)")]
    public void SetLevel5()
    {
        upgradeLevel = 5;
        upgradeTier = 2;
        DebugLog("Set to Level 5 for Tier 2 upgrades");
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        // Draw positions for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerStopPosition, 1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(finalExitPosition, 1f);

        // Draw path
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(centerStopPosition, finalExitPosition);
    }
}