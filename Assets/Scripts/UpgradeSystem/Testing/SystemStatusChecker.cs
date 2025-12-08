using UnityEngine;

/// <summary>
/// 系統狀態檢查器 - 在遊戲開始時顯示所有關鍵系統的狀態
/// </summary>
public class SystemStatusChecker : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(CheckSystemStatus), 1f);
    }

    void CheckSystemStatus()
    {
        Debug.Log("========== 系統狀態檢查 ==========");

        // 檢查 PlayerDataManager
        if (PlayerDataManager.Instance != null)
        {
            Debug.Log("✅ PlayerDataManager: 存在");
            string savedTransform = PlayerDataManager.Instance.GetCurrentTankTransformation();
            Debug.Log($"   - 保存的變形: {savedTransform}");
            Debug.Log($"   - 移動速度等級: {PlayerDataManager.Instance.moveSpeedLevel}");
            Debug.Log($"   - 子彈速度等級: {PlayerDataManager.Instance.bulletSpeedLevel}");
            Debug.Log($"   - 射速等級: {PlayerDataManager.Instance.fireRateLevel}");
            Debug.Log($"   - 升級點數: {PlayerDataManager.Instance.availableUpgradePoints}");
        }
        else
        {
            Debug.LogError("❌ PlayerDataManager: 不存在！");
        }

        // 檢查玩家坦克
        GameObject player = GameManager.GetPlayerTank();
        if (player != null)
        {
            Debug.Log($"✅ 玩家坦克: {player.name}");
            
            // 檢查子物件
            Debug.Log($"   子物件數量: {player.transform.childCount}");
            for (int i = 0; i < player.transform.childCount; i++)
            {
                Transform child = player.transform.GetChild(i);
                Debug.Log($"      [{i}] {child.name} (Active: {child.gameObject.activeSelf})");
            }

            // 檢查 TankStats
            TankStats tankStats = player.GetComponent<TankStats>();
            if (tankStats != null)
            {
                Debug.Log("   ✅ TankStats 組件存在");
                Debug.Log($"      - 移動速度: {tankStats.GetCurrentMoveSpeed():F2}");
                Debug.Log($"      - 子彈速度: {tankStats.GetCurrentBulletSpeed():F2}");
                Debug.Log($"      - 射速: {tankStats.GetCurrentFireRate():F2}");
            }
            else
            {
                Debug.LogWarning("   ⚠️ TankStats 組件不存在");
            }

            // 檢查 TankTransformationManager
            TankTransformationManager transformManager = player.GetComponent<TankTransformationManager>();
            if (transformManager != null)
            {
                Debug.Log("   ✅ TankTransformationManager 組件存在");
            }
            else
            {
                Debug.LogWarning("   ⚠️ TankTransformationManager 組件不存在");
            }
        }
        else
        {
            Debug.LogError("❌ 玩家坦克不存在！");
        }

        // 檢查 Transition 系統
        var enhancedMover = FindFirstObjectByType<EnhancedTransitionMover>();
        if (enhancedMover != null)
        {
            Debug.Log("  ✅ EnhancedTransitionMover found!");
        }

        var transitionUpgrade = FindFirstObjectByType<TransitionWheelUpgrade>();
        if (transitionUpgrade != null)
        {
            Debug.Log("  ✅ TransitionWheelUpgrade found!");
        }

        var upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();
        if (upgradeWheelUI != null)
        {
            Debug.Log("✅ UpgradeWheelUI: 存在");
        }
        else
        {
            Debug.LogWarning("⚠️ UpgradeWheelUI: 不存在（第二次變形需要此組件）");
        }

        Debug.Log("====================================");
    }

    void Update()
    {
        // 按 F12 重新檢查狀態
        if (Input.GetKeyDown(KeyCode.F12))
        {
            CheckSystemStatus();
        }
    }
}
