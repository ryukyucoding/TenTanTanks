using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class UpgradeSegment : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image segmentImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;

    [Header("Visual States")]
    [SerializeField] private Color availableColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0f, 0.9f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
    [SerializeField] private Color hoverColor = new Color(0f, 1f, 1f, 0.7f);

    public enum SegmentState
    {
        Available,   // Can be clicked
        Selected,    // Currently selected
        Disabled     // Cannot be clicked (grayed out)
    }

    private UpgradeOption upgradeOption;
    private Action onClickCallback;
    private SegmentState currentState = SegmentState.Available;

    void Awake()
    {
        // Auto-find components if not assigned
        if (segmentImage == null)
            segmentImage = GetComponent<Image>();

        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(UpgradeOption option, Action clickCallback)
    {
        upgradeOption = option;
        onClickCallback = clickCallback;

        // Set text
        if (nameText != null)
        {
            nameText.text = option.upgradeName;
        }

        // Set icon if available
        if (iconImage != null && option.icon != null)
        {
            iconImage.sprite = option.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        // Set initial state
        SetSegmentState(SegmentState.Available);
    }

    public void SetSegmentState(SegmentState state)
    {
        currentState = state;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        Color targetColor;
        float textAlpha = 1f;

        switch (currentState)
        {
            case SegmentState.Available:
                targetColor = availableColor;
                textAlpha = 1f;
                break;
            case SegmentState.Selected:
                targetColor = selectedColor;
                textAlpha = 1f;
                break;
            case SegmentState.Disabled:
                targetColor = disabledColor;
                textAlpha = 0.5f;
                break;
            default:
                targetColor = availableColor;
                textAlpha = 1f;
                break;
        }

        // Apply color to segment
        if (segmentImage != null)
        {
            segmentImage.color = targetColor;
        }

        // Apply alpha to text
        if (nameText != null)
        {
            var textColor = nameText.color;
            textColor.a = textAlpha;
            nameText.color = textColor;
        }

        // Apply alpha to icon
        if (iconImage != null)
        {
            var iconColor = iconImage.color;
            iconColor.a = textAlpha;
            iconImage.color = iconColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == SegmentState.Selected || currentState == SegmentState.Disabled)
            return;

        if (segmentImage != null)
        {
            segmentImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == SegmentState.Selected || currentState == SegmentState.Disabled)
            return;

        UpdateVisualState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentState == SegmentState.Disabled) return;

        onClickCallback?.Invoke();
    }

    public UpgradeOption GetUpgradeOption()
    {
        return upgradeOption;
    }

    public SegmentState GetCurrentState()
    {
        return currentState;
    }
}