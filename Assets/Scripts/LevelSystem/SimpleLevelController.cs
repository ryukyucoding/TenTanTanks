using UnityEngine;

public class SimpleLevelController : MonoBehaviour
{
    [Header("簡化關卡控制")]
    [SerializeField] private LevelDataAsset levelDataAsset;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float waveStartDelay = 2f;
    
    [Header("狀態")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool isWaveActive = false;
    [SerializeField] private int enemiesSpawnedInWave = 0;
    [SerializeField] private int enemiesKilledInWave = 0;
    
    private LevelData currentLevelData;
    private int totalWaves = 0;
    private UpgradePointManager upgradeManager; // 缓存引用
    
    private void Start()
    {
        Debug.Log("=== 簡化關卡控制器啟動 ===");
        
        // 清理現有敵人
        ClearAllEnemies();
        
        // 初始化關卡
        InitializeLevel();
        
        // 初始化 UpgradePointManager
        InitializeUpgradeManager();
        
        // 開始第一波
        StartCoroutine(StartFirstWave());
    }
    
    private void InitializeLevel()
    {
        if (levelDataAsset != null)
        {
            currentLevelData = levelDataAsset.levelData;
            totalWaves = currentLevelData.enemyWaves.Count;
            
            Debug.Log($"關卡初始化: {currentLevelData.levelName}");
            Debug.Log($"總波數: {totalWaves}");
            
            for (int i = 0; i < totalWaves; i++)
            {
                var wave = currentLevelData.enemyWaves[i];
                Debug.Log($"  波數 {i + 1}: {wave.enemyCount} 個敵人");
            }
        }
        else
        {
            Debug.LogError("沒有設定關卡數據！");
        }
    }
    
    private void InitializeUpgradeManager()
    {
        // 尋找現有的 UpgradePointManager
        upgradeManager = FindFirstObjectByType<UpgradePointManager>();
        
        if (upgradeManager == null)
        {
            // 如果不存在，自動創建
            Debug.Log("[SimpleLevelController] 場景中沒有 UpgradePointManager，自動創建...");
            GameObject managerObj = new GameObject("UpgradePointManager");
            upgradeManager = managerObj.AddComponent<UpgradePointManager>();
            Debug.Log("[SimpleLevelController] ✓ UpgradePointManager 已創建");
        }
        else
        {
            Debug.Log($"[SimpleLevelController] ✓ 找到 UpgradePointManager: {upgradeManager.gameObject.name}");
        }
    }
    
    private System.Collections.IEnumerator StartFirstWave()
    {
        yield return new WaitForSeconds(waveStartDelay);
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        if (currentWaveIndex >= totalWaves)
        {
            Debug.Log("所有波數已完成！");
            return;
        }
        
        var currentWave = currentLevelData.enemyWaves[currentWaveIndex];
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        isWaveActive = true;
        
        Debug.Log($"開始第 {currentWaveIndex + 1} 波，敵人數量: {currentWave.enemyCount}");
        
        // 開始生成敵人
        StartCoroutine(SpawnWaveEnemies(currentWave));
    }
    
    private System.Collections.IEnumerator SpawnWaveEnemies(EnemyWave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave, i);
            enemiesSpawnedInWave++;
            
            if (i < wave.enemyCount - 1)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }
    
