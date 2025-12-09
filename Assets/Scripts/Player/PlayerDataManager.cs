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
            Debug.Log($"[PlayerDataManager] ✓ 已創建並設為持久化 (GameObject: {gameObject.name}, InstanceID: {GetInstanceID()})");
            Debug.Log($"[PlayerDataManager] 当前数据: 移動Lv={moveSpeedLevel}, 子彈Lv={bulletSpeedLevel}, 射速Lv={fireRateLevel}, 點數={availableUpgradePoints}");
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"[PlayerDataManager] ⚠️ 已存在實例，銷毀新物件");
            Debug.LogWarning($"  保留實例: {Instance.gameObject.name} (ID:{Instance.GetInstanceID()}) - 移動Lv={Instance.moveSpeedLevel}, 點數={Instance.availableUpgradePoints}");
            Debug.LogWarning($"  銷毀實例: {gameObject.name} (ID:{GetInstanceID()}) - 移動Lv={moveSpeedLevel}, 點數={availableUpgradePoints}");
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
                Debug.Log($"✅ Loading tank transformation: {currentTankTransformation}");
                
                // ✅ 支持所有 Tier 1 和 Tier 2 變形
                string lowerName = currentTankTransformation.ToLower();
                
                // Tier 1
                if (lowerName == "heavy")
                    transformManager.SelectHeavyUpgrade();
                else if (lowerName == "rapid")
                    transformManager.SelectRapidUpgrade();
                else if (lowerName == "balanced")
                    transformManager.SelectBalancedUpgrade();
                // Tier 2 Heavy
                else if (lowerName == "armorpiercing")
                    transformManager.SelectArmorPiercingUpgrade();
                else if (lowerName == "superheavy")
                    transformManager.SelectSuperHeavyUpgrade();
                // Tier 2 Rapid
                else if (lowerName == "burst")
                    transformManager.SelectBurstUpgrade();
                else if (lowerName == "machinegun")
                    transformManager.SelectMachineGunUpgrade();
                // Tier 2 Balanced
                else if (lowerName == "tactical")
                    transformManager.SelectTacticalUpgrade();
                else if (lowerName == "versatile")
                    transformManager.SelectVersatileUpgrade();
                else
                    Debug.LogWarning($"⚠️ Unknown transformation: {currentTankTransformation}");
                
                Debug.Log($"✅ Applied saved tank transformation: {currentTankTransformation}");
            }
            else
            {
                Debug.LogWarning("⚠️ TankTransformationManager not found, cannot apply transformation");
            }
        }
        else
        {
            Debug.Log("No saved transformation or using Basic tank");
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
        if (stats == null)
        {
            Debug.LogWarning("[PlayerDataManager] SavePlayerStats: stats 為 null");
            return;
        }

        int oldMoveLevel = moveSpeedLevel;
        int oldBulletLevel = bulletSpeedLevel;
        int oldFireLevel = fireRateLevel;
        int oldPoints = availableUpgradePoints;

        moveSpeedLevel = stats.GetMoveSpeedLevel();
        bulletSpeedLevel = stats.GetBulletSpeedLevel();
        fireRateLevel = stats.GetFireRateLevel();
        availableUpgradePoints = stats.GetAvailableUpgradePoints();

        Debug.Log($"[PlayerDataManager] ✓ 保存玩家數據 (PDM Instance ID: {GetInstanceID()}, GameObject: {gameObject.name}):");
        Debug.Log($"  TankStats來源: {stats.gameObject.name} (InstanceID: {stats.gameObject.GetInstanceID()})");
        Debug.Log($"  之前: 移動Lv{oldMoveLevel}, 子彈Lv{oldBulletLevel}, 射速Lv{oldFireLevel}, 點數{oldPoints}");
        Debug.Log($"  现在: 移動Lv{moveSpeedLevel}, 子彈Lv{bulletSpeedLevel}, 射速Lv{fireRateLevel}, 點數{availableUpgradePoints}");
        
        // 验证数据是否合理
        if (moveSpeedLevel == 0 && bulletSpeedLevel == 0 && fireRateLevel == 0 && availableUpgradePoints == 0)
        {
            Debug.LogWarning("[PlayerDataManager] ⚠️ 保存的數據全為 0，這可能不正確！");
            Debug.LogWarning($"[PlayerDataManager] TankStats 物件路徑: {GetGameObjectPath(stats.gameObject)}");
        }
    }

    /// <summary>
    /// 將保存的數據載入到 TankStats
    /// </summary>
    public bool LoadPlayerStats(TankStats stats)
    {
        if (stats == null)
        {
            Debug.LogWarning("[PlayerDataManager] LoadPlayerStats: stats 為 null");
            return false;
        }

        Debug.Log($"[PlayerDataManager] 嘗試載入數據 (PDM Instance ID: {GetInstanceID()}, GameObject: {gameObject.name}):");
        Debug.Log($"  目標TankStats: {stats.gameObject.name} (InstanceID: {stats.gameObject.GetInstanceID()})");
        Debug.Log($"  TankStats 物件路徑: {GetGameObjectPath(stats.gameObject)}");
        Debug.Log($"  移動 Lv.{moveSpeedLevel}, 子彈 Lv.{bulletSpeedLevel}, 射速 Lv.{fireRateLevel}, 可用點數 {availableUpgradePoints}");
        Debug.Log($"  Instance == PlayerDataManager.Instance: {this == PlayerDataManager.Instance}");

        // ✅ 修复：只要有升级点数或任何等级大于0，就算有数据
        // 因为玩家可能有点数但还没升级任何属性
        bool hasData = (moveSpeedLevel > 0 || bulletSpeedLevel > 0 || fireRateLevel > 0 || availableUpgradePoints > 0);

        if (hasData)
        {
            // 直接設置等級（不消耗升級點數）
            stats.SetLevels(moveSpeedLevel, bulletSpeedLevel, fireRateLevel, availableUpgradePoints);
            Debug.Log($"[PlayerDataManager] ✓ 載入玩家數據成功: 移動 Lv.{moveSpeedLevel}, 子彈 Lv.{bulletSpeedLevel}, 射速 Lv.{fireRateLevel}, 可用點數 {availableUpgradePoints}");
            return true;
        }

        Debug.Log("[PlayerDataManager] ⚠️ 無保存的玩家數據 (所有等級=0且點數=0)");
        return false;
    }

    /// <summary>
    /// 获取 GameObject 的完整路径
    /// </summary>
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
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