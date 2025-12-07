using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試變形系統的工具
/// F1-F3: 測試 Tier 1 變形
/// F4-F9: 測試 Tier 2 變形
/// F10: 顯示當前狀態
/// F11: 添加升級點數並升級屬性
/// </summary>
public class TransformationTester : MonoBehaviour
{
    private TankTransformationManager transformationManager;
    private TankStats tankStats;
    private bool hasInitialized = false;

    void Start()
    {
        // 延遲初始化，等待玩家坦克生成
        StartCoroutine(InitializeDelayed());
    }

    private System.Collections.IEnumerator InitializeDelayed()
    {
        // 等待 1 秒讓玩家坦克生成
        yield return new WaitForSeconds(1f);
        
        TryFindPlayer();
        
        if (!hasInitialized)
        {
            Debug.LogWarning("[TransformationTester] 延遲初始化後仍找不到玩家，將在 Update 中持續尋找");
        }
    }

    private void TryFindPlayer()
    {
        if (hasInitialized) return;

        GameObject player = GameManager.GetPlayerTank();
        
        // 如果 GameManager 沒找到，嘗試其他方法
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player == null)
        {
            var allTankStats = FindObjectsByType<TankStats>(FindObjectsSortMode.None);
            if (allTankStats.Length > 0)
            {
                player = allTankStats[0].gameObject;
                Debug.Log($"[TransformationTester] 通過 TankStats 找到玩家: {player.name}");
            }
        }

        if (player != null)
        {
            transformationManager = player.GetComponent<TankTransformationManager>();
            tankStats = player.GetComponent<TankStats>();
            
            hasInitialized = true;
            Debug.Log("[TransformationTester] ✅ 初始化完成");
            Debug.Log($"  - 玩家: {player.name}");
            Debug.Log($"  - TankTransformationManager: {(transformationManager != null ? "✓" : "❌")}");
            Debug.Log($"  - TankStats: {(tankStats != null ? "✓" : "❌")}");
        }
        else
        {
            Debug.LogWarning("[TransformationTester] ⚠️ 找不到玩家坦克");
        }
    }

    void Update()
    {
        // 如果還沒初始化，持續嘗試找玩家
        if (!hasInitialized)
        {
            TryFindPlayer();
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // === TIER 1 TRANSFORMATIONS ===
        if (keyboard.f1Key.wasPressedThisFrame)
        {
            Debug.Log("[TransformationTester] 🔑 按下 F1 鍵");
            TestTransformation("Heavy");
        }
        else if (keyboard.f2Key.wasPressedThisFrame)
        {
            Debug.Log("[TransformationTester] 🔑 按下 F2 鍵");
            TestTransformation("Rapid");
        }
        else if (keyboard.f3Key.wasPressedThisFrame)
        {
            Debug.Log("[TransformationTester] 🔑 按下 F3 鍵");
            TestTransformation("Balanced");
        }

        // === TIER 2 HEAVY TRANSFORMATIONS ===
        else if (keyboard.f4Key.wasPressedThisFrame)
        {
            TestTransformation("ArmorPiercing");
        }
        else if (keyboard.f5Key.wasPressedThisFrame)
        {
            TestTransformation("SuperHeavy");
        }

        // === TIER 2 RAPID TRANSFORMATIONS ===
        else if (keyboard.f6Key.wasPressedThisFrame)
        {
            TestTransformation("Burst");
        }
        else if (keyboard.f7Key.wasPressedThisFrame)
        {
            TestTransformation("MachineGun");
        }

        // === TIER 2 BALANCED TRANSFORMATIONS ===
        else if (keyboard.f8Key.wasPressedThisFrame)
        {
            TestTransformation("Tactical");
        }
        else if (keyboard.f9Key.wasPressedThisFrame)
        {
            TestTransformation("Versatile");
        }

        // === STATUS AND UPGRADE TESTING ===
        else if (keyboard.f10Key.wasPressedThisFrame)
        {
            ShowCurrentStatus();
        }
        else if (keyboard.f11Key.wasPressedThisFrame)
        {
            TestUpgradeStats();
        }
    }

    private void TestTransformation(string upgradeName)
    {
        if (!hasInitialized)
        {
            Debug.LogWarning("[TransformationTester] ⚠️ 尚未初始化，無法測試變形");
            TryFindPlayer(); // 再次嘗試找玩家
            return;
        }

        if (transformationManager == null)
        {
            Debug.LogError("[TransformationTester] ❌ TankTransformationManager 未找到！");
            Debug.LogError("[TransformationTester] 請確認玩家坦克上是否有 TankTransformationManager 組件");
            
            // 嘗試重新找玩家
            GameObject player = GameManager.GetPlayerTank();
            if (player != null)
            {
                Debug.Log($"[TransformationTester] 找到玩家: {player.name}");
                Debug.Log($"[TransformationTester] 玩家組件列表:");
                var components = player.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp != null)
                        Debug.Log($"  - {comp.GetType().Name}");
                }
            }
            return;
        }

        Debug.Log($"[TransformationTester] ========== 測試變形: {upgradeName} ==========");
        Debug.Log($"[TransformationTester] 調用 TankTransformationManager.OnUpgradeSelected(\"{upgradeName}\")");
        
        try
        {
            transformationManager.OnUpgradeSelected(upgradeName);
            Debug.Log($"[TransformationTester] ✅ 變形方法調用成功，等待 0.5 秒後顯示狀態...");
            Invoke(nameof(ShowCurrentStatus), 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TransformationTester] ❌ 變形失敗: {e.Message}");
            Debug.LogError($"[TransformationTester] Stack trace: {e.StackTrace}");
        }
    }

    private void ShowCurrentStatus()
    {
        Debug.Log("========== 當前坦克狀態 ==========");
        
        if (!hasInitialized)
        {
            Debug.LogWarning("⚠️ 尚未找到玩家坦克，嘗試重新尋找...");
            TryFindPlayer();
            if (!hasInitialized)
            {
                Debug.LogError("❌ 仍然找不到玩家坦克！");
                return;
            }
        }

        GameObject player = GameManager.GetPlayerTank();
        if (player == null && tankStats != null)
        {
            player = tankStats.gameObject;
        }
        
        if (player == null)
        {
            Debug.LogError("❌ 找不到玩家坦克！");
            return;
        }

        // 顯示子物件
        Debug.Log($"PlayerTank 子物件數量: {player.transform.childCount}");
        for (int i = 0; i < player.transform.childCount; i++)
        {
            Transform child = player.transform.GetChild(i);
            Debug.Log($"  [{i}] {child.name} (Active: {child.gameObject.activeSelf})");
        }

        // 顯示屬性
        if (tankStats != null)
        {
            Debug.Log($"\n當前屬性:");
            Debug.Log($"  - 移動速度: {tankStats.GetCurrentMoveSpeed():F2}");
            Debug.Log($"  - 子彈速度: {tankStats.GetCurrentBulletSpeed():F2}");
            Debug.Log($"  - 射速: {tankStats.GetCurrentFireRate():F2}");
            Debug.Log($"  - 升級點數: {tankStats.GetAvailableUpgradePoints()}");
            Debug.Log($"  - 移動速度等級: {tankStats.GetMoveSpeedLevel()}");
            Debug.Log($"  - 子彈速度等級: {tankStats.GetBulletSpeedLevel()}");
            Debug.Log($"  - 射速等級: {tankStats.GetFireRateLevel()}");
        }

        // 顯示當前變形
        if (transformationManager != null)
        {
            // 使用反射獲取私有變數
            var field = typeof(TankTransformationManager).GetField("currentUpgrade", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                string currentUpgrade = field.GetValue(transformationManager) as string;
                Debug.Log($"\n當前變形: {currentUpgrade}");
            }
        }

        Debug.Log("===================================");
    }

    private void TestUpgradeStats()
    {
        if (!hasInitialized)
        {
            Debug.LogWarning("[TransformationTester] ⚠️ 尚未初始化，無法測試升級");
            TryFindPlayer();
            return;
        }

        if (tankStats == null)
        {
            Debug.LogError("[TransformationTester] ❌ TankStats 未找到！");
            return;
        }

        Debug.Log("[TransformationTester] 添加升級點數並升級屬性...");
        
        // 添加升級點數
        tankStats.AddUpgradePoints(3);
        
        // 升級各項屬性
        tankStats.TryUpgradeStat(TankStats.StatType.MoveSpeed);
        tankStats.TryUpgradeStat(TankStats.StatType.BulletSpeed);
        tankStats.TryUpgradeStat(TankStats.StatType.FireRate);
        
        Debug.Log("[TransformationTester] 升級完成！");
        ShowCurrentStatus();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        GUILayout.Label("=== 變形系統測試 ===");
        GUILayout.Label("Tier 1:");
        GUILayout.Label("  F1: Heavy (重型)");
        GUILayout.Label("  F2: Rapid (快速)");
        GUILayout.Label("  F3: Balanced (平衡)");
        GUILayout.Label("");
        GUILayout.Label("Tier 2 Heavy:");
        GUILayout.Label("  F4: ArmorPiercing (破甲)");
        GUILayout.Label("  F5: SuperHeavy (超重型)");
        GUILayout.Label("");
        GUILayout.Label("Tier 2 Rapid:");
        GUILayout.Label("  F6: Burst (爆發)");
        GUILayout.Label("  F7: MachineGun (機槍)");
        GUILayout.Label("");
        GUILayout.Label("Tier 2 Balanced:");
        GUILayout.Label("  F8: Tactical (戰術)");
        GUILayout.Label("  F9: Versatile (多功能)");
        GUILayout.Label("");
        GUILayout.Label("其他:");
        GUILayout.Label("  F10: 顯示當前狀態");
        GUILayout.Label("  F11: 升級屬性測試");
        GUILayout.EndArea();
    }
}
