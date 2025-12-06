using UnityEngine;

/// <summary>
/// 玩家數據管理器 - 跨場景持久化玩家的升級數據
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("當前升級等級")]
    public int moveSpeedLevel = 0;
    public int bulletSpeedLevel = 0;
    public int fireRateLevel = 0;
    public int availableUpgradePoints = 0;

    [Header("玩家生命值")]
    public int currentHealth = 3;  // 玩家當前生命值
    public bool hasHealthData = false;  // 是否有保存的生命值數據

    [Header("Tank Transformation")]
    [SerializeField] private string currentTankTransformation = "Basic";  // Current tank visual style

    void Awake()
    {
        // 單例模式 + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"✓ PlayerDataManager 已創建並設為持久化 (GameObject: {gameObject.name})");
        }
        else
        {
            Debug.LogWarning($"PlayerDataManager 已存在，銷毀重複物件 (嘗試創建的物件: {gameObject.name}, 現有實例: {Instance.gameObject.name})");
            Destroy(gameObject);
        }
    }

    #region Tank Transformation Methods
    /// <summary>
    /// Save tank transformation state
    /// </summary>
    public void SaveTankTransformation(string transformationName)
    {
        currentTankTransformation = transformationName;
        Debug.Log($"✓ Saved tank transformation: {transformationName}");
    }

    /// <summary>
    /// Load tank transformation and apply it
    /// </summary>
    public void LoadTankTransformation()
    {
        if (!string.IsNullOrEmpty(currentTankTransformation) && currentTankTransformation != "Basic")
        {
            var transformManager = FindFirstObjectByType<TankTransformationManager>();
            if (transformManager != null)
            {
                // Apply the saved transformation
                switch (currentTankTransformation.ToLower())
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
                Debug.Log($"✓ Loaded and applied tank transformation: {currentTankTransformation}");
            }
        }
    }

    /// <summary>
    /// Get current tank transformation name
    /// </summary>
    public string GetCurrentTankTransformation()
    {
        return currentTankTransformation;
    }
    #endregion

    /// <summary>
    /// 保存玩家當前的升級數據
    /// </summary>
    public void SavePlayerStats(TankStats stats)
    {
        if (stats == null) return;

        moveSpeedLevel = stats.GetMoveSpeedLevel();
        bulletSpeedLevel = stats.GetBulletSpeedLevel();
        fireRateLevel = stats.GetFireRateLevel();
        availableUpgradePoints = stats.GetAvailableUpgradePoints();

        Debug.Log($"✓ 保存玩家數據: 移動 Lv.{moveSpeedLevel}, 子彈 Lv.{bulletSpeedLevel}, 射速 Lv.{fireRateLevel}, 點數 {availableUpgradePoints}");
    }

    /// <summary>
    /// 將保存的數據載入到 TankStats
    /// </summary>
    public bool LoadPlayerStats(TankStats stats)
    {
        if (stats == null) return false;

        // 檢查是否有任何保存的數據
        bool hasData = (moveSpeedLevel > 0 || bulletSpeedLevel > 0 || fireRateLevel > 0 || availableUpgradePoints > 0);

        if (hasData)
        {
            // 直接設置等級（不消耗升級點數）
            stats.SetLevels(moveSpeedLevel, bulletSpeedLevel, fireRateLevel, availableUpgradePoints);
            Debug.Log($"✓ 載入玩家數據: 移動 Lv.{moveSpeedLevel}, 子彈 Lv.{bulletSpeedLevel}, 射速 Lv.{fireRateLevel}, 點數 {availableUpgradePoints}");
            return true;
        }

        Debug.Log("無保存的玩家數據");
        return false;
    }

    /// <summary>
    /// 重置所有數據（新遊戲時使用）
    /// </summary>
    public void ResetData()
    {
        moveSpeedLevel = 0;
        bulletSpeedLevel = 0;
        fireRateLevel = 0;
        availableUpgradePoints = 0;
        currentHealth = 3;
        hasHealthData = false;
        currentTankTransformation = "Basic";
        Debug.Log("✓ 玩家數據已重置");
    }

    /// <summary>
    /// 保存玩家當前的生命值
    /// </summary>
    public void SavePlayerHealth(int health)
    {
        currentHealth = health;
        hasHealthData = true;
        Debug.Log($"✓ 保存玩家生命值: {currentHealth}");
    }

    /// <summary>
    /// 載入保存的生命值到 PlayerHealth
    /// </summary>
    public bool LoadPlayerHealth(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
        {
            Debug.LogError("[PlayerDataManager] PlayerHealth 為 null！");
            return false;
        }

        if (!hasHealthData)
        {
            return false;
        }

        // 直接設置生命值（不觸發事件）
        playerHealth.SetHealthDirect(currentHealth);
        return true;
    }

    /// <summary>
    /// 增加玩家生命值（在 Transition 場景中使用）
    /// </summary>
    public void AddHealth(int amount = 1)
    {
        if (!hasHealthData)
        {
            // 如果沒有數據，設置為初始值
            currentHealth = 3;
            hasHealthData = true;
        }

        currentHealth += amount;
        Debug.Log($"✓ 增加生命值 +{amount}，當前: {currentHealth}");
    }

    /// <summary>
    /// 獲取當前生命值
    /// </summary>
    public int GetCurrentHealth() => currentHealth;
}