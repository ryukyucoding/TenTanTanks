using UnityEngine;

public class LevelSystemDebugger : MonoBehaviour
{
    [Header("調試設定")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log("=== 關卡系統調試開始 ===");
            CheckSystemStatus();
        }
    }
    
    private void Update()
    {
        if (showDebugInfo && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame)
        {
            CheckSystemStatus();
        }
    }
    
    [ContextMenu("檢查系統狀態")]
    public void CheckSystemStatus()
    {
        Debug.Log("=== 關卡系統狀態檢查 ===");
        
        // 檢查 LevelManager
        if (LevelManager.Instance != null)
        {
            Debug.Log($"✅ LevelManager 存在");
            Debug.Log($"   當前關卡索引: {LevelManager.Instance.CurrentLevelIndex}");
            Debug.Log($"   總關卡數: {LevelManager.Instance.TotalLevels}");
            Debug.Log($"   關卡是否激活: {LevelManager.Instance.IsLevelActive}");
            
            if (LevelManager.Instance.CurrentLevelData != null)
            {
                var levelData = LevelManager.Instance.CurrentLevelData;
                Debug.Log($"   當前關卡: {levelData.levelName}");
                Debug.Log($"   敵人波數: {levelData.enemyWaves.Count}");
                
                for (int i = 0; i < levelData.enemyWaves.Count; i++)
                {
                    var wave = levelData.enemyWaves[i];
                    Debug.Log($"     波數 {i + 1}: {wave.enemyCount} 個敵人, 預製體: {(wave.enemyPrefab != null ? wave.enemyPrefab.name : "未設定")}");
                }
            }
            else
            {
                Debug.LogWarning("❌ 當前關卡數據為空！");
            }
        }
        else
        {
            Debug.LogError("❌ LevelManager 不存在！");
        }
        
        // 檢查 WaveManager
        if (WaveManager.Instance != null)
        {
            Debug.Log($"✅ WaveManager 存在");
            Debug.Log($"   當前波數索引: {WaveManager.Instance.CurrentWaveIndex}");
            Debug.Log($"   總波數: {WaveManager.Instance.TotalWaves}");
            Debug.Log($"   當前波敵人數: {WaveManager.Instance.EnemiesInCurrentWave}");
            Debug.Log($"   已生成敵人: {WaveManager.Instance.EnemiesSpawnedInWave}");
            Debug.Log($"   已消滅敵人: {WaveManager.Instance.EnemiesKilledInWave}");
            Debug.Log($"   波數是否激活: {WaveManager.Instance.IsWaveActive}");
            Debug.Log($"   所有波數完成: {WaveManager.Instance.IsAllWavesComplete}");
            Debug.Log($"   波數信息: {WaveManager.Instance.GetCurrentWaveInfo()}");
        }
        else
        {
            Debug.LogError("❌ WaveManager 不存在！");
        }
        
        // 檢查 GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log($"✅ GameManager 存在");
            Debug.Log($"   遊戲狀態: {GameManager.Instance.CurrentState}");
            Debug.Log($"   剩餘時間: {GameManager.Instance.RemainingTime:F1}");
            Debug.Log($"   剩餘敵人: {GameManager.Instance.RemainingEnemies}");
        }
        else
        {
            Debug.LogError("❌ GameManager 不存在！");
        }
        
        // 檢查場景中的敵人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"場景中敵人數量: {enemies.Length}");
        
        // 檢查生成點
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        Debug.Log($"場景中生成點數量: {spawnPoints.Length}");
        
        // 檢查 WaveManager 的 defaultSpawnPoints
        if (WaveManager.Instance != null)
        {
            var waveManager = WaveManager.Instance.GetComponent<WaveManager>();
            if (waveManager != null)
            {
                // 使用反射來檢查 defaultSpawnPoints
                var field = typeof(WaveManager).GetField("defaultSpawnPoints", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var defaultSpawnPoints = field.GetValue(waveManager) as Transform[];
                    Debug.Log($"WaveManager defaultSpawnPoints 數量: {(defaultSpawnPoints != null ? defaultSpawnPoints.Length : 0)}");
                }
            }
        }
        
        Debug.Log("=== 關卡系統狀態檢查完成 ===");
    }
    
    [ContextMenu("強制開始第一波")]
    public void ForceStartFirstWave()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartNextWave();
            Debug.Log("強制開始第一波");
        }
        else
        {
            Debug.LogError("WaveManager 不存在，無法開始波數");
        }
    }
    
    [ContextMenu("重新載入關卡")]
    public void ReloadLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
            Debug.Log("重新載入關卡");
        }
        else
        {
            Debug.LogError("LevelManager 不存在，無法重新載入關卡");
        }
    }
}