    /// <summary>
    /// 生成單一敵人，支援：
    /// - 每一隻敵人獨立的 prefab / spawnPoint（使用 EnemyWave.enemyEntries）
    /// - 整波共用 prefab / spawnPoints
    /// - 最後退回到 SimpleLevelController 自己的 spawnPoints 或隨機位置
    /// </summary>
    private void SpawnEnemy(EnemyWave wave, int indexInWave)
    {
        GameObject prefabToUse = ResolveEnemyPrefab(wave, indexInWave);
        if (prefabToUse == null)
        {
            Debug.LogError($"敵人預製體未設定！（波數 {currentWaveIndex + 1}, 敵人索引 {indexInWave}）");
            return;
        }
        
        Vector3 spawnPosition = ResolveSpawnPosition(wave, indexInWave);
        GameObject enemy = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy";
        
        Debug.Log($"生成敵人: {enemy.name} 在位置 {spawnPosition}");

        // 通知 GameManager 有新敵人生成，維護總敵人數量
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemySpawned();
        }
    }
    
    /// <summary>
    /// 根據 per-enemy / per-wave / 控制器預設決定要用哪個 prefab。
    /// 優先順序：
    /// 1. wave.enemyEntries[index].enemyPrefab
    /// 2. wave.enemyPrefab
    /// </summary>
    private GameObject ResolveEnemyPrefab(EnemyWave wave, int indexInWave)
    {
        // 1. 每一隻敵人獨立設定
        if (wave.enemyEntries != null &&
            indexInWave >= 0 &&
            indexInWave < wave.enemyEntries.Length)
        {
            var entry = wave.enemyEntries[indexInWave];
            if (entry != null && entry.enemyPrefab != null)
            {
                return entry.enemyPrefab;
            }
        }
        
        // 2. 整波共用設定（LevelData / LevelDataNew 裡面設定）
        if (wave.enemyPrefab != null)
        {
            return wave.enemyPrefab;
        }

        // 如果都沒有設定，就返回 null，讓外層決定是否報錯
        return null;
    }
    
    /// <summary>
    /// 根據 per-enemy / 控制器預設決定生成位置。
    /// 優先順序：
    /// 1. wave.enemyEntries[index].spawnPointIndex 對應控制器的 spawnPoints
    /// 2. SimpleLevelController.spawnPoints（如果有，隨機挑一個）
    /// 3. 最後使用隨機位置
    /// </summary>
    private Vector3 ResolveSpawnPosition(EnemyWave wave, int indexInWave)
    {
        // 1. 每一隻敵人獨立設定（使用索引對應控制器的 spawnPoints）
        if (wave.enemyEntries != null &&
            indexInWave >= 0 &&
            indexInWave < wave.enemyEntries.Length)
        {
            var entry = wave.enemyEntries[indexInWave];
            if (entry != null && entry.spawnPointIndex >= 0 &&
                spawnPoints != null &&
                entry.spawnPointIndex < spawnPoints.Length)
            {
                var pointByIndex = spawnPoints[entry.spawnPointIndex];
                if (pointByIndex != null)
                {
                    return pointByIndex.position;
                }
            }
        }
        
        // 2. 控制器共用 spawnPoints
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            var point = spawnPoints[randomIndex];
            if (point != null)
            {
                return point.position;
            }
        }
        
        // 3. 完全隨機
        return new Vector3(
            Random.Range(-15f, 15f),
            0f,
            Random.Range(-15f, 15f)
        );
    }
    
    public void OnEnemyDestroyed()
    {
        if (!isWaveActive) return;
        
        enemiesKilledInWave++;
        Debug.Log($"敵人被消滅: {enemiesKilledInWave}/{enemiesSpawnedInWave}");
        
        // 檢查當前波是否完成
        if (enemiesKilledInWave >= enemiesSpawnedInWave)
        {
            CompleteCurrentWave();
        }
    }
    
    private void CompleteCurrentWave()
    {
        isWaveActive = false;
        Debug.Log($"[SimpleLevelController] 第 {currentWaveIndex + 1} 波完成！");
        
        // 通知 UpgradePointManager 波次完成，給予升級點數
        if (upgradeManager != null)
        {
            Debug.Log($"[SimpleLevelController] ✓ 呼叫 OnWaveComplete({currentWaveIndex + 1})");
            upgradeManager.OnWaveComplete(currentWaveIndex + 1);
        }
        else
        {
            Debug.LogWarning("[SimpleLevelController] ⚠️ UpgradePointManager 未初始化！");
        }
        
        currentWaveIndex++;
        
        // 檢查是否還有下一波
        if (currentWaveIndex < totalWaves)
        {
            var nextWave = currentLevelData.enemyWaves[currentWaveIndex];
            StartCoroutine(WaitForNextWave(nextWave.waveDelay));
        }
        else
        {
            Debug.Log("所有波數已完成！");
            
            // 通知 GameManager 關卡完成（全部波數打完）
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Victory();
            }
        }
    }
    
    private System.Collections.IEnumerator WaitForNextWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }
    
    private void ClearAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                DestroyImmediate(enemy);
            }
        }
        Debug.Log($"清除了 {enemies.Length} 個敵人");
    }
    
    [ContextMenu("重新開始關卡")]
    public void RestartLevel()
    {
        ClearAllEnemies();
        currentWaveIndex = 0;
        isWaveActive = false;
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        
        InitializeLevel();
        StartCoroutine(StartFirstWave());
    }
    
    [ContextMenu("強制開始下一波")]
    public void ForceNextWave()
    {
        if (isWaveActive)
        {
            CompleteCurrentWave();
        }
        else
        {
            StartNextWave();
        }
    }

    // 供場景管理器調用的方法（兼容）
    public void SetLevelData(LevelDataAsset levelDataAsset)
    {
        if (levelDataAsset != null)
        {
            currentLevelData = levelDataAsset.levelData;
            totalWaves = currentLevelData != null && currentLevelData.enemyWaves != null
                ? currentLevelData.enemyWaves.Count
                : 0;
            Debug.Log($"設定關卡數據: {currentLevelData?.levelName ?? "(null)"}");
        }
    }

    public void SetSpawnPoints(Transform[] spawnPoints)
    {
        this.spawnPoints = spawnPoints;
        Debug.Log($"設定生成點: {spawnPoints?.Length ?? 0} 個");
    }

}
