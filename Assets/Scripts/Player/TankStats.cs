using UnityEngine;
using System;

/// <summary>
/// 坦克屬性管理系統
/// 管理坦克的所有可升級屬性
/// </summary>
public class TankStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseMoveSpeed = 2.5f;
    [SerializeField] private float baseBulletSpeed = 5f;
    [SerializeField] private float baseFireRate = 1.2f;

    [Header("Upgrade Increments")]
    [SerializeField] private float moveSpeedIncrement = 0.3f;
    [SerializeField] private float bulletSpeedIncrement = 0.5f;
    [SerializeField] private float fireRateIncrement = 0.3f;

    [Header("Max Levels")]
    [SerializeField] private int maxMoveSpeedLevel = 10;
    [SerializeField] private int maxBulletSpeedLevel = 10;
    [SerializeField] private int maxFireRateLevel = 10;

    // 當前等級（從 0 開始）
    private int moveSpeedLevel = 0;
    private int bulletSpeedLevel = 0;
    private int fireRateLevel = 0;

    // 可用的升級點數
    private int availableUpgradePoints = 0;

    // 組件引用
    private TankController tankController;
    private TankShooting tankShooting;

    // 事件：當升級點數改變時
    public event Action<int> OnUpgradePointsChanged;
    // 事件：當屬性升級時
    public event Action<StatType, int, int> OnStatUpgraded; // (類型, 新等級, 最大等級)

    public enum StatType
    {
        MoveSpeed,
        BulletSpeed,
        FireRate
    }

    void Awake()
    {
        Debug.Log($"[Awake] TankStats 初始化... (物件: {gameObject.name}, InstanceID: {gameObject.GetInstanceID()})");
        Debug.Log($"[Awake] 這是 Clone 嗎？{gameObject.name.Contains("Clone")}");
        
        // Awake 阶段强制查找并锁定引用
        tankController = GetComponent<TankController>();
        tankShooting = GetComponent<TankShooting>();
        
        Debug.Log($"[Awake] TankController = {(tankController != null ? $"✓ 找到 (InstanceID: {tankController.GetInstanceID()})" : "❌ null")}");
        Debug.Log($"[Awake] TankShooting = {(tankShooting != null ? $"✓ 找到 (InstanceID: {tankShooting.GetInstanceID()})" : "❌ null")}");
        
        // 如果在 Awake 时找到了，立即应用基础属性
        if (tankController != null && tankShooting != null)
        {
            Debug.Log($"[Awake] 立即應用基礎屬性");
            tankController.SetMoveSpeed(baseMoveSpeed);
            tankShooting.SetBulletSpeed(baseBulletSpeed);
            tankShooting.SetFireRate(baseFireRate);
        }
    }

    void Start()
    {
        Debug.Log($"[Start] TankStats 開始執行... (物件: {gameObject.name})");
        
        // 延迟加载数据，确保其他系统已准备好
        StartCoroutine(LoadDataAfterFrame());
    }

    private System.Collections.IEnumerator LoadDataAfterFrame()
    {
        // 等待一帧
        yield return null;
        
        Debug.Log($"[LoadData] 檢查保存的數據...");
        
        // 验证组件引用仍然有效
        if (tankController == null)
        {
            Debug.LogError($"❌ [LoadData] tankController 丟失了！重新查找...");
            tankController = GetComponent<TankController>();
        }
        
        if (tankShooting == null)
        {
            Debug.LogError($"❌ [LoadData] tankShooting 丟失了！重新查找...");
            tankShooting = GetComponent<TankShooting>();
        }
        
        // 加载保存的数据
        bool hasLoadedData = false;
        if (PlayerDataManager.Instance != null)
        {
            hasLoadedData = PlayerDataManager.Instance.LoadPlayerStats(this);
        }
        
        Debug.Log($"[LoadData] 完成 (有保存數據: {hasLoadedData})");
    }

    void OnDestroy()
    {
        // 當物件被銷毀時保存數據（換場景時）
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerStats(this);
        }
    }

    /// <summary>
    /// 增加升級點數（打倒一波敵人時呼叫）
    /// </summary>
    public void AddUpgradePoints(int points)
    {
        availableUpgradePoints += points;
        OnUpgradePointsChanged?.Invoke(availableUpgradePoints);
        Debug.Log($"獲得 {points} 升級點數！剩餘: {availableUpgradePoints}");
    }

    /// <summary>
    /// 嘗試升級指定屬性
    /// </summary>
    public bool TryUpgradeStat(StatType statType)
    {
        // 檢查是否有足夠的點數
        if (availableUpgradePoints <= 0)
        {
            Debug.Log("沒有足夠的升級點數！");
            return false;
        }

        bool upgraded = false;

        switch (statType)
        {
            case StatType.MoveSpeed:
                if (moveSpeedLevel < maxMoveSpeedLevel)
                {
                    moveSpeedLevel++;
                    availableUpgradePoints--;
                    ApplyMoveSpeed();
                    OnStatUpgraded?.Invoke(StatType.MoveSpeed, moveSpeedLevel, maxMoveSpeedLevel);
                    upgraded = true;
                    Debug.Log($"移動速度升級至 Lv.{moveSpeedLevel} -> {GetCurrentMoveSpeed():F2}");
                }
                else
                {
                    Debug.Log("移動速度已達最大等級！");
                }
                break;

            case StatType.BulletSpeed:
                if (bulletSpeedLevel < maxBulletSpeedLevel)
                {
                    bulletSpeedLevel++;
                    availableUpgradePoints--;
                    ApplyBulletSpeed();
                    OnStatUpgraded?.Invoke(StatType.BulletSpeed, bulletSpeedLevel, maxBulletSpeedLevel);
                    upgraded = true;
                    Debug.Log($"子彈速度升級至 Lv.{bulletSpeedLevel} -> {GetCurrentBulletSpeed():F2}");
                }
                else
                {
                    Debug.Log("子彈速度已達最大等級！");
                }
                break;

            case StatType.FireRate:
                if (fireRateLevel < maxFireRateLevel)
                {
                    fireRateLevel++;
                    availableUpgradePoints--;
                    ApplyFireRate();
                    OnStatUpgraded?.Invoke(StatType.FireRate, fireRateLevel, maxFireRateLevel);
                    upgraded = true;
                    Debug.Log($"射速升級至 Lv.{fireRateLevel} -> {GetCurrentFireRate():F2}");
                }
                else
                {
                    Debug.Log("射速已達最大等級！");
                }
                break;
        }

        if (upgraded)
        {
            OnUpgradePointsChanged?.Invoke(availableUpgradePoints);
        }

        return upgraded;
    }

    /// <summary>
    /// 應用所有屬性
    /// </summary>
    private void ApplyStats()
    {
        ApplyMoveSpeed();
        ApplyBulletSpeed();
        ApplyFireRate();
    }

    private void ApplyMoveSpeed()
    {
        Debug.Log($"[ApplyMoveSpeed] 被調用 (物件: {gameObject.name}, InstanceID: {gameObject.GetInstanceID()})");
        Debug.Log($"[ApplyMoveSpeed] tankController 是否為 null: {tankController == null}");
        
        // 如果 tankController 是 null，强制重新查找
        if (tankController == null)
        {
            Debug.LogWarning($"⚠️ ApplyMoveSpeed: tankController 是 null，强制重新查找...");
            Debug.LogWarning($"   当前 GameObject: {gameObject.name}");
            Debug.LogWarning($"   GameObject 路径: {GetGameObjectPath(gameObject)}");
            Debug.LogWarning($"   GameObject InstanceID: {gameObject.GetInstanceID()}");
            
            // 尝试所有查找方法
            tankController = GetComponent<TankController>();
            Debug.LogWarning($"   GetComponent: {(tankController != null ? $"✓ 找到 (InstanceID: {tankController.GetInstanceID()})" : "✗ null")}");
            
            if (tankController == null)
            {
                tankController = GetComponentInParent<TankController>();
                Debug.LogWarning($"   GetComponentInParent: {(tankController != null ? "✓ 找到" : "✗ null")}");
            }
            
            if (tankController == null)
            {
                tankController = GetComponentInChildren<TankController>();
                Debug.LogWarning($"   GetComponentInChildren: {(tankController != null ? "✓ 找到" : "✗ null")}");
            }
            
            if (tankController == null)
            {
                // 最后尝试全局查找
                var allControllers = FindObjectsByType<TankController>(FindObjectsSortMode.None);
                Debug.LogWarning($"   场景中共有 {allControllers.Length} 个 TankController");
                if (allControllers.Length > 0)
                {
                    tankController = allControllers[0];
                    Debug.LogWarning($"   使用第一个找到的: {tankController.gameObject.name} (InstanceID: {tankController.gameObject.GetInstanceID()})");
                }
            }
        }
        else
        {
            Debug.Log($"[ApplyMoveSpeed] tankController 有效 (InstanceID: {tankController.GetInstanceID()})");
        }
        
        if (tankController != null)
        {
            float speed = GetCurrentMoveSpeed();
            tankController.SetMoveSpeed(speed);
            Debug.Log($"✓ 應用移動速度: {speed:F2}");
        }
        else
        {
            Debug.LogError($"❌ TankController 是 null，無法應用移動速度！物件: {gameObject.name}");
        }
    }
    
    // 辅助方法：获取 GameObject 的完整路径
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    private void ApplyBulletSpeed()
    {
        if (tankShooting != null)
        {
            float speed = GetCurrentBulletSpeed();
            tankShooting.SetBulletSpeed(speed);
            Debug.Log($"✓ 應用子彈速度: {speed:F2}");
        }
        else
        {
            Debug.LogError($"❌ TankShooting 是 null，無法應用子彈速度！物件: {gameObject.name}");
        }
    }

    private void ApplyFireRate()
    {
        if (tankShooting != null)
        {
            float rate = GetCurrentFireRate();
            tankShooting.SetFireRate(rate);
            Debug.Log($"✓ 應用射速: {rate:F2}");
        }
        else
        {
            Debug.LogError($"❌ TankShooting 是 null，無法應用射速！物件: {gameObject.name}");
        }
    }

    // Getter 方法
    public float GetCurrentMoveSpeed() => baseMoveSpeed + (moveSpeedLevel * moveSpeedIncrement);
    public float GetCurrentBulletSpeed() => baseBulletSpeed + (bulletSpeedLevel * bulletSpeedIncrement);
    public float GetCurrentFireRate() => baseFireRate + (fireRateLevel * fireRateIncrement);

    public int GetMoveSpeedLevel() => moveSpeedLevel;
    public int GetBulletSpeedLevel() => bulletSpeedLevel;
    public int GetFireRateLevel() => fireRateLevel;

    public int GetMaxMoveSpeedLevel() => maxMoveSpeedLevel;
    public int GetMaxBulletSpeedLevel() => maxBulletSpeedLevel;
    public int GetMaxFireRateLevel() => maxFireRateLevel;

    public int GetAvailableUpgradePoints() => availableUpgradePoints;

    public bool CanUpgrade(StatType statType)
    {
        if (availableUpgradePoints <= 0) return false;

        switch (statType)
        {
            case StatType.MoveSpeed:
                return moveSpeedLevel < maxMoveSpeedLevel;
            case StatType.BulletSpeed:
                return bulletSpeedLevel < maxBulletSpeedLevel;
            case StatType.FireRate:
                return fireRateLevel < maxFireRateLevel;
            default:
                return false;
        }
    }

    /// <summary>
    /// 直接設置等級（用於載入存檔，不消耗升級點數）
    /// </summary>
    public void SetLevels(int moveLevel, int bulletLevel, int fireLevel, int points)
    {
        moveSpeedLevel = Mathf.Clamp(moveLevel, 0, maxMoveSpeedLevel);
        bulletSpeedLevel = Mathf.Clamp(bulletLevel, 0, maxBulletSpeedLevel);
        fireRateLevel = Mathf.Clamp(fireLevel, 0, maxFireRateLevel);
        availableUpgradePoints = points;

        // 立即應用所有屬性
        ApplyStats();
        
        // 通知UI更新
        OnUpgradePointsChanged?.Invoke(availableUpgradePoints);
        OnStatUpgraded?.Invoke(StatType.MoveSpeed, moveSpeedLevel, maxMoveSpeedLevel);
        OnStatUpgraded?.Invoke(StatType.BulletSpeed, bulletSpeedLevel, maxBulletSpeedLevel);
        OnStatUpgraded?.Invoke(StatType.FireRate, fireRateLevel, maxFireRateLevel);

        Debug.Log($"✓ 直接設置等級: 移動 Lv.{moveSpeedLevel}, 子彈 Lv.{bulletSpeedLevel}, 射速 Lv.{fireRateLevel}, 點數 {availableUpgradePoints}");
    }
}
