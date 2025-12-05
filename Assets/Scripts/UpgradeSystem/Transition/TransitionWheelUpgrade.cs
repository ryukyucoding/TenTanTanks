using UnityEngine;
using WheelUpgradeSystem;

/// <summary>
/// TARGETED FIX for inactive UpgradeWheelContainer
/// This version specifically fixes the "Coroutine couldn't be started" error
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;
    [SerializeField] private SimpleTransitionDialog confirmationDialog;

    [Header("Testing")]
    [SerializeField] private bool enableDebugLogs = true;

    private WheelUpgradeOption selectedUpgrade;
    private EnhancedTransitionMover enhancedTransitionMover;

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
    /// FIXED VERSION: Ensures all GameObjects are active before showing wheel
    /// </summary>
    private void ShowUpgradeWheel()
    {
        DebugLog("=== ShowUpgradeWheel - FIXED VERSION ===");

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
        if (upgradeWheelUI != null)
        {
            // Look for UpgradeWheelContainer child
            Transform wheelContainer = null;

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

        // STEP 5: Verify wheel is now visible
        Invoke(nameof(VerifyWheelVisibility), 1f);
    }

    private void VerifyWheelVisibility()
    {
        DebugLog("=== VERIFYING WHEEL VISIBILITY ===");

        bool isVisible = false;

        if (upgradeCanvas != null && upgradeCanvas.gameObject.activeInHierarchy)
        {
            DebugLog($"Canvas is active: {upgradeCanvas.name}");
            isVisible = true;
        }

        if (upgradeWheelUI != null && upgradeWheelUI.gameObject.activeInHierarchy)
        {
            DebugLog($"UpgradeWheelUI is active: {upgradeWheelUI.name}");

            // Check if UpgradeWheelContainer is now active
            Transform wheelContainer = upgradeWheelUI.transform.Find("UpgradeWheelContainer");
            if (wheelContainer == null)
            {
                // Try alternative names
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

            if (wheelContainer != null)
            {
                if (wheelContainer.gameObject.activeInHierarchy)
                {
                    DebugLog($"WHEEL CONTAINER IS NOW ACTIVE: {wheelContainer.name}");
                    isVisible = true;
                }
                else
                {
                    DebugLog($"Wheel container still inactive: {wheelContainer.name}");
                }
            }
        }

        if (!isVisible)
        {
            DebugLog("WHEEL STILL NOT VISIBLE - Need to investigate further");
        }
        else
        {
            DebugLog("SUCCESS: WHEEL IS NOW VISIBLE!");
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
            confirmationDialog.ShowDialog(upgrade, OnUpgradeConfirmed, OnUpgradeCanceled);
        }
        else
        {
            DebugLog("No dialog found, auto-confirming");
            OnUpgradeConfirmed();
        }
    }

    private void OnUpgradeConfirmed()
    {
        DebugLog($"Upgrade confirmed: {selectedUpgrade?.upgradeName}");

        // Apply upgrade
        if (tankUpgradeSystem != null && selectedUpgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
        }

        // Hide wheel
        HideWheel();

        // Continue movement
        if (enhancedTransitionMover != null)
        {
            enhancedTransitionMover.ResumeMovement();
        }
    }

    private void OnUpgradeCanceled()
    {
        DebugLog("Upgrade canceled");
        selectedUpgrade = null;
    }

    private void HideWheel()
    {
        DebugLog("Hiding wheel");

        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.HideWheel();
        }

        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(false);
        }
    }

    public bool IsUpgradeInProgress()
    {
        // Simple check
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
        ShowUpgradeWheel();
    }

    [ContextMenu("Force Hide Wheel")]
    public void ForceHideWheel()
    {
        HideWheel();
    }

    void Update()
    {
        // Press SPACE to test show wheel
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DebugLog("SPACE pressed - showing wheel");
            ShowUpgradeWheel();
        }
    }
}