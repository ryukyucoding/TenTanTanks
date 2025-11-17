using WheelUpgradeSystem;

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class UpgradeWheelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private GameObject wheelContainer;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Background Blur")]
    [SerializeField] private Image blurBackground;
    [SerializeField] private float blurAnimationDuration = 0.3f;

    [Header("Wheel Layout")]
    [SerializeField] private Transform centerPoint;           // �����I
    [SerializeField] private Transform tier1Container;        // �ĤG�h�e��
    [SerializeField] private Transform tier2Container;        // �ĤT�h�e��
    [SerializeField] private float tier1Radius = 320f;       // �ĤG�h�b�|
    [SerializeField] private float tier2Radius = 480f;       // �ĤT�h�b�|

    [Header("Upgrade Button")]
    [SerializeField] private GameObject upgradeButtonPrefab; // �ɯū��s�w�s��
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;

    [Header("Animation")]
    [SerializeField] private float wheelScaleInDuration = 0.5f;
    [SerializeField] private float buttonFadeInDuration = 0.3f;
    [SerializeField] private float buttonFadeDelay = 0.1f;

    private TankUpgradeSystem upgradeSystem;
    private List<WheelUpgradeButton> tier1Buttons = new List<WheelUpgradeButton>();
    private List<WheelUpgradeButton> tier2Buttons = new List<WheelUpgradeButton>();
    private WheelUpgradeOption selectedTier1Option;
    private WheelUpgradeOption selectedTier2Option;
    private UpgradeState currentState = UpgradeState.SelectingTier1;

    // Store original center area properties to preserve them during animation
    private Vector3 originalCenterScale;
    private Vector3 originalCenterPosition;
    private bool centerPropertiesStored = false;

    private enum UpgradeState
    {
        SelectingTier1,  // ��ܲĤG�h
        SelectingTier2,  // ��ܲĤT�h
        Confirmed        // �w�T�{
    }

    void Start()
    {
        upgradeSystem = FindObjectOfType<TankUpgradeSystem>();
        SetupUI();
        StoreCenterProperties();
        HideWheel();
    }

    private void StoreCenterProperties()
    {
        if (centerPoint != null)
        {
            originalCenterScale = centerPoint.localScale;
            originalCenterPosition = centerPoint.localPosition;
            centerPropertiesStored = true;
            Debug.Log($"Stored center properties: Position={originalCenterPosition}, Scale={originalCenterScale}");
        }
    }

    private void RestoreCenterProperties()
    {
        if (centerPoint != null && centerPropertiesStored)
        {
            centerPoint.localScale = originalCenterScale;
            centerPoint.localPosition = originalCenterPosition;
        }
    }

    private void SetupUI()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(HideWheel);

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmUpgrade);
            confirmButton.gameObject.SetActive(false);
        }

        // �]�m�I���ҽk
        if (blurBackground != null)
        {
            var color = blurBackground.color;
            color.a = 0f;
            blurBackground.color = color;
        }
    }

    public void ShowWheel()
    {
        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(true);

        // Store center properties if not already stored
        if (!centerPropertiesStored)
            StoreCenterProperties();

        StartCoroutine(ShowWheelAnimation());
        currentState = UpgradeState.SelectingTier1;

        // Create all buttons at once
        CreateAllUpgradeButtons();
        UpdateButtonStates();

        UpdateTitle("��ܤɯŤ�V");
        UpdateDescription("��ܧA�Q�n���Z�J�ɯŸ��u");
    }

    public void HideWheel()
    {
        StartCoroutine(HideWheelAnimation());
    }

    private IEnumerator ShowWheelAnimation()
    {
        // �I���ҽk�ĪG
        if (blurBackground != null)
        {
            float elapsed = 0f;
            Color startColor = blurBackground.color;
            Color endColor = startColor;
            endColor.a = 0.7f;

            while (elapsed < blurAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / blurAnimationDuration;
                blurBackground.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            blurBackground.color = endColor;
        }

        // ���L�Y��ʵe
        if (wheelContainer != null)
        {
            wheelContainer.transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < wheelScaleInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / wheelScaleInDuration);
                wheelContainer.transform.localScale = Vector3.one * progress;

                // Keep center area properties intact during animation
                RestoreCenterProperties();

                yield return null;
            }
            wheelContainer.transform.localScale = Vector3.one;

            // Final restoration to ensure center area is correctly positioned
            RestoreCenterProperties();
        }
    }

    private IEnumerator HideWheelAnimation()
    {
        // Clear all buttons
        ClearAllButtons();

        // ���L�Y��ʵe�]�ϦV�^
        if (wheelContainer != null)
        {
            float elapsed = 0f;
            Vector3 startScale = wheelContainer.transform.localScale;

            while (elapsed < wheelScaleInDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (wheelScaleInDuration * 0.5f);
                wheelContainer.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

                RestoreCenterProperties();

                yield return null;
            }
        }

        // �I���ҽk����
        if (blurBackground != null)
        {
            float elapsed = 0f;
            Color startColor = blurBackground.color;
            Color endColor = startColor;
            endColor.a = 0f;

            while (elapsed < blurAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / blurAnimationDuration;
                blurBackground.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            blurBackground.color = endColor;
        }

        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(false);
    }

    private void CreateAllUpgradeButtons()
    {
        if (upgradeSystem == null) return;

        // Create Tier 1 buttons (3 buttons at 120�X intervals)
        var tier1Options = upgradeSystem.GetAvailableUpgrades(1);
        CreateTier1Buttons(tier1Options);

        // Create Tier 2 buttons (6 buttons at 60�X intervals)
        var allTier2Options = new List<WheelUpgradeOption>();
        foreach (var tier1Option in tier1Options)
        {
            var tier2Options = upgradeSystem.GetAvailableUpgrades(2, tier1Option.upgradeName);
            allTier2Options.AddRange(tier2Options);
        }
        CreateTier2Buttons(allTier2Options);
    }

    private void CreateTier1Buttons(List<WheelUpgradeOption> options)
    {
        // 3 buttons at 120�X intervals starting from top
        float angleStep = 120f;
        float startAngle = -90f; // Start from top

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            float angle = startAngle + (angleStep * i);
            Vector3 position = GetCirclePosition(angle, tier1Radius);

            var button = CreateUpgradeButton(option, tier1Container, position, OnTier1Selected, i * buttonFadeDelay);
            tier1Buttons.Add(button);
        }
    }

    private void CreateTier2Buttons(List<WheelUpgradeOption> options)
    {
        // 6 buttons at 60�X intervals starting from top
        float angleStep = 60f;
        float startAngle = -120f; // Start from top

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            float angle = startAngle + (angleStep * i);
            Vector3 position = GetCirclePosition(angle, tier2Radius);

            var button = CreateUpgradeButton(option, tier2Container, position, OnTier2Selected, i * buttonFadeDelay);
            tier2Buttons.Add(button);
        }
    }

    private Vector3 GetCirclePosition(float angle, float radius)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radian) * radius;
        float y = Mathf.Sin(radian) * radius;
        return new Vector3(x, y, 0f);
    }

    private WheelUpgradeButton CreateUpgradeButton(WheelUpgradeOption option, Transform container, Vector3 position, System.Action<WheelUpgradeOption> onClickCallback, float delay)
    {
        if (upgradeButtonPrefab == null) return null;

        GameObject buttonObj = Instantiate(upgradeButtonPrefab, container);
        buttonObj.transform.localPosition = position;

        var upgradeButton = buttonObj.GetComponent<WheelUpgradeButton>();
        if (upgradeButton == null)
            upgradeButton = buttonObj.AddComponent<WheelUpgradeButton>();

        upgradeButton.Setup(option, () => onClickCallback(option));

        // �H�J�ʵe
        StartCoroutine(FadeInButton(upgradeButton, delay));

        return upgradeButton;
    }

    private IEnumerator FadeInButton(WheelUpgradeButton button, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        var canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / buttonFadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void UpdateButtonStates()
    {
        // Update Tier 1 buttons
        foreach (var button in tier1Buttons)
        {
            if (selectedTier1Option != null && button.GetUpgradeOption().upgradeName == selectedTier1Option.upgradeName)
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Selected);
            }
            else
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
            }
        }

        // Update Tier 2 buttons
        foreach (var button in tier2Buttons)
        {
            if (selectedTier1Option != null && button.GetUpgradeOption().parentUpgradeName == selectedTier1Option.upgradeName)
            {
                if (selectedTier2Option != null && button.GetUpgradeOption().upgradeName == selectedTier2Option.upgradeName)
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Selected);
                }
                else
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
                }
            }
            else
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
            }
        }
    }

    private void OnTier1Selected(WheelUpgradeOption option)
    {
        selectedTier1Option = option;
        selectedTier2Option = null; // Reset tier 2 selection
        currentState = UpgradeState.SelectingTier2;

        UpdateButtonStates();
        UpdateTitle($"��� {option.upgradeName} ������");
        UpdateDescription(option.description);

        // Hide confirm button until tier 2 is selected
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);
    }

    private void OnTier2Selected(WheelUpgradeOption option)
    {
        selectedTier2Option = option;
        currentState = UpgradeState.Confirmed;

        UpdateButtonStates();
        UpdateTitle($"確認選擇: {option.upgradeName}");
        UpdateDescription(option.description);

        // Show confirm button
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(true);
    }

    private void ConfirmUpgrade()
    {
        if (selectedTier2Option != null && upgradeSystem != null)
        {
            upgradeSystem.ApplyUpgrade(selectedTier2Option.upgradeName);
            HideWheel();

            Debug.Log($"Upgrade confirmed: {selectedTier2Option.upgradeName}");
        }
    }

    private void ClearAllButtons()
    {
        foreach (var button in tier1Buttons)
        {
            if (button != null && button.gameObject != null)
                Destroy(button.gameObject);
        }
        tier1Buttons.Clear();

        foreach (var button in tier2Buttons)
        {
            if (button != null && button.gameObject != null)
                Destroy(button.gameObject);
        }
        tier2Buttons.Clear();
    }

    private void UpdateTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    private void UpdateDescription(string description)
    {
        if (descriptionText != null)
            descriptionText.text = description;
    }

    public void BackToTier1()
    {
        selectedTier1Option = null;
        selectedTier2Option = null;
        currentState = UpgradeState.SelectingTier1;

        UpdateButtonStates();
        UpdateTitle("��ܤɯŤ�V");
        UpdateDescription("��ܧA�Q�n���Z�J�ɯŸ��u");

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);
    }

    // Debug method to manually fix center area positioning
    [ContextMenu("Fix Center Area Position")]
    public void FixCenterAreaPosition()
    {
        if (centerPoint != null)
        {
            centerPoint.localPosition = Vector3.zero;
            centerPoint.localScale = Vector3.one;
            Debug.Log("Center area position manually fixed");
        }
    }
}