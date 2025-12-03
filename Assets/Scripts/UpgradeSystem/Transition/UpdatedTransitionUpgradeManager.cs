using UnityEngine;
using UnityEngine.UI;
using WheelUpgradeSystem;
using TMPro;
using System.Collections;

/// <summary>
/// 轉場升級管理器 - 整合現有的坦克模型系統
/// 支援在Level 2→3, 4→5轉場時顯示升級選項
/// </summary>
public class TransitionUpgradeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UpgradeWheelUI upgradeWheelUI;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Button confirmUpgradeButton;
    [SerializeField] private Button cancelUpgradeButton;
    [SerializeField] private TextMeshProUGUI confirmationText;

    [Header("Tank References")]
    [SerializeField] private Transform playerTank;
    [SerializeField] private ModularTankController modularTankController;

    [Header("Tank Model References")]
    [SerializeField] private GameObject armTankModel;     // ArmTank.gltf
    [SerializeField] private GameObject baseModel;        // Base.gltf  
    [SerializeField] private GameObject barrelModel;      // Barrel.gltf
    [SerializeField] private GameObject doubleheadModel;  // doublehead
    [SerializeField] private GameObject fourheadModel;    // fourhead
    [SerializeField] private GameObject hugeModel;        // HUGE
    [SerializeField] private GameObject smallModel;       // SMALL

    [Header("Upgrade Configuration")]
    [SerializeField] private bool enableTransitionUpgrades = true;
    [SerializeField] private float pauseAtCenterX = 0f;
    [SerializeField] private float pauseDetectionRange = 1f;

    private TankUpgradeSystem upgradeSystem;
    private TransitionMover transitionMover;
    private WheelUpgradeOption selectedUpgrade;
    private string targetScene;
    private bool isUpgradeInProgress = false;
    private bool upgradeCompleted = false;

    // 轉場類型
    private enum TransitionType
    {
        Level2To3,  // Basic → 第一層 (doublehead, HUGE, SMALL)
        Level4To5   // 第一層 → 第二層 (fourhead, HUGE-3turrets, SMALL-3turrets)
    }
    private TransitionType currentTransitionType;

    void Awake()
    {
        FindRequiredComponents();
    }

    void Start()
    {
        DetermineTransitionType();
        SetupConfirmationDialog();
        HideAllUpgradeUI();
        Debug.Log($"[TransitionUpgradeManager] 初始化完成，轉場類型: {currentTransitionType}");
    }

    void Update()
    {
        if (!enableTransitionUpgrades || isUpgradeInProgress || upgradeCompleted)
            return;

        CheckTankPosition();
    }

    /// <summary>
    /// 尋找必要的組件
    /// </summary>
    private void FindRequiredComponents()
    {
        upgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();
        transitionMover = FindFirstObjectByType<TransitionMover>();

        if (upgradeWheelUI == null)
            upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();

        if (playerTank == null)
        {
            GameObject tank = GameObject.FindGameObjectWithTag("Player");
            if (tank != null) playerTank = tank.transform;
        }

        if (modularTankController == null && playerTank != null)
            modularTankController = playerTank.GetComponent<ModularTankController>();
    }

    /// <summary>
    /// 根據目標場景確定轉場類型
    /// </summary>
    private void DetermineTransitionType()
    {
        targetScene = SceneTransitionManager.GetNextSceneName();

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[TransitionUpgradeManager] 無法獲取目標場景名稱");
            return;
        }

        if (targetScene == "Level3")
        {
            currentTransitionType = TransitionType.Level2To3;
            Debug.Log("[TransitionUpgradeManager] 設定為 Level2→3 轉場");
        }
        else if (targetScene == "Level5")
        {
            currentTransitionType = TransitionType.Level4To5;
            Debug.Log("[TransitionUpgradeManager] 設定為 Level4→5 轉場");
        }
        else
        {
            enableTransitionUpgrades = false;
            Debug.Log($"[TransitionUpgradeManager] 場景 '{targetScene}' 不需要轉場升級");
        }
    }

    /// <summary>
    /// 檢查坦克位置
    /// </summary>
    private void CheckTankPosition()
    {
        if (playerTank == null) return;

        float distanceToCenter = Mathf.Abs(playerTank.position.x - pauseAtCenterX);

        if (distanceToCenter <= pauseDetectionRange)
        {
            TriggerTransitionUpgrade();
        }
    }

    /// <summary>
    /// 觸發轉場升級
    /// </summary>
    public void TriggerTransitionUpgrade()
    {
        if (isUpgradeInProgress) return;

        Debug.Log("[TransitionUpgradeManager] 觸發轉場升級");

        isUpgradeInProgress = true;

        // 暫停坦克移動
        if (transitionMover != null)
            transitionMover.enabled = false;

        // 顯示升級盤
        StartCoroutine(ShowTransitionUpgradeWheel());
    }

    /// <summary>
    /// 顯示轉場升級盤
    /// </summary>
    private IEnumerator ShowTransitionUpgradeWheel()
    {
        yield return new WaitForSeconds(0.5f);

        if (upgradeWheelUI != null && upgradeSystem != null)
        {
            // 設置轉場模式的升級選項
            SetupTransitionUpgradeOptions();

            // 顯示升級盤
            upgradeWheelUI.ShowWheel();

            Debug.Log("[TransitionUpgradeManager] 升級盤已顯示");
        }
        else
        {
            Debug.LogError("[TransitionUpgradeManager] 缺少升級相關組件！");
            ResumeTransition();
        }
    }

    /// <summary>
    /// 設置轉場升級選項
    /// </summary>
    private void SetupTransitionUpgradeOptions()
    {
        if (upgradeSystem == null) return;

        // 根據轉場類型配置不同的升級選項
        switch (currentTransitionType)
        {
            case TransitionType.Level2To3:
                ConfigureLevelTwoToThreeUpgrades();
                break;

            case TransitionType.Level4To5:
                ConfigureLevelFourToFiveUpgrades();
                break;
        }
    }

    /// <summary>
    /// 配置Level 2→3的升級選項
    /// </summary>
    private void ConfigureLevelTwoToThreeUpgrades()
    {
        Debug.Log("[TransitionUpgradeManager] 配置Level 2→3升級選項");

        // 這裡需要修改TankUpgradeSystem來提供特定的升級選項
        // 或者直接在UpgradeWheelUI中設置模式

        // Level 2→3 的三個選項：
        // 1. doublehead (middle path) - 保持原砲管，增加一個
        // 2. HUGE (bigger barrel) - 更大砲管，更慢但更強
        // 3. SMALL (smaller barrel) - 更小砲管，更快但較弱
    }

    /// <summary>
    /// 配置Level 4→5的升級選項
    /// </summary>
    private void ConfigureLevelFourToFiveUpgrades()
    {
        Debug.Log("[TransitionUpgradeManager] 配置Level 4→5升級選項");

        // Level 4→5 根據之前的選擇提供不同選項：
        // doublehead → fourhead 或其他配置
        // HUGE → 3個大砲管配置
        // SMALL → 3個小砲管配置
    }

    /// <summary>
    /// 當玩家選擇升級選項時調用
    /// </summary>
    public void OnTransitionUpgradeSelected(WheelUpgradeOption upgrade)
    {
        selectedUpgrade = upgrade;

        // 隱藏升級盤
        if (upgradeWheelUI != null)
            upgradeWheelUI.HideWheel();

        // 顯示確認對話框
        ShowConfirmationDialog(upgrade);
    }

    /// <summary>
    /// 顯示確認對話框
    /// </summary>
    private void ShowConfirmationDialog(WheelUpgradeOption upgrade)
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);

            if (confirmationText != null)
            {
                confirmationText.text = $"確定要選擇 '{upgrade.upgradeName}' 升級嗎？\n\n{upgrade.description}";
            }

            Debug.Log($"[TransitionUpgradeManager] 顯示確認對話框: {upgrade.upgradeName}");
        }
        else
        {
            // 如果沒有確認對話框，直接確認升級
            ConfirmUpgrade();
        }
    }

    /// <summary>
    /// 確認升級
    /// </summary>
    public void ConfirmUpgrade()
    {
        if (selectedUpgrade == null)
        {
            Debug.LogError("[TransitionUpgradeManager] 沒有選中的升級選項！");
            return;
        }

        Debug.Log($"[TransitionUpgradeManager] 確認升級: {selectedUpgrade.upgradeName}");

        // 隱藏確認對話框
        HideConfirmationDialog();

        // 應用升級
        StartCoroutine(ApplyUpgradeAndContinue());
    }

    /// <summary>
    /// 取消升級
    /// </summary>
    public void CancelUpgrade()
    {
        Debug.Log("[TransitionUpgradeManager] 取消升級，重新顯示升級盤");

        selectedUpgrade = null;
        HideConfirmationDialog();

        // 重新顯示升級盤
        if (upgradeWheelUI != null)
            upgradeWheelUI.ShowWheel();
    }

    /// <summary>
    /// 應用升級並繼續轉場
    /// </summary>
    private IEnumerator ApplyUpgradeAndContinue()
    {
        // 應用升級到升級系統
        if (upgradeSystem != null && selectedUpgrade != null)
        {
            upgradeSystem.ApplyUpgrade(selectedUpgrade.upgradeName);
            Debug.Log($"[TransitionUpgradeManager] 已應用升級: {selectedUpgrade.upgradeName}");
        }

        // 應用坦克模型變換
        ApplyTankModelTransformation(selectedUpgrade);

        // 等待變形動畫完成
        yield return new WaitForSeconds(1f);

        // 標記升級完成
        upgradeCompleted = true;

        // 繼續轉場移動
        ResumeTransition();

        Debug.Log("[TransitionUpgradeManager] 升級完成，繼續轉場");
    }

    /// <summary>
    /// 應用坦克模型變換
    /// </summary>
    private void ApplyTankModelTransformation(WheelUpgradeOption upgrade)
    {
        if (upgrade == null || playerTank == null)
        {
            Debug.LogWarning("[TransitionUpgradeManager] 無法應用坦克變形：缺少升級選項或玩家坦克");
            return;
        }

        Debug.Log($"[TransitionUpgradeManager] 應用坦克模型變形: {upgrade.upgradeName}");

        // 根據升級名稱切換到對應的坦克模型
        switch (upgrade.upgradeName.ToLower())
        {
            case "doublehead":
                SwitchToTankModel(doubleheadModel, "doublehead");
                break;
            case "fourhead":
                SwitchToTankModel(fourheadModel, "fourhead");
                break;
            case "huge":
                SwitchToTankModel(hugeModel, "HUGE");
                break;
            case "small":
                SwitchToTankModel(smallModel, "SMALL");
                break;
            default:
                Debug.LogWarning($"[TransitionUpgradeManager] 未知的升級類型: {upgrade.upgradeName}");
                break;
        }

        // 如果有ModularTankController，也更新它
        if (modularTankController != null)
        {
            // 這裡可以調用ModularTankController的相關方法
            // modularTankController.ApplyConfiguration(upgrade);
        }
    }

    /// <summary>
    /// 切換到指定的坦克模型
    /// </summary>
    private void SwitchToTankModel(GameObject newModel, string modelName)
    {
        if (newModel == null)
        {
            Debug.LogError($"[TransitionUpgradeManager] {modelName} 模型未設置！");
            return;
        }

        // 隱藏當前所有模型
        HideAllTankModels();

        // 啟用新模型
        newModel.SetActive(true);

        Debug.Log($"[TransitionUpgradeManager] 已切換到 {modelName} 模型");
    }

    /// <summary>
    /// 隱藏所有坦克模型
    /// </summary>
    private void HideAllTankModels()
    {
        if (armTankModel != null) armTankModel.SetActive(false);
        if (baseModel != null) baseModel.SetActive(false);
        if (barrelModel != null) barrelModel.SetActive(false);
        if (doubleheadModel != null) doubleheadModel.SetActive(false);
        if (fourheadModel != null) fourheadModel.SetActive(false);
        if (hugeModel != null) hugeModel.SetActive(false);
        if (smallModel != null) smallModel.SetActive(false);
    }

    /// <summary>
    /// 恢復轉場移動
    /// </summary>
    private void ResumeTransition()
    {
        HideAllUpgradeUI();

        if (transitionMover != null)
            transitionMover.enabled = true;

        isUpgradeInProgress = false;
        selectedUpgrade = null;

        Debug.Log("[TransitionUpgradeManager] 轉場繼續");
    }

    /// <summary>
    /// 設置確認對話框按鈕
    /// </summary>
    private void SetupConfirmationDialog()
    {
        if (confirmUpgradeButton != null)
            confirmUpgradeButton.onClick.AddListener(ConfirmUpgrade);

        if (cancelUpgradeButton != null)
            cancelUpgradeButton.onClick.AddListener(CancelUpgrade);
    }

    /// <summary>
    /// 隱藏確認對話框
    /// </summary>
    private void HideConfirmationDialog()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    /// <summary>
    /// 隱藏所有升級相關UI
    /// </summary>
    private void HideAllUpgradeUI()
    {
        if (upgradeCanvas != null)
            upgradeCanvas.gameObject.SetActive(false);

        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    // Debug方法
    [ContextMenu("強制觸發轉場升級")]
    public void DebugTriggerUpgrade()
    {
        TriggerTransitionUpgrade();
    }

    [ContextMenu("跳過升級繼續轉場")]
    public void DebugSkipUpgrade()
    {
        upgradeCompleted = true;
        ResumeTransition();
    }

    [ContextMenu("測試切換到doublehead")]
    public void DebugSwitchToDoublehead()
    {
        SwitchToTankModel(doubleheadModel, "doublehead");
    }

    [ContextMenu("測試切換到HUGE")]
    public void DebugSwitchToHuge()
    {
        SwitchToTankModel(hugeModel, "HUGE");
    }

    [ContextMenu("測試切換到SMALL")]
    public void DebugSwitchToSmall()
    {
        SwitchToTankModel(smallModel, "SMALL");
    }
}