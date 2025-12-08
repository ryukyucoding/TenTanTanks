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
        }

        SetupButtons();
        HideWheelInstant();
    }

    private void SetupButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideWheel);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmUpgrade);
        }
    }

    #region Debug Methods
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[UpgradeWheelUI] {message}");
        }
    }
    #endregion

    #region Transition Mode Methods

    /// <summary>
    /// Set the wheel to transition mode for use in transition scenes
    /// </summary>
    /// <param name="allowedTier">Which tier can be selected (1 or 2)</param>
    /// <param name="parentUpgrade">Parent upgrade name for tier 2 selections</param>
    public void SetTransitionMode(int allowedTier, string parentUpgrade = "")
    {
        isTransitionMode = true;
        transitionAllowedTier = allowedTier;
        transitionParentUpgrade = string.IsNullOrEmpty(parentUpgrade) ? "" : parentUpgrade;
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
            DebugLog($"Set selectedTier1Option to: {transitionParentUpgrade}");

            // ★★★ NEW: 高亮已選擇的 Tier 1 選項 ★★★
            HighlightSelectedTier1Option(transitionParentUpgrade);

            // ★★★ 關鍵：生成並顯示對應的 Tier 2 升級選項 ★★★
            GenerateAndShowTier2Options(transitionParentUpgrade);
        }

        // added to update
        UpdateButtonStatesTransition();
        DebugLog("✅ Button states updated after SetTransitionMode");
    }

    private void HighlightSelectedTier1Option(string selectedUpgrade)
    {
        DebugLog($"Highlighting Tier 1 option: {selectedUpgrade}");

        if (tier1Container == null)
        {
            DebugLog("❌ Tier1Container is null, cannot highlight");
            return;
        }

        // 確保 Tier 1 container 是可見的
        tier1Container.gameObject.SetActive(true);

        // 找到所有 Tier 1 按鈕並設定狀態
        foreach (var button in tier1Buttons)
        {
            if (button != null && button.GetUpgradeOption() != null)
            {
                string buttonUpgradeName = button.GetUpgradeOption().upgradeName;

                if (buttonUpgradeName.Equals(selectedUpgrade, System.StringComparison.OrdinalIgnoreCase))
                {
                    // ✅ 使用 PreviousChoice 狀態來高亮選中的按鈕（橙色）
                    button.SetButtonState(WheelUpgradeButton.ButtonState.PreviousChoice);
                    DebugLog($"✅ Set as PreviousChoice: {buttonUpgradeName}");
                }
                else
                {
                    // ✅ 使用 Disabled 狀態讓其他按鈕變暗
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
                    DebugLog($"Set as Disabled: {buttonUpgradeName}");
                }
            }
        }

        DebugLog($"✅ Highlighted Tier 1 selection complete");
    }

    private void GenerateAndShowTier2Options(string parentUpgrade)
    {
        DebugLog($"Generating Tier 2 options for parent: {parentUpgrade}");

        // 只清除 Tier 2 按鈕，保留 Tier 1 按鈕的高亮狀態
        ClearTier2ButtonsOnly();

        // 根據父級升級生成對應的 Tier 2 選項
        List<WheelUpgradeOption> tier2Options = GetTier2OptionsForParent(parentUpgrade);

        if (tier2Options.Count == 0)
        {
            DebugLog($"❌ No Tier 2 options found for parent: {parentUpgrade}");
            return;
        }

        DebugLog($"✅ Found {tier2Options.Count} Tier 2 options for {parentUpgrade}");

        // 創建並顯示 Tier 2 按鈕
        CreateTier2Buttons(tier2Options);

        // 確保 Tier 2 container 是可見的
        if (tier2Container != null)
        {
            tier2Container.gameObject.SetActive(true);
            DebugLog("Activated Tier 2 container");
        }
    }

    /// <summary>
    /// 獲取指定父級升級的 Tier 2 選項列表
    /// </summary>
    private List<WheelUpgradeOption> GetTier2OptionsForParent(string parentUpgrade)
    {
        List<WheelUpgradeOption> tier2Options = new List<WheelUpgradeOption>();

        switch (parentUpgrade.ToLower())
        {
            case "heavy":
                tier2Options.Add(CreateSuperHeavyOption());
                tier2Options.Add(CreateArmorPiercingOption());
                break;

            case "rapid":
                tier2Options.Add(CreateMachineGunOption());
                tier2Options.Add(CreateBurstOption());
                break;

            case "balanced":
                tier2Options.Add(CreateVersatileOption());
                tier2Options.Add(CreateTacticalOption());
                break;

            default:
                DebugLog($"❌ Unknown parent upgrade: {parentUpgrade}");
                // 如果找不到特定的 Tier 2 選項，使用現有的系統方法
                if (upgradeSystem != null)
                {
                    tier2Options = upgradeSystem.GetAvailableUpgrades(2, parentUpgrade);
                }
                break;
        }

        return tier2Options;
    }

    /// <summary>
    /// 只清除 Tier 2 按鈕，保留 Tier 1 按鈕
    /// </summary>
    private void ClearTier2ButtonsOnly()
    {
        foreach (var button in tier2Buttons)
        {
            if (button != null && button.gameObject != null)
                DestroyImmediate(button.gameObject);
        }
        tier2Buttons.Clear();
        DebugLog("Cleared Tier 2 buttons only");
    }

    // === Tier 2 level up options^^ ===

    private WheelUpgradeOption CreateSuperHeavyOption()
    {
        return new WheelUpgradeOption
        {
            upgradeName = "SuperHeavy",
            description = "Super heavy barrel - Extreme damage, very slow",
            tier = 2,
            parentUpgradeName = "Heavy",
            damageMultiplier = 3f,
            fireRateMultiplier = 0.3f,
            bulletSizeMultiplier = 2f,
            moveSpeedMultiplier = 0.6f,
            healthBonus = 50,
            tankColor = new Color(0.9f, 0.2f, 0.2f)
        };
    }

    private WheelUpgradeOption CreateArmorPiercingOption()
    {
        return new WheelUpgradeOption
        {
            upgradeName = "ArmorPiercing",
            description = "Armor piercing barrel - Penetrates armor",
            tier = 2,
            parentUpgradeName = "Heavy",
            damageMultiplier = 1.6f,
            fireRateMultiplier = 0.8f,
            bulletSizeMultiplier = 1.2f,
            moveSpeedMultiplier = 0.9f,
            healthBonus = 10,
            tankColor = new Color(0.7f, 0.3f, 0.3f)
        };
    }

    private WheelUpgradeOption CreateMachineGunOption()
    {
        return new WheelUpgradeOption
        {
            upgradeName = "MachineGun",
            description = "Machine gun barrel - Extreme fire rate",
            tier = 2,
            parentUpgradeName = "Rapid",
            damageMultiplier = 0.3f,
            fireRateMultiplier = 5f,
            bulletSizeMultiplier = 0.5f,
            moveSpeedMultiplier = 1.4f,
            healthBonus = -40,
            tankColor = new Color(0.2f, 0.9f, 0.2f)
        };
    }

    private WheelUpgradeOption CreateBurstOption()
    {
        return new WheelUpgradeOption
        {
            upgradeName = "Burst",
            description = "Burst barrel - Three-shot burst",
            tier = 2,
            parentUpgradeName = "Rapid",
            damageMultiplier = 0.8f,
            fireRateMultiplier = 2f,
            bulletSizeMultiplier = 0.8f,
            moveSpeedMultiplier = 1.1f,
            healthBonus = -10,
            tankColor = new Color(0.3f, 0.7f, 0.3f)
        };
    }

    private WheelUpgradeOption CreateVersatileOption()
    {
        return new WheelUpgradeOption
        {
            upgradeName = "Versatile",
            description = "Versatile barrel - All-around improvement",
            tier = 2,
            parentUpgradeName = "Balanced",
            damageMultiplier = 1.4f,
            fireRateMultiplier = 1.8f,
            bulletSizeMultiplier = 1.1f,
            moveSpeedMultiplier = 1.2f,
            healthBonus = 5,
            tankColor = new Color(0.5f, 0.3f, 0.9f)
        };
    }

    private WheelUpgradeOption CreateTacticalOption()
    {
        return new WheelUpgradeOption
        {
            upgradeName = "Tactical",
            description = "Tactical barrel - Balanced with speed focus",
            tier = 2,
            parentUpgradeName = "Balanced",
            damageMultiplier = 1.2f,
            fireRateMultiplier = 1.5f,
            bulletSizeMultiplier = 1f,
            moveSpeedMultiplier = 1.3f,
            healthBonus = 0,
            tankColor = new Color(0.4f, 0.4f, 0.9f)
        };
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
        CreateAllUpgradeButtonsWithMode();
        UpdateButtonStatesTransition();

        if (transitionAllowedTier == 1)
        {
            UpdateTitle("Choose Tank Upgrade");
            UpdateDescription("Select your tank upgrade for this level");
        }
        else
        {
            UpdateTitle("Choose Advanced Upgrade");
            UpdateDescription($"Select advanced upgrade for {transitionParentUpgrade}");
        }
    }

    public void HideWheel()
    {
        DebugLog("HideWheel called - starting hide process");

        // Stop all running coroutines that might interfere
        StopAllCoroutines();

        StartCoroutine(HideWheelAnimation());
    }

    private void HideWheelInstant()
    {
        DebugLog("HideWheelInstant called");

        if (upgradeCanvas != null)
        {
            upgradeCanvas.gameObject.SetActive(false);
            DebugLog("Instantly deactivated upgrade canvas");
        }

        if (wheelContainer != null)
        {
            wheelContainer.SetActive(false);
            DebugLog("Instantly deactivated wheel container");
        }

        ClearAllButtons();
    }

    #endregion

    #region Animation Coroutines

    private IEnumerator ShowWheelAnimation()
    {
        DebugLog("Starting show animation");

        if (wheelContainer != null)
        {
            wheelContainer.SetActive(true);
            wheelContainer.transform.localScale = Vector3.zero;
        }

        if (blurBackground != null)
        {
            Color startColor = blurBackground.color;
            startColor.a = 0f;
            blurBackground.color = startColor;

            float elapsed = 0f;
            Color endColor = startColor;
            endColor.a = 0.8f;

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
            float elapsed = 0f;
            Vector3 targetScale = Vector3.one;

            while (elapsed < wheelScaleInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / wheelScaleInDuration);
                wheelContainer.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
                RestoreCenterProperties();
                yield return null;
            }

            wheelContainer.transform.localScale = targetScale;
            RestoreCenterProperties();
        }

        yield return StartCoroutine(AnimateButtonsIn());

        DebugLog("Show animation completed");
    }

    private IEnumerator HideWheelAnimation()
    {
        DebugLog("Starting hide animation");

        // Immediately clear buttons to prevent interaction
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

            wheelContainer.transform.localScale = Vector3.zero;
            wheelContainer.SetActive(false);
            DebugLog("Wheel container hidden and deactivated");
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
        {
            upgradeCanvas.gameObject.SetActive(false);
            DebugLog("Upgrade canvas deactivated");
        }

        DebugLog("Hide animation completed");
    }

    private IEnumerator AnimateButtonsIn()
    {
        List<WheelUpgradeButton> allButtons = new List<WheelUpgradeButton>();
        allButtons.AddRange(tier1Buttons);
        allButtons.AddRange(tier2Buttons);

        // Use CanvasGroup for fade animation
        foreach (var button in allButtons)
        {
            if (button != null)
            {
                var canvasGroup = button.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
            }
        }

        yield return new WaitForSeconds(buttonFadeDelay);

        for (int i = 0; i < allButtons.Count; i++)
        {
            if (allButtons[i] != null)
            {
                StartCoroutine(FadeInButton(allButtons[i]));
                yield return new WaitForSeconds(buttonFadeDelay);
            }
        }
    }

    private IEnumerator FadeInButton(WheelUpgradeButton button)
    {
        var canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.SmoothStep(0f, 1f, elapsed / buttonFadeInDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;
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

            var button = CreateUpgradeButton(option, position, tier1Container, callback);
            if (button != null)
            {
                button.transform.SetParent(tier1Container, false);
                tier1Buttons.Add(button);
                DebugLog($"Created Tier1 button: {option.upgradeName} at position {position}");
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
        float startAngle = -150f;

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            float angle = startAngle + (angleStep * i);
            Vector3 position = GetCirclePosition(angle, tier2Radius);

            System.Action<WheelUpgradeOption> callback = isTransitionMode ? OnTier2SelectedTransition : OnTier2Selected;

            var button = CreateUpgradeButton(option, position, tier2Container, callback);
            if (button != null)
            {
                button.transform.SetParent(tier2Container, false);
                tier2Buttons.Add(button);
                DebugLog($"Created Tier2 button: {option.upgradeName} at position {position}");
            }
        }
    }

    private WheelUpgradeButton CreateUpgradeButton(
    WheelUpgradeOption option,
    Vector3 position,
    Transform parent,
    System.Action<WheelUpgradeOption> onClickCallback)
    {
        GameObject buttonGO = Instantiate(upgradeButtonPrefab, parent, false);
        buttonGO.transform.localPosition = position;

        var upgradeButton = buttonGO.GetComponent<WheelUpgradeButton>();
        upgradeButton.Setup(option, () => onClickCallback(option));

        return upgradeButton;
    }


    private Vector3 GetCirclePosition(float angle, float radius)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Sin(radians) * radius,
            Mathf.Cos(radians) * radius,
            0f
        );
    }

    private void ClearAllButtons()
    {
        DebugLog("Clearing all buttons");

        foreach (var button in tier1Buttons)
        {
            if (button != null && button.gameObject != null)
                DestroyImmediate(button.gameObject);
        }

        foreach (var button in tier2Buttons)
        {
            if (button != null && button.gameObject != null)
                DestroyImmediate(button.gameObject);
        }

        tier1Buttons.Clear();
        tier2Buttons.Clear();

        DebugLog("All buttons cleared");
    }

    #endregion

    #region Selection Handling

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

        DebugLog($"Tier 2 selected: {option.upgradeName}");
        UpdateButtonStates();
        UpdateTitle($"Ready to Confirm");
        UpdateDescription($"Confirm upgrade: {option.upgradeName}?");
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

            // Apply tank transformation immediately
            TankTransformationManager transformManager = FindFirstObjectByType<TankTransformationManager>();
            if (transformManager != null)
            {
                string upgradeName = option.upgradeName.ToLower();
                switch (upgradeName)
                {
                    case "heavy":
                        transformManager.SelectHeavyUpgrade();
                        break;
                    case "rapid":
                        transformManager.SelectRapidUpgrade();
                        break;
                    case "balanced":
                        transformManager.SelectBalancedUpgrade();
                        break;
                }
                DebugLog($"Applied tank transformation: {option.upgradeName}");
            }
            else
            {
                Debug.LogError("TankTransformationManager not found! Tank appearance will not change.");
            }

            // In transition mode, immediately trigger the transition manager
            var transitionManager = FindFirstObjectByType<TransitionWheelUpgrade>();
            if (transitionManager != null)
            {
                DebugLog($"Calling TransitionWheelUpgrade.OnUpgradeSelected for: {option.upgradeName}");
                transitionManager.OnUpgradeSelected(option);
            }
            else
            {
                DebugLog("ERROR: TransitionWheelUpgrade not found! Cannot continue transition.");
            }
        }
        else
        {
            OnTier1Selected(option);
        }
    }

    private void OnTier2SelectedTransition(WheelUpgradeOption option)
    {
        if (isTransitionMode && transitionAllowedTier == 2)
        {
            DebugLog($"Tier 2 selected in transition mode: {option.upgradeName}");

            // ★★★ NEW: Apply Tier 2 tank transformation immediately ★★★
            TankTransformationManager transformManager = FindFirstObjectByType<TankTransformationManager>();
            if (transformManager != null)
            {
                string upgradeName = option.upgradeName.ToLower();
                switch (upgradeName)
                {
                    // Heavy Tier 2 upgrades
                    case "armorpiercing":
                        transformManager.SelectArmorPiercingUpgrade();
                        break;
                    case "superheavy":
                        transformManager.SelectSuperHeavyUpgrade();
                        break;

                    // Rapid Tier 2 upgrades
                    case "burst":
                        transformManager.SelectBurstUpgrade();
                        break;
                    case "machinegun":
                        transformManager.SelectMachineGunUpgrade();
                        break;

                    // Balanced Tier 2 upgrades
                    case "tactical":
                        transformManager.SelectTacticalUpgrade();
                        break;
                    case "versatile":
                        transformManager.SelectVersatileUpgrade();
                        break;

                    default:
                        Debug.LogWarning($"Unknown Tier 2 upgrade: {option.upgradeName}");
                        break;
                }
                DebugLog($"Applied Tier 2 tank transformation: {option.upgradeName}");
            }
            else
            {
                Debug.LogError("TankTransformationManager not found! Tank appearance will not change.");
            }

            // In transition mode, immediately trigger the transition manager
            var transitionManager = FindFirstObjectByType<TransitionWheelUpgrade>();
            if (transitionManager != null)
            {
                DebugLog($"Calling TransitionWheelUpgrade.OnUpgradeSelected for: {option.upgradeName}");
                transitionManager.OnUpgradeSelected(option);
            }
            else
            {
                DebugLog("ERROR: TransitionWheelUpgrade not found! Cannot continue transition.");
            }
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

                // Hide the wheel first
                HideWheel();

                var transitionManager = FindFirstObjectByType<TransitionWheelUpgrade>();
                if (transitionManager != null)
                {
                    DebugLog($"Calling TransitionWheelUpgrade.OnUpgradeSelected for: {selectedOption.upgradeName}");
                    transitionManager.OnUpgradeSelected(selectedOption);
                }
                else
                {
                    DebugLog("ERROR: TransitionWheelUpgrade not found! Cannot continue transition.");
                }
            }
            else
            {
                DebugLog("ERROR: No option selected for confirmation!");
            }
        }
    }

    #endregion

    #region Button State Management

    private void UpdateButtonStates()
    {
        UpdateTier1ButtonStates();
        UpdateTier2ButtonStates();
    }

    private void UpdateButtonStatesTransition()
    {
        DebugLog($"UpdateButtonStatesTransition called - AllowedTier: {transitionAllowedTier}, Parent: {transitionParentUpgrade}");

        if (transitionAllowedTier == 1)
        {
            foreach (var button in tier1Buttons)
            {
                if (button != null)
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
                }
            }

            foreach (var button in tier2Buttons)
            {
                if (button != null)
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Preview);
                }
            }
        }
        else if (transitionAllowedTier == 2)
        {
            // Tier 1 按鈕設為不可用（除了已選擇的）
            foreach (var button in tier1Buttons)
            {
                if (button != null)
                {
                    string buttonName = button.GetUpgradeOption()?.upgradeName ?? "";
                    if (buttonName.Equals(transitionParentUpgrade, System.StringComparison.OrdinalIgnoreCase))
                    {
                        button.SetButtonState(WheelUpgradeButton.ButtonState.PreviousChoice);
                        DebugLog($"✅ Set Tier1 button {buttonName} as PreviousChoice");
                    }
                    else
                    {
                        button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
                        DebugLog($"Set Tier1 button {buttonName} as Disabled");
                    }
                }
            }

            // Tier 2 按鈕：只有符合父升級的才可用
            foreach (var button in tier2Buttons)
            {
                if (button != null && button.GetUpgradeOption() != null)
                {
                    string parentName = button.GetUpgradeOption().parentUpgradeName ?? "";
                    string buttonName = button.GetUpgradeOption().upgradeName ?? "";

                    if (parentName.Equals(transitionParentUpgrade, System.StringComparison.OrdinalIgnoreCase))
                    {
                        button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
                        DebugLog($"✅ Set Tier2 button {buttonName} as Available (parent: {parentName})");
                    }
                    else
                    {
                        button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
                        DebugLog($"Set Tier2 button {buttonName} as Disabled (wrong parent: {parentName})");
                    }
                }
            }
        }

        DebugLog("UpdateButtonStatesTransition completed");
    }

    private void UpdateTier1ButtonStates()
    {
        foreach (var button in tier1Buttons)
        {
            if (button != null)
            {
                bool isSelected = selectedTier1Option != null && button.GetUpgradeOption().upgradeName == selectedTier1Option.upgradeName;

                if (isSelected)
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Selected);
                }
                else if (currentState == UpgradeState.SelectingTier1)
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
                }
                else
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
                }
            }
        }
    }

    private void UpdateTier2ButtonStates()
    {
        foreach (var button in tier2Buttons)
        {
            if (button != null)
            {
                bool isValidForSelectedTier1 = selectedTier1Option != null && button.GetUpgradeOption().parentUpgradeName == selectedTier1Option.upgradeName;
                bool isSelected = selectedTier2Option != null && button.GetUpgradeOption().upgradeName == selectedTier2Option.upgradeName;

                if (isSelected)
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Selected);
                }
                else if (currentState == UpgradeState.SelectingTier2 && isValidForSelectedTier1)
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Available);
                }
                else
                {
                    button.SetButtonState(WheelUpgradeButton.ButtonState.Disabled);
                }
            }
        }
    }

    #endregion

    #region Center Properties Management

    private void StoreCenterProperties()
    {
        if (centerPoint != null && !centerPropertiesStored)
        {
            originalCenterScale = centerPoint.localScale;
            originalCenterPosition = centerPoint.localPosition;
            centerPropertiesStored = true;
            DebugLog("Center properties stored for preservation during animation");
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

    #endregion

    #region UI Text Updates

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

    #endregion

    #region Context Menu Testing

    [ContextMenu("Test Show Wheel")]
    public void TestShowWheel()
    {
        ShowWheel();
    }

    [ContextMenu("Test Hide Wheel")]
    public void TestHideWheel()
    {
        HideWheel();
    }

    [ContextMenu("Test Transition Mode Tier 1")]
    public void TestTransitionModeTier1()
    {
        SetTransitionMode(1);
        ShowWheelForTransition();
    }

    [ContextMenu("Test Transition Mode Tier 2")]
    public void TestTransitionModeTier2()
    {
        SetTransitionMode(2, "Heavy");
        ShowWheelForTransition();
    }

    #endregion
}