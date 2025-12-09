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
    [SerializeField] private Color previewColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    [SerializeField] private Color previousChoiceColor = new Color(0.9f, 0.7f, 0.2f);

    // ★★★ 新增：智能字體大小設定 ★★★
    private const float TIER1_FONT_SIZE = 30f; // 第一層字體大小
    private const float TIER2_FONT_SIZE = 20f; // 第二層字體大小
    private const float DEFAULT_FONT_SIZE = 25f; // 預設字體大小

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset minecraftFont;

    // ★★★ 新增：文字框設定 ★★★
    [Header("Text Box Settings")]
    [SerializeField] private float textBoxWidth = 120f;
    [SerializeField] private float textBoxHeight = 40f;
    [SerializeField] private bool autoAdjustWidth = true;
    [SerializeField] private float paddingHorizontal = 10f;
    [SerializeField] private bool allowUnlimitedWidth = false;
    [SerializeField] private float maxWidthLimit = 200f;

    [Header("Text Effects")]
    [SerializeField] private bool useTextOutline = true;
    [SerializeField] private float outlineWidth = 0.2f;
    [SerializeField] private Color outlineColor = Color.black;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showTextBoxBounds = false; // 顯示文字框邊界（除錯用）

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
    private int buttonTier = 1; // 按鈕層級（自動判斷）

    void Awake()
    {
        AutoFindComponents();
        SetupButton();
        LoadMinecraftFont(); // ★★★ 新增：載入 Minecraft 字體
    }

    /// <summary>
    /// ★★★ 新增：載入 Minecraft 字體 ★★★
    /// </summary>
    private void LoadMinecraftFont()
    {
        if (minecraftFont == null)
        {
            minecraftFont = Resources.Load<TMP_FontAsset>("Fonts/MinecraftTen-VGORe SDF");

            if (minecraftFont != null)
            {
                DebugLog("✅ Successfully loaded Minecraft font: " + minecraftFont.name);
            }
            else
            {
                DebugLog("❌ Failed to load Minecraft font");
            }
        }
    }

    /// <summary>
    /// ★★★ 保留舊版的組件自動尋找邏輯 ★★★
    /// </summary>
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

    /// <summary>
    /// ★★★ 新增：設定純文字按鈕外觀 ★★★
    /// </summary>
    private void SetupButton()
    {
        // ★★★ 新增：隱藏背景圖片但保留組件（用於點擊檢測區域）
        if (backgroundImage != null)
        {
            if (showTextBoxBounds)
            {
                // 除錯模式：顯示半透明邊界
                backgroundImage.color = new Color(1f, 0f, 0f, 0.2f);
            }
            else
            {
                // 正常模式：完全透明
                backgroundImage.color = Color.clear;
            }
        }

        // Set up button click - 保留舊版邏輯
        if (button != null)
        {
            // ★★★ 新增：設定按鈕為透明 ★★★
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (showTextBoxBounds)
                {
                    buttonImage.color = new Color(0f, 1f, 0f, 0.2f); // 除錯：綠色邊界
                }
                else
                {
                    buttonImage.color = Color.clear;
                }
            }

            // ★★★ 新增：設定按鈕為無過渡效果
            button.transition = Selectable.Transition.None;

            button.onClick.RemoveAllListeners(); // Clear any existing listeners
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError($"[WheelUpgradeButton] Button component not found on {gameObject.name}");
        }

        DebugLog("Pure text button setup completed");
    }

    /// <summary>
    /// ★★★ 保留舊版的 Setup 邏輯，加上新版的字體處理 ★★★
    /// </summary>
    public void Setup(WheelUpgradeOption option, Action clickCallback)
    {
        if (option == null)
        {
            Debug.LogError("[WheelUpgradeButton] Cannot setup with null upgrade option");
            return;
        }

        upgradeOption = option;
        onClickCallback = clickCallback;

        // ★★★ 新增：自動判斷按鈕層級 ★★★
        DetermineButtonTier(option);
        float fontSize = GetFontSizeForTier();

        // Set button text - 保留舊版邏輯，加上新版字體處理
        if (nameText != null)
        {
            nameText.text = option.upgradeName;
            // ★★★ 新增：套用 Minecraft 字體和智能字體大小 ★★★
            ApplyTextStyle(nameText, fontSize);
            AdjustTextBoxSize(); // ★★★ 新增：調整文字框大小
        }
        else
        {
            // Create text component if missing - 加上新版字體處理
            CreateTextComponent(option.upgradeName, fontSize);
        }

        // ★★★ 新增：隱藏圖標（純文字模式）
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        // Set initial visual state - 保留舊版邏輯
        SetButtonState(ButtonState.Available);

        DebugLog($"Setup completed for {option.upgradeName} (Tier{buttonTier}, fontSize={fontSize})");
    }

    /// <summary>
    /// ★★★ 新增：根據升級選項判斷按鈕層級 ★★★
    /// </summary>
    private void DetermineButtonTier(WheelUpgradeOption option)
    {
        // 判斷邏輯：如果有 parentUpgradeName，就是第二層
        if (!string.IsNullOrEmpty(option.parentUpgradeName))
        {
            buttonTier = 2;
            DebugLog($"[TIER] {option.upgradeName} is Tier 2 (parent: {option.parentUpgradeName})");
        }
        else
        {
            buttonTier = 1;
            DebugLog($"[TIER] {option.upgradeName} is Tier 1");
        }
    }

    /// <summary>
    /// ★★★ 新增：根據層級取得對應的字體大小 ★★★
    /// </summary>
    private float GetFontSizeForTier()
    {
        switch (buttonTier)
        {
            case 1:
                return TIER1_FONT_SIZE; // 30f
            case 2:
                return TIER2_FONT_SIZE; // 20f
            default:
                return DEFAULT_FONT_SIZE; // 25f
        }
    }

    /// <summary>
    /// ★★★ 新增：套用 Minecraft 文字樣式 ★★★
    /// </summary>
    private void ApplyTextStyle(TextMeshProUGUI textComponent, float fontSize)
    {
        if (textComponent == null) return;

        if (minecraftFont != null)
        {
            textComponent.font = minecraftFont;
            DebugLog($"✅ Applied Minecraft font to {textComponent.name}");
        }

        textComponent.fontSize = fontSize;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;

        // ★★★ 強制禁用換行，讓文字自由伸縮 ★★★
        textComponent.enableWordWrapping = false; // 禁用自動換行
        textComponent.overflowMode = TextOverflowModes.Overflow; // 允許文字溢出邊界
        textComponent.enableAutoSizing = false; // 禁用自動調整字體大小
        textComponent.textWrappingMode = TMPro.TextWrappingModes.NoWrap; // 強制不換行

        if (useTextOutline)
        {
            textComponent.outlineWidth = outlineWidth;
            textComponent.outlineColor = outlineColor;
        }

        textComponent.ForceMeshUpdate();
    }

    /// <summary>
    /// ★★★ 新增：調整文字框大小 ★★★
    /// </summary>
    private void AdjustTextBoxSize()
    {
        if (nameText == null) return;

        var rectTransform = nameText.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        float finalWidth = textBoxWidth;

        // 自動調整寬度（根據文字長度）
        if (autoAdjustWidth)
        {
            // 計算文字的實際寬度
            nameText.ForceMeshUpdate();
            Vector2 textSize = nameText.GetRenderedValues(false);
            finalWidth = textSize.x + paddingHorizontal * 2f;

            // 設定最小寬度
            finalWidth = Mathf.Max(finalWidth, 60f);

            // 根據設定決定是否限制最大寬度
            if (!allowUnlimitedWidth)
            {
                finalWidth = Mathf.Min(finalWidth, maxWidthLimit);
            }
        }

        // 設定文字框大小（中心點不變）
        rectTransform.sizeDelta = new Vector2(finalWidth, textBoxHeight);
        AdjustButtonClickArea(finalWidth);
    }

    /// <summary>
    /// ★★★ 新增：調整按鈕的點擊區域 ★★★
    /// </summary>
    private void AdjustButtonClickArea(float textWidth)
    {
        if (button == null) return;

        var buttonRect = button.GetComponent<RectTransform>();
        if (buttonRect == null) return;

        // 讓點擊區域稍微大一點，更容易點擊
        float clickAreaWidth = textWidth + 20f; // 左右各多10px
        float clickAreaHeight = textBoxHeight + 10f; // 上下各多5px

        buttonRect.sizeDelta = new Vector2(clickAreaWidth, clickAreaHeight);
    }

    /// <summary>
    /// ★★★ 修改：創建文字組件，加上新版字體處理 ★★★
    /// </summary>
    private void CreateTextComponent(string text, float fontSize)
    {
        // Create a text component if none exists
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(transform, false);

        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = text;

        // ★★★ 新增：套用 Minecraft 字體樣式
        ApplyTextStyle(nameText, fontSize);

        // Set up RectTransform
        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(textBoxWidth, textBoxHeight);
        rectTransform.anchoredPosition = Vector2.zero;

        // ★★★ 新增：調整大小
        AdjustTextBoxSize();

        DebugLog("Created text component with Minecraft font");
    }

    /// <summary>
    /// ★★★ 保留舊版的點擊邏輯 ★★★
    /// </summary>
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

    /// <summary>
    /// ★★★ 保留舊版的狀態設定邏輯 ★★★
    /// </summary>
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

    /// <summary>
    /// ★★★ 修改：視覺狀態更新，只影響文字顏色（因為背景已透明）★★★
    /// </summary>
    private void UpdateVisualState()
    {
        Color targetTextColor;
        float alpha = 1f;

        // Determine target color based on state and hover
        if (isHovered && (currentState == ButtonState.Available || currentState == ButtonState.Preview))
        {
            targetTextColor = hoverColor;
            alpha = currentState == ButtonState.Preview ? 0.8f : 1f;
        }
        else
        {
            switch (currentState)
            {
                case ButtonState.Available:
                    targetTextColor = availableColor;
                    alpha = 1f;
                    break;
                case ButtonState.Selected:
                    targetTextColor = selectedColor;
                    alpha = 1f;
                    break;
                case ButtonState.Disabled:
                    targetTextColor = disabledColor;
                    alpha = 0.4f;
                    break;
                case ButtonState.Preview:
                    targetTextColor = previewColor;
                    alpha = 0.6f;
                    break;
                case ButtonState.PreviousChoice:
                    targetTextColor = previousChoiceColor;
                    alpha = 0.7f;
                    break;
                default:
                    targetTextColor = availableColor;
                    alpha = 1f;
                    break;
            }
        }

        // ★★★ 只更新文字顏色，背景保持透明
        if (nameText != null)
        {
            var textColor = targetTextColor;
            textColor.a = alpha;
            nameText.color = textColor;
        }
    }

    /// <summary>
    /// ★★★ 保留舊版的滑鼠懸停邏輯 ★★★
    /// </summary>
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

    /// <summary>
    /// ★★★ 保留舊版的公開方法 ★★★
    /// </summary>
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

    /// <summary>
    /// ★★★ 新增：外部字體大小設定方法（會被忽略）★★★
    /// </summary>
    public void SetFontSize(float newFontSize)
    {
        float actualFontSize = GetFontSizeForTier();
        DebugLog($"SetFontSize({newFontSize}) ignored, using Tier{buttonTier} size: {actualFontSize}");

        if (nameText != null)
        {
            nameText.fontSize = actualFontSize;
            nameText.ForceMeshUpdate();
            AdjustTextBoxSize();
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[WheelUpgradeButton] {message}");
    }

    // ★★★ 保留舊版的 Context menu methods，加上新版診斷 ★★★
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
        Debug.Log($"BackgroundImage: {(backgroundImage != null ? "Y (Transparent)" : "N")}");
        Debug.Log($"IconImage: {(iconImage != null ? "Y (Hidden)" : "N")}");
        Debug.Log($"UpgradeOption: {(upgradeOption != null ? upgradeOption.upgradeName : "null")}");
        Debug.Log($"Current State: {currentState}");

        // ★★★ 新增：字體診斷
        Debug.Log($"Button Tier: {buttonTier}");
        Debug.Log($"Font Size for Tier: {GetFontSizeForTier()}");
        if (nameText != null)
        {
            Debug.Log($"Actual text fontSize: {nameText.fontSize}");
            Debug.Log($"Parent upgrade: {upgradeOption?.parentUpgradeName ?? "None"}");
        }
    }

    [ContextMenu("Force Update Visual")]
    public void ForceUpdateVisual()
    {
        UpdateVisualState();
        Debug.Log("Visual state forcefully updated");
    }

    [ContextMenu("Toggle Debug Bounds")]
    public void ToggleDebugBounds()
    {
        showTextBoxBounds = !showTextBoxBounds;
        SetupButton();
        Debug.Log($"Debug bounds: {(showTextBoxBounds ? "ON" : "OFF")}");
    }
}