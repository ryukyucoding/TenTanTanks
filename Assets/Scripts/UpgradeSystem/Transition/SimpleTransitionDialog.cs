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
    [SerializeField] private Vector2 dialogSize = new Vector2(500, 300);
    [SerializeField] private string yesButtonText = "YES";
    [SerializeField] private string noButtonText = "NO";

    [Header("Styling")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color textColor = Color.white;

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
        Debug.Log("[SimpleTransitionDialog] Auto-creating dialog UI...");

        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SimpleTransitionDialog] No Canvas found in scene!");
            return;
        }

        // Create main dialog panel
        GameObject panelGO = new GameObject("TransitionConfirmDialog");
        panelGO.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.5f); // Semi-transparent background

        // Create dialog box
        GameObject dialogGO = new GameObject("DialogBox");
        dialogGO.transform.SetParent(panelGO.transform, false);

        RectTransform dialogRect = dialogGO.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.sizeDelta = dialogSize;
        dialogRect.anchoredPosition = Vector2.zero;

        Image dialogBg = dialogGO.AddComponent<Image>();
        dialogBg.color = backgroundColor;

        // Create upgrade name text
        upgradeNameText = CreateText("UpgradeName", dialogGO.transform, new Vector2(0, 80), "UPGRADE NAME", 24);
        upgradeNameText.fontStyle = FontStyles.Bold;

        // Create message text
        messageText = CreateText("Message", dialogGO.transform, new Vector2(0, 20), "Confirm this upgrade?", 18);

        // Create buttons
        yesButton = CreateButton("YesButton", dialogGO.transform, new Vector2(-100, -80), yesButtonText);
        noButton = CreateButton("NoButton", dialogGO.transform, new Vector2(100, -80), noButtonText);

        dialogPanel = panelGO;

        Debug.Log("[SimpleTransitionDialog] Dialog UI created successfully!");
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, string text, int fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(400, 50);
        textRect.anchoredPosition = position;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Normal;

        return tmp;
    }

    private Button CreateButton(string name, Transform parent, Vector2 position, string text)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(120, 50);
        buttonRect.anchoredPosition = position;

        Image buttonImg = buttonGO.AddComponent<Image>();
        buttonImg.color = buttonColor;

        Button button = buttonGO.AddComponent<Button>();

        // Create button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;

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

    [ContextMenu("Force Create UI")]
    public void ForceCreateUI()
    {
        if (dialogPanel != null)
        {
            DestroyImmediate(dialogPanel);
        }
        CreateSimpleDialog();
        SetupButtons();
        HideDialog();
    }
}