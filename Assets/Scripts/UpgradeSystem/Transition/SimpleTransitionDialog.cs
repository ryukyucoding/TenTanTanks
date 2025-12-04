using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WheelUpgradeSystem;

/// <summary>
/// Simple visual confirmation dialog for transition upgrades
/// This creates a clear visual confirmation that can be easily added to any scene
/// </summary>
public class SimpleTransitionDialog : MonoBehaviour
{
    [Header("Auto-Setup Components")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Dialog Settings")]
    [SerializeField] private bool autoCreateUI = true;
    [SerializeField] private Vector2 dialogSize = new Vector2(400, 200);

    private WheelUpgradeOption currentUpgrade;
    private System.Action onConfirm;
    private System.Action onCancel;

    void Start()
    {
        if (autoCreateUI && dialogPanel == null)
        {
            CreateSimpleDialog();
        }

        SetupButtons();
        HideDialog();
    }

    /// <summary>
    /// Auto-create a simple dialog UI if none exists
    /// </summary>
    private void CreateSimpleDialog()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SimpleTransitionDialog] No Canvas found in scene!");
            return;
        }

        // Create dialog panel
        dialogPanel = new GameObject("ConfirmationDialog");
        dialogPanel.transform.SetParent(canvas.transform, false);

        var image = dialogPanel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);

        var rectTransform = dialogPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Create content panel
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(dialogPanel.transform, false);

        var contentImage = contentPanel.AddComponent<Image>();
        contentImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        var contentRect = contentPanel.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = dialogSize;
        contentRect.anchoredPosition = Vector2.zero;

        // Create upgrade name text
        GameObject nameObj = new GameObject("UpgradeName");
        nameObj.transform.SetParent(contentPanel.transform, false);
        upgradeNameText = nameObj.AddComponent<TextMeshProUGUI>();
        upgradeNameText.text = "Upgrade Name";
        upgradeNameText.fontSize = 24;
        upgradeNameText.color = Color.yellow;
        upgradeNameText.alignment = TextAlignmentOptions.Center;

        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.7f);
        nameRect.anchorMax = new Vector2(1, 0.9f);
        nameRect.sizeDelta = Vector2.zero;
        nameRect.anchoredPosition = Vector2.zero;

        // Create message text
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(contentPanel.transform, false);
        messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "Do you want to choose this upgrade?";
        messageText.fontSize = 16;
        messageText.color = Color.white;
        messageText.alignment = TextAlignmentOptions.Center;

        var messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.4f);
        messageRect.anchorMax = new Vector2(1, 0.7f);
        messageRect.sizeDelta = Vector2.zero;
        messageRect.anchoredPosition = Vector2.zero;

        // Create Yes button
        yesButton = CreateButton("YES", new Vector2(-50, -20), new Vector2(80, 40), Color.green);
        yesButton.transform.SetParent(contentPanel.transform, false);

        // Create No button  
        noButton = CreateButton("NO", new Vector2(50, -20), new Vector2(80, 40), Color.red);
        noButton.transform.SetParent(contentPanel.transform, false);

        Debug.Log("[SimpleTransitionDialog] Auto-created dialog UI");
    }

    private Button CreateButton(string text, Vector2 position, Vector2 size, Color color)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");

        var image = buttonObj.AddComponent<Image>();
        image.color = color;

        var button = buttonObj.AddComponent<Button>();

        var rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;

        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 16;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        return button;
    }

    private void SetupButtons()
    {
        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(OnYesClicked);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(OnNoClicked);
        }
    }

    /// <summary>
    /// Show the confirmation dialog
    /// </summary>
    public void ShowDialog(WheelUpgradeOption upgrade, System.Action confirmCallback, System.Action cancelCallback = null)
    {
        currentUpgrade = upgrade;
        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        if (upgradeNameText != null)
            upgradeNameText.text = upgrade.upgradeName;

        if (messageText != null)
            messageText.text = $"Choose '{upgrade.upgradeName}'?\n\n{upgrade.description}\n\nThis will be your tank upgrade!";

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        Debug.Log($"[SimpleTransitionDialog] Showing confirmation for: {upgrade.upgradeName}");
    }

    /// <summary>
    /// Hide the dialog
    /// </summary>
    public void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        currentUpgrade = null;
        onConfirm = null;
        onCancel = null;
    }

    private void OnYesClicked()
    {
        Debug.Log($"[SimpleTransitionDialog] YES clicked for: {currentUpgrade?.upgradeName}");
        HideDialog();
        onConfirm?.Invoke();
    }

    private void OnNoClicked()
    {
        Debug.Log($"[SimpleTransitionDialog] NO clicked for: {currentUpgrade?.upgradeName}");
        HideDialog();
        onCancel?.Invoke();
    }

    // Test methods
    [ContextMenu("Test Show Dialog")]
    public void TestShowDialog()
    {
        var testUpgrade = new WheelUpgradeOption("TestUpgrade", "This is a test upgrade", 1);
        ShowDialog(testUpgrade,
            () => Debug.Log("Test: YES clicked"),
            () => Debug.Log("Test: NO clicked"));
    }

    [ContextMenu("Test Hide Dialog")]
    public void TestHideDialog()
    {
        HideDialog();
    }
}