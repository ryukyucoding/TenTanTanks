using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// UpgradeUI 測試工具
/// F9: 添加 1 個升級點數
/// F10: 顯示詳細狀態
/// </summary>
public class UpgradeUITester : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null) return;

        // F9: 添加升級點數
        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            GameObject playerObj = GameManager.GetPlayerTank();
            if (playerObj != null)
            {
                TankStats stats = playerObj.GetComponent<TankStats>();
                if (stats != null)
                {
                    stats.AddUpgradePoints(1);
                    Debug.Log($"[測試] ✅ 添加了 1 個升級點數，目前總共: {stats.GetAvailableUpgradePoints()}");
                }
                else
                {
                    Debug.LogError("[測試] ❌ 玩家沒有 TankStats 組件！");
                }
            }
            else
            {
                Debug.LogError("[測試] ❌ 找不到玩家物件！");
            }
        }

        // F10: 顯示狀態
        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {
            Debug.Log("========== UpgradeUI 狀態檢查 ==========");
            
            // 檢查玩家
            GameObject playerObj = GameManager.GetPlayerTank();
            Debug.Log($"玩家物件: {(playerObj != null ? playerObj.name : "null")}");
            
            if (playerObj != null)
            {
                TankStats stats = playerObj.GetComponent<TankStats>();
                Debug.Log($"TankStats: {(stats != null ? "✓" : "❌")}");
                if (stats != null)
                {
                    Debug.Log($"  - 升級點數: {stats.GetAvailableUpgradePoints()}");
                    Debug.Log($"  - InstanceID: {stats.GetInstanceID()}");
                }
            }
            
            // 檢查 UpgradeUI
            UpgradeUI ui = FindObjectOfType<UpgradeUI>();
            Debug.Log($"UpgradeUI: {(ui != null ? "✓" : "❌")}");
            
            if (ui != null)
            {
                // 使用反射檢查私有變數
                var tankStatsField = typeof(UpgradeUI).GetField("tankStats", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var upgradePanelField = typeof(UpgradeUI).GetField("upgradePanel", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var hasSubscribedField = typeof(UpgradeUI).GetField("hasSubscribedToEvents", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (tankStatsField != null)
                {
                    TankStats uiTankStats = tankStatsField.GetValue(ui) as TankStats;
                    Debug.Log($"  - tankStats: {(uiTankStats != null ? uiTankStats.gameObject.name : "null")}");
                    if (uiTankStats != null)
                    {
                        Debug.Log($"    - InstanceID: {uiTankStats.GetInstanceID()}");
                    }
                }
                
                if (upgradePanelField != null)
                {
                    GameObject panel = upgradePanelField.GetValue(ui) as GameObject;
                    Debug.Log($"  - upgradePanel: {(panel != null ? panel.name : "null")}");
                    if (panel != null)
                    {
                        Debug.Log($"    - activeSelf: {panel.activeSelf}");
                        Debug.Log($"    - activeInHierarchy: {panel.activeInHierarchy}");
                    }
                }
                
                if (hasSubscribedField != null)
                {
                    bool hasSubscribed = (bool)hasSubscribedField.GetValue(ui);
                    Debug.Log($"  - hasSubscribedToEvents: {hasSubscribed}");
                }
            }
            
            Debug.Log("=====================================");
        }
    }
}
