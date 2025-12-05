using UnityEngine;
using WheelUpgradeSystem;

/// <summary>
/// FINAL COMPLETE FIX - Solves both wheel hiding and Input System errors
/// This version ensures wheel disappears after YES and removes Input errors
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
    private Transform wheelContainer; // Cache the wheel container reference

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
    /// Show the upgrade wheel - ensures all GameObjects are active
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

        // STEP 3: Find and activate UpgradeWheelContainer specifically
        FindAndActivateWheelContainer();

        // STEP 4: Now try to show the wheel
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

        // Look for UpgradeWheelContainer child
        wheelContainer = null;

        // Try different possible names
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

            // Emergency: Activate ALL children of UpgradeWheelUI
            for (int i = 0; i < upgradeWheelUI.transform.childCount; i++)
            {
                var child = upgradeWheelUI.transform.GetChild(i);
                if (!child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(true);
                    DebugLog($"Emergency activated child: {child.name}");
                }
            }
        }
    }

    /// <summary>
    /// Called when upgrade is selected
    /// </summary>
    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        DebugLog($"OnUpgradeSelected: {upgrade.upgradeName}");
        selectedUpgrade = upgrade;

        // Show confirmation dialog
        if (confirmationDialog != null)
        {
            DebugLog("Showing confirmation dialog");
            confirmationDialog.ShowDialog(upgrade, OnUpgradeConfirmed, OnUpgradeCanceled);
        }
        else
        {
            DebugLog("No dialog found, auto-confirming");
            OnUpgradeConfirmed();
        }
    }

    /// <summary>
    /// FIXED: Called when player confirms upgrade - ensures wheel is hidden
    /// </summary>
    private void OnUpgradeConfirmed()
    {
        DebugLog($"=== UPGRADE CONFIRMED: {selectedUpgrade?.upgradeName} ===");
        DebugLog("NOW HIDING WHEEL COMPLETELY");

        // STEP 1: Apply upgrade first
        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            DebugLog($"Applied upgrade: {selectedUpgrade.upgradeName}");
        }

        // STEP 2: FORCE HIDE wheel using multiple methods
        ForceHideWheelCompletely();

        // STEP 3: Continue movement
        if (enhancedTransitionMover != null)
        {
            DebugLog("Notifying EnhancedTransitionMover to resume");
            enhancedTransitionMover.ResumeMovement();
        }

        DebugLog("=== UPGRADE CONFIRMATION COMPLETE ===");
    }

    /// <summary>
    /// ENHANCED: Force hide wheel using every possible method
    /// </summary>
    private void ForceHideWheelCompletely()
    {
        DebugLog("=== FORCE HIDING WHEEL COMPLETELY ===");

        // Method 1: Call normal hide method
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.HideWheel();
            DebugLog("Called upgradeWheelUI.HideWheel()");
        }

        // Method 2: Deactivate the canvas
        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(false);
            DebugLog("Deactivated upgradeCanvas");
        }

        // Method 3: Deactivate the wheel container directly
        if (wheelContainer != null)
        {
            wheelContainer.gameObject.SetActive(false);
            DebugLog($"Deactivated wheelContainer: {wheelContainer.name}");
        }

        // Method 4: Deactivate the UpgradeWheelUI GameObject
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.gameObject.SetActive(false);
            DebugLog($"Deactivated UpgradeWheelUI GameObject: {upgradeWheelUI.name}");
        }

        // Method 5: Find and deactivate ANY GameObject with "upgrade" or "wheel"
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            string name = obj.name.ToLower();
            if ((name.Contains("upgrade") || name.Contains("wheel")) &&
                obj.activeInHierarchy &&
                obj != upgradeWheelUI?.gameObject) // Don't double-deactivate
            {
                obj.SetActive(false);
                DebugLog($"Force deactivated: {obj.name}");
            }
        }

        DebugLog("=== FORCE HIDE COMPLETE ===");
    }

    private void OnUpgradeCanceled()
    {
        DebugLog("Upgrade canceled");
        selectedUpgrade = null;
        // Don't hide wheel - let player select again
    }

    /// <summary>
    /// Manual hide method for testing
    /// </summary>
    private void HideWheel()
    {
        DebugLog("Manual hide wheel called");
        ForceHideWheelCompletely();
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

    // Testing methods - NO INPUT SYSTEM ERRORS
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
        ForceHideWheelCompletely();
    }

    [ContextMenu("Test Confirm Upgrade")]
    public void TestConfirmUpgrade()
    {
        selectedUpgrade = new WheelUpgradeOption("TestUpgrade", "Test upgrade", 1);
        OnUpgradeConfirmed();
    }

    // REMOVED: All Input.GetKeyDown calls that caused Input System errors
    // Use Context Menu methods for manual testing instead
}