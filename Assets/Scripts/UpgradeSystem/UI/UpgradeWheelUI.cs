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
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform tier1Container;
    [SerializeField] private Transform tier2Container;
    [SerializeField] private float tier1Radius = 320f;
    [SerializeField] private float tier2Radius = 480f;

    [Header("Upgrade Button")]
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;

    [Header("Animation")]
    [SerializeField] private float wheelScaleInDuration = 0.5f;
    [SerializeField] private float buttonFadeInDuration = 0.3f;
    [SerializeField] private float buttonFadeDelay = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

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

    // Transition mode variables
    private bool isTransitionMode = false;
    private int transitionAllowedTier = 1;
    private string transitionParentUpgrade = "";

    private enum UpgradeState
    {
        SelectingTier1,
        SelectingTier2,
        Confirmed
    }

    void Start()
    {
        upgradeSystem = FindObjectOfType<TankUpgradeSystem>();
        if (upgradeSystem == null)
        {
            Debug.LogError("TankUpgradeSystem not found! Make sure it exists in the scene.");
            return;
        }

        SetupUI();
        StoreCenterProperties();
        HideWheel();

        DebugLog("UpgradeWheelUI initialized successfully");
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[UpgradeWheelUI] {message}");
    }

    private void StoreCenterProperties()
    {
        if (centerPoint != null)
        {
            originalCenterScale = centerPoint.localScale;
            originalCenterPosition = centerPoint.localPosition;
            centerPropertiesStored = true;
            DebugLog($"Stored center properties: Position={originalCenterPosition}, Scale={originalCenterScale}");
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

        if (blurBackground != null)
        {
            var color = blurBackground.color;
            color.a = 0f;
            blurBackground.color = color;
        }

        DebugLog("UI setup completed");
    }

    #region Transition Mode Methods

    /// <summary>
    /// Set the wheel to transition mode with specific constraints
    /// </summary>
    public void SetTransitionMode(int allowedTier, string parentUpgrade = null)
    {
        isTransitionMode = true;
        transitionAllowedTier = allowedTier;
        transitionParentUpgrade = parentUpgrade ?? "";

        DebugLog($"Set to transition mode: Tier {allowedTier}, Parent: {transitionParentUpgrade}");

        // Update the state based on allowed tier
        if (allowedTier == 1)
        {
            currentState = UpgradeState.SelectingTier1;
        }
        else if (allowedTier == 2)
        {
            currentState = UpgradeState.SelectingTier2;
            // Set a dummy tier 1 selection to enable tier 2 buttons
            selectedTier1Option = new WheelUpgradeOption(transitionParentUpgrade, "", 1);
        }
    }

    /// <summary>
    /// Exit transition mode and return to normal wheel mode
    /// </summary>
    public void ExitTransitionMode()
    {
        isTransitionMode = false;
        transitionAllowedTier = 1;
        transitionParentUpgrade = "";
        currentState = UpgradeState.SelectingTier1;
    }

    #endregion

    #region Show/Hide Methods

    public void ShowWheel()
    {
        if (isTransitionMode)
        {
            ShowWheelForTransition();
        }
        else
        {
            ShowWheelNormal();
        }
    }

    private void ShowWheelNormal()
    {
        DebugLog("ShowWheel called for normal mode");

        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(true);

        if (!centerPropertiesStored)
            StoreCenterProperties();

        StartCoroutine(ShowWheelAnimation());
        currentState = UpgradeState.SelectingTier1;

        CreateAllUpgradeButtons();
        UpdateButtonStates();

        UpdateTitle("Select Upgrade");
        UpdateDescription("Please select an upgrade option");
    }

    public void ShowWheelForTransition()
    {
        DebugLog("ShowWheel called for transition mode");

        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(true);

        if (!centerPropertiesStored)
            StoreCenterProperties();

        StartCoroutine(ShowWheelAnimation());

        // Don't reset state if we're in transition mode
        if (!isTransitionMode)
            currentState = UpgradeState.SelectingTier1;

        CreateAllUpgradeButtonsWithMode();

        if (isTransitionMode)
        {
            UpdateButtonStatesTransition();
        }
        else
        {
            UpdateButtonStates();
        }

        UpdateTitle("Select Upgrade");
        UpdateDescription("Please select an upgrade option");
    }

    public void HideWheel()
    {
        DebugLog("HideWheel called");
        StartCoroutine(HideWheelAnimation());
    }

    #endregion

    #region Animation Methods

    private IEnumerator ShowWheelAnimation()
    {
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

        if (wheelContainer != null)
        {
            wheelContainer.transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < wheelScaleInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / wheelScaleInDuration);
                wheelContainer.transform.localScale = Vector3.one * progress;

                RestoreCenterProperties();
                yield return null;
            }
            wheelContainer.transform.localScale = Vector3.one;
            RestoreCenterProperties();
        }

        DebugLog("Show animation completed");
    }

    private IEnumerator HideWheelAnimation()
    {
        ClearAllButtons();

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

        DebugLog("Hide animation completed");
    }

    #endregion

    #region Button Creation Methods

    private void CreateAllUpgradeButtonsWithMode()
    {
        if (isTransitionMode)
        {
            CreateAllUpgradeButtonsTransition();
        }
        else
        {
            CreateAllUpgradeButtons();
        }
    }

    private void CreateAllUpgradeButtons()
    {
        if (upgradeSystem == null)
        {
            Debug.LogError("Cannot create buttons: TankUpgradeSystem is null");
            return;
        }

        DebugLog("Creating upgrade buttons...");

        var tier1Options = upgradeSystem.GetAvailableUpgrades(1);
        DebugLog($"Found {tier1Options.Count} tier 1 options");
        CreateTier1Buttons(tier1Options);

        var allTier2Options = new List<WheelUpgradeOption>();
        foreach (var tier1Option in tier1Options)
        {
            var tier2Options = upgradeSystem.GetAvailableUpgrades(2, tier1Option.upgradeName);
            allTier2Options.AddRange(tier2Options);
        }
        DebugLog($"Found {allTier2Options.Count} total tier 2 options");
        CreateTier2Buttons(allTier2Options);

        DebugLog($"Button creation completed: {tier1Buttons.Count} tier1, {tier2Buttons.Count} tier2");
    }

    private void CreateAllUpgradeButtonsTransition()
    {
        if (upgradeSystem == null)
        {
            Debug.LogError("Cannot create buttons: TankUpgradeSystem is null");
            return;
        }

        DebugLog("Creating upgrade buttons for transition mode...");

        var tier1Options = upgradeSystem.GetAvailableUpgrades(1);
        DebugLog($"Found {tier1Options.Count} tier 1 options");
        CreateTier1Buttons(tier1Options);

        if (transitionAllowedTier == 1)
        {
            var allTier2Options = new List<WheelUpgradeOption>();
            foreach (var tier1Option in tier1Options)
            {
                var tier2Options = upgradeSystem.GetAvailableUpgrades(2, tier1Option.upgradeName);
                allTier2Options.AddRange(tier2Options);
            }
            DebugLog($"Found {allTier2Options.Count} total tier 2 options for preview");
            CreateTier2Buttons(allTier2Options);
        }
        else if (transitionAllowedTier == 2)
        {
            var tier2Options = upgradeSystem.GetAvailableUpgrades(2, transitionParentUpgrade);
            DebugLog($"Found {tier2Options.Count} tier 2 options for parent '{transitionParentUpgrade}'");
            CreateTier2Buttons(tier2Options);
        }

        DebugLog($"Transition button creation completed: {tier1Buttons.Count} tier1, {tier2Buttons.Count} tier2");
    }

    private void CreateTier1Buttons(List<WheelUpgradeOption> options)
    {
        if (tier1Container == null)
        {
            Debug.LogError("Tier1Container is null! Cannot create tier 1 buttons.");
            return;
        }

        float angleStep = 120f;
        float startAngle = -90f;

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            float angle = startAngle + (angleStep * i);
            Vector3 position = GetCirclePosition(angle, tier1Radius);

            System.Action<WheelUpgradeOption> callback = isTransitionMode ? OnTier1SelectedTransition : OnTier1Selected;

            var button = CreateUpgradeButton(option, tier1Container, position, callback, i * buttonFadeDelay);
            if (button != null)
            {
                tier1Buttons.Add(button);
                DebugLog($"Created tier 1 button: {option.upgradeName} at position {position}");
            }
            else
            {
                Debug.LogError($"Failed to create tier 1 button for {option.upgradeName}");
            }
        }
    }

    private void CreateTier2Buttons(List<WheelUpgradeOption> options)
    {
        if (tier2Container == null)
        {
            Debug.LogError("Tier2Container is null! Cannot create tier 2 buttons.");
            return;
        }

        float angleStep = 60f;
        float startAngle = -120f;

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            float angle = startAngle + (angleStep * i);
            Vector3 position = GetCirclePosition(angle, tier2Radius);

            System.Action<WheelUpgradeOption> callback = isTransitionMode ? OnTier2SelectedTransition : OnTier2Selected;

            var button = CreateUpgradeButton(option, tier2Container, position, callback, i * buttonFadeDelay);
            if (button != null)
            {
                tier2Buttons.Add(button);
                DebugLog($"Created tier 2 button: {option.upgradeName} at position {position}");
            }
            else
            {
                Debug.LogError($"Failed to create tier 2 button for {option.upgradeName}");
            }
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
        if (upgradeButtonPrefab == null)
        {
            Debug.LogError("upgradeButtonPrefab is null! Cannot create buttons. Please assign the prefab in the inspector.");
            return null;
        }

        if (container == null)
        {
            Debug.LogError($"Container is null for {option.upgradeName}! Cannot create button.");
            return null;
        }

        GameObject buttonObj = Instantiate(upgradeButtonPrefab, container);
        if (buttonObj == null)
        {
            Debug.LogError($"Failed to instantiate button prefab for {option.upgradeName}");
            return null;
        }

        buttonObj.transform.localPosition = position;
        buttonObj.name = $"UpgradeButton_{option.upgradeName}";

        var upgradeButton = buttonObj.GetComponent<WheelUpgradeButton>();
        if (upgradeButton == null)
        {
            upgradeButton = buttonObj.AddComponent<WheelUpgradeButton>();
            DebugLog($"Added WheelUpgradeButton component to {option.upgradeName}");
        }

        upgradeButton.Setup(option, () => onClickCallback(option));
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

    #endregion

    #region Button State Management

    private void UpdateButtonStates()
    {
        foreach (var button in tier1Buttons)
        {
            if (button == null) continue;

            if (selectedTier1Option != null && button.GetUpgradeOption().upgradeName == selectedTier1Option.upgradeName)
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Selected);
            }
            else
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
            }
        }

        foreach (var button in tier2Buttons)
        {
            if (button == null) continue;

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

    private void UpdateButtonStatesTransition()
    {
        if (isTransitionMode)
        {
            if (transitionAllowedTier == 1)
            {
                UpdateTier1ButtonsForSelection();
                UpdateTier2ButtonsForPreview();
            }
            else if (transitionAllowedTier == 2)
            {
                UpdateTier1ButtonsForPreviousChoice();
                UpdateTier2ButtonsForSelection();
            }
        }
        else
        {
            UpdateButtonStates();
        }
    }

    private void UpdateTier1ButtonsForSelection()
    {
        foreach (var button in tier1Buttons)
        {
            if (button == null) continue;

            if (selectedTier1Option != null && button.GetUpgradeOption().upgradeName == selectedTier1Option.upgradeName)
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Selected);
            }
            else
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
            }
        }
    }

    private void UpdateTier2ButtonsForPreview()
    {
        foreach (var button in tier2Buttons)
        {
            if (button == null) continue;

            if (selectedTier1Option != null && button.GetUpgradeOption().parentUpgradeName == selectedTier1Option.upgradeName)
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Preview);
            }
            else
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
            }
        }
    }

    private void UpdateTier1ButtonsForPreviousChoice()
    {
        foreach (var button in tier1Buttons)
        {
            if (button == null) continue;

            if (button.GetUpgradeOption().upgradeName == transitionParentUpgrade)
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.PreviousChoice);
            }
            else
            {
                button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
            }
        }
    }

    private void UpdateTier2ButtonsForSelection()
    {
        foreach (var button in tier2Buttons)
        {
            if (button == null) continue;

            if (button.GetUpgradeOption().parentUpgradeName == transitionParentUpgrade)
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

    #endregion

    #region Selection Handlers

    private void OnTier1Selected(WheelUpgradeOption option)
    {
        selectedTier1Option = option;
        selectedTier2Option = null;
        currentState = UpgradeState.SelectingTier2;

        UpdateButtonStates();
        UpdateTitle($"Tier 1: {option.upgradeName}");
        UpdateDescription(option.description);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        DebugLog($"Tier 1 selected: {option.upgradeName}");
    }

    private void OnTier2Selected(WheelUpgradeOption option)
    {
        selectedTier2Option = option;
        currentState = UpgradeState.Confirmed;

        UpdateButtonStates();
        UpdateTitle($"Tier 2: {option.upgradeName}");
        UpdateDescription(option.description);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(true);

        DebugLog($"Tier 2 selected: {option.upgradeName}");
    }

    private void OnTier1SelectedTransition(WheelUpgradeOption option)
    {
        selectedTier1Option = option;

        if (isTransitionMode && transitionAllowedTier == 1)
        {
            UpdateButtonStatesTransition();
            UpdateTitle($"Selected: {option.upgradeName}");
            UpdateDescription(option.description);

            if (confirmButton != null)
                confirmButton.gameObject.SetActive(true);

            DebugLog($"Tier 1 selected in transition mode: {option.upgradeName}");
        }
        else
        {
            OnTier1Selected(option);
        }
    }

    private void OnTier2SelectedTransition(WheelUpgradeOption option)
    {
        selectedTier2Option = option;

        if (isTransitionMode && transitionAllowedTier == 2)
        {
            UpdateButtonStatesTransition();
            UpdateTitle($"Selected: {option.upgradeName}");
            UpdateDescription(option.description);

            if (confirmButton != null)
                confirmButton.gameObject.SetActive(true);

            DebugLog($"Tier 2 selected in transition mode: {option.upgradeName}");
        }
        else
        {
            OnTier2Selected(option);
        }
    }

    #endregion

    #region Confirmation

    private void ConfirmUpgrade()
    {
        if (isTransitionMode)
        {
            ConfirmUpgradeTransition();
        }
        else
        {
            if (selectedTier2Option != null && upgradeSystem != null)
            {
                upgradeSystem.ApplyUpgrade(selectedTier2Option.upgradeName);
                HideWheel();
                DebugLog($"Upgrade confirmed: {selectedTier2Option.upgradeName}");
            }
            else
            {
                Debug.LogError("Cannot confirm upgrade: selectedTier2Option or upgradeSystem is null");
            }
        }
    }

    public void ConfirmUpgradeTransition()
    {
        WheelUpgradeOption selectedOption = null;

        if (isTransitionMode)
        {
            if (transitionAllowedTier == 1 && selectedTier1Option != null)
            {
                selectedOption = selectedTier1Option;
            }
            else if (transitionAllowedTier == 2 && selectedTier2Option != null)
            {
                selectedOption = selectedTier2Option;
            }

            if (selectedOption != null)
            {
                DebugLog($"Transition upgrade confirmed: {selectedOption.upgradeName}");

                var transitionManager = FindFirstObjectByType<TransitionWheelUpgrade>();
                if (transitionManager != null)
                {
                    transitionManager.OnUpgradeSelected(selectedOption);
                }
            }
        }
    }

    #endregion

    #region Utility Methods

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

        DebugLog("All buttons cleared");
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
        UpdateTitle("Select Upgrade");
        UpdateDescription("Please select an upgrade option");

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        DebugLog("Back to tier 1 selection");
    }

    #endregion

    #region Debug Methods

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

    [ContextMenu("Test Button Creation")]
    public void TestButtonCreation()
    {
        DebugLog("=== Testing Button Creation ===");
        DebugLog($"UpgradeButtonPrefab: {(upgradeButtonPrefab != null ? "y" : "n")}");
        DebugLog($"Tier1Container: {(tier1Container != null ? "y" : "n")}");
        DebugLog($"Tier2Container: {(tier2Container != null ? "y" : "n")}");
        DebugLog($"UpgradeSystem: {(upgradeSystem != null ? "y" : "n")}");

        if (upgradeSystem != null)
        {
            var tier1Options = upgradeSystem.GetAvailableUpgrades(1);
            DebugLog($"Available tier 1 upgrades: {tier1Options.Count}");
            foreach (var option in tier1Options)
            {
                DebugLog($"  - {option.upgradeName}: {option.description}");
            }
        }
    }

    #endregion
}