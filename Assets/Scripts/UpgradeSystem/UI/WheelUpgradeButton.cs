using WheelUpgradeSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class WheelUpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Button button; // 保留但隱藏，只用於點擊檢測
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage; // 將被隱藏

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
    [SerializeField] private bool useTextGlow = true; // 文字發光效果
    [SerializeField] private float glowIntensity = 0.3f;
    [SerializeField] private bool useTextOutline = true; // 文字邊框
    [SerializeField] private float outlineWidth = 0.2f;
    [SerializeField] private Color outlineColor = Color.black;

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
        SetupTextOnlyButton();
        LoadMinecraftFont();
    }

    /// <summary>
    /// 設定純文字按鈕 - 隱藏背景，只顯示文字
    /// </summary>
    private void SetupTextOnlyButton()
    {
        // 隱藏背景圖片但保留組件（用於點擊檢測區域）
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.clear; // 完全透明
            DebugLog("Background image set to transparent");
        }

        // 如果有 Button 組件，設定為無背景
        if (button != null)
        {
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.clear; // 透明按鈕背景
            }

            // 設定按鈕為無過渡效果
            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }

        DebugLog("Text-only button setup completed");
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
                DebugLog("❌ Failed to load Minecraft font from Resources/Fonts/MinecraftTen-VGORe SDF");
            }
        }
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

        DebugLog($"Auto-found components: Button={button != null}, Text={nameText != null}, BG={backgroundImage != null}, Icon={iconImage != null}");
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
        }
        else
        {
            CreateTextComponent(option.upgradeName);
        }

        // 隱藏圖標（純文字模式）
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        SetButtonState(ButtonState.Available);
        DebugLog($"Setup completed for {option.upgradeName}");
    }

    private void ApplyMinecraftTextStyle(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        // 套用 Minecraft 字體
        if (minecraftFont != null)
        {
            textComponent.font = minecraftFont;
            DebugLog($"✅ Applied Minecraft font to {textComponent.name}");
        }

        // 設定字體樣式
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;

        // 設定文字邊框
        if (useTextOutline)
        {
            textComponent.outlineWidth = outlineWidth;
            textComponent.outlineColor = outlineColor;
        }

        // 設定文字發光（使用 Material 屬性）
        if (useTextGlow)
        {
            // 這會讓文字在選中或懸停時有發光效果
            textComponent.fontMaterial = textComponent.fontSharedMaterial;
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

        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

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

    /// <summary>
    /// 只更新文字的視覺狀態，不處理背景
    /// </summary>
    private void UpdateTextVisualState()
    {
        if (nameText == null) return;

        Color targetTextColor;
        float alpha = 1f;
        float glowPower = 0f;

        // 根據狀態和懸停設定顏色
        if (isHovered && (currentState == ButtonState.Available || currentState == ButtonState.Preview))
        {
            targetTextColor = hoverTextColor;
            alpha = currentState == ButtonState.Preview ? 0.8f : 1f;
            glowPower = useTextGlow ? glowIntensity : 0f;
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
                    glowPower = useTextGlow ? glowIntensity * 2f : 0f; // 選中時發光更強
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

        // 套用文字顏色
        var textColor = targetTextColor;
        textColor.a = alpha;
        nameText.color = textColor;

        // 套用發光效果（如果啟用）
        if (useTextGlow && nameText.fontMaterial != null)
        {
            nameText.fontMaterial.SetFloat("_GlowPower", glowPower);
            nameText.fontMaterial.SetColor("_GlowColor", targetTextColor);
        }
    }

    // 實現 IPointerClickHandler 來處理點擊
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

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[WheelUpgradeButton] {message}");
    }

    [ContextMenu("Test Text Button Click")]
    public void TestButtonClick()
    {
        OnButtonClick();
    }

    [ContextMenu("Toggle Text Glow")]
    public void ToggleTextGlow()
    {
        useTextGlow = !useTextGlow;
        UpdateTextVisualState();
        Debug.Log($"Text glow: {(useTextGlow ? "ON" : "OFF")}");
    }

    [ContextMenu("Check Text Components")]
    public void CheckComponents()
    {
        Debug.Log("=== WheelUpgradeButton Text Component Check ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Button: {(button != null ? "Y (Hidden)" : "N")}");
        Debug.Log($"NameText: {(nameText != null ? "Y" : "N")}");
        Debug.Log($"Background: {(backgroundImage != null ? "Y (Transparent)" : "N")}");
        Debug.Log($"Minecraft Font: {(minecraftFont != null ? minecraftFont.name : "NULL")}");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Text Glow: {(useTextGlow ? "ON" : "OFF")}");
    }
}