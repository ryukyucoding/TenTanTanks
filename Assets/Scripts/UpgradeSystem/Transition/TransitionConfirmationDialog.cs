using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using WheelUpgradeSystem;

/// <summary>
/// Simple confirmation dialog for transition upgrade choices
/// </summary>
public class TransitionConfirmationDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeDescriptionText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Image upgradeIcon;

    [Header("Dialog Settings")]
    [SerializeField] private string confirmMessage = "Do you want to choose this upgrade?";
    [SerializeField] private Color confirmButtonColor = Color.green;
    [SerializeField] private Color cancelButtonColor = Color.red;

    private WheelUpgradeOption currentUpgrade;
    private Action onConfirm;
    private Action onCancel;

    void Start()
    {
        // Set up button listeners
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);

            // Set button color
            var buttonImage = confirmButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = confirmButtonColor;
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);

            // Set button color
            var buttonImage = cancelButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = cancelButtonColor;
        }

        // Hide dialog initially
        HideDialog();
    }

    /// <summary>
    /// Show confirmation dialog for an upgrade choice
    /// </summary>
    public void ShowDialog(WheelUpgradeOption upgrade, Action confirmCallback, Action cancelCallback = null)
    {
        currentUpgrade = upgrade;
        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        // Set upgrade info
        if (upgradeNameText != null)
            upgradeNameText.text = upgrade.upgradeName;

        if (upgradeDescriptionText != null)
            upgradeDescriptionText.text = upgrade.description;

        if (messageText != null)
            messageText.text = confirmMessage;

        if (upgradeIcon != null && upgrade.icon != null)
        {
            upgradeIcon.sprite = upgrade.icon;
            upgradeIcon.gameObject.SetActive(true);
        }
        else if (upgradeIcon != null)
        {
            upgradeIcon.gameObject.SetActive(false);
        }

        // Show the dialog
        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        Debug.Log($"[TransitionConfirmationDialog] Showing confirmation for: {upgrade.upgradeName}");
    }

    /// <summary>
    /// Hide the confirmation dialog
    /// </summary>
    public void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        currentUpgrade = null;
        onConfirm = null;
        onCancel = null;

        Debug.Log("[TransitionConfirmationDialog] Dialog hidden");
    }

    /// <summary>
    /// Handle confirm button click
    /// </summary>
    private void OnConfirmClicked()
    {
        Debug.Log($"[TransitionConfirmationDialog] Upgrade confirmed: {currentUpgrade?.upgradeName}");

        // Hide dialog first
        HideDialog();

        // Call confirm callback
        onConfirm?.Invoke();
    }

    /// <summary>
    /// Handle cancel button click
    /// </summary>
    private void OnCancelClicked()
    {
        Debug.Log($"[TransitionConfirmationDialog] Upgrade canceled: {currentUpgrade?.upgradeName}");

        // Hide dialog first
        HideDialog();

        // Call cancel callback
        onCancel?.Invoke();
    }

    /// <summary>
    /// Set custom confirmation message
    /// </summary>
    public void SetConfirmMessage(string message)
    {
        confirmMessage = message;
        if (messageText != null)
            messageText.text = message;
    }

    // Context menu methods for testing
    [ContextMenu("Test Show Dialog")]
    public void TestShowDialog()
    {
        var testUpgrade = new WheelUpgradeOption("TestUpgrade", "This is a test upgrade for dialog testing", 1);
        ShowDialog(testUpgrade, () => Debug.Log("Test confirmed"), () => Debug.Log("Test canceled"));
    }

    [ContextMenu("Test Hide Dialog")]
    public void TestHideDialog()
    {
        HideDialog();
    }
}