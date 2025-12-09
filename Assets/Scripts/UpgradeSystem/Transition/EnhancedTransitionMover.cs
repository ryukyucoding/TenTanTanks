using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enhanced Transition Mover - 帶有詳細Debug信息的診斷版本
/// 用於診斷提早換關問題
/// </summary>
public class EnhancedTransitionMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float targetX = 12f;
    [SerializeField] private string nextScene = "Level1";

    [Header("Upgrade Trigger Settings")]
    [SerializeField] private float upgradePositionX = -5f; // X position to trigger upgrade
    [SerializeField] private float upgradeDetectionRange = 10f; // Detection range
    [SerializeField] private bool enableUpgrades = true; // Whether to enable upgrade functionality
    [SerializeField] private bool pauseForUpgrade = true; // Whether to pause for upgrade

    [Header("Debug Settings")]
    [SerializeField] private bool enableVerboseDebug = true; // 啟用詳細debug
    [SerializeField] private float debugInterval = 0.5f; // debug輸出間隔（秒）

    [Header("Scenes Requiring Upgrades")]
    [SerializeField] private string[] upgradeScenes = { "Level2", "Level4" }; // Target scenes that need upgrades

    [Header("Transition Upgrade Testing")]
    [SerializeField] private TransitionWheelUpgrade transitionUpgrade; // Optional transition upgrade component

    private bool hasTriggeredUpgrade = false;
    private bool isUpgradeInProgress = false;
    private bool canMove = true;

    // Debug變數
    private float lastDebugTime = 0f;
    private Vector3 lastPosition;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        lastPosition = transform.position;

        Debug.Log("🚀 [EnhancedTransitionMover] === 開始初始化 ===");
        Debug.Log($"[EnhancedTransitionMover] 初始位置: {transform.position.x:F2}");
        Debug.Log($"[EnhancedTransitionMover] 目標位置: {targetX:F2}");
        Debug.Log($"[EnhancedTransitionMover] 升級觸發位置: {upgradePositionX:F2}");
        Debug.Log($"[EnhancedTransitionMover] 移動速度: {speed}");

        InitializeTransition();
        FindUpgradeComponents();

        Debug.Log("✅ [EnhancedTransitionMover] === 初始化完成 ===");
    }

    void Update()
    {
        // 📊 詳細Debug信息
        if (enableVerboseDebug && Time.time - lastDebugTime >= debugInterval)
        {
            DebugCurrentStatus();
            lastDebugTime = Time.time;
        }

        if (!canMove)
        {
            if (enableVerboseDebug && Time.frameCount % 120 == 0) // 每2秒輸出一次
            {
                // Debug.Log("⏸️ [EnhancedTransitionMover] 移動被暫停，原因: " + GetPauseReason());
            }
            return;
        }

        Vector3 pos = transform.position;

        // 🔍 移動距離檢測
        float movementThisFrame = Mathf.Abs(pos.x - lastPosition.x);
        if (movementThisFrame > 0.01f && enableVerboseDebug)
        {
            // Debug.Log($"📍 [EnhancedTransitionMover] 移動中: {lastPosition.x:F2} → {pos.x:F2} (距離: {movementThisFrame:F3})");
        }
        lastPosition = pos;

        // 🎯 升級觸發檢查
        if (!hasTriggeredUpgrade && enableUpgrades && ShouldTriggerUpgrade(pos.x))
        {
            Debug.Log($"🎯 [EnhancedTransitionMover] ===== 升級觸發！=====");
            Debug.Log($"[EnhancedTransitionMover] 觸發位置: {pos.x:F2}");
            Debug.Log($"[EnhancedTransitionMover] 目標場景: {nextScene}");
            Debug.Log($"[EnhancedTransitionMover] 觸發時間: {Time.time - startTime:F2}秒後");
            TriggerUpgrade();
            return;
        }

        // 🚶‍♂️ 正常移動邏輯
        if (pos.x < targetX)
        {
            float step = speed * Time.deltaTime;
            float oldX = pos.x;
            pos.x = Mathf.MoveTowards(pos.x, targetX, step);
            transform.position = pos;

            // 接近目標時增加警告
            float distanceToTarget = targetX - pos.x;
            if (distanceToTarget < 2f && enableVerboseDebug)
            {
                Debug.Log($"⚠️ [EnhancedTransitionMover] 接近目標位置！距離: {distanceToTarget:F3}");
            }
        }
        else
        {
            // 🚨 關鍵：場景切換觸發點
            Debug.Log($"🚨 [EnhancedTransitionMover] ===== 到達目標位置，準備換場景！=====");
            Debug.Log($"[EnhancedTransitionMover] 當前位置: {pos.x:F2}");
            Debug.Log($"[EnhancedTransitionMover] 目標位置: {targetX:F2}");
            Debug.Log($"[EnhancedTransitionMover] 已觸發升級: {hasTriggeredUpgrade}");
            Debug.Log($"[EnhancedTransitionMover] 升級進行中: {isUpgradeInProgress}");
            Debug.Log($"[EnhancedTransitionMover] 目標場景: {nextScene}");
            Debug.Log($"[EnhancedTransitionMover] 總耗時: {Time.time - startTime:F2}秒");
            Debug.Log($"[EnhancedTransitionMover] ===== 即將呼叫LoadNext()！=====");

            LoadNext();
        }
    }

    /// <summary>
    /// 📊 詳細狀態Debug
    /// </summary>
    private void DebugCurrentStatus()
    {
        Vector3 pos = transform.position;
        float distanceToUpgrade = Mathf.Abs(pos.x - upgradePositionX);
        float distanceToTarget = Mathf.Abs(pos.x - targetX);

        Debug.Log($"📊 [Status] X:{pos.x:F2} | 到升級:{distanceToUpgrade:F2} | 到目標:{distanceToTarget:F2} | 升級:{hasTriggeredUpgrade} | 進行中:{isUpgradeInProgress} | 可移動:{canMove}");
    }

    /// <summary>
    /// 獲取暫停原因
    /// </summary>
    private string GetPauseReason()
    {
        if (isUpgradeInProgress) return "升級進行中";
        if (!enableUpgrades) return "升級功能已禁用";
        return "未知原因";
    }

    /// <summary>
    /// Initialize transition settings
    /// </summary>
    private void InitializeTransition()
    {
        Debug.Log("🔧 [EnhancedTransitionMover] 開始初始化轉場設定...");

        // Prioritize scene set by SceneTransitionManager
        string transitionScene = SceneTransitionManager.GetNextSceneName();
        if (!string.IsNullOrEmpty(transitionScene))
        {
            string oldScene = nextScene;
            nextScene = transitionScene;
            Debug.Log($"[EnhancedTransitionMover] 場景更新: {oldScene} → {nextScene} (來自SceneTransitionManager)");
        }
        else
        {
            Debug.Log($"[EnhancedTransitionMover] 使用預設場景: {nextScene}");
        }

        // Check PlayerDataManager
        CheckPlayerDataManager();

        // Apply health bonus for specific level transitions
        ApplyHealthBonus(nextScene);

        Debug.Log("✅ [EnhancedTransitionMover] 轉場設定初始化完成");
    }

    /// <summary>
    /// Find upgrade components
    /// </summary>
    private void FindUpgradeComponents()
    {
        Debug.Log("🔍 [EnhancedTransitionMover] 搜尋升級組件...");

        // Try to find transition wheel upgrade component first
        if (transitionUpgrade == null)
            transitionUpgrade = FindFirstObjectByType<TransitionWheelUpgrade>();

        if (transitionUpgrade != null)
        {
            Debug.Log($"✅ [EnhancedTransitionMover] 找到升級組件: {transitionUpgrade.name}");
        }
        else if (enableUpgrades)
        {
            Debug.LogWarning("⚠️ [EnhancedTransitionMover] 啟用了升級功能但找不到升級組件");
        }
        else
        {
            Debug.Log("ℹ️ [EnhancedTransitionMover] 升級功能已禁用，不需要升級組件");
        }
    }

    /// <summary>
    /// Check if should trigger upgrade
    /// </summary>
    private bool ShouldTriggerUpgrade(float currentX)
    {
        if (!enableUpgrades)
        {
            if (enableVerboseDebug && Time.frameCount % 300 == 0) // 每5秒輸出一次
            {
                Debug.Log("🔒 [EnhancedTransitionMover] 升級功能已禁用");
            }
            return false;
        }

        if (!IsUpgradeScene(nextScene))
        {
            if (enableVerboseDebug && Time.frameCount % 300 == 0)
            {
                Debug.Log($"🔒 [EnhancedTransitionMover] 場景 {nextScene} 不需要升級");
            }
            return false;
        }

        // Check if reached upgrade trigger position
        float distanceToUpgradePoint = Mathf.Abs(currentX - upgradePositionX);
        bool shouldTrigger = distanceToUpgradePoint <= upgradeDetectionRange;

        if (distanceToUpgradePoint < upgradeDetectionRange + 2f && enableVerboseDebug) // 在接近時增加debug
        {
            Debug.Log($"🎯 [EnhancedTransitionMover] 升級檢查: 距離={distanceToUpgradePoint:F2}, 範圍={upgradeDetectionRange}, 應觸發={shouldTrigger}");
        }

        return shouldTrigger;
    }

    /// <summary>
    /// Check if scene requires upgrade
    /// </summary>
    private bool IsUpgradeScene(string sceneName)
    {
        foreach (string upgradeScene in upgradeScenes)
        {
            if (upgradeScene == sceneName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Trigger upgrade process
    /// </summary>
    private void TriggerUpgrade()
    {
        Debug.Log("🎯 [EnhancedTransitionMover] ===== 開始升級流程 =====");

        hasTriggeredUpgrade = true;
        isUpgradeInProgress = true;

        if (pauseForUpgrade)
        {
            canMove = false;
            Debug.Log("⏸️ [EnhancedTransitionMover] 移動已暫停，等待升級完成");
        }

        Debug.Log($"[EnhancedTransitionMover] 升級觸發位置: {transform.position.x:F2}");
        Debug.Log($"[EnhancedTransitionMover] 目標場景: {nextScene}");

        // Try different upgrade systems
        if (TryTriggerTransitionUpgrade())
        {
            Debug.Log("✅ [EnhancedTransitionMover] 使用TransitionWheelUpgrade系統");
        }
        else if (TryTriggerAdvancedUpgrade())
        {
            Debug.Log("✅ [EnhancedTransitionMover] 使用進階升級系統");
        }
        else
        {
            Debug.LogWarning("⚠️ [EnhancedTransitionMover] 找不到可用的升級系統，恢復移動");
            ResumeMovement();
        }

        Debug.Log("🎯 [EnhancedTransitionMover] ===== 升級流程初始化完成 =====");
    }

    /// <summary>
    /// Try to trigger transition wheel upgrade system
    /// </summary>
    private bool TryTriggerTransitionUpgrade()
    {
        if (transitionUpgrade != null)
        {
            Debug.Log("🎮 [EnhancedTransitionMover] 嘗試觸發TransitionWheelUpgrade...");

            string transitionType = "Level 1 to Level 2"; // default

            if (nextScene == "Level2")
            {
                transitionType = "Level 1 to Level 2";
                Debug.Log("[EnhancedTransitionMover] Level1→Level2: 顯示Tier 1選項");

                transitionUpgrade.ShowUpgradePanel(transitionType);
                return true;
            }
            else if (nextScene == "Level4")
            {
                transitionType = "Level 3 to Level 4";
                Debug.Log("[EnhancedTransitionMover] Level3→Level4: 檢查Tier 2選項");

                string savedTier1 = "";
                if (PlayerDataManager.Instance != null)
                {
                    savedTier1 = PlayerDataManager.Instance.GetCurrentTankTransformation();
                    Debug.Log($"[EnhancedTransitionMover] 已保存的Tier 1變形: {savedTier1}");
                }

                if (!string.IsNullOrEmpty(savedTier1) && savedTier1.ToLower() != "basic")
                {
                    Debug.Log($"✅ [EnhancedTransitionMover] 顯示{savedTier1}的Tier 2選項");
                    transitionUpgrade.ShowUpgradePanel(transitionType);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"⚠️ [EnhancedTransitionMover] 無有效的Tier 1變形，跳過升級");
                    ResumeMovement();
                    return false;
                }
            }
            else
            {
                Debug.Log($"ℹ️ [EnhancedTransitionMover] 場景{nextScene}不需要升級");
                return false;
            }
        }

        Debug.Log("❌ [EnhancedTransitionMover] TransitionWheelUpgrade組件不存在");
        return false;
    }

    /// <summary>
    /// Try to trigger advanced upgrade system
    /// </summary>
    private bool TryTriggerAdvancedUpgrade()
    {
        Debug.Log("🔍 [EnhancedTransitionMover] 搜尋進階升級系統...");
        return false; // 暫時禁用進階系統，專注於主要系統
    }

    /// <summary>
    /// Resume movement (called by upgrade systems)
    /// </summary>
    public void ResumeMovement()
    {
        Debug.Log("▶️ [EnhancedTransitionMover] ===== 恢復移動 =====");
        Debug.Log($"[EnhancedTransitionMover] 恢復時位置: {transform.position.x:F2}");
        Debug.Log($"[EnhancedTransitionMover] 目標位置: {targetX:F2}");
        Debug.Log($"[EnhancedTransitionMover] 剩餘距離: {targetX - transform.position.x:F2}");

        isUpgradeInProgress = false;
        canMove = true;

        Debug.Log("✅ [EnhancedTransitionMover] 移動已恢復");
    }

    /// <summary>
    /// Check PlayerDataManager and report status
    /// </summary>
    private void CheckPlayerDataManager()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("⚠️ [EnhancedTransitionMover] PlayerDataManager.Instance 為null！");
        }
        else
        {
            Debug.Log($"✅ [EnhancedTransitionMover] PlayerDataManager存在，當前生命值: {PlayerDataManager.Instance.GetCurrentHealth()}");
        }
    }

    /// <summary>
    /// Apply health bonus for specific level transitions
    /// </summary>
    private void ApplyHealthBonus(string targetScene)
    {
        if (PlayerDataManager.Instance == null) return;

        if (targetScene == "Level3" || targetScene == "Level5")
        {
            int beforeHealth = PlayerDataManager.Instance.GetCurrentHealth();
            PlayerDataManager.Instance.AddHealth(1);
            int afterHealth = PlayerDataManager.Instance.GetCurrentHealth();
            Debug.Log($"💚 [EnhancedTransitionMover] 進入{targetScene}，獲得額外生命 +1 (前:{beforeHealth}, 後:{afterHealth})");
        }
    }

    /// <summary>
    /// Load next scene
    /// </summary>
    private void LoadNext()
    {
        Debug.Log("🚀 [EnhancedTransitionMover] ===== 開始載入下一個場景 =====");

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            Debug.Log("[EnhancedTransitionMover] 恢復時間縮放");
        }

        SceneTransitionManager.ClearNextSceneName();

        Debug.Log($"[EnhancedTransitionMover] 載入場景: {nextScene}");
        Debug.Log($"[EnhancedTransitionMover] 總耗時: {Time.time - startTime:F2}秒");
        Debug.Log("🚀 [EnhancedTransitionMover] ===== 場景載入中... =====");

        SceneManager.LoadScene(nextScene);
        enabled = false; // 防止重複呼叫
    }

    // Debug methods
    [ContextMenu("Force Trigger Upgrade")]
    public void DebugTriggerUpgrade()
    {
        Debug.Log("🔧 [Debug] 強制觸發升級");
        if (!hasTriggeredUpgrade)
        {
            TriggerUpgrade();
        }
        else
        {
            Debug.Log("⚠️ [Debug] 升級已經被觸發過了");
        }
    }

    [ContextMenu("Resume Movement")]
    public void DebugResumeMovement()
    {
        Debug.Log("🔧 [Debug] 強制恢復移動");
        ResumeMovement();
    }

    [ContextMenu("Force Load Next Scene")]
    public void DebugForceLoadNext()
    {
        Debug.Log("🔧 [Debug] 強制載入下一場景");
        LoadNext();
    }

    [ContextMenu("Show Detailed Status")]
    public void DebugShowDetailedStatus()
    {
        Debug.Log("=== EnhancedTransitionMover 詳細狀態 ===");
        Debug.Log($"目標場景: {nextScene}");
        Debug.Log($"當前位置: {transform.position.x:F2}");
        Debug.Log($"目標位置: {targetX:F2}");
        Debug.Log($"升級觸發位置: {upgradePositionX:F2}");
        Debug.Log($"到目標距離: {targetX - transform.position.x:F2}");
        Debug.Log($"到升級距離: {Mathf.Abs(transform.position.x - upgradePositionX):F2}");
        Debug.Log($"已觸發升級: {hasTriggeredUpgrade}");
        Debug.Log($"升級進行中: {isUpgradeInProgress}");
        Debug.Log($"可以移動: {canMove}");
        Debug.Log($"是升級場景: {IsUpgradeScene(nextScene)}");
        Debug.Log($"升級組件存在: {transitionUpgrade != null}");
        Debug.Log($"運行時間: {Time.time - startTime:F2}秒");
        Debug.Log("=====================================");
    }

    // Public methods for other scripts to use
    public bool IsUpgradeInProgress => isUpgradeInProgress;
    public bool HasTriggeredUpgrade => hasTriggeredUpgrade;
    public bool CanMove => canMove;
    public string NextScene => nextScene;
    public float CurrentProgress => (targetX - transform.position.x) / (targetX - upgradePositionX);
}