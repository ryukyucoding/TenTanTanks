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
    [SerializeField] private Transform centerPoint;           // 中心點
    [SerializeField] private Transform tier1Container;        // 第二層容器
    [SerializeField] private Transform tier2Container;        // 第三層容器
    [SerializeField] private float tier1Radius = 320f;       // 第二層半徑
    [SerializeField] private float tier2Radius = 640f;       // 第三層半徑

    [Header("Upgrade Button")]
    [SerializeField] private GameObject upgradeButtonPrefab; // 升級按鈕預製體
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;

    [Header("Animation")]
    [SerializeField] private float wheelScaleInDuration = 0.5f;
    [SerializeField] private float buttonFadeInDuration = 0.3f;
    [SerializeField] private float buttonFadeDelay = 0.1f;

    private TankUpgradeSystem upgradeSystem;
    private List<UpgradeButton> currentButtons = new List<UpgradeButton>();
    private UpgradeOption selectedTier1Option;
    private UpgradeOption selectedTier2Option;
    private UpgradeState currentState = UpgradeState.SelectingTier1;

    // Store original center area properties to preserve them during animation
    private Vector3 originalCenterScale;
    private Vector3 originalCenterPosition;
    private bool centerPropertiesStored = false;

    private enum UpgradeState
    {
        SelectingTier1,  // 選擇第二層
        SelectingTier2,  // 選擇第三層
        Confirmed        // 已確認
    }

    void Start()
    {
        upgradeSystem = FindObjectOfType<TankUpgradeSystem>();
        SetupUI();
        StoreCenterProperties(); // Store center area properties on start
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

        // 設置背景模糊
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
        ShowTier1Options();

        UpdateTitle("選擇升級方向");
        UpdateDescription("選擇你想要的坦克升級路線");
    }

    public void HideWheel()
    {
        StartCoroutine(HideWheelAnimation());
    }

    private IEnumerator ShowWheelAnimation()
    {
        // 背景模糊效果
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

        // 輪盤縮放動畫 - FIXED VERSION
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
        // 輪盤縮放動畫（反向）
        if (wheelContainer != null)
        {
            float elapsed = 0f;
            Vector3 startScale = wheelContainer.transform.localScale;

            while (elapsed < wheelScaleInDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (wheelScaleInDuration * 0.5f);
                wheelContainer.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

                // Keep center area properties intact during hide animation
                RestoreCenterProperties();

                yield return null;
            }
        }

        // 背景模糊消失
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

    private void ShowTier1Options()
    {
        ClearCurrentButtons();

        if (upgradeSystem == null) return;

        var tier1Options = upgradeSystem.GetAvailableUpgrades(1);
        CreateButtonsInCircle(tier1Options, tier1Container, tier1Radius, OnTier1Selected);
    }

    private void ShowTier2Options(string parentUpgradeName)
    {
        ClearCurrentButtons();

        if (upgradeSystem == null) return;

        var tier2Options = upgradeSystem.GetAvailableUpgrades(2, parentUpgradeName);
        CreateButtonsInCircle(tier2Options, tier2Container, tier2Radius, OnTier2Selected);

        // 顯示確認按鈕
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false); // 等選擇第三層後才顯示
    }

    private void CreateButtonsInCircle(List<UpgradeOption> options, Transform container, float radius, System.Action<UpgradeOption> onClickCallback)
    {
        if (options.Count == 0) return;

        float angleStep = 360f / options.Count;
        float startAngle = -90f; // 從上方開始

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            float angle = startAngle + (angleStep * i);
            Vector3 position = GetCirclePosition(angle, radius);

            CreateUpgradeButton(option, container, position, onClickCallback, i * buttonFadeDelay);
        }
    }

    private Vector3 GetCirclePosition(float angle, float radius)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radian) * radius;
        float y = Mathf.Sin(radian) * radius;
        return new Vector3(x, y, 0f);
    }

    private void CreateUpgradeButton(UpgradeOption option, Transform container, Vector3 position, System.Action<UpgradeOption> onClickCallback, float delay)
    {
        if (upgradeButtonPrefab == null) return;

        GameObject buttonObj = Instantiate(upgradeButtonPrefab, container);
        buttonObj.transform.localPosition = position;

        var upgradeButton = buttonObj.GetComponent<UpgradeButton>();
        if (upgradeButton == null)
            upgradeButton = buttonObj.AddComponent<UpgradeButton>();

        upgradeButton.Setup(option, () => onClickCallback(option));
        currentButtons.Add(upgradeButton);

        // 淡入動畫
        StartCoroutine(FadeInButton(upgradeButton, delay));
    }

    private IEnumerator FadeInButton(UpgradeButton button, float delay)
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

    private void OnTier1Selected(UpgradeOption option)
    {
        selectedTier1Option = option;
        currentState = UpgradeState.SelectingTier2;

        UpdateTitle($"選擇 {option.upgradeName} 的變體");
        UpdateDescription(option.description);

        // 顯示第三層選項
        ShowTier2Options(option.upgradeName);
    }

    private void OnTier2Selected(UpgradeOption option)
    {
        selectedTier2Option = option;
        currentState = UpgradeState.Confirmed;

        UpdateTitle($"確認選擇: {option.upgradeName}");
        UpdateDescription(option.description);

        // 顯示確認按鈕
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(true);

        // 高亮選中的按鈕
        HighlightSelectedButton(option);
    }

    private void HighlightSelectedButton(UpgradeOption option)
    {
        foreach (var button in currentButtons)
        {
            if (button.GetUpgradeOption().upgradeName == option.upgradeName)
            {
                button.SetSelected(true);
            }
            else
            {
                button.SetSelected(false);
            }
        }
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

    private void ClearCurrentButtons()
    {
        foreach (var button in currentButtons)
        {
            if (button != null && button.gameObject != null)
                Destroy(button.gameObject);
        }
        currentButtons.Clear();
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
        if (currentState == UpgradeState.SelectingTier2)
        {
            currentState = UpgradeState.SelectingTier1;
            selectedTier1Option = null;
            selectedTier2Option = null;

            if (confirmButton != null)
                confirmButton.gameObject.SetActive(false);

            ShowTier1Options();
            UpdateTitle("選擇升級方向");
            UpdateDescription("選擇你想要的坦克升級路線");
        }
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