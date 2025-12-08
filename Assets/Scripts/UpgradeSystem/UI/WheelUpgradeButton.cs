using WheelUpgradeSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class WheelUpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    [Header("Text Box Settings")]
    [SerializeField] private float textBoxWidth = 120f; // 文字框寬度
    [SerializeField] private float textBoxHeight = 40f; // 文字框高度
    [SerializeField] private bool autoAdjustWidth = true; // 自動調整寬度
    [SerializeField] private float paddingHorizontal = 10f; // 左右邊距
    [SerializeField] private bool allowUnlimitedWidth = false; // ★★★ 允許無限寬度 ★★★
    [SerializeField] private float maxWidthLimit = 200f; // 最大寬度限制（當不允許無限寬度時）

    [Header("Text Visual States")]
    [SerializeField] private Color availableTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = Color.yellow;
    [SerializeField] private Color disabledTextColor = Color.gray;
    [SerializeField] private Color hoverTextColor = Color.cyan;
    [SerializeField] private Color previewTextColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    [SerializeField] private Color previousChoiceTextColor = new Color(0.9f, 0.7f, 0.2f);

    [Header("Font Settings")]
    [SerializeField] private float fontSize = 16f;
    [SerializeField] private TMP_FontAsset minecraftFont;

    [Header("Text Effects")]
    [SerializeField] private bool useTextOutline = true;
    [SerializeField] private float outlineWidth = 0.2f;
    [SerializeField] private Color outlineColor = Color.black;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showTextBoxBounds = false; // 顯示文字框邊界（除錯用）

    public enum ButtonState
    {
        Available,
        Selected,
        Disabled,
        Preview,
        PreviousChoice
    }

    private WheelUpgradeOption upgradeOption;
    private Action onClickCallback;
    private ButtonState currentState = ButtonState.Available;
    private bool isHovered = false;

    void Awake()
    {
        AutoFindComponents();
        SetupTextOnlyButton();
        LoadMinecraftFont();
    }

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

    private void SetupTextOnlyButton()
    {
        // 隱藏背景圖片但保留組件（用於點擊檢測區域）
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

        // 設定按鈕為透明
        if (button != null)
        {
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

            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }

        DebugLog("Text-only button setup completed");
    }

    private void AutoFindComponents()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

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

        DebugLog($"Auto-found components: Button={button != null}, Text={nameText != null}, BG={backgroundImage != null}");
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

        if (nameText != null)
        {
            nameText.text = option.upgradeName;
            ApplyMinecraftTextStyle(nameText);
            AdjustTextBoxSize(); // 調整文字框大小
        }
        else
        {
            CreateTextComponent(option.upgradeName);
        }

        // 隱藏圖標
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        SetButtonState(ButtonState.Available);
        DebugLog($"Setup completed for {option.upgradeName}");
    }

    /// <summary>
    /// 調整文字框大小（不影響按鈕定位）
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

            // ★★★ 根據設定決定是否限制最大寬度 ★★★
            if (!allowUnlimitedWidth)
            {
                finalWidth = Mathf.Min(finalWidth, maxWidthLimit);
                DebugLog($"Width limited to {maxWidthLimit}");
            }
            else
            {
                DebugLog($"Unlimited width allowed, actual width: {finalWidth}");
            }
        }

        // 設定文字框大小（中心點不變）
        rectTransform.sizeDelta = new Vector2(finalWidth, textBoxHeight);

        DebugLog($"Text box adjusted: {finalWidth} x {textBoxHeight} for '{nameText.text}' " +
                $"(unlimited: {allowUnlimitedWidth})");

        // 同時調整按鈕的點擊區域（可選）
        AdjustButtonClickArea(finalWidth);
    }

    /// <summary>
    /// 調整按鈕的點擊區域（可選）
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

        DebugLog($"Click area adjusted: {clickAreaWidth} x {clickAreaHeight}");
    }

    private void ApplyMinecraftTextStyle(TextMeshProUGUI textComponent)
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

        // ★★★ 額外的防護設定 ★★★
        textComponent.enableAutoSizing = false; // 禁用自動調整字體大小
        textComponent.textWrappingMode = TMPro.TextWrappingModes.NoWrap; // 強制不換行

        if (useTextOutline)
        {
            textComponent.outlineWidth = outlineWidth;
            textComponent.outlineColor = outlineColor;
        }

        textComponent.ForceMeshUpdate();
    }

    private void CreateTextComponent(string text)
    {
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(transform, false);

        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = text;

        ApplyMinecraftTextStyle(nameText);

        // 設定初始大小和位置
        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(textBoxWidth, textBoxHeight);
        rectTransform.anchoredPosition = Vector2.zero;

        // 調整大小
        AdjustTextBoxSize();

        DebugLog("Created text component with Minecraft font");
    }

    public void SetButtonState(ButtonState state)
    {
        currentState = state;

        if (button != null)
        {
            button.interactable = (state != ButtonState.Disabled);
        }

        UpdateTextVisualState();
        DebugLog($"Button state changed to {state} for {upgradeOption?.upgradeName}");
    }

    private void UpdateTextVisualState()
    {
        if (nameText == null) return;

        Color targetTextColor;
        float alpha = 1f;

        if (isHovered && (currentState == ButtonState.Available || currentState == ButtonState.Preview))
        {
            targetTextColor = hoverTextColor;
            alpha = currentState == ButtonState.Preview ? 0.8f : 1f;
        }
        else
        {
            switch (currentState)
            {
                case ButtonState.Available:
                    targetTextColor = availableTextColor;
                    alpha = 1f;
                    break;
                case ButtonState.Selected:
                    targetTextColor = selectedTextColor;
                    alpha = 1f;
                    break;
                case ButtonState.Disabled:
                    targetTextColor = disabledTextColor;
                    alpha = 0.4f;
                    break;
                case ButtonState.Preview:
                    targetTextColor = previewTextColor;
                    alpha = 0.6f;
                    break;
                case ButtonState.PreviousChoice:
                    targetTextColor = previousChoiceTextColor;
                    alpha = 0.7f;
                    break;
                default:
                    targetTextColor = availableTextColor;
                    alpha = 1f;
                    break;
            }
        }

        var textColor = targetTextColor;
        textColor.a = alpha;
        nameText.color = textColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnButtonClick();
    }

    private void OnButtonClick()
    {
        if (currentState == ButtonState.Disabled ||
            currentState == ButtonState.Preview ||
            currentState == ButtonState.PreviousChoice)
        {
            DebugLog($"Button {upgradeOption?.upgradeName} click ignored - button state is {currentState}");
            return;
        }

        DebugLog($"Text button clicked: {upgradeOption?.upgradeName}");
        onClickCallback?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTextVisualState();
        DebugLog($"Mouse entered text button: {upgradeOption?.upgradeName}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateTextVisualState();
        DebugLog($"Mouse exited text button: {upgradeOption?.upgradeName}");
    }

    public WheelUpgradeOption GetUpgradeOption()
    {
        return upgradeOption;
    }

    public ButtonState GetCurrentState()
    {
        return currentState;
    }

    public void SetSelected(bool selected)
    {
        SetButtonState(selected ? ButtonState.Selected : ButtonState.Available);
    }

    /// <summary>
    /// ★★★ 外部字體大小設定方法 - 現在會被忽略，使用自己的設定 ★★★
    /// </summary>
    public void SetFontSize(float newFontSize)
    {
        // ★★★ 完全忽略外部字體大小，永遠使用自己的 fontSize 設定 ★★★
        DebugLog($"✅ External font size {newFontSize} ignored, using own setting: {fontSize}");

        // 如果文字組件已經存在，套用自己的字體大小
        if (nameText != null)
        {
            nameText.fontSize = fontSize; // 使用自己的 fontSize，不是傳入的 newFontSize
            nameText.ForceMeshUpdate();
            AdjustTextBoxSize(); // 重新調整文字框大小
            DebugLog($"✅ Applied own font size {fontSize} to {nameText.text}");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[WheelUpgradeButton] {message}");
    }

    [ContextMenu("Adjust Text Box Size")]
    public void ManualAdjustTextBoxSize()
    {
        AdjustTextBoxSize();
        Debug.Log("Text box size manually adjusted");
    }

    [ContextMenu("Toggle Debug Bounds")]
    public void ToggleDebugBounds()
    {
        showTextBoxBounds = !showTextBoxBounds;
        SetupTextOnlyButton();
        Debug.Log($"Debug bounds: {(showTextBoxBounds ? "ON" : "OFF")}");
    }

    [ContextMenu("Check Text Components")]
    public void CheckComponents()
    {
        Debug.Log("=== WheelUpgradeButton Text Component Check ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Button: {(button != null ? "Y" : "N")}");
        Debug.Log($"NameText: {(nameText != null ? "Y" : "N")}");
        Debug.Log($"Text Box Size: {textBoxWidth} x {textBoxHeight}");
        Debug.Log($"Auto Adjust Width: {autoAdjustWidth}");
        Debug.Log($"Minecraft Font: {(minecraftFont != null ? minecraftFont.name : "NULL")}");
        Debug.Log($"Current State: {currentState}");

        // ★★★ 加上字體大小檢查 ★★★
        Debug.Log($"Button fontSize field: {fontSize}");

        if (nameText != null)
        {
            var rect = nameText.GetComponent<RectTransform>();
            Debug.Log($"Actual Text Size: {rect.sizeDelta}");
            Debug.Log($"Actual text fontSize: {nameText.fontSize}");
            Debug.Log($"Text content: '{nameText.text}'");
        }
    }

    [ContextMenu("Force Apply Own Font Size")]
    public void ForceApplyOwnFontSize()
    {
        if (nameText != null)
        {
            nameText.fontSize = fontSize;
            nameText.ForceMeshUpdate();
            AdjustTextBoxSize();
            Debug.Log($"✅ Force applied own font size: {fontSize}");
        }
        else
        {
            Debug.Log("❌ nameText is NULL, cannot apply font size");
        }
    }
}