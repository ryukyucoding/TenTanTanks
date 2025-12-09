using UnityEngine;
using WheelUpgradeSystem;

/// <summary>
/// 遊戲數據重置工具類
/// 用於在玩家死亡後回到主選單時重置所有升級和配置
/// </summary>
public static class GameDataResetter
{
    /// <summary>
    /// 重置所有遊戲數據到初始狀態
    /// 包括：
    /// - 玩家升級點數和等級
    /// - 玩家生命值
    /// - 坦克變形狀態
    /// - 輪盤升級配置
    /// </summary>
    public static void ResetAllGameData()
    {
        Debug.Log("========== 開始重置所有遊戲數據 ==========");

        // 1. 重置 PlayerDataManager（升級點數、等級、生命值、坦克變形）
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetData();
            Debug.Log("✓ 已重置 PlayerDataManager 數據");
        }
        else
        {
            Debug.LogWarning("⚠ PlayerDataManager.Instance 不存在");
        }

        // 2. 重置輪盤升級系統到 Basic
        var wheelSystem = Object.FindFirstObjectByType<TankUpgradeSystem>();
        if (wheelSystem != null)
        {
            wheelSystem.ApplyUpgrade("Basic");
            Debug.Log("✓ 已重置輪盤升級系統到 Basic");
        }
        else
        {
            Debug.Log("⚠ TankUpgradeSystem 不存在（可能在非遊戲場景中）");
        }

        // 3. 清除 PlayerPrefs 中保存的配置
        PlayerPrefs.DeleteKey("WheelUpgradePath");
        PlayerPrefs.Save();
        Debug.Log("✓ 已清除保存的輪盤配置");

        Debug.Log("========== 遊戲數據重置完成 ==========");
    }

    /// <summary>
    /// 僅重置玩家統計數據（不影響輪盤配置）
    /// </summary>
    public static void ResetPlayerStatsOnly()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetData();
            Debug.Log("✓ 已重置玩家統計數據");
        }
    }

    /// <summary>
    /// 僅重置輪盤升級配置（不影響玩家統計）
    /// </summary>
    public static void ResetWheelUpgradesOnly()
    {
        var wheelSystem = Object.FindFirstObjectByType<TankUpgradeSystem>();
        if (wheelSystem != null)
        {
            wheelSystem.ApplyUpgrade("Basic");
        }

        PlayerPrefs.DeleteKey("WheelUpgradePath");
        PlayerPrefs.Save();
        Debug.Log("✓ 已重置輪盤升級配置");
    }
}
