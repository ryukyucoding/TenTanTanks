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
    [SerializeField] private float textBoxWidth = 120f;
    [SerializeField] private float textBoxHeight = 40f;
    [SerializeField] private bool autoAdjustWidth = true;
    [SerializeField] private float paddingHorizontal = 10f;
    [SerializeField] private bool allowUnlimitedWidth = false;
    [SerializeField] private float maxWidthLimit = 200f;

    [Header("Text Visual States")]
    [SerializeField] private Color availableTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = Color.yellow;
    [SerializeField] private Color disabledTextColor = Color.gray;
    [SerializeField] private Color hoverTextColor = Color.cyan;
    [SerializeField] private Color previewTextColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    [SerializeField] private Color previousChoiceTextColor = new Color(0.9f, 0.7f, 0.2f);

    // ★★★ 根據層級設定不同字體大小 ★★★
    private const float TIER1_FONT_SIZE = 30f; // 第一層字體大小  
    private const float TIER2_FONT_SIZE = 20f; // 第二層字體大小
    private const float DEFAULT_FONT_SIZE = 25f; // 預設字體大小

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset minecraftFont;

    // ★★★ 按鈕層級（在 Setup 時自動判斷）★★★
    private int buttonTier = 1;

    [Header("Text Effects")]
    [SerializeField] private bool useTextOutline = true;
    [SerializeField] private float outlineWidth = 0.2f;
    [SerializeField] private Color outlineColor = Color.black;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showTextBoxBounds = false;

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
                Debug.Log("✅ Successfully loaded Minecraft font: " + minecraftFont.name);
            }
            else
            {
                Debug.Log("❌ Failed to load Minecraft font");
            }
        }
    }

    private void SetupTextOnlyButton()
    {
        if (backgroundImage != null)
        {
            if (showTextBoxBounds)
            {
                backgroundImage.color = new Color(1f, 0f, 0f, 0.2f);
            }
            else
            {
                backgroundImage.color = Color.clear;
            }
        }

        if (button != null)
        {
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (showTextBoxBounds)
                {
                    buttonImage.color = new Color(0f, 1f, 0f, 0.2f);
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

        Debug.Log("Text-only button setup completed");
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

        Debug.Log($"Auto-found components: Button={button != null}, Text={nameText != null}, BG={backgroundImage != null}");
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

        // ★★★ 自動判斷按鈕層級 ★★★
        DetermineButtonTier(option);
        float fontSize = GetFontSizeForTier();

        if (nameText != null)
        {
            nameText.text = option.upgradeName;

            Debug.Log($"[FONT] Setting up '{option.upgradeName}' as Tier{buttonTier} with fontSize: {fontSize}");

            ApplyTextStyle(nameText, fontSize);
            AdjustTextBoxSize();
        }
        else
        {
            CreateTextComponent(option.upgradeName, fontSize);
        }

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        SetButtonState(ButtonState.Available);
        Debug.Log($"Setup completed for {option.upgradeName} (Tier{buttonTier})");
    }

    /// <summary>
    /// ★★★ 根據升級選項判斷按鈕層級（與 UpgradeWheelUI 邏輯相容）★★★
    /// </summary>
    private void DetermineButtonTier(WheelUpgradeOption option)
    {
        // 優先級1：檢查 tier 屬性
        if (option.tier > 1)
        {
            buttonTier = option.tier;
            Debug.Log($"[TIER] {option.upgradeName} is Tier {option.tier} (from tier property)");
            return;
        }

        // 優先級2：檢查 parentUpgradeName
        if (!string.IsNullOrEmpty(option.parentUpgradeName))
        {
            buttonTier = 2;
            Debug.Log($"[TIER] {option.upgradeName} is Tier 2 (parent: {option.parentUpgradeName})");
            return;
        }

        // 優先級3：從升級名稱推斷（備用方案）
        string upgradeName = option.upgradeName?.ToLower() ?? "";
        if (upgradeName.Contains("superheavy") || upgradeName.Contains("armorpiercing") ||
            upgradeName.Contains("machinegun") || upgradeName.Contains("burst") ||
            upgradeName.Contains("versatile") || upgradeName.Contains("tactical"))
        {
            buttonTier = 2;
            Debug.Log($"[TIER] {option.upgradeName} is Tier 2 (inferred from name)");
            return;
        }

        // 預設為 Tier 1
        buttonTier = 1;
        Debug.Log($"[TIER] {option.upgradeName} is Tier 1 (default)");
    }

    /// <summary>
    /// ★★★ 根據層級取得對應的字體大小 ★★★
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

    private void ApplyTextStyle(TextMeshProUGUI textComponent, float fontSize)
    {
        if (textComponent == null) return;

        Debug.Log($"[FONT] Before applying style - textComponent.fontSize: {textComponent.fontSize}");

        if (minecraftFont != null)
        {
            textComponent.font = minecraftFont;
            Debug.Log("✅ Applied Minecraft font");
        }

        textComponent.fontSize = fontSize;
        Debug.Log($"[FONT] Set fontSize to: {fontSize}");

        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;

        // 防換行設定
        textComponent.enableWordWrapping = false;
        textComponent.overflowMode = TextOverflowModes.Overflow;
        textComponent.enableAutoSizing = false;
        textComponent.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

        if (useTextOutline)
        {
            textComponent.outlineWidth = outlineWidth;
            textComponent.outlineColor = outlineColor;
        }

        textComponent.ForceMeshUpdate();

        Debug.Log($"[FONT] After applying style - textComponent.fontSize: {textComponent.fontSize}");
    }

    private void AdjustTextBoxSize()
    {
        if (nameText == null) return;

        var rectTransform = nameText.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        float finalWidth = textBoxWidth;

        if (autoAdjustWidth)
        {
            nameText.ForceMeshUpdate();
            Vector2 textSize = nameText.GetRenderedValues(false);
            finalWidth = textSize.x + paddingHorizontal * 2f;
            finalWidth = Mathf.Max(finalWidth, 60f);

            if (!allowUnlimitedWidth)
            {
                finalWidth = Mathf.Min(finalWidth, maxWidthLimit);
            }
        }

        rectTransform.sizeDelta = new Vector2(finalWidth, textBoxHeight);
        AdjustButtonClickArea(finalWidth);
    }

    private void AdjustButtonClickArea(float textWidth)
    {
        if (button == null) return;

        var buttonRect = button.GetComponent<RectTransform>();
        if (buttonRect == null) return;

        float clickAreaWidth = textWidth + 20f;
        float clickAreaHeight = textBoxHeight + 10f;

        buttonRect.sizeDelta = new Vector2(clickAreaWidth, clickAreaHeight);
    }

    private void CreateTextComponent(string text, float fontSize)
    {
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(transform, false);

        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = text;

        ApplyTextStyle(nameText, fontSize);

        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(textBoxWidth, textBoxHeight);
        rectTransform.anchoredPosition = Vector2.zero;

        AdjustTextBoxSize();

        Debug.Log("Created text component with Minecraft font");
    }

    // ★★★ 為了與舊版相容，保留原始的 CreateTextComponent 方法 ★★★
    private void CreateTextComponent(string text)
    {
        // 使用預設字體大小創建
        float defaultSize = GetFontSizeForTier();
        CreateTextComponent(text, defaultSize);
        Debug.Log($"Created text component with default font size: {defaultSize}");
    }

    public void SetButtonState(ButtonState state)
    {
        currentState = state;

        if (button != null)
        {
            button.interactable = (state != ButtonState.Disabled);
        }

        UpdateTextVisualState();
        Debug.Log($"[STATE] Button {upgradeOption?.upgradeName} (Tier{buttonTier}) state changed to {state}");
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
            Debug.Log($"[CLICK] Button {upgradeOption?.upgradeName} (Tier{buttonTier}) click ignored - state is {currentState}");
            return;
        }

        Debug.Log($"[CLICK] Button {upgradeOption?.upgradeName} (Tier{buttonTier}) clicked!");
        onClickCallback?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTextVisualState();
        Debug.Log($"[HOVER] Mouse entered {upgradeOption?.upgradeName} (Tier{buttonTier})");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateTextVisualState();
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
    /// ★★★ 外部字體大小設定方法（會被忽略，使用層級固定大小）★★★
    /// </summary>
    public void SetFontSize(float newFontSize)
    {
        float actualFontSize = GetFontSizeForTier();
        Debug.Log($"[FONT] SetFontSize({newFontSize}) ignored, using Tier{buttonTier} size: {actualFontSize}");

        if (nameText != null)
        {
            nameText.fontSize = actualFontSize;
            nameText.ForceMeshUpdate();
            AdjustTextBoxSize();
            Debug.Log($"[FONT] Applied Tier{buttonTier} font size {actualFontSize}");
        }
    }

    [ContextMenu("Check Font Size")]
    public void CheckFontSize()
    {
        Debug.Log("=== Font Size Check ===");
        Debug.Log($"Button Tier: {buttonTier}");
        Debug.Log($"Tier 1 size: {TIER1_FONT_SIZE}");
        Debug.Log($"Tier 2 size: {TIER2_FONT_SIZE}");
        Debug.Log($"Current font size for this tier: {GetFontSizeForTier()}");
        if (nameText != null)
        {
            Debug.Log($"Actual nameText.fontSize: {nameText.fontSize}");
            Debug.Log($"Text: '{nameText.text}'");
            if (upgradeOption != null)
            {
                Debug.Log($"Option tier property: {upgradeOption.tier}");
                Debug.Log($"Parent upgrade: {upgradeOption.parentUpgradeName ?? "None"}");
            }
        }
        else
        {
            Debug.Log("nameText is NULL");
        }
    }

    [ContextMenu("Check Button State")]
    public void CheckButtonState()
    {
        Debug.Log("=== Button State Check ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Button Tier: {buttonTier}");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Button Interactable: {(button != null ? button.interactable.ToString() : "No Button")}");
        Debug.Log($"Upgrade Option: {(upgradeOption != null ? upgradeOption.upgradeName : "NULL")}");
        if (upgradeOption != null)
        {
            Debug.Log($"Parent Upgrade: {upgradeOption.parentUpgradeName ?? "None"}");
            Debug.Log($"Option Tier: {upgradeOption.tier}");
        }
    }

    [ContextMenu("Force Apply Font Size")]
    public void ForceApplyFontSize()
    {
        float fontSize = GetFontSizeForTier();
        if (nameText != null)
        {
            nameText.fontSize = fontSize;
            nameText.ForceMeshUpdate();
            Debug.Log($"✅ Force applied Tier{buttonTier} font size: {fontSize}");
        }
    }
}