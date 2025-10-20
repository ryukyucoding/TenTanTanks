using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("波數管理設定")]
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private Transform[] defaultSpawnPoints;
    [SerializeField] private float waveStartDelay = 3f;
    
    [Header("波數狀態")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private int totalWaves = 0;
    [SerializeField] private int enemiesInCurrentWave = 0;
    [SerializeField] private int enemiesSpawnedInWave = 0;
    [SerializeField] private int enemiesKilledInWave = 0;
    [SerializeField] private bool isWaveActive = false;
    [SerializeField] private bool isAllWavesComplete = false;
    
    // 事件
    public System.Action<int, int> OnWaveStarted; // (waveIndex, totalWaves)
    public System.Action<int, int> OnWaveCompleted; // (waveIndex, totalWaves)
    public System.Action OnAllWavesCompleted;
    public System.Action<int, int> OnEnemySpawned; // (enemiesSpawned, totalInWave)
    public System.Action<int, int> OnEnemyKilled; // (enemiesKilled, totalInWave)
    
    // 單例
    public static WaveManager Instance { get; private set; }
    
    // 屬性
    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaves => totalWaves;
    public int EnemiesInCurrentWave => enemiesInCurrentWave;
    public int EnemiesSpawnedInWave => enemiesSpawnedInWave;
    public int EnemiesKilledInWave => enemiesKilledInWave;
    public bool IsWaveActive => isWaveActive;
    public bool IsAllWavesComplete => isAllWavesComplete;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 等待 LevelManager 初始化完成後再檢查
        StartCoroutine(WaitForLevelManager());
    }
    
    private System.Collections.IEnumerator WaitForLevelManager()
    {
        // 等待幾幀讓 LevelManager 有時間初始化
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // 檢查是否有 LevelManager 且有關卡數據
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
        {
            InitializeLevel(LevelManager.Instance.CurrentLevelData);
        }
        else if (currentLevelData != null)
        {
            // 如果沒有 LevelManager，使用 Inspector 中設定的數據
            InitializeLevel(currentLevelData);
        }
        else
        {
            Debug.LogWarning("WaveManager: 沒有找到關卡數據！請確保 LevelManager 已正確設定或手動設定 currentLevelData。");
        }
    }
    
    public void InitializeLevel(LevelData levelData)
    {
        currentLevelData = levelData;
        totalWaves = levelData.enemyWaves.Count;
        currentWaveIndex = 0;
        enemiesInCurrentWave = 0;
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        isWaveActive = false;
        isAllWavesComplete = false;
        
        Debug.Log($"關卡初始化: {levelData.levelName}, 總波數: {totalWaves}");
        
        // 延遲開始第一波
        StartCoroutine(StartFirstWave());
    }
    
    private IEnumerator StartFirstWave()
    {
        yield return new WaitForSeconds(waveStartDelay);
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        if (currentWaveIndex >= totalWaves)
        {
            Debug.Log("所有波數已完成");
            isAllWavesComplete = true;
            OnAllWavesCompleted?.Invoke();
            return;
        }
        
        var currentWave = currentLevelData.enemyWaves[currentWaveIndex];
        enemiesInCurrentWave = currentWave.enemyCount;
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        isWaveActive = true;
        
        Debug.Log($"開始第 {currentWaveIndex + 1} 波，敵人數量: {enemiesInCurrentWave}");
        
        OnWaveStarted?.Invoke(currentWaveIndex, totalWaves);
        
        // 開始生成敵人
        StartCoroutine(SpawnWaveEnemies(currentWave));
    }
    
    private IEnumerator SpawnWaveEnemies(EnemyWave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave);
            enemiesSpawnedInWave++;
            OnEnemySpawned?.Invoke(enemiesSpawnedInWave, enemiesInCurrentWave);
            
            if (i < wave.enemyCount - 1) // 不是最後一個敵人
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }
    
    private void SpawnEnemy(EnemyWave wave)
    {
        if (wave.enemyPrefab == null)
        {
            Debug.LogError("敵人預製體未設定！");
            return;
        }
        
        Vector3 spawnPosition = GetSpawnPosition(wave);
        GameObject enemy = Instantiate(wave.enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy";
        
        // 應用屬性調整
        ApplyStatsModifier(enemy, wave.statsModifier);
        
        Debug.Log($"生成敵人: {enemy.name} 在位置 {spawnPosition}");
    }
    
    private Vector3 GetSpawnPosition(EnemyWave wave)
    {
        Transform[] spawnPoints = wave.spawnPoints != null && wave.spawnPoints.Length > 0 
            ? wave.spawnPoints 
            : defaultSpawnPoints;
        
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
    
    private void ApplyStatsModifier(GameObject enemy, EnemyStatsModifier modifier)
    {
        // 調整敵人血量
        var enemyHealth = enemy.GetComponent<IDamageable>();
        if (enemyHealth != null && modifier.healthMultiplier != 1f)
        {
            // 這裡需要根據你的IDamageable實現來調整
            // 可能需要添加SetMaxHealth方法
        }
        
        // 調整敵人移動速度
        var enemyTank = enemy.GetComponent<EnemyTank>();
        if (enemyTank != null && modifier.speedMultiplier != 1f)
        {
            // 這裡需要根據EnemyTank的實現來調整速度
            // 可能需要添加SetSpeed方法
        }
        
        // 其他屬性調整可以根據需要添加
    }
    
    public void OnEnemyDestroyed()
    {
        if (!isWaveActive) return;
        
        enemiesKilledInWave++;
        OnEnemyKilled?.Invoke(enemiesKilledInWave, enemiesInCurrentWave);
        
        Debug.Log($"敵人被消滅: {enemiesKilledInWave}/{enemiesInCurrentWave}");
        
        // 檢查當前波是否完成
        if (enemiesKilledInWave >= enemiesInCurrentWave)
        {
            CompleteCurrentWave();
        }
    }
    
    private void CompleteCurrentWave()
    {
        isWaveActive = false;
        Debug.Log($"第 {currentWaveIndex + 1} 波完成！");
        
        OnWaveCompleted?.Invoke(currentWaveIndex, totalWaves);
        
        currentWaveIndex++;
        
        // 檢查是否還有下一波
        if (currentWaveIndex < totalWaves)
        {
            var nextWave = currentLevelData.enemyWaves[currentWaveIndex];
            StartCoroutine(WaitForNextWave(nextWave.waveDelay));
        }
        else
        {
            isAllWavesComplete = true;
            OnAllWavesCompleted?.Invoke();
        }
    }
    
    private IEnumerator WaitForNextWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }
    
    public void ForceCompleteWave()
    {
        if (isWaveActive)
        {
            CompleteCurrentWave();
        }
    }
    
    public void ResetWaves()
    {
        currentWaveIndex = 0;
        enemiesInCurrentWave = 0;
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        isWaveActive = false;
        isAllWavesComplete = false;
        
        StopAllCoroutines();
    }
    
    // 獲取當前波數信息
    public string GetCurrentWaveInfo()
    {
        if (isAllWavesComplete)
        {
            return "所有波數已完成";
        }
        
        if (isWaveActive)
        {
            return $"第 {currentWaveIndex + 1} 波進行中 ({enemiesKilledInWave}/{enemiesInCurrentWave})";
        }
        else
        {
            return $"準備第 {currentWaveIndex + 1} 波";
        }
    }
}
