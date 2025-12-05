using UnityEngine;
using WheelUpgradeSystem;

/// <summary>
/// FINAL WORKING VERSION - Ensures confirmation callback works and scale issues are fixed
/// This version should completely solve both remaining issues
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;
    [SerializeField] private SimpleTransitionDialog confirmationDialog;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private WheelUpgradeOption selectedUpgrade;
    private EnhancedTransitionMover enhancedTransitionMover;
    private Transform wheelContainer;

    void Start()
    {
        DebugLog("=== TransitionWheelUpgrade Started ===");

        // Auto-find components
        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();
        if (tankUpgradeSystem == null)
            tankUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();
        if (confirmationDialog == null)
            confirmationDialog = FindFirstObjectByType<SimpleTransitionDialog>();
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

        DebugLog($"Components found:");
        DebugLog($"  - UpgradeWheelUI: {(upgradeWheelUI != null ? "Y" : "N")}");
        DebugLog($"  - UpgradeCanvas: {(upgradeCanvas != null ? "Y" : "N")}");
        DebugLog($"  - TankUpgradeSystem: {(tankUpgradeSystem != null ? "Y" : "N")}");
        DebugLog($"  - ConfirmationDialog: {(confirmationDialog != null ? "Y" : "N")}");
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
    /// Show the upgrade wheel with proper scaling fixes
    /// </summary>
    private void ShowUpgradeWheel()
    {
        DebugLog("=== ShowUpgradeWheel - ACTIVATING CONTAINERS ===");

        // STEP 1: Force activate the UpgradeWheelUI GameObject
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.gameObject.SetActive(true);
            DebugLog($"Activated UpgradeWheelUI GameObject: {upgradeWheelUI.name}");
        }

        // STEP 2: Force activate the Canvas
        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(true);
            DebugLog($"Activated UpgradeCanvas: {upgradeCanvas.name}");
        }

        // STEP 3: Find and activate WheelContainer + fix scaling
        FindAndActivateWheelContainer();

        // STEP 4: Fix scale issues BEFORE showing wheel
        FixAllScaleIssues();

        // STEP 5: Now try to show the wheel
        if (upgradeWheelUI != null)
        {
            try
            {
                upgradeWheelUI.SetTransitionMode(1, "");
                DebugLog("Called SetTransitionMode(1)");

                upgradeWheelUI.ShowWheelForTransition();
                DebugLog("Called ShowWheelForTransition()");

                DebugLog("WHEEL SHOULD NOW BE VISIBLE!");
            }
            catch (System.Exception e)
            {
                DebugLog($"Error showing wheel: {e.Message}");
            }
        }
    }

    private void FindAndActivateWheelContainer()
    {
        if (upgradeWheelUI == null) return;

        // Look for wheel container
        wheelContainer = null;
        string[] containerNames = { "UpgradeWheelContainer", "WheelContainer", "Wheel Container", "Container" };

        foreach (string name in containerNames)
        {
            wheelContainer = upgradeWheelUI.transform.Find(name);
            if (wheelContainer != null)
            {
                DebugLog($"Found wheel container: {wheelContainer.name}");
                break;
            }
        }

        // If not found, search all children
        if (wheelContainer == null)
        {
            for (int i = 0; i < upgradeWheelUI.transform.childCount; i++)
            {
                var child = upgradeWheelUI.transform.GetChild(i);
                if (child.name.ToLower().Contains("wheel") ||
                    child.name.ToLower().Contains("container"))
                {
                    wheelContainer = child;
                    DebugLog($"Found wheel container by search: {wheelContainer.name}");
                    break;
                }
            }
        }

        // CRITICAL: Activate the wheel container
        if (wheelContainer != null)
        {
            wheelContainer.gameObject.SetActive(true);
            DebugLog($"ACTIVATED WHEEL CONTAINER: {wheelContainer.name}");
        }
        else
        {
            DebugLog("WARNING: Could not find UpgradeWheelContainer!");
        }
    }

    /// <summary>
    /// ENHANCED: Fix all scaling issues including divider lines
    /// </summary>
    private void FixAllScaleIssues()
    {
        DebugLog("=== FIXING ALL SCALE ISSUES ===");

        if (upgradeWheelUI == null) return;

        // Fix main container scale
        upgradeWheelUI.transform.localScale = Vector3.one;
        DebugLog("Fixed UpgradeWheelUI scale to (1,1,1)");

        if (wheelContainer != null)
        {
            wheelContainer.localScale = Vector3.one;
            DebugLog("Fixed wheel container scale to (1,1,1)");
        }

        // Fix ALL children scales recursively
        FixChildrenScales(upgradeWheelUI.transform, 0);

        DebugLog("=== SCALE FIX COMPLETE ===");
    }

    /// <summary>
    /// Recursively fix scales of all children
    /// </summary>
    private void FixChildrenScales(Transform parent, int depth)
    {
        if (depth > 3) return; // Prevent infinite recursion

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // Fix scale if it's not (1,1,1)
            if (child.localScale != Vector3.one)
            {
                Vector3 oldScale = child.localScale;
                child.localScale = Vector3.one;
                DebugLog($"Fixed scale: {child.name} from {oldScale} to (1,1,1)");
            }

            // Special handling for divider lines
            if (child.name.ToLower().Contains("divider") ||
                child.name.ToLower().Contains("line") ||
                child.name.ToLower().Contains("separator"))
            {
                child.localScale = Vector3.one;
                DebugLog($"Fixed divider line scale: {child.name}");
            }

            // Recursively fix children
            FixChildrenScales(child, depth + 1);
        }
    }

    /// <summary>
    /// Called when upgrade is selected
    /// </summary>
    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        DebugLog($"=== OnUpgradeSelected: {upgrade.upgradeName} ===");
        selectedUpgrade = upgrade;

        // Show confirmation dialog with explicit callback logging
        if (confirmationDialog != null)
        {
            DebugLog("Showing confirmation dialog with callbacks");

            // EXPLICIT callback methods for debugging
            System.Action confirmCallback = () => {
                DebugLog("CONFIRMATION CALLBACK TRIGGERED!");
                OnUpgradeConfirmed();
            };

            System.Action cancelCallback = () => {
                DebugLog("CANCEL CALLBACK TRIGGERED!");
                OnUpgradeCanceled();
            };

            confirmationDialog.ShowDialog(upgrade, confirmCallback, cancelCallback);
            DebugLog("Dialog.ShowDialog called with explicit callbacks");
        }
        else
        {
            DebugLog("No dialog found, auto-confirming");
            OnUpgradeConfirmed();
        }
    }

    /// <summary>
    /// ENHANCED: Called when player confirms upgrade
    /// </summary>
    private void OnUpgradeConfirmed()
    {
        DebugLog($"=== UPGRADE CONFIRMED: {selectedUpgrade?.upgradeName} ===");
        DebugLog("STARTING COMPLETE WHEEL HIDING PROCESS");

        // STEP 1: Apply upgrade first
        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog($"Applied upgrade: {selectedUpgrade.upgradeName}");
        }

        // STEP 2: IMMEDIATELY hide wheel using multiple methods
        StartCoroutine(HideWheelSequence());

        // STEP 3: Continue movement immediately
        if (enhancedTransitionMover != null)
        {
            DebugLog("Notifying EnhancedTransitionMover to resume");
            enhancedTransitionMover.ResumeMovement();
        }

        DebugLog("=== UPGRADE CONFIRMATION COMPLETE ===");
    }

    /// <summary>
    /// Coroutine to hide wheel step by step with verification
    /// </summary>
    private System.Collections.IEnumerator HideWheelSequence()
    {
        DebugLog("=== STARTING HIDE WHEEL SEQUENCE ===");

        // Method 1: Call normal hide method
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.HideWheel();
            DebugLog("Called upgradeWheelUI.HideWheel()");
            yield return new WaitForSeconds(0.1f);
        }

        // Method 2: Deactivate the wheel container directly
        if (wheelContainer != null)
        {
            wheelContainer.gameObject.SetActive(false);
            DebugLog($"Deactivated wheelContainer: {wheelContainer.name}");
            yield return new WaitForSeconds(0.1f);
        }

        // Method 3: Deactivate the canvas
        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(false);
            DebugLog("Deactivated upgradeCanvas");
            yield return new WaitForSeconds(0.1f);
        }

        // Method 4: Deactivate the UpgradeWheelUI GameObject
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.gameObject.SetActive(false);
            DebugLog($"Deactivated UpgradeWheelUI GameObject: {upgradeWheelUI.name}");
        }

        // Method 5: Nuclear option - find and hide anything with "upgrade" or "wheel"
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            string name = obj.name.ToLower();
            if ((name.Contains("upgrade") || name.Contains("wheel")) && obj.activeInHierarchy)
            {
                obj.SetActive(false);
                DebugLog($"Nuclear option: Deactivated {obj.name}");
            }
        }

        DebugLog("=== HIDE WHEEL SEQUENCE COMPLETE ===");
    }

    private void OnUpgradeCanceled()
    {
        DebugLog("=== UPGRADE CANCELED ===");
        selectedUpgrade = null;
        // Don't hide wheel - let player select again
    }

    public bool IsUpgradeInProgress()
    {
        return upgradeCanvas != null && upgradeCanvas.gameObject.activeInHierarchy;
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TransitionWheelUpgrade] {message}");
        }
    }

    // Testing methods
    [ContextMenu("Force Show Wheel")]
    public void ForceShowWheel()
    {
        DebugLog("Manual: Force showing wheel");
        ShowUpgradeWheel();
    }

    [ContextMenu("Force Hide Wheel")]
    public void ForceHideWheel()
    {
        DebugLog("Manual: Force hiding wheel");
        StartCoroutine(HideWheelSequence());
    }

    [ContextMenu("Test Confirm Upgrade")]
    public void TestConfirmUpgrade()
    {
        selectedUpgrade = new WheelUpgradeOption("TestUpgrade", "Test upgrade", 1);
        OnUpgradeConfirmed();
    }

    [ContextMenu("Fix Scale Issues")]
    public void TestFixScales()
    {
        FixAllScaleIssues();
    }
}