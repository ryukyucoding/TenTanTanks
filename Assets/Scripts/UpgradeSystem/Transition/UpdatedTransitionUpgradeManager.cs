using UnityEngine;
using UnityEngine.UI;
using WheelUpgradeSystem;
using TMPro;
using System.Collections;

/// <summary>
/// Transition Upgrade Manager - Displays upgrade wheel during scene transitions
/// Provides upgrade options when transitioning between Level 2 to 3 and 4 to 5
/// </summary>
public class TransitionUpgradeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Button confirmUpgradeButton;
    [SerializeField] private Button cancelUpgradeButton;
    [SerializeField] private TextMeshProUGUI confirmationText;

    [Header("Tank References")]
    [SerializeField] private Transform playerTank;
    [SerializeField] private ModularTankController modularTankController;

    [Header("Tank Model References")]
    [SerializeField] private GameObject armTankModel;     // ArmTank.gltf
    [SerializeField] private GameObject baseModel;        // Base.gltf
    [SerializeField] private GameObject barrelModel;      // Barrel.gltf
    [SerializeField] private GameObject doubleheadModel;  // doublehead
    [SerializeField] private GameObject fourheadModel;    // fourhead
    [SerializeField] private GameObject hugeModel;        // HUGE
    [SerializeField] private GameObject smallModel;       // SMALL

    [Header("Upgrade Configuration")]
    [SerializeField] private bool enableTransitionUpgrades = true;
    [SerializeField] private float pauseAtCenterX = 0f;
    [SerializeField] private float pauseDetectionRange = 1f;

    private TankUpgradeSystem upgradeSystem;
    private TransitionMover transitionMover;
    private WheelUpgradeOption selectedUpgrade;
    private string targetScene;
    private bool isUpgradeInProgress = false;
    private bool upgradeCompleted = false;

    // Transition type
    private enum TransitionType
    {
        Level2To3,  // Basic to First Evolution (doublehead, HUGE, SMALL)
        Level4To5   // First Evolution to Second Evolution (fourhead, HUGE-3turrets, SMALL-3turrets)
    }
    private TransitionType currentTransitionType;

    void Awake()
    {
        FindRequiredComponents();
    }

    void Start()
    {
        DetermineTransitionType();
        SetupConfirmationDialog();
        HideAllUpgradeUI();
        Debug.Log($"[TransitionUpgradeManager] Initialization complete, transition type: {currentTransitionType}");
    }

    void Update()
    {
        if (!enableTransitionUpgrades || isUpgradeInProgress || upgradeCompleted)
            return;

        CheckTankPosition();
    }

    /// <summary>
    /// Find required components
    /// </summary>
    private void FindRequiredComponents()
    {
        upgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();
        transitionMover = FindFirstObjectByType<TransitionMover>();

        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();

        if (playerTank == null)
        {
            GameObject tank = GameObject.FindGameObjectWithTag("Player");
            if (tank != null) playerTank = tank.transform;
        }

        if (modularTankController == null && playerTank != null)
            modularTankController = playerTank.GetComponent<ModularTankController>();
    }

    /// <summary>
    /// Determine transition type based on target scene
    /// </summary>
    private void DetermineTransitionType()
    {
        targetScene = SceneTransitionManager.GetNextSceneName();

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[TransitionUpgradeManager] Unable to get target scene name");
            return;
        }

        if (targetScene == "Level3")
        {
            currentTransitionType = TransitionType.Level2To3;
            Debug.Log("[TransitionUpgradeManager] Set to Level2 to 3 transition");
        }
        else if (targetScene == "Level5")
        {
            currentTransitionType = TransitionType.Level4To5;
            Debug.Log("[TransitionUpgradeManager] Set to Level4 to 5 transition");
        }
        else
        {
            enableTransitionUpgrades = false;
            Debug.Log($"[TransitionUpgradeManager] Scene '{targetScene}' does not require transition upgrade");
        }
    }

    /// <summary>
    /// Check tank position
    /// </summary>
    private void CheckTankPosition()
    {
        if (playerTank == null) return;

        float distanceToCenter = Mathf.Abs(playerTank.position.x - pauseAtCenterX);

        if (distanceToCenter <= pauseDetectionRange)
        {
            TriggerTransitionUpgrade();
        }
    }

    /// <summary>
    /// Trigger transition upgrade
    /// </summary>
    public void TriggerTransitionUpgrade()
    {
        if (isUpgradeInProgress) return;

        Debug.Log("[TransitionUpgradeManager] Triggering transition upgrade");

        isUpgradeInProgress = true;

        // Pause tank movement
        if (transitionMover != null)
            transitionMover.enabled = false;

        // Show upgrade wheel
        StartCoroutine(ShowTransitionUpgradeWheel());
    }

    /// <summary>
    /// Show transition upgrade wheel
    /// </summary>
    private IEnumerator ShowTransitionUpgradeWheel()
    {
        yield return new WaitForSeconds(0.5f);

        if (upgradeWheelUI != null && upgradeSystem != null)
        {
            // Setup upgrade options for current transition
            SetupTransitionUpgradeOptions();

            // Show upgrade wheel
            upgradeWheelUI.ShowWheel();

            Debug.Log("[TransitionUpgradeManager] Upgrade wheel displayed");
        }
        else
        {
            Debug.LogError("[TransitionUpgradeManager] Missing upgrade-related components!");
            ResumeTransition();
        }
    }

    /// <summary>
    /// Setup transition upgrade options
    /// </summary>
    private void SetupTransitionUpgradeOptions()
    {
        if (upgradeSystem == null) return;

        // Configure different upgrade options based on transition type
        switch (currentTransitionType)
        {
            case TransitionType.Level2To3:
                ConfigureLevelTwoToThreeUpgrades();
                break;

            case TransitionType.Level4To5:
                ConfigureLevelFourToFiveUpgrades();
                break;
        }
    }

    /// <summary>
    /// Configure Level 2 to 3 upgrade options
    /// </summary>
    private void ConfigureLevelTwoToThreeUpgrades()
    {
        Debug.Log("[TransitionUpgradeManager] Configuring Level 2 to 3 upgrade options");

        // Need to call TankUpgradeSystem to provide specific upgrade options
        // Or configure them in UpgradeWheelUI

        // Level 2 to 3 has three options:
        // 1. doublehead (middle path) - Keep original turret, add one more
        // 2. HUGE (bigger barrel) - Larger turret, higher damage
        // 3. SMALL (smaller barrel) - Smaller turret, faster fire rate
    }

    /// <summary>
    /// Configure Level 4 to 5 upgrade options
    /// </summary>
    private void ConfigureLevelFourToFiveUpgrades()
    {
        Debug.Log("[TransitionUpgradeManager] Configuring Level 4 to 5 upgrade options");

        // Level 4 to 5 provides different options based on current selection:
        // doublehead -> fourhead (no configuration needed)
        // HUGE -> 3 large turrets configuration
        // SMALL -> 3 small turrets configuration
    }

    /// <summary>
    /// Called when player selects an upgrade option
    /// </summary>
    public void OnTransitionUpgradeSelected(WheelUpgradeOption upgrade)
    {
        selectedUpgrade = upgrade;

        // Hide upgrade wheel
        if (upgradeWheelUI != null)
            upgradeWheelUI.HideWheel();

        // Show confirmation dialog
        ShowConfirmationDialog(upgrade);
    }

    /// <summary>
    /// Show confirmation dialog
    /// </summary>
    private void ShowConfirmationDialog(WheelUpgradeOption upgrade)
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);

            if (confirmationText != null)
            {
                confirmationText.text = $"Confirm selection of '{upgrade.upgradeName}' upgrade?\n\n{upgrade.description}";
            }

            Debug.Log($"[TransitionUpgradeManager] Showing confirmation dialog: {upgrade.upgradeName}");
        }
        else
        {
            // If no confirmation dialog, directly confirm upgrade
            ConfirmUpgrade();
        }
    }

    /// <summary>
    /// Confirm upgrade
    /// </summary>
    public void ConfirmUpgrade()
    {
        if (selectedUpgrade == null)
        {
            Debug.LogError("[TransitionUpgradeManager] No selected upgrade option!");
            return;
        }

        Debug.Log($"[TransitionUpgradeManager] Confirming upgrade: {selectedUpgrade.upgradeName}");

        // Hide confirmation dialog
        HideConfirmationDialog();

        // Apply upgrade
        StartCoroutine(ApplyUpgradeAndContinue());
    }

    /// <summary>
    /// Cancel upgrade
    /// </summary>
    public void CancelUpgrade()
    {
        Debug.Log("[TransitionUpgradeManager] Canceling upgrade, showing wheel again");

        selectedUpgrade = null;
        HideConfirmationDialog();

        // Show upgrade wheel again
        if (upgradeWheelUI != null)
            upgradeWheelUI.ShowWheel();
    }

    /// <summary>
    /// Apply upgrade and continue transition
    /// </summary>
    private IEnumerator ApplyUpgradeAndContinue()
    {
        // Apply upgrade via upgrade system
        if (upgradeSystem != null && selectedUpgrade != null)
        {
            upgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            Debug.Log($"[TransitionUpgradeManager] Applied upgrade: {selectedUpgrade.upgradeName}");
        }

        // Apply tank model transformation
        ApplyTankModelTransformation(selectedUpgrade);

        // Wait for transformation animation to complete
        yield return new WaitForSeconds(1f);

        // Mark upgrade as complete
        upgradeCompleted = true;

        // Continue transition
        ResumeTransition();

        Debug.Log("[TransitionUpgradeManager] Upgrade complete, continuing transition");
    }

    /// <summary>
    /// Apply tank model transformation
    /// </summary>
    private void ApplyTankModelTransformation(WheelUpgradeOption upgrade)
    {
        if (upgrade == null || playerTank == null)
        {
            Debug.LogWarning("[TransitionUpgradeManager] Cannot apply tank transformation: missing upgrade option or player tank");
            return;
        }

        Debug.Log($"[TransitionUpgradeManager] Applying tank model transformation: {upgrade.upgradeName}");

        // Switch to corresponding tank model based on upgrade name
        switch (upgrade.upgradeName.ToLower())
        {
            case "doublehead":
                SwitchToTankModel(doubleheadModel, "doublehead");
                break;
            case "fourhead":
                SwitchToTankModel(fourheadModel, "fourhead");
                break;
            case "huge":
                SwitchToTankModel(hugeModel, "HUGE");
                break;
            case "small":
                SwitchToTankModel(smallModel, "SMALL");
                break;
            default:
                Debug.LogWarning($"[TransitionUpgradeManager] Unknown upgrade type: {upgrade.upgradeName}");
                break;
        }

        // If has ModularTankController, also update it
        if (modularTankController != null)
        {
            // Can call ModularTankController methods here
            // modularTankController.ApplyConfiguration(upgrade);
        }
    }

    /// <summary>
    /// Switch to specified tank model
    /// </summary>
    private void SwitchToTankModel(GameObject newModel, string modelName)
    {
        if (newModel == null)
        {
            Debug.LogError($"[TransitionUpgradeManager] {modelName} model is not configured!");
            return;
        }

        // Hide all current models
        HideAllTankModels();

        // Show new model
        newModel.SetActive(true);

        Debug.Log($"[TransitionUpgradeManager] Switched to {modelName} model");
    }

    /// <summary>
    /// Hide all tank models
    /// </summary>
    private void HideAllTankModels()
    {
        if (armTankModel != null) armTankModel.SetActive(false);
        if (baseModel != null) baseModel.SetActive(false);
        if (barrelModel != null) barrelModel.SetActive(false);
        if (doubleheadModel != null) doubleheadModel.SetActive(false);
        if (fourheadModel != null) fourheadModel.SetActive(false);
        if (hugeModel != null) hugeModel.SetActive(false);
        if (smallModel != null) smallModel.SetActive(false);
    }

    /// <summary>
    /// Resume transition
    /// </summary>
    private void ResumeTransition()
    {
        HideAllUpgradeUI();

        if (transitionMover != null)
            transitionMover.enabled = true;

        isUpgradeInProgress = false;
        selectedUpgrade = null;

        Debug.Log("[TransitionUpgradeManager] Transition resumed");
    }

    /// <summary>
    /// Setup confirmation dialog button listeners
    /// </summary>
    private void SetupConfirmationDialog()
    {
        if (confirmUpgradeButton != null)
            confirmUpgradeButton.onClick.AddListener(ConfirmUpgrade);

        if (cancelUpgradeButton != null)
            cancelUpgradeButton.onClick.AddListener(CancelUpgrade);
    }

    /// <summary>
    /// Hide confirmation dialog
    /// </summary>
    private void HideConfirmationDialog()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    /// <summary>
    /// Hide all upgrade-related UI
    /// </summary>
    private void HideAllUpgradeUI()
    {
        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(false);

        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    // Debug methods
    [ContextMenu("Force Trigger Transition Upgrade")]
    public void DebugTriggerUpgrade()
    {
        TriggerTransitionUpgrade();
    }

    [ContextMenu("Skip Upgrade and Continue")]
    public void DebugSkipUpgrade()
    {
        upgradeCompleted = true;
        ResumeTransition();
    }

    [ContextMenu("Force Switch to doublehead")]
    public void DebugSwitchToDoublehead()
    {
        SwitchToTankModel(doubleheadModel, "doublehead");
    }

    [ContextMenu("Force Switch to HUGE")]
    public void DebugSwitchToHuge()
    {
        SwitchToTankModel(hugeModel, "HUGE");
    }

    [ContextMenu("Force Switch to SMALL")]
    public void DebugSwitchToSmall()
    {
        SwitchToTankModel(smallModel, "SMALL");
    }
}
