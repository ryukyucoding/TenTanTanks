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

    void Awake()
    {
        // 單例模式 + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✓ PlayerDataManager 已創建並設為持久化");
        }
        else
        {
            Debug.Log("PlayerDataManager 已存在，銷毀重複物件");
            Destroy(gameObject);
        }
    }

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
        Debug.Log("✓ 玩家數據已重置");
    }
}
