using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    [Header("Visual States")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color hoverColor = Color.cyan;

    private UpgradeOption upgradeOption;
    private Action onClickCallback;
    private bool isSelected = false;

    void Awake()
    {
        // Auto-find components if not assigned
        if (button == null)
            button = GetComponent<Button>();

        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // Set up button click
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    public void Setup(UpgradeOption option, Action clickCallback)
    {
        upgradeOption = option;
        onClickCallback = clickCallback;

        // Set button text
        if (nameText != null)
        {
            nameText.text = option.upgradeName;
        }

        // Set button icon if available
        if (iconImage != null && option.icon != null)
        {
            iconImage.sprite = option.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        // Set initial visual state
        SetSelected(false);
    }

    private void OnButtonClick()
    {
        onClickCallback?.Invoke();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }

    public void SetHover(bool hover)
    {
        if (isSelected) return; // Don't change color if selected

        if (backgroundImage != null)
        {
            backgroundImage.color = hover ? hoverColor : normalColor;
        }
    }

    public UpgradeOption GetUpgradeOption()
    {
        return upgradeOption;
    }

    // Optional: Add hover effects
    public void OnPointerEnter()
    {
        SetHover(true);
    }

    public void OnPointerExit()
    {
        SetHover(false);
    }
}