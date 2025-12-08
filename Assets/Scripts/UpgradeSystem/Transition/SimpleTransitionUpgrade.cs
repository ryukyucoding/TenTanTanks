using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple Transition Upgrade - Fully standalone version for quick testing
/// Uses basic button UI instead of complex wheel interface
/// No external dependencies - works immediately
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
    private string selectedUpgradeName;
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
        selectedUpgradeName = upgradeName;

        Debug.Log("[SimpleTransitionUpgrade] Selected upgrade: " + upgradeName);

        // Show confirmation dialog
        ShowConfirmationDialog(upgradeName);
    }

    /// <summary>
    /// Show confirmation dialog
    /// </summary>
    private void ShowConfirmationDialog(string upgradeName)
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(true);

            if (confirmText != null)
            {
                string displayName = GetSimpleDisplayName(upgradeName);
                string description = GetUpgradeDescription(upgradeName);
                confirmText.text = "Confirm upgrade to " + displayName + "?\n\n" + description;
            }
        }
    }

    /// <summary>
    /// Confirm the upgrade
    /// </summary>
    public void ConfirmUpgrade()
    {
        if (string.IsNullOrEmpty(selectedUpgradeName)) return;

        Debug.Log("[SimpleTransitionUpgrade] Confirming upgrade: " + selectedUpgradeName);

        // Apply tank model transformation
        ApplyTankModel(selectedUpgradeName);

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
        selectedUpgradeName = "";

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
                if (fourheadModel != null)
                {
                    fourheadModel.SetActive(true);
                    Debug.Log("[SimpleTransitionUpgrade] Switched to fourhead model");
                }
                else if (doubleheadModel != null)
                {
                    doubleheadModel.SetActive(true);
                    Debug.Log("[SimpleTransitionUpgrade] Switched to doublehead model");
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

            default:
                Debug.LogWarning("[SimpleTransitionUpgrade] Unknown upgrade: " + upgradeName);
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
    /// Get simple display name for upgrade
    /// </summary>
    private string GetSimpleDisplayName(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "doublehead": return "Dual Cannon";
            case "huge": return "Heavy Tank";
            case "small": return "Light Tank";
            case "fourhead_front_back": return "Quad Front-Back";
            case "fourhead_cross": return "Cross Fire";
            case "huge_triple_front": return "Triple Heavy";
            case "huge_triple_120": return "Heavy Triangle";
            case "small_triple_front": return "Triple Rapid";
            case "small_triple_120": return "Rapid Triangle";
            default: return upgradeName;
        }
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
        Debug.Log("Tank Models:");
        Debug.Log("  - Doublehead: " + (doubleheadModel != null ? "OK" : "MISSING"));
        Debug.Log("  - Fourhead: " + (fourheadModel != null ? "OK" : "MISSING"));
        Debug.Log("  - HUGE: " + (hugeModel != null ? "OK" : "MISSING"));
        Debug.Log("  - SMALL: " + (smallModel != null ? "OK" : "MISSING"));
    }
}