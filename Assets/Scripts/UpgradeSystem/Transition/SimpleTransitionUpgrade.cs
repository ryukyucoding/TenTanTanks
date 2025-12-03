using UnityEngine;
using UnityEngine.UI;
using WheelUpgradeSystem;
using TMPro;

/// <summary>
/// Simple Transition Upgrade - Simplified version for quick testing
/// Uses basic button UI instead of complex wheel interface
/// </summary>
public class SimpleTransitionUpgrade : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button doubleheadButton;
    [SerializeField] private Button hugeButton;
    [SerializeField] private Button smallButton;
    [SerializeField] private Button fourheadFrontBackButton;
    [SerializeField] private Button fourheadCrossButton;
    [SerializeField] private Button hugeTripleFrontButton;
    [SerializeField] private Button hugeTriple120Button;
    [SerializeField] private Button smallTripleFrontButton;
    [SerializeField] private Button smallTriple120Button;

    [Header("Confirmation Dialog")]
    [SerializeField] private GameObject confirmDialog;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Tank Model References")]
    [SerializeField] private GameObject doubleheadModel;
    [SerializeField] private GameObject fourheadModel;
    [SerializeField] private GameObject hugeModel;
    [SerializeField] private GameObject smallModel;

    private EnhancedTransitionMover transitionMover;
    private WheelUpgradeOption selectedUpgrade;
    private string currentTransitionType;

    void Start()
    {
        transitionMover = FindFirstObjectByType<EnhancedTransitionMover>();
        SetupButtons();
        HideAllUI();
    }

    /// <summary>
    /// Show upgrade panel based on transition type
    /// </summary>
    public void ShowUpgradePanel(string transitionType = "Level2To3")
    {
        currentTransitionType = transitionType;

        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        SetupUpgradeButtons(transitionType);

        if (titleText != null)
        {
            titleText.text = transitionType == "Level2To3" ?
                "Choose Your First Upgrade" :
                "Choose Your Final Upgrade";
        }

        Debug.Log("[SimpleTransitionUpgrade] Showing upgrade panel for: " + transitionType);
    }

    /// <summary>
    /// Setup upgrade buttons based on transition type
    /// </summary>
    private void SetupUpgradeButtons(string transitionType)
    {
        // Hide all buttons first
        HideAllUpgradeButtons();

        if (transitionType == "Level2To3")
        {
            // Show first tier options
            ShowButton(doubleheadButton, "Dual Cannon");
            ShowButton(hugeButton, "Heavy Tank");
            ShowButton(smallButton, "Light Tank");
        }
        else if (transitionType == "Level4To5")
        {
            // Show second tier options (all for simplicity, could be filtered by parent)
            ShowButton(fourheadFrontBackButton, "Quad Front-Back");
            ShowButton(fourheadCrossButton, "Cross Fire");
            ShowButton(hugeTripleFrontButton, "Triple Heavy");
            ShowButton(hugeTriple120Button, "Heavy Triangle");
            ShowButton(smallTripleFrontButton, "Triple Rapid");
            ShowButton(smallTriple120Button, "Rapid Triangle");
        }
    }

    /// <summary>
    /// Show and configure a button
    /// </summary>
    private void ShowButton(Button button, string text)
    {
        if (button != null)
        {
            button.gameObject.SetActive(true);

            // Update button text if it has TextMeshProUGUI component
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = text;
        }
    }

    /// <summary>
    /// Hide all upgrade buttons
    /// </summary>
    private void HideAllUpgradeButtons()
    {
        if (doubleheadButton != null) doubleheadButton.gameObject.SetActive(false);
        if (hugeButton != null) hugeButton.gameObject.SetActive(false);
        if (smallButton != null) smallButton.gameObject.SetActive(false);
        if (fourheadFrontBackButton != null) fourheadFrontBackButton.gameObject.SetActive(false);
        if (fourheadCrossButton != null) fourheadCrossButton.gameObject.SetActive(false);
        if (hugeTripleFrontButton != null) hugeTripleFrontButton.gameObject.SetActive(false);
        if (hugeTriple120Button != null) hugeTriple120Button.gameObject.SetActive(false);
        if (smallTripleFrontButton != null) smallTripleFrontButton.gameObject.SetActive(false);
        if (smallTriple120Button != null) smallTriple120Button.gameObject.SetActive(false);
    }

    /// <summary>
    /// Setup button listeners
    /// </summary>
    private void SetupButtons()
    {
        // First tier buttons
        if (doubleheadButton != null)
            doubleheadButton.onClick.AddListener(() => SelectUpgrade("doublehead"));
        if (hugeButton != null)
            hugeButton.onClick.AddListener(() => SelectUpgrade("HUGE"));
        if (smallButton != null)
            smallButton.onClick.AddListener(() => SelectUpgrade("SMALL"));

        // Second tier buttons
        if (fourheadFrontBackButton != null)
            fourheadFrontBackButton.onClick.AddListener(() => SelectUpgrade("fourhead_front_back"));
        if (fourheadCrossButton != null)
            fourheadCrossButton.onClick.AddListener(() => SelectUpgrade("fourhead_cross"));
        if (hugeTripleFrontButton != null)
            hugeTripleFrontButton.onClick.AddListener(() => SelectUpgrade("HUGE_triple_front"));
        if (hugeTriple120Button != null)
            hugeTriple120Button.onClick.AddListener(() => SelectUpgrade("HUGE_triple_120"));
        if (smallTripleFrontButton != null)
            smallTripleFrontButton.onClick.AddListener(() => SelectUpgrade("SMALL_triple_front"));
        if (smallTriple120Button != null)
            smallTriple120Button.onClick.AddListener(() => SelectUpgrade("SMALL_triple_120"));

        // Confirmation buttons
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmUpgrade);
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(CancelUpgrade);
    }

    /// <summary>
    /// Select an upgrade option
    /// </summary>
    private void SelectUpgrade(string upgradeName)
    {
        // Create upgrade option based on name
        selectedUpgrade = CreateUpgradeOption(upgradeName);

        Debug.Log("[SimpleTransitionUpgrade] Selected upgrade: " + upgradeName);

        // Show confirmation dialog
        ShowConfirmationDialog(selectedUpgrade);
    }

    /// <summary>
    /// Create upgrade option based on upgrade name
    /// </summary>
    private WheelUpgradeOption CreateUpgradeOption(string upgradeName)
    {
        WheelUpgradeOption option = new WheelUpgradeOption();
        option.upgradeName = upgradeName;
        option.description = GetUpgradeDescription(upgradeName);

        // Set basic multipliers (can be refined later)
        switch (upgradeName.ToLower())
        {
            case "doublehead":
                option.damageMultiplier = 1.2f;
                option.fireRateMultiplier = 1.1f;
                option.moveSpeedMultiplier = 0.95f;
                break;
            case "huge":
                option.damageMultiplier = 1.8f;
                option.fireRateMultiplier = 0.7f;
                option.moveSpeedMultiplier = 0.8f;
                break;
            case "small":
                option.damageMultiplier = 0.8f;
                option.fireRateMultiplier = 1.4f;
                option.moveSpeedMultiplier = 1.3f;
                break;
            default:
                option.damageMultiplier = 1.0f;
                option.fireRateMultiplier = 1.0f;
                option.moveSpeedMultiplier = 1.0f;
                break;
        }

        return option;
    }

    /// <summary>
    /// Get user-friendly description of upgrade
    /// </summary>
    private string GetUpgradeDescription(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "doublehead":
                return "Dual cannons provide balanced firepower upgrade with moderate speed reduction.";
            case "huge":
                return "Massive cannon delivers devastating damage but significantly reduces mobility.";
            case "small":
                return "Small cannon enables rapid fire and high mobility at the cost of damage.";
            case "fourhead_front_back":
                return "Four cannons (2 front, 2 back) provide comprehensive directional firepower.";
            case "fourhead_cross":
                return "Cross-positioned cannons eliminate all blind spots with 360¢X coverage.";
            case "huge_triple_front":
                return "Three massive front cannons create overwhelming frontal assault capability.";
            case "huge_triple_120":
                return "Three heavy cannons at 120¢X intervals provide devastating area coverage.";
            case "small_triple_front":
                return "Triple small cannons deliver extreme rate of fire for frontal suppression.";
            case "small_triple_120":
                return "Three rapid-fire cannons at 120¢X create mobile bullet storm.";
            default:
                return "Standard tank upgrade with balanced performance improvements.";
        }
    }

    /// <summary>
    /// Show confirmation dialog
    /// </summary>
    private void ShowConfirmationDialog(WheelUpgradeOption upgrade)
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(true);

            if (confirmText != null)
            {
                confirmText.text = "Confirm upgrade to " + TransitionUpgradeConfigs.GetDisplayName(upgrade.upgradeName) + "?\n\n" + upgrade.description;
            }
        }
    }

    /// <summary>
    /// Confirm the upgrade
    /// </summary>
    public void ConfirmUpgrade()
    {
        if (selectedUpgrade == null) return;

        Debug.Log("[SimpleTransitionUpgrade] Confirming upgrade: " + selectedUpgrade.upgradeName);

        // Apply tank model transformation
        ApplyTankModel(selectedUpgrade.upgradeName);

        // Hide all UI
        HideAllUI();

        // Resume transition movement
        if (transitionMover != null)
            transitionMover.ResumeMovement();
    }

    /// <summary>
    /// Cancel the upgrade
    /// </summary>
    public void CancelUpgrade()
    {
        selectedUpgrade = null;

        if (confirmDialog != null)
            confirmDialog.SetActive(false);

        Debug.Log("[SimpleTransitionUpgrade] Upgrade canceled");
    }

    /// <summary>
    /// Apply tank model transformation
    /// </summary>
    private void ApplyTankModel(string upgradeName)
    {
        // Hide all models first
        HideAllTankModels();

        // Show appropriate model
        switch (upgradeName.ToLower())
        {
            case "doublehead":
            case "fourhead_front_back":
            case "fourhead_cross":
                if (doubleheadModel != null || fourheadModel != null)
                {
                    // Use fourhead if available, otherwise doublehead
                    GameObject modelToShow = fourheadModel != null ? fourheadModel : doubleheadModel;
                    if (modelToShow != null)
                    {
                        modelToShow.SetActive(true);
                        Debug.Log("[SimpleTransitionUpgrade] Switched to " + modelToShow.name + " model");
                    }
                }
                break;

            case "huge":
            case "huge_triple_front":
            case "huge_triple_120":
                if (hugeModel != null)
                {
                    hugeModel.SetActive(true);
                    Debug.Log("[SimpleTransitionUpgrade] Switched to HUGE model");
                }
                break;

            case "small":
            case "small_triple_front":
            case "small_triple_120":
                if (smallModel != null)
                {
                    smallModel.SetActive(true);
                    Debug.Log("[SimpleTransitionUpgrade] Switched to SMALL model");
                }
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
    /// Hide all UI elements
    /// </summary>
    private void HideAllUI()
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }

    // Debug methods
    [ContextMenu("Debug Show Panel Level2To3")]
    public void DebugShowPanel()
    {
        ShowUpgradePanel("Level2To3");
    }

    [ContextMenu("Debug Show Panel Level4To5")]
    public void DebugShowPanelLevel4To5()
    {
        ShowUpgradePanel("Level4To5");
    }

    [ContextMenu("Debug Check References")]
    public void DebugCheckReferences()
    {
        Debug.Log("=== SimpleTransitionUpgrade References ===");
        Debug.Log("Upgrade Panel: " + (upgradePanel != null ? "OK" : "MISSING"));
        Debug.Log("Doublehead Button: " + (doubleheadButton != null ? "OK" : "MISSING"));
        Debug.Log("HUGE Button: " + (hugeButton != null ? "OK" : "MISSING"));
        Debug.Log("SMALL Button: " + (smallButton != null ? "OK" : "MISSING"));
        Debug.Log("Confirm Dialog: " + (confirmDialog != null ? "OK" : "MISSING"));
        Debug.Log("Transition Mover: " + (transitionMover != null ? "FOUND" : "NOT FOUND"));
    }
}