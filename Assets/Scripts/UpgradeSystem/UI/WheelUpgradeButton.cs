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
    [SerializeField] private Color previewColor = new Color(0.8f, 0.8f, 0.8f, 0.6f); // Light gray with transparency
    [SerializeField] private Color previousChoiceColor = new Color(0.9f, 0.7f, 0.2f); // Orange-ish

    [Header("Font Settings")]
    [SerializeField] private float fontSize = 16f; // 調整字體大小
    [SerializeField] private TMP_FontAsset minecraftFont; // Minecraft 字體

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    public enum ButtonState
    {
        Available,      // Can be clicked
        Selected,       // Currently selected
        Disabled,       // Cannot be clicked (grayed out)
        Preview,        // Visible but not clickable (for future tier preview)
        PreviousChoice  // Shows previous choice (like selected but disabled)
    }

    private WheelUpgradeOption upgradeOption;
    private Action onClickCallback;
    private ButtonState currentState = ButtonState.Available;
    private bool isHovered = false;

    void Awake()
    {
        AutoFindComponents();
        SetupButton();
        LoadMinecraftFont(); // 自動載入 Minecraft 字體
    }

    /// <summary>
    /// 自動載入 Minecraft 字體
    /// </summary>
    private void LoadMinecraftFont()
    {
        // 如果沒有手動指定字體，就自動載入
        if (minecraftFont == null)
        {
            minecraftFont = Resources.Load<TMP_FontAsset>("Fonts/MinecraftTen-VGORe SDF");

            if (minecraftFont != null)
            {
                DebugLog("✅ Successfully loaded Minecraft font: " + minecraftFont.name);
            }
            else
            {
                DebugLog("❌ Failed to load Minecraft font from Resources/Fonts/MinecraftTen-VGORe SDF");
            }
        }
        else
        {
            DebugLog("✅ Minecraft font already assigned: " + minecraftFont.name);
        }
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
            // 套用 Minecraft 字體
            ApplyMinecraftFont(nameText);
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

    /// <summary>
    /// 套用 Minecraft 字體到文字組件
    /// </summary>
    private void ApplyMinecraftFont(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        // 套用 Minecraft 字體
        if (minecraftFont != null)
        {
            textComponent.font = minecraftFont;
            DebugLog($"✅ Applied Minecraft font to {textComponent.name}");
        }
        else
        {
            DebugLog($"❌ No Minecraft font available for {textComponent.name}");
        }

        // 設定字體大小和其他屬性
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = FontStyles.Bold; // Minecraft 風格通常是粗體
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;

        // 加上輪廓讓字體更清楚
        textComponent.outlineWidth = 0.2f;
        textComponent.outlineColor = Color.black;

        // 強制更新文字
        textComponent.ForceMeshUpdate();
    }

    private void CreateTextComponent(string text)
    {
        // Create a text component if none exists
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(transform, false);

        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = text;

        // 套用 Minecraft 字體設定
        ApplyMinecraftFont(nameText);

        // Set up RectTransform
        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        DebugLog("Created text component automatically with Minecraft font");
    }

    private void OnButtonClick()
    {
        // Only allow clicks for Available and Selected states
        if (currentState == ButtonState.Disabled ||
            currentState == ButtonState.Preview ||
            currentState == ButtonState.PreviousChoice)
        {
            DebugLog($"Button {upgradeOption?.upgradeName} click ignored - button state is {currentState}");
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
        if (isHovered && (currentState == ButtonState.Available || currentState == ButtonState.Preview))
        {
            targetColor = hoverColor;
            alpha = currentState == ButtonState.Preview ? 0.8f : 1f;
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
                    alpha = 0.4f;
                    break;
                case ButtonState.Preview:
                    targetColor = previewColor;
                    alpha = 0.6f;
                    break;
                case ButtonState.PreviousChoice:
                    targetColor = previousChoiceColor;
                    alpha = 0.7f;
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
        Debug.Log($"Minecraft Font: {(minecraftFont != null ? minecraftFont.name : "NULL")}");
        Debug.Log($"UpgradeOption: {(upgradeOption != null ? upgradeOption.upgradeName : "null")}");
        Debug.Log($"Current State: {currentState}");
    }

    [ContextMenu("Force Update Visual")]
    public void ForceUpdateVisual()
    {
        UpdateVisualState();
        Debug.Log("Visual state forcefully updated");
    }

    [ContextMenu("Reload Minecraft Font")]
    public void ReloadMinecraftFont()
    {
        minecraftFont = null;
        LoadMinecraftFont();
        if (nameText != null)
        {
            ApplyMinecraftFont(nameText);
        }
        Debug.Log("Minecraft font reloaded and applied");
    }
}