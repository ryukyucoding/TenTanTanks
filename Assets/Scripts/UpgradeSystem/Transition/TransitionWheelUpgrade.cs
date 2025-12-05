using UnityEngine;
using UnityEngine.SceneManagement;
using WheelUpgradeSystem;

/// <summary>
/// Manages the transition scene upgrade system with tank movement and wheel display
/// This is a FINAL COMPATIBLE version that includes ShowUpgradePanel method for EnhancedTransitionMover
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

    void Start()
    {
        // Initialize existing components (keep your existing logic)
        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();
        if (tankUpgradeSystem == null)
            tankUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();
        if (confirmationDialog == null)
            confirmationDialog = FindFirstObjectByType<SimpleTransitionDialog>();

        // NEW: Find player tank if not set
        if (playerTank == null)
        {
            GameObject tankGO = GameObject.FindGameObjectWithTag("Player");
            if (tankGO != null)
                playerTank = tankGO.transform;
        }

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

    // ADDED: Method that EnhancedTransitionMover is trying to call
    /// <summary>
    /// Show upgrade panel - called by EnhancedTransitionMover
    /// This method provides compatibility with your existing EnhancedTransitionMover script
    /// </summary>
    public void ShowUpgradePanel()
    {
        DebugLog("ShowUpgradePanel called by EnhancedTransitionMover");

        // Force show the upgrade wheel regardless of level
        upgradeLevel = 1; // Force to a level that shows upgrades
        currentState = TransitionState.ShowingWheel;
        ShowUpgradeWheel();
    }

    // ADDED: Alternative method name that might be called
    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public void ShowUpgrades()
    {
        DebugLog("ShowUpgrades called - redirecting to ShowUpgradePanel");
        ShowUpgradePanel();
    }

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
        DebugLog("Tank reached center, showing upgrade wheel");

        if (upgradeWheelUI != null)
        {
            // Set wheel to transition mode
            upgradeWheelUI.SetTransitionMode(upgradeTier, "");
            upgradeWheelUI.ShowWheelForTransition();
        }
        else
        {
            Debug.LogError("UpgradeWheelUI not found! Cannot show upgrade wheel.");
            ContinueToExit();
        }
    }

    // NEW: Called by UpgradeWheelUI when an upgrade is selected
    // THIS IS THE MISSING METHOD THAT WAS CAUSING THE ERROR
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
        if (confirmationDialog != null)
        {
            confirmationDialog.ShowDialog(upgrade, OnUpgradeConfirmed, OnUpgradeCanceled);
        }
        else
        {
            Debug.LogWarning("No confirmation dialog found, auto-confirming upgrade");
            OnUpgradeConfirmed();
        }
    }

    // NEW: Called when player confirms the upgrade
    private void OnUpgradeConfirmed()
    {
        DebugLog($"Upgrade confirmed: {selectedUpgrade.upgradeName}");

        // Apply the upgrade
        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog($"Applied upgrade to tank: {selectedUpgrade.upgradeName}");
        }

        // Continue to exit
        ContinueToExit();
    }

    // NEW: Called when player cancels the upgrade
    private void OnUpgradeCanceled()
    {
        DebugLog("Upgrade canceled, showing wheel again");

        // Reset to showing wheel
        currentState = TransitionState.ShowingWheel;
        selectedUpgrade = null;

        // Show wheel again
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.ShowWheelForTransition();
        }
    }

    // NEW: Continue tank movement to exit
    private void ContinueToExit()
    {
        DebugLog("Continuing tank movement to exit");
        currentState = TransitionState.MovingToExit;
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

    // NEW: Load the next scene - FIXED to use existing SceneTransitionManager methods
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
        // FIXED: Use existing SceneTransitionManager.LoadSceneWithTransition method
        DebugLog($"Using SceneTransitionManager.LoadSceneWithTransition for: {nextSceneName}");
        SceneTransitionManager.LoadSceneWithTransition(nextSceneName);
    }

    // ADDED: Public method to notify when upgrade is complete (for EnhancedTransitionMover)
    /// <summary>
    /// Called when upgrade process is complete - can be used by other scripts
    /// </summary>
    public void OnUpgradeComplete()
    {
        DebugLog("Upgrade process complete");
        ContinueToExit();
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

    // NEW: Force skip upgrade (for testing)
    [ContextMenu("Skip Upgrade")]
    public void SkipUpgrade()
    {
        ContinueToExit();
    }

    // NEW: Force show upgrade wheel (for testing)
    [ContextMenu("Force Show Wheel")]
    public void ForceShowWheel()
    {
        upgradeLevel = 1; // Force to level that needs upgrade
        currentState = TransitionState.MovingToCenter;
        CheckUpgradeCondition();
    }

    // ADDED: Test ShowUpgradePanel method
    [ContextMenu("Test ShowUpgradePanel")]
    public void TestShowUpgradePanel()
    {
        ShowUpgradePanel();
    }

    // NEW: Set upgrade level for testing
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

    void OnDrawGizmosSelected()
    {
        // NEW: Draw positions for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerStopPosition, 1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(finalExitPosition, 1f);

        // Draw path
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(centerStopPosition, finalExitPosition);
    }

    // Keep any existing methods you might have in your original script
    // Add them here if they exist and are needed
}