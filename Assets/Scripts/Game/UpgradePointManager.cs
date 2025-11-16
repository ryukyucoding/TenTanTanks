using UnityEngine;
using System.Collections;

/// <summary>
/// 升級點數管理器
/// 管理敵人擊殺並在每波結束時給予玩家升級點數
/// </summary>
public class UpgradePointManager : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private int upgradePointsPerWave = 3;  // 每波結束給予的升級點數
    
    [Header("References")]
    [SerializeField] private TankStats playerTankStats;
    
    private int enemiesKilledThisWave = 0;

    void Start()
    {
        Debug.Log("[UpgradePointManager] 初始化...");
        
        // 延迟查找玩家坦克，因为可能还没生成
        StartCoroutine(FindPlayerTankDelayed());
    }
    
    private IEnumerator FindPlayerTankDelayed()
    {
        // 等待 0.5 秒让 GameManager 先生成玩家
        yield return new WaitForSeconds(0.5f);
        
        if (playerTankStats == null)
        {
            FindPlayerTank();
        }
        else
        {
            Debug.Log($"[UpgradePointManager] TankStats 已设定: {playerTankStats.gameObject.name}");
        }
    }
    
    private void FindPlayerTank()
    {
        // 方法 1: 从 GameManager 获取（最可靠）
        GameObject playerObj = GameManager.GetPlayerTank();
        if (playerObj != null)
        {
            playerTankStats = playerObj.GetComponent<TankStats>();
            if (playerTankStats != null)
            {
                Debug.Log($"[UpgradePointManager] ✓ 从 GameManager 获取玩家: {playerObj.name}");
                Debug.Log($"  - InstanceID: {playerTankStats.GetInstanceID()}");
                Debug.Log($"  - GameObject InstanceID: {playerObj.GetInstanceID()}");
                Debug.Log($"  - Active: {playerObj.activeInHierarchy}");
                return;
            }
        }
        
        Debug.LogWarning("[UpgradePointManager] GameManager 未返回玩家，尝试 Player Tag...");
        
        // 方法 2: 使用 Player Tag 查找
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTankStats = playerObj.GetComponent<TankStats>();
            if (playerTankStats != null)
            {
                Debug.Log($"[UpgradePointManager] ✓ 通过 Tag 找到玩家: {playerObj.name}");
                Debug.Log($"  - InstanceID: {playerTankStats.GetInstanceID()}");
                return;
            }
        }
        
        Debug.LogWarning("[UpgradePointManager] 未找到 Player Tag，尝试备用方法...");
        
        // 方法 3: 查找活动且名字包含 Clone 的
        TankStats[] allStats = FindObjectsByType<TankStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"[UpgradePointManager] 找到 {allStats.Length} 个活动的 TankStats");
        
        foreach (var stats in allStats)
        {
            Debug.Log($"  - {stats.gameObject.name} (InstanceID: {stats.GetInstanceID()}, Active: {stats.gameObject.activeInHierarchy})");
            
            if (stats.gameObject.name.Contains("Clone") && stats.gameObject.activeInHierarchy)
            {
                playerTankStats = stats;
                Debug.Log($"[UpgradePointManager] ✓ 找到玩家 Clone: {stats.gameObject.name}");
                return;
            }
        }
        
        Debug.LogError("[UpgradePointManager] ❌ 未找到任何有效的玩家 TankStats");
    }

    /// <summary>
    /// 通知有敵人死亡
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilledThisWave++;
        Debug.Log($"[UpgradePointManager] 敵人死亡！本波已擊殺: {enemiesKilledThisWave}");
    }

    /// <summary>
    /// 波次完成時呼叫（由 WaveManager 呼叫）
    /// </summary>
    public void OnWaveComplete(int waveNumber)
    {
        Debug.Log($"===== [UpgradePointManager] 第 {waveNumber} 波完成！=====");
        Debug.Log($"[UpgradePointManager] 本波擊殺敵人數: {enemiesKilledThisWave}");
        
        // 如果还没找到玩家，尝试查找
        if (playerTankStats == null)
        {
            Debug.LogWarning("[UpgradePointManager] playerTankStats 是 null，尝试重新查找...");
            FindPlayerTank();
        }
        
        Debug.Log($"[UpgradePointManager] playerTankStats 是否为 null: {playerTankStats == null}");

        // 給予玩家升級點數
        if (playerTankStats != null)
        {
            Debug.Log($"[UpgradePointManager] 准备给予 {upgradePointsPerWave} 升级点数...");
            int pointsBefore = playerTankStats.GetAvailableUpgradePoints();
            playerTankStats.AddUpgradePoints(upgradePointsPerWave);
            int pointsAfter = playerTankStats.GetAvailableUpgradePoints();
            Debug.Log($"[UpgradePointManager] ✓ 升级点数: {pointsBefore} → {pointsAfter}");
        }
        else
        {
            Debug.LogError("[UpgradePointManager] ❌ playerTankStats 是 null，无法给予升级点数！");
        }

        // 重置計數
        enemiesKilledThisWave = 0;
    }

    /// <summary>
    /// 手動給予升級點數（測試用）
    /// </summary>
    [ContextMenu("Add 3 Upgrade Points (Test)")]
    public void TestAddPoints()
    {
        if (playerTankStats != null)
        {
            playerTankStats.AddUpgradePoints(3);
        }
    }
}
