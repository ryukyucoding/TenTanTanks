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

    private const float FONT_SIZE = 30f;

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset minecraftFont;

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

        if (nameText != null)
        {
            nameText.text = option.upgradeName;

            Debug.Log($"[FONT] Setting up '{option.upgradeName}' with fontSize: {FONT_SIZE}");

            ApplyTextStyle(nameText);
            AdjustTextBoxSize();
        }
        else
        {
            CreateTextComponent(option.upgradeName);
        }

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        SetButtonState(ButtonState.Available);
        Debug.Log($"Setup completed for {option.upgradeName}");
    }

    private void ApplyTextStyle(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        Debug.Log($"[FONT] Before applying style - textComponent.fontSize: {textComponent.fontSize}");

        if (minecraftFont != null)
        {
            textComponent.font = minecraftFont;
            Debug.Log("✅ Applied Minecraft font");
        }

        textComponent.fontSize = FONT_SIZE;
        Debug.Log($"[FONT] Set fontSize to: {FONT_SIZE}");

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

    private void CreateTextComponent(string text)
    {
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(transform, false);

        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = text;

        ApplyTextStyle(nameText);

        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(textBoxWidth, textBoxHeight);
        rectTransform.anchoredPosition = Vector2.zero;

        AdjustTextBoxSize();

        Debug.Log("Created text component with Minecraft font");
    }

    public void SetButtonState(ButtonState state)
    {
        currentState = state;

        if (button != null)
        {
            button.interactable = (state != ButtonState.Disabled);
        }

        UpdateTextVisualState();
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
            return;
        }

        Debug.Log($"Text button clicked: {upgradeOption?.upgradeName}");
        onClickCallback?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTextVisualState();
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
    /// ★★★ 外部字體大小設定方法 - 完全忽略，使用固定大小 ★★★
    /// </summary>
    public void SetFontSize(float newFontSize)
    {
        Debug.Log($"[FONT] SetFontSize({newFontSize}) called but ignored, using fixed size: {FONT_SIZE}");

        if (nameText != null)
        {
            nameText.fontSize = FONT_SIZE; 
            nameText.ForceMeshUpdate();
            AdjustTextBoxSize();
            Debug.Log($"[FONT] Applied fixed font size {FONT_SIZE}");
        }
    }

    [ContextMenu("Check Font Size")]
    public void CheckFontSize()
    {
        Debug.Log("=== Font Size Check ===");
        Debug.Log($"Fixed FONT_SIZE constant: {FONT_SIZE}");
        if (nameText != null)
        {
            Debug.Log($"Actual nameText.fontSize: {nameText.fontSize}");
            Debug.Log($"Text: '{nameText.text}'");
        }
        else
        {
            Debug.Log("nameText is NULL");
        }
    }

    [ContextMenu("Force Apply Font Size")]
    public void ForceApplyFontSize()
    {
        if (nameText != null)
        {
            nameText.fontSize = FONT_SIZE;
            nameText.ForceMeshUpdate();
            Debug.Log($"✅ Force applied font size: {FONT_SIZE}");
        }
    }
}