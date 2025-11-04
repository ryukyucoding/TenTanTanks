using UnityEngine;
using System.Collections.Generic;

public class SimpleLevelController : MonoBehaviour
{
    [Header("簡化關卡控制")]
    [SerializeField] private List<LevelDataAsset> availableLevels = new List<LevelDataAsset>();
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private bool autoLoadNextLevel = true;
    [SerializeField] private float levelTransitionDelay = 2f;
    
    [Header("生成設定")]
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
    
    // 事件
    public System.Action<LevelData> OnLevelStarted;
    public System.Action<LevelData, bool> OnLevelCompleted;
    
    private void Start()
    {
        Debug.Log("=== 簡化關卡控制器啟動 ===");
        
        // 清理現有敵人
        ClearAllEnemies();
        
        // WebGL 兼容性檢查
        StartCoroutine(WebGLCompatibleStart());
    }
    
    private System.Collections.IEnumerator WebGLCompatibleStart()
    {
        // 等待幾幀讓 WebGL 完全初始化
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // 檢查關卡配置
        if (availableLevels == null || availableLevels.Count == 0)
        {
            Debug.LogWarning("SimpleLevelController: 沒有關卡配置，嘗試從其他來源獲取...");
            
            // 嘗試從 AutoLevelSetup 獲取關卡
            var autoSetup = FindFirstObjectByType<AutoLevelSetup>();
            if (autoSetup != null)
            {
                yield return new WaitForSeconds(0.1f); // 給 AutoLevelSetup 時間初始化
            }
        }
        
        // 載入當前關卡
        LoadLevel(currentLevelIndex);
    }
    
    public void LoadLevel(int levelIndex)
    {
        // 檢查關卡配置
        if (availableLevels == null || availableLevels.Count == 0)
        {
            Debug.LogError("SimpleLevelController: 沒有可用的關卡配置！請確保關卡數據已正確設定。");
            return;
        }
        
        if (levelIndex < 0 || levelIndex >= availableLevels.Count)
        {
            Debug.LogError($"無效的關卡索引: {levelIndex}，可用關卡數: {availableLevels.Count}");
            return;
        }
        
        currentLevelIndex = levelIndex;
        currentLevelData = availableLevels[levelIndex].levelData;
        totalWaves = currentLevelData.enemyWaves.Count;
        
        Debug.Log($"載入關卡: {currentLevelData.levelName}");
        Debug.Log($"總波數: {totalWaves}");
        
        // WebGL 兼容性檢查：如果波數為0，嘗試修復
        if (totalWaves == 0)
        {
            Debug.LogWarning("檢測到波數為0，嘗試修復關卡配置...");
            FixEmptyLevelData();
        }
        
        // 重置波數狀態
        currentWaveIndex = 0;
        isWaveActive = false;
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        
        // 清理現有敵人
        ClearAllEnemies();
        
        // 觸發關卡開始事件
        OnLevelStarted?.Invoke(currentLevelData);
        
        // 開始第一波
        StartCoroutine(StartFirstWave());
    }
    
    private void FixEmptyLevelData()
    {
        Debug.Log("嘗試修復空的關卡數據...");
        
        // 如果關卡數據為空，創建默認波數
        if (currentLevelData.enemyWaves == null || currentLevelData.enemyWaves.Count == 0)
        {
            Debug.Log("創建默認波數配置...");
            currentLevelData.enemyWaves = new System.Collections.Generic.List<EnemyWave>();
            
            // 添加默認波數
            currentLevelData.enemyWaves.Add(new EnemyWave
            {
                enemyCount = 2,
                enemyPrefab = enemyPrefab,
                waveDelay = 2f,
                spawnInterval = 1f,
                spawnPoints = spawnPoints
            });
            
            currentLevelData.enemyWaves.Add(new EnemyWave
            {
                enemyCount = 3,
                enemyPrefab = enemyPrefab,
                waveDelay = 3f,
                spawnInterval = 0.8f,
                spawnPoints = spawnPoints
            });
            
            totalWaves = currentLevelData.enemyWaves.Count;
            Debug.Log($"已創建 {totalWaves} 個默認波數");
        }
    }
    
    public void LoadNextLevel()
    {
        if (currentLevelIndex + 1 < availableLevels.Count)
        {
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
            Debug.Log("沒有更多關卡了！遊戲完成！");
            // 可以在這裡處理遊戲結束邏輯
        }
    }
    
    private void InitializeLevel()
    {
        if (currentLevelData != null)
        {
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
        Debug.Log($"第 {currentWaveIndex + 1} 波完成！");
        
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
            CompleteLevel();
        }
    }
    
    private System.Collections.IEnumerator WaitForNextWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }
    
    private void CompleteLevel()
    {
        Debug.Log($"關卡完成: {currentLevelData.levelName}");
        
        // 觸發關卡完成事件
        OnLevelCompleted?.Invoke(currentLevelData, true);
        
        // 通知場景關卡管理器
        if (SceneLevelManager.Instance != null)
        {
            Debug.Log("通知 SceneLevelManager 關卡完成");
            SceneLevelManager.Instance.CompleteLevel(true);
        }
        else
        {
            Debug.LogWarning("找不到 SceneLevelManager，使用 SimpleLevelController 的跳轉邏輯");
            // 如果沒有場景管理器，使用原有的跳轉邏輯
            if (autoLoadNextLevel)
            {
                Invoke(nameof(LoadNextLevel), levelTransitionDelay);
            }
        }
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
        LoadLevel(currentLevelIndex);
    }
    
    [ContextMenu("載入下一關")]
    public void ForceNextLevel()
    {
        LoadNextLevel();
    }
    
    // 供場景管理器調用的方法
    public void SetLevelData(LevelDataAsset levelDataAsset)
    {
        if (levelDataAsset != null)
        {
            currentLevelData = levelDataAsset.levelData;
            totalWaves = currentLevelData.enemyWaves.Count;
            
            Debug.Log($"設定關卡數據: {currentLevelData.levelName}");
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
}
