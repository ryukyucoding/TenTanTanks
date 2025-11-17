using WheelUpgradeSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class WheelUpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    [Header("Visual States")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color hoverColor = Color.cyan;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    public enum ButtonState
    {
        Available,   // Can be clicked
        Selected,    // Currently selected
        Disabled     // Cannot be clicked (grayed out)
    }

    private WheelUpgradeOption upgradeOption;
    private Action onClickCallback;
    private ButtonState currentState = ButtonState.Available;
    private bool isHovered = false;

    void Awake()
    {
        AutoFindComponents();
        SetupButton();
    }

    private void AutoFindComponents()
    {
        // Auto-find components if not assigned
        if (button == null)
            button = GetComponent<Button>();

        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // If no background image on this object, try to find one in children
        if (backgroundImage == null)
        {
            var images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.ToLower().Contains("background") || img.name.ToLower().Contains("bg"))
                {
                    backgroundImage = img;
                    break;
                }
            }
            // If still null, use the first image that's not an icon
            if (backgroundImage == null && images.Length > 0)
            {
                backgroundImage = images[0];
            }
        }

        // Try to find icon image
        if (iconImage == null)
        {
            var images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.ToLower().Contains("icon") && img != backgroundImage)
                {
                    iconImage = img;
                    break;
                }
            }
        }

        DebugLog($"Auto-found components: Button={button != null}, Text={nameText != null}, BG={backgroundImage != null}, Icon={iconImage != null}");
    }

    private void SetupButton()
    {
        // Set up button click
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // Clear any existing listeners
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError($"[WheelUpgradeButton] Button component not found on {gameObject.name}");
        }
    }

    public void Setup(WheelUpgradeOption option, Action clickCallback)
    {
        if (option == null)
        {
            Debug.LogError("[WheelUpgradeButton] Cannot setup with null upgrade option");
            return;
        }

        upgradeOption = option;
        onClickCallback = clickCallback;

        // Set button text
        if (nameText != null)
        {
            nameText.text = option.upgradeName;
        }
        else
        {
            // Create text component if missing
            CreateTextComponent(option.upgradeName);
        }

        // Set button icon if available
        if (iconImage != null)
        {
            if (option.icon != null)
            {
                iconImage.sprite = option.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        // Set initial visual state
        SetButtonState(ButtonState.Available);

        DebugLog($"Setup completed for {option.upgradeName}");
    }

    private void CreateTextComponent(string text)
    {
        // Create a text component if none exists
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(transform, false);

        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = text;
        nameText.fontSize = 14;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;

        // Set up RectTransform
        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        DebugLog("Created text component automatically");
    }

    private void OnButtonClick()
    {
        // Only allow clicks if button is available or selected
        if (currentState == ButtonState.Disabled)
        {
            DebugLog($"Button {upgradeOption?.upgradeName} click ignored - button is disabled");
            return;
        }

        DebugLog($"Button clicked: {upgradeOption?.upgradeName}");
        onClickCallback?.Invoke();
    }

    public void SetButtonState(ButtonState state)
    {
        currentState = state;

        if (button != null)
        {
            button.interactable = (state != ButtonState.Disabled);
        }

        UpdateVisualState();
        DebugLog($"Button state changed to {state} for {upgradeOption?.upgradeName}");
    }

    private void UpdateVisualState()
    {
        Color targetColor;
        float alpha = 1f;

        // Determine target color based on state and hover
        if (isHovered && currentState == ButtonState.Available)
        {
            targetColor = hoverColor;
            alpha = 1f;
        }
        else
        {
            switch (currentState)
            {
                case ButtonState.Available:
                    targetColor = availableColor;
                    alpha = 1f;
                    break;
                case ButtonState.Selected:
                    targetColor = selectedColor;
                    alpha = 1f;
                    break;
                case ButtonState.Disabled:
                    targetColor = disabledColor;
                    alpha = 0.5f;
                    break;
                default:
                    targetColor = availableColor;
                    alpha = 1f;
                    break;
            }
        }

        // Apply color to background
        if (backgroundImage != null)
        {
            var color = targetColor;
            color.a = alpha;
            backgroundImage.color = color;
        }

        // Apply alpha to text
        if (nameText != null)
        {
            var textColor = nameText.color;
            textColor.a = alpha;
            nameText.color = textColor;
        }

        // Apply alpha to icon
        if (iconImage != null && iconImage.gameObject.activeInHierarchy)
        {
            var iconColor = iconImage.color;
            iconColor.a = alpha;
            iconImage.color = iconColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisualState();
        DebugLog($"Mouse entered button: {upgradeOption?.upgradeName}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisualState();
        DebugLog($"Mouse exited button: {upgradeOption?.upgradeName}");
    }

    public WheelUpgradeOption GetUpgradeOption()
    {
        return upgradeOption;
    }

    public ButtonState GetCurrentState()
    {
        return currentState;
    }

    // Legacy method for backward compatibility
    public void SetSelected(bool selected)
    {
        SetButtonState(selected ? ButtonState.Selected : ButtonState.Available);
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[WheelUpgradeButton] {message}");
    }

    // Context menu methods for debugging
    [ContextMenu("Test Button Click")]
    public void TestButtonClick()
    {
        OnButtonClick();
    }

    [ContextMenu("Check Components")]
    public void CheckComponents()
    {
        Debug.Log("=== WheelUpgradeButton Component Check ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Button: {(button != null ? "Y" : "N")}");
        Debug.Log($"NameText: {(nameText != null ? "Y" : "N")}");
        Debug.Log($"BackgroundImage: {(backgroundImage != null ? "Y" : "N")}");
        Debug.Log($"IconImage: {(iconImage != null ? "Y" : "N")}");
        Debug.Log($"UpgradeOption: {(upgradeOption != null ? upgradeOption.upgradeName : "null")}");
        Debug.Log($"Current State: {currentState}");
    }

    [ContextMenu("Force Update Visual")]
    public void ForceUpdateVisual()
    {
        UpdateVisualState();
        Debug.Log("Visual state forcefully updated");
    }
}