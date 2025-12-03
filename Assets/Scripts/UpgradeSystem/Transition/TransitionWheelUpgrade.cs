using UnityEngine;
using UnityEngine.UI;
using WheelUpgradeSystem;
using TMPro;
using System.Collections;

/// <summary>
/// Transition Wheel Upgrade - Integrates with existing UpgradeWheelUI system
/// Shows the upgrade wheel during transition and handles tank model switching
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Existing Wheel System References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;
    [SerializeField] private Canvas upgradeCanvas;

    [Header("Tank Model References")]
    [SerializeField] private GameObject doubleheadModel;  // doublehead tank
    [SerializeField] private GameObject fourheadModel;    // fourhead tank
    [SerializeField] private GameObject hugeModel;        // HUGE tank
    [SerializeField] private GameObject smallModel;       // SMALL tank

    [Header("Transition Settings")]
    [SerializeField] private bool useWheelForTransition = true;

    private EnhancedTransitionMover transitionMover;
    private string currentTransitionType;
    private bool isTransitionMode = false;
    private WheelUpgradeOption selectedUpgrade;

    void Start()
    {
        FindComponents();
        SetupWheelForTransition();
    }

    /// <summary>
    /// Find required components automatically
    /// </summary>
    private void FindComponents()
    {
        // Find transition mover
        transitionMover = FindFirstObjectByType<EnhancedTransitionMover>();

        // Find upgrade wheel UI if not assigned
        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();

        // Find tank upgrade system if not assigned  
        if (tankUpgradeSystem == null)
            tankUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();

        // Find upgrade canvas if not assigned
        if (upgradeCanvas == null && upgradeWheelUI != null)
        {
            // Try to get canvas from the UpgradeWheelUI's game object hierarchy
            upgradeCanvas = upgradeWheelUI.GetComponentInParent<Canvas>();
        }

        Debug.Log("[TransitionWheelUpgrade] Component search complete:");
        Debug.Log("  - UpgradeWheelUI: " + (upgradeWheelUI != null ? "Found" : "Missing"));
        Debug.Log("  - TankUpgradeSystem: " + (tankUpgradeSystem != null ? "Found" : "Missing"));
        Debug.Log("  - UpgradeCanvas: " + (upgradeCanvas != null ? "Found" : "Missing"));
        Debug.Log("  - TransitionMover: " + (transitionMover != null ? "Found" : "Missing"));
    }

    /// <summary>
    /// Setup the wheel for transition use
    /// </summary>
    private void SetupWheelForTransition()
    {
        if (upgradeWheelUI != null)
        {
            Debug.Log("[TransitionWheelUpgrade] Setting up wheel for transition mode");
        }
    }

    /// <summary>
    /// Show upgrade panel for transition (called by EnhancedTransitionMover)
    /// </summary>
    public void ShowUpgradePanel(string transitionType = "Level2To3")
    {
        currentTransitionType = transitionType;
        isTransitionMode = true;

        Debug.Log("[TransitionWheelUpgrade] Showing upgrade panel for: " + transitionType);

        if (upgradeWheelUI != null)
        {
            // Show the existing upgrade wheel
            StartCoroutine(ShowWheelWithDelay());
        }
        else
        {
            Debug.LogError("[TransitionWheelUpgrade] UpgradeWheelUI not found!");
            // Fall back to continuing transition
            if (transitionMover != null)
                transitionMover.ResumeMovement();
        }
    }

    /// <summary>
    /// Show wheel with small delay for stability
    /// </summary>
    private IEnumerator ShowWheelWithDelay()
    {
        yield return new WaitForSeconds(0.2f);

        // Make sure canvas is active
        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(true);

        // Show the wheel using the existing system
        upgradeWheelUI.ShowWheel();

        Debug.Log("[TransitionWheelUpgrade] Upgrade wheel should now be visible");

        // Set up transition-specific mode
        SetupTransitionMode();
    }

    /// <summary>
    /// Setup the wheel for transition-specific behavior
    /// </summary>
    private void SetupTransitionMode()
    {
        // Filter upgrade options based on transition type
        if (currentTransitionType == "Level2To3")
        {
            Debug.Log("[TransitionWheelUpgrade] Configuring for Level 2¡÷3 transition (First tier upgrades)");
            // Show only tier 1 upgrades: doublehead, HUGE, SMALL
        }
        else if (currentTransitionType == "Level4To5")
        {
            Debug.Log("[TransitionWheelUpgrade] Configuring for Level 4¡÷5 transition (Second tier upgrades)");
            // Show tier 2 upgrades based on previous selection
        }

        // Subscribe to upgrade selection events if available
        // Note: This depends on your existing UpgradeWheelUI implementation
    }

    /// <summary>
    /// Handle upgrade selection (this should be called when player selects an upgrade)
    /// You may need to modify your existing UpgradeWheelUI to call this
    /// </summary>
    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        selectedUpgrade = upgrade;

        Debug.Log("[TransitionWheelUpgrade] Upgrade selected: " + upgrade.upgradeName);

        // Apply the upgrade
        ApplyUpgrade(upgrade);

        // Apply visual transformation
        ApplyTankModelTransformation(upgrade);

        // Hide the wheel
        if (upgradeWheelUI != null)
            upgradeWheelUI.HideWheel();

        // Continue transition after small delay
        StartCoroutine(ContinueTransitionAfterDelay());
    }

    /// <summary>
    /// Apply the upgrade to the tank upgrade system
    /// </summary>
    private void ApplyUpgrade(WheelUpgradeOption upgrade)
    {
        if (tankUpgradeSystem != null && upgrade != null)
        {
            tankUpgradeSystem.ApplyUpgrade(upgrade.upgradeName);
            Debug.Log("[TransitionWheelUpgrade] Applied upgrade to system: " + upgrade.upgradeName);
        }
    }

    /// <summary>
    /// Apply tank model transformation based on upgrade
    /// </summary>
    private void ApplyTankModelTransformation(WheelUpgradeOption upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogWarning("[TransitionWheelUpgrade] Cannot apply tank transformation: upgrade is null");
            return;
        }

        Debug.Log("[TransitionWheelUpgrade] Applying tank model transformation: " + upgrade.upgradeName);

        // Hide all tank models first
        HideAllTankModels();

        // Show appropriate model based on upgrade
        switch (upgrade.upgradeName.ToLower())
        {
            case "doublehead":
                if (doubleheadModel != null)
                {
                    doubleheadModel.SetActive(true);
                    Debug.Log("[TransitionWheelUpgrade] Switched to doublehead model");
                }
                break;

            case "fourhead":
            case "fourhead_front_back":
            case "fourhead_cross":
                if (fourheadModel != null)
                {
                    fourheadModel.SetActive(true);
                    Debug.Log("[TransitionWheelUpgrade] Switched to fourhead model");
                }
                break;

            case "huge":
            case "huge_triple_front":
            case "huge_triple_120":
                if (hugeModel != null)
                {
                    hugeModel.SetActive(true);
                    Debug.Log("[TransitionWheelUpgrade] Switched to HUGE model");
                }
                break;

            case "small":
            case "small_triple_front":
            case "small_triple_120":
                if (smallModel != null)
                {
                    smallModel.SetActive(true);
                    Debug.Log("[TransitionWheelUpgrade] Switched to SMALL model");
                }
                break;

            default:
                Debug.LogWarning("[TransitionWheelUpgrade] Unknown upgrade type: " + upgrade.upgradeName);
                break;
        }
    }

    /// <summary>
    /// Hide all tank models
    /// </summary>
    private void HideAllTankModels()
    {
        if (doubleheadModel != null) doubleheadModel.SetActive(false);
        if (fourheadModel != null) fourheadModel.SetActive(false);
        if (hugeModel != null) hugeModel.SetActive(false);
        if (smallModel != null) smallModel.SetActive(false);
    }

    /// <summary>
    /// Continue transition after upgrade is applied
    /// </summary>
    private IEnumerator ContinueTransitionAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        isTransitionMode = false;

        if (transitionMover != null)
        {
            transitionMover.ResumeMovement();
            Debug.Log("[TransitionWheelUpgrade] Transition resumed");
        }
    }

    /// <summary>
    /// Called when wheel is closed without selection (fallback)
    /// </summary>
    public void OnWheelClosed()
    {
        if (isTransitionMode)
        {
            Debug.Log("[TransitionWheelUpgrade] Wheel closed during transition, continuing without upgrade");
            if (transitionMover != null)
                transitionMover.ResumeMovement();
        }
    }

    // Debug methods
    [ContextMenu("Test Show Upgrade Panel Level2To3")]
    public void DebugShowLevel2To3()
    {
        ShowUpgradePanel("Level2To3");
    }

    [ContextMenu("Test Show Upgrade Panel Level4To5")]
    public void DebugShowLevel4To5()
    {
        ShowUpgradePanel("Level4To5");
    }

    [ContextMenu("Check Component References")]
    public void DebugCheckReferences()
    {
        Debug.Log("=== TransitionWheelUpgrade References ===");
        Debug.Log("UpgradeWheelUI: " + (upgradeWheelUI != null ? "Connected" : "Missing"));
        Debug.Log("TankUpgradeSystem: " + (tankUpgradeSystem != null ? "Connected" : "Missing"));
        Debug.Log("UpgradeCanvas: " + (upgradeCanvas != null ? "Connected" : "Missing"));
        Debug.Log("TransitionMover: " + (transitionMover != null ? "Found" : "Missing"));
        Debug.Log("Tank Models:");
        Debug.Log("  - Doublehead: " + (doubleheadModel != null ? "Y" : "N"));
        Debug.Log("  - Fourhead: " + (fourheadModel != null ? "Y" : "N"));
        Debug.Log("  - HUGE: " + (hugeModel != null ? "Y" : "N"));
        Debug.Log("  - SMALL: " + (smallModel != null ? "Y" : "N"));
    }

    [ContextMenu("Test Tank Model Switch - Doublehead")]
    public void DebugSwitchToDoublehead()
    {
        var testUpgrade = new WheelUpgradeOption { upgradeName = "doublehead" };
        ApplyTankModelTransformation(testUpgrade);
    }

    [ContextMenu("Test Tank Model Switch - HUGE")]
    public void DebugSwitchToHuge()
    {
        var testUpgrade = new WheelUpgradeOption { upgradeName = "HUGE" };
        ApplyTankModelTransformation(testUpgrade);
    }
}