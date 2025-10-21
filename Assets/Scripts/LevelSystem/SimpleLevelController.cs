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
        
        // 載入當前關卡
        LoadLevel(currentLevelIndex);
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= availableLevels.Count)
        {
            Debug.LogError($"無效的關卡索引: {levelIndex}");
            return;
        }
        
        currentLevelIndex = levelIndex;
        currentLevelData = availableLevels[levelIndex].levelData;
        totalWaves = currentLevelData.enemyWaves.Count;
        
        Debug.Log($"載入關卡: {currentLevelData.levelName}");
        Debug.Log($"總波數: {totalWaves}");
        
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
