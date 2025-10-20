using UnityEngine;

public class WaveDebugger : MonoBehaviour
{
    [Header("調試設定")]
    [SerializeField] private bool enableDetailedLogs = true;
    
    private void Start()
    {
        if (enableDetailedLogs)
        {
            StartCoroutine(DebugWaveSystem());
        }
    }
    
    private System.Collections.IEnumerator DebugWaveSystem()
    {
        // 等待系統初始化
        yield return new WaitForSeconds(1f);
        
        Debug.Log("=== 詳細波數系統調試 ===");
        
        // 檢查 LevelManager
        if (LevelManager.Instance != null)
        {
            Debug.Log($"LevelManager 狀態:");
            Debug.Log($"  當前關卡索引: {LevelManager.Instance.CurrentLevelIndex}");
            Debug.Log($"  總關卡數: {LevelManager.Instance.TotalLevels}");
            Debug.Log($"  關卡激活: {LevelManager.Instance.IsLevelActive}");
            
            if (LevelManager.Instance.CurrentLevelData != null)
            {
                var levelData = LevelManager.Instance.CurrentLevelData;
                Debug.Log($"  關卡名稱: {levelData.levelName}");
                Debug.Log($"  敵人波數: {levelData.enemyWaves.Count}");
                
                for (int i = 0; i < levelData.enemyWaves.Count; i++)
                {
                    var wave = levelData.enemyWaves[i];
                    Debug.Log($"    波數 {i + 1}:");
                    Debug.Log($"      敵人數量: {wave.enemyCount}");
                    Debug.Log($"      敵人預製體: {(wave.enemyPrefab != null ? wave.enemyPrefab.name : "未設定")}");
                    Debug.Log($"      波數延遲: {wave.waveDelay}");
                    Debug.Log($"      生成間隔: {wave.spawnInterval}");
                    Debug.Log($"      生成點數量: {(wave.spawnPoints != null ? wave.spawnPoints.Length : 0)}");
                }
            }
        }
        
        // 檢查 WaveManager
        if (WaveManager.Instance != null)
        {
            Debug.Log($"WaveManager 狀態:");
            Debug.Log($"  當前波數索引: {WaveManager.Instance.CurrentWaveIndex}");
            Debug.Log($"  總波數: {WaveManager.Instance.TotalWaves}");
            Debug.Log($"  波數激活: {WaveManager.Instance.IsWaveActive}");
            Debug.Log($"  所有波數完成: {WaveManager.Instance.IsAllWavesComplete}");
            
            // 檢查 defaultSpawnPoints
            var waveManager = WaveManager.Instance.GetComponent<WaveManager>();
            if (waveManager != null)
            {
                var field = typeof(WaveManager).GetField("defaultSpawnPoints", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var defaultSpawnPoints = field.GetValue(waveManager) as Transform[];
                    Debug.Log($"  Default Spawn Points: {(defaultSpawnPoints != null ? defaultSpawnPoints.Length : 0)} 個");
                    
                    if (defaultSpawnPoints != null)
                    {
                        for (int i = 0; i < defaultSpawnPoints.Length; i++)
                        {
                            Debug.Log($"    SpawnPoint {i}: {(defaultSpawnPoints[i] != null ? defaultSpawnPoints[i].name : "null")}");
                        }
                    }
                }
            }
        }
        
        // 檢查場景中的生成點
        try
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            Debug.Log($"場景中 SpawnPoint 標籤物件: {spawnPoints.Length} 個");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"無法使用 SpawnPoint 標籤查找物件: {e.Message}");
        }
        
        // 檢查所有可能的生成點
        Transform[] allSpawnPoints = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        int spawnPointCount = 0;
        foreach (var t in allSpawnPoints)
        {
            if (t.name.ToLower().Contains("spawn"))
            {
                Debug.Log($"  找到生成點: {t.name} 位置: {t.position}");
                spawnPointCount++;
            }
        }
        Debug.Log($"場景中名稱包含 'spawn' 的物件: {spawnPointCount} 個");
        
        // 檢查敵人預製體
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
        {
            var levelData = LevelManager.Instance.CurrentLevelData;
            for (int i = 0; i < levelData.enemyWaves.Count; i++)
            {
                var wave = levelData.enemyWaves[i];
                if (wave.enemyPrefab == null)
                {
                    Debug.LogError($"波數 {i + 1} 的敵人預製體未設定！");
                }
                else
                {
                    Debug.Log($"波數 {i + 1} 敵人預製體: {wave.enemyPrefab.name}");
                }
            }
        }
        
        Debug.Log("=== 詳細調試完成 ===");
    }
    
    [ContextMenu("強制開始波數")]
    public void ForceStartWave()
    {
        if (WaveManager.Instance != null)
        {
            Debug.Log("強制開始波數...");
            WaveManager.Instance.StartNextWave();
        }
        else
        {
            Debug.LogError("WaveManager 不存在！");
        }
    }
    
    [ContextMenu("檢查生成點")]
    public void CheckSpawnPoints()
    {
        Debug.Log("=== 生成點檢查 ===");
        
        // 檢查標籤為 SpawnPoint 的物件
        try
        {
            GameObject[] taggedSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            Debug.Log($"標籤為 'SpawnPoint' 的物件: {taggedSpawnPoints.Length} 個");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"無法使用 SpawnPoint 標籤查找物件: {e.Message}");
        }
        
        // 檢查名稱包含 spawn 的物件
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var t in allTransforms)
        {
            if (t.name.ToLower().Contains("spawn"))
            {
                Debug.Log($"找到生成點: {t.name} 位置: {t.position}");
            }
        }
    }
}
