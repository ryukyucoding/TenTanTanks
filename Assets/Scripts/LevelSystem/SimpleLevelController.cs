using UnityEngine;

public class SimpleLevelController : MonoBehaviour
{
    [Header("簡化關卡控制")]
    [SerializeField] private LevelDataAsset levelDataAsset;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject enemyPrefab;
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
            SpawnEnemy();
            enemiesSpawnedInWave++;
            
            if (i < wave.enemyCount - 1)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }
    
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("敵人預製體未設定！");
            return;
        }
        
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy";
        
        Debug.Log($"生成敵人: {enemy.name} 在位置 {spawnPosition}");
    }
    
    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex].position;
        }
        else
        {
            // 使用隨機位置
            return new Vector3(
                Random.Range(-15f, 15f),
                0f,
                Random.Range(-15f, 15f)
            );
        }
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

    public void SetEnemyPrefab(GameObject enemyPrefab)
    {
        this.enemyPrefab = enemyPrefab;
        Debug.Log($"設定敵人預製體: {enemyPrefab?.name ?? "無"}");
    }
}
