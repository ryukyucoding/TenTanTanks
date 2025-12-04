using UnityEngine;
using System.Collections;
using WheelUpgradeSystem;

/// <summary>
/// Handles transition upgrade integration with the existing UpgradeWheelUI system
/// Supports tier filtering and confirmation for Level 2¡÷3 and Level 4¡÷5 transitions
/// </summary>
public class TransitionWheelUpgrade : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private EnhancedTransitionMover transitionMover;

    [Header("Tank Models")]
    [SerializeField] private GameObject doubleheadModel;
    [SerializeField] private GameObject fourheadModel;
    [SerializeField] private GameObject hugeModel;
    [SerializeField] private GameObject smallModel;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Private variables
    private bool isTransitionMode = false;
    private string currentTransitionType = "";
    private WheelUpgradeOption selectedUpgrade;

    void Start()
    {
        FindComponents();
        SetupWheelForTransition();
    }

    /// <summary>
    /// Find required components automatically if not assigned
    /// </summary>
    private void FindComponents()
    {
        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();

        if (tankUpgradeSystem == null)
            tankUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();

        if (upgradeCanvas == null)
            upgradeCanvas = FindFirstObjectByType<Canvas>();

        if (transitionMover == null)
            transitionMover = FindFirstObjectByType<EnhancedTransitionMover>();

        DebugLog("Component search complete:");
        DebugLog($"  - UpgradeWheelUI: {(upgradeWheelUI != null ? "Found" : "Missing")}");
        DebugLog($"  - TankUpgradeSystem: {(tankUpgradeSystem != null ? "Found" : "Missing")}");
        DebugLog($"  - UpgradeCanvas: {(upgradeCanvas != null ? "Found" : "Missing")}");
        DebugLog($"  - TransitionMover: {(transitionMover != null ? "Found" : "Missing")}");
    }

    /// <summary>
    /// Setup the wheel for transition-specific behavior
    /// </summary>
    private void SetupWheelForTransition()
    {
        if (upgradeWheelUI != null)
        {
            DebugLog("Setting up wheel for transition mode");
        }
    }

    /// <summary>
    /// Show upgrade panel for transition (called by EnhancedTransitionMover)
    /// </summary>
    public void ShowUpgradePanel(string transitionType = "Level2To3")
    {
        currentTransitionType = transitionType;
        isTransitionMode = true;

        DebugLog("Showing upgrade panel for: " + transitionType);

        if (upgradeWheelUI != null)
        {
            StartCoroutine(ShowWheelWithDelay());
        }
        else
        {
            DebugLog("UpgradeWheelUI not found!");
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

        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(true);

        // IMPORTANT: Set up transition-specific mode BEFORE showing the wheel
        SetupTransitionMode();

        // NOW show the wheel (it will use transition mode)
        upgradeWheelUI.ShowWheel();

        DebugLog("Upgrade wheel should now be visible with transition mode applied");
    }

    /// <summary>
    /// Setup the wheel for transition-specific behavior
    /// </summary>
    private void SetupTransitionMode()
    {
        if (upgradeWheelUI == null) return;

        if (currentTransitionType == "Level2To3")
        {
            DebugLog("Configuring for Level 2¡÷3 transition (First tier upgrades only)");
            SetWheelTransitionMode(1, null);
        }
        else if (currentTransitionType == "Level4To5")
        {
            DebugLog("Configuring for Level 4¡÷5 transition (Second tier upgrades only)");
            string previousChoice = GetPreviousTier1Choice();
            SetWheelTransitionMode(2, previousChoice);
        }
    }

    /// <summary>
    /// Set the wheel to transition mode with specific tier filtering
    /// </summary>
    private void SetWheelTransitionMode(int allowedTier, string parentUpgrade = null)
    {
        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.SetTransitionMode(allowedTier, parentUpgrade);
            DebugLog($"Set wheel to transition mode: Tier {allowedTier}, Parent: {parentUpgrade}");
        }
        else
        {
            DebugLog("UpgradeWheelUI is null, cannot set transition mode");
        }
    }

    /// <summary>
    /// Handle upgrade selection (called by UpgradeWheelUI when player selects)
    /// </summary>
    public void OnUpgradeSelected(WheelUpgradeOption upgrade)
    {
        selectedUpgrade = upgrade;

        DebugLog("Upgrade selected: " + upgrade.upgradeName);

        // Hide the wheel first
        if (upgradeWheelUI != null)
            upgradeWheelUI.HideWheel();

        // Show confirmation dialog
        ShowConfirmationDialog(upgrade);
    }

    /// <summary>
    /// Show confirmation dialog for the selected upgrade
    /// </summary>
    private void ShowConfirmationDialog(WheelUpgradeOption upgrade)
    {
        var confirmationDialog = FindFirstObjectByType<TransitionConfirmationDialog>();

        if (confirmationDialog != null)
        {
            confirmationDialog.ShowDialog(
                upgrade,
                () => ConfirmUpgradeChoice(),
                () => CancelUpgradeChoice()
            );
        }
        else
        {
            DebugLog("No TransitionConfirmationDialog found, using simple confirmation");
            ShowSimpleConfirmation(upgrade);
        }
    }

    /// <summary>
    /// Simple fallback confirmation system
    /// </summary>
    private void ShowSimpleConfirmation(WheelUpgradeOption upgrade)
    {
        string message = $"Choose '{upgrade.upgradeName}'?\n\n{upgrade.description}\n\nThis choice will affect your tank for the rest of the game.";

        DebugLog("Confirmation: " + message);
        StartCoroutine(AutoConfirmAfterDelay(2f));
    }

    /// <summary>
    /// Auto-confirm after delay (for testing without dialog UI)
    /// </summary>
    private System.Collections.IEnumerator AutoConfirmAfterDelay(float delay)
    {
        DebugLog($"Auto-confirming in {delay} seconds...");
        yield return new WaitForSeconds(delay);

        DebugLog("Auto-confirmed!");
        ConfirmUpgradeChoice();
    }

    /// <summary>
    /// Confirm the upgrade choice
    /// </summary>
    public void ConfirmUpgradeChoice()
    {
        if (selectedUpgrade == null)
        {
            DebugLog("No upgrade selected for confirmation!");
            return;
        }

        DebugLog("Confirming upgrade: " + selectedUpgrade.upgradeName);

        // Save the choice if this is a tier 1 upgrade
        if (currentTransitionType == "Level2To3")
        {
            SaveTier1Choice(selectedUpgrade.upgradeName);
        }

        // Apply the upgrade
        ApplyUpgrade(selectedUpgrade);

        // Apply visual transformation
        ApplyTankModelTransformation(selectedUpgrade);

        // Continue transition after small delay
        StartCoroutine(ContinueTransitionAfterDelay());
    }

    /// <summary>
    /// Cancel the upgrade choice
    /// </summary>
    public void CancelUpgradeChoice()
    {
        DebugLog("Upgrade choice canceled, showing wheel again");

        selectedUpgrade = null;

        if (upgradeWheelUI != null)
        {
            StartCoroutine(ShowWheelAfterDelay());
        }
    }

    /// <summary>
    /// Show wheel again after small delay
    /// </summary>
    private IEnumerator ShowWheelAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        upgradeWheelUI.ShowWheel();
        SetupTransitionMode();
    }

    /// <summary>
    /// Save tier 1 choice for Level 4¡÷5 use
    /// </summary>
    private void SaveTier1Choice(string upgradeName)
    {
        if (PlayerDataManager.Instance != null)
        {
            // TODO: Add this method to PlayerDataManager
            // PlayerDataManager.Instance.SaveUpgradeChoice(upgradeName);
            DebugLog("Saved tier 1 choice: " + upgradeName);
        }
    }

    /// <summary>
    /// Get the player's previous tier 1 upgrade choice
    /// </summary>
    private string GetPreviousTier1Choice()
    {
        if (PlayerDataManager.Instance != null)
        {
            // TODO: Get from PlayerDataManager
            // For testing, return default
            return "Heavy";
        }

        return "Heavy"; // Default for testing
    }

    /// <summary>
    /// Apply the selected upgrade to the tank system
    /// </summary>
    private void ApplyUpgrade(WheelUpgradeOption upgrade)
    {
        if (tankUpgradeSystem != null)
        {
            tankUpgradeSystem.ApplyUpgrade(upgrade.upgradeName);
            DebugLog("Applied upgrade to tank system: " + upgrade.upgradeName);
        }
        else
        {
            DebugLog("TankUpgradeSystem not found!");
        }
    }

    /// <summary>
    /// Apply visual tank model transformation
    /// </summary>
    private void ApplyTankModelTransformation(WheelUpgradeOption upgrade)
    {
        HideAllTankModels();

        string upgradeName = upgrade.upgradeName.ToLower();

        if (upgradeName.Contains("doublehead") && doubleheadModel != null)
        {
            doubleheadModel.SetActive(true);
            DebugLog("Switched to doublehead model");
        }
        else if (upgradeName.Contains("fourhead") && fourheadModel != null)
        {
            fourheadModel.SetActive(true);
            DebugLog("Switched to fourhead model");
        }
        else if (upgradeName.Contains("huge") && hugeModel != null)
        {
            hugeModel.SetActive(true);
            DebugLog("Switched to huge model");
        }
        else if (upgradeName.Contains("small") && smallModel != null)
        {
            smallModel.SetActive(true);
            DebugLog("Switched to small model");
        }
        else
        {
            DebugLog("No matching tank model found for: " + upgradeName);
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
    /// Continue transition after delay
    /// </summary>
    private IEnumerator ContinueTransitionAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        isTransitionMode = false;

        if (transitionMover != null)
        {
            transitionMover.ResumeMovement();
            DebugLog("Tank movement resumed");
        }
        else
        {
            DebugLog("TransitionMover not found!");
        }
    }

    /// <summary>
    /// Debug logging with prefix
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[TransitionWheelUpgrade] {message}");
    }

    // Debug methods for testing
    [ContextMenu("Debug Show Level 2¡÷3")]
    public void DebugShowLevel2To3()
    {
        ShowUpgradePanel("Level2To3");
    }

    [ContextMenu("Debug Show Level 4¡÷5")]
    public void DebugShowLevel4To5()
    {
        ShowUpgradePanel("Level4To5");
    }

    [ContextMenu("Debug Check References")]
    public void DebugCheckReferences()
    {
        DebugLog("=== Component Reference Check ===");
        DebugLog($"UpgradeWheelUI: {(upgradeWheelUI != null ? "Y" : "N")}");
        DebugLog($"TankUpgradeSystem: {(tankUpgradeSystem != null ? "Y" : "N")}");
        DebugLog($"UpgradeCanvas: {(upgradeCanvas != null ? "Y" : "N")}");
        DebugLog($"TransitionMover: {(transitionMover != null ? "Y" : "N")}");
        DebugLog($"DoubleheadModel: {(doubleheadModel != null ? "Y" : "N")}");
        DebugLog($"FourheadModel: {(fourheadModel != null ? "Y" : "N")}");
        DebugLog($"HugeModel: {(hugeModel != null ? "Y" : "N")}");
        DebugLog($"SmallModel: {(smallModel != null ? "Y" : "N")}");
    }
}