using UnityEngine;
using System.Collections.Generic;

public class SimpleLevelController : MonoBehaviour
{
    [Header("簡化關卡控制")]
    [SerializeField] private LevelDataAsset levelDataAsset; // 舊系統兼容，可留空
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float waveStartDelay = 2f;

    [Header("敵人 Prefab 註冊表")]
    [Tooltip("在 Inspector 中拖拽所有敵人 Prefab")]
    [SerializeField] private GameObject enemyTankGreen;
    [SerializeField] private GameObject enemyTankGray;
    [SerializeField] private GameObject enemyTankSoil;
    [SerializeField] private GameObject enemyTankPurple;

    [Header("UI References")]
    [Tooltip("Wave Progress Bar UI 控制器")]
    [SerializeField] private WaveProgressBarUI waveProgressBarUI;

    [Header("狀態")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool isWaveActive = false;
    [SerializeField] private int enemiesSpawnedInWave = 0;
    [SerializeField] private int enemiesKilledInWave = 0;

    // 新系統：使用 LevelDataConfig
    private LevelDataConfig currentLevelConfig;

    // 舊系統兼容
    private LevelData currentLevelData;

    private int totalWaves = 0;
    private UpgradePointManager upgradeManager;

    // Prefab 查找字典
    private Dictionary<string, GameObject> enemyPrefabs;

    // 時間驅動系統
    private float levelStartTime = 0f;
    private bool useTimeDrivenWaves = true;  // 使用新的時間驅動系統
    private HashSet<int> spawnedWaves = new HashSet<int>();  // 記錄已生成的波次

    // 關卡總敵人數追蹤
    private int totalEnemiesInLevel = 0;     // 關卡中所有敵人的總數
    private int totalEnemiesKilled = 0;       // 已消滅的敵人總數
    
    private void Start()
    {
        Debug.Log("=== 簡化關卡控制器啟動 ===");

        // 初始化 Prefab 字典
        InitializeEnemyPrefabs();

        // 清理現有敵人
        ClearAllEnemies();

        // 初始化關卡
        InitializeLevel();

        // 初始化 UpgradePointManager
        InitializeUpgradeManager();

        // 初始化 Wave Progress Bar（新的時間軸系統）
        InitializeWaveProgressBar();

        // 記錄關卡開始時間
        levelStartTime = Time.time;

        // 如果使用時間驅動，不再使用舊的 StartFirstWave
        if (!useTimeDrivenWaves)
        {
            StartCoroutine(StartFirstWave());
        }
    }

    private void Update()
    {
        // 時間驅動的波次系統
        if (useTimeDrivenWaves && currentLevelConfig != null)
        {
            CheckAndSpawnTimeDrivenWaves();
        }
    }

    /// <summary>
    /// 檢查並生成時間驅動的波次
    /// </summary>
    private void CheckAndSpawnTimeDrivenWaves()
    {
        float elapsedTime = Time.time - levelStartTime;

        for (int i = 0; i < currentLevelConfig.waves.Length; i++)
        {
            // 如果這波還沒生成
            if (!spawnedWaves.Contains(i))
            {
                WaveConfig wave = currentLevelConfig.waves[i];

                // 使用 spawnTime（如果有設定），否則使用累積的 waveDelay
                float targetSpawnTime = wave.spawnTime;

                if (targetSpawnTime <= 0 && i > 0)
                {
                    // 如果沒設定 spawnTime，計算累積時間
                    targetSpawnTime = 0f;
                    for (int j = 0; j <= i; j++)
                    {
                        targetSpawnTime += currentLevelConfig.waves[j].waveDelay;
                    }
                }

                // 時間到了就生成
                if (elapsedTime >= targetSpawnTime)
                {
                    Debug.Log($"[TimeDriven] Spawning Wave {i + 1} at {elapsedTime:F1}s (target: {targetSpawnTime:F1}s)");
                    SpawnWaveImmediate(i, wave);
                    spawnedWaves.Add(i);
                }
            }
        }
    }

    /// <summary>
    /// 立即生成指定波次
    /// </summary>
    private void SpawnWaveImmediate(int waveIndex, WaveConfig wave)
    {
        currentWaveIndex = waveIndex;
        isWaveActive = true;
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;

        Debug.Log($"開始第 {waveIndex + 1} 波，敵人數量: {wave.enemies.Length}");
        StartCoroutine(SpawnWaveEnemiesNew(wave));
    }

    /// <summary>
    /// 初始化 Wave Progress Bar 的整關時間軸
    /// </summary>
    private void InitializeWaveProgressBar()
    {
        if (waveProgressBarUI != null && currentLevelConfig != null)
        {
            Debug.Log("[SimpleLevelController] Initializing Wave Progress Bar timeline");
            waveProgressBarUI.InitializeLevelTimeline(currentLevelConfig);

            // 訂閱時間結束事件
            waveProgressBarUI.OnTimeUp = OnLevelTimeUp;
        }
        else if (waveProgressBarUI == null)
        {
            Debug.LogWarning("[SimpleLevelController] WaveProgressBarUI is not assigned");
        }
    }

    /// <summary>
    /// 當關卡時間結束時調用
    /// </summary>
    private void OnLevelTimeUp()
    {
        Debug.Log("[SimpleLevelController] 時間到！檢查是否所有敵人都被消滅...");

        // 如果還有敵人存活，則失敗
        if (totalEnemiesKilled < totalEnemiesInLevel)
        {
            int remainingEnemies = totalEnemiesInLevel - totalEnemiesKilled;
            Debug.Log($"[SimpleLevelController] 時間內未消滅所有敵人 ({totalEnemiesKilled}/{totalEnemiesInLevel})，關卡失敗！");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver($"時間到！還有 {remainingEnemies} 個敵人未消滅");
            }
        }
        else
        {
            Debug.Log("[SimpleLevelController] 所有敵人已被消滅，關卡完成！");
            // 勝利條件已經在 OnEnemyDestroyed 中觸發，這裡不需要再次觸發
        }
    }

    /// <summary>
    /// 初始化敵人 Prefab 查找字典
    /// </summary>
    private void InitializeEnemyPrefabs()
    {
        enemyPrefabs = new Dictionary<string, GameObject>
        {
            { LevelDatabase.ENEMY_GREEN, enemyTankGreen },
            { LevelDatabase.ENEMY_GRAY, enemyTankGray },
            { LevelDatabase.ENEMY_SOIL, enemyTankSoil },
            { LevelDatabase.ENEMY_PURPLE, enemyTankPurple }
        };

        Debug.Log("[SimpleLevelController] 敵人 Prefab 字典已初始化");

        // 檢查是否有未設定的 Prefab
        foreach (var kvp in enemyPrefabs)
        {
            if (kvp.Value == null)
            {
                Debug.LogWarning($"[SimpleLevelController] 警告：{kvp.Key} Prefab 未在 Inspector 中設定！");
            }
        }
    }

    /// <summary>
    /// 根據字串 key 獲取敵人 Prefab
    /// </summary>
    public GameObject GetEnemyPrefab(string key)
    {
        if (enemyPrefabs != null && enemyPrefabs.TryGetValue(key, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogError($"[SimpleLevelController] 找不到敵人 Prefab: {key}");
        return null;
    }
    
    private void InitializeLevel()
    {
        // 優先使用新系統：從 LevelDatabase 載入
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int levelNumber = ExtractLevelNumber(sceneName);

        currentLevelConfig = LevelDatabase.GetLevelData(levelNumber);

        if (currentLevelConfig != null)
        {
            // 使用新系統
            totalWaves = currentLevelConfig.waves.Length;

            // 計算關卡總敵人數
            totalEnemiesInLevel = 0;
            for (int i = 0; i < totalWaves; i++)
            {
                totalEnemiesInLevel += currentLevelConfig.waves[i].enemies.Length;
            }
            totalEnemiesKilled = 0;

            Debug.Log($"[SimpleLevelController] 使用新系統載入關卡");
            Debug.Log($"關卡初始化: {currentLevelConfig.levelName}");
            Debug.Log($"總波數: {totalWaves}，總敵人數: {totalEnemiesInLevel}");

            for (int i = 0; i < totalWaves; i++)
            {
                var wave = currentLevelConfig.waves[i];
                Debug.Log($"  波數 {i + 1}: {wave.enemies.Length} 個敵人");
            }
        }
        else
        {
            // 舊系統兼容：嘗試從 ScriptableObject Asset 載入
            Debug.LogWarning($"[SimpleLevelController] LevelDatabase 中找不到關卡 {levelNumber}，嘗試使用舊系統...");

            if (levelDataAsset == null)
            {
                levelDataAsset = AutoLoadLevelDataAsset();
            }

            if (levelDataAsset != null)
            {
                currentLevelData = levelDataAsset.levelData;
                totalWaves = currentLevelData.enemyWaves.Count;

                Debug.Log($"[SimpleLevelController] 使用舊系統載入關卡");
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
                Debug.LogError("[SimpleLevelController] 沒有設定關卡數據，且無法自動加載！");
            }
        }
    }
    
    /// <summary>
    /// 根據場景名稱自動加載對應的 LevelDataAsset
    /// 優先順序：
    /// 1. 使用 Resources 文件夾（運行時可用）
    /// 2. 使用 AssetDatabase（僅 Editor，更可靠）
    /// </summary>
    private LevelDataAsset AutoLoadLevelDataAsset()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"[SimpleLevelController] 嘗試自動加載關卡數據，場景名稱: {sceneName}");
        
        // 提取場景編號（Level1 -> 1, Level2 -> 2）
        int levelNumber = ExtractLevelNumber(sceneName);
        string expectedAssetName = levelNumber > 0 ? $"Level{levelNumber}_Data" : $"{sceneName}_Data";
        
        Debug.Log($"[SimpleLevelController] 預期資源名稱: {expectedAssetName}");
        
        // 方法1: 使用 Resources 文件夾（運行時可用）
        string resourcePath = $"LevelConfigs/{expectedAssetName}";
        LevelDataAsset asset = Resources.Load<LevelDataAsset>(resourcePath);
        if (asset != null)
        {
            Debug.Log($"[SimpleLevelController] ✓ 從 Resources 加載: {resourcePath}");
            return asset;
        }
        else
        {
            Debug.Log($"[SimpleLevelController] Resources 中未找到: {resourcePath}");
        }
        
        // 方法2: 在 Editor 中使用 AssetDatabase（更可靠）
        #if UNITY_EDITOR
        // 先嘗試精確匹配文件名
        string[] allGuids = UnityEditor.AssetDatabase.FindAssets("t:LevelDataAsset");
        Debug.Log($"[SimpleLevelController] 找到 {allGuids.Length} 個 LevelDataAsset");
        
        foreach (string guid in allGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            LevelDataAsset tempAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
            
            if (tempAsset != null)
            {
                // 方法2a: 精確匹配資源名稱
                if (tempAsset.name == expectedAssetName)
                {
                    Debug.Log($"[SimpleLevelController] ✓ 從 AssetDatabase 精確匹配加載: {path} (名稱: {tempAsset.name})");
                    return tempAsset;
                }
                
                // 方法2b: 匹配文件名（不含擴展名）
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (fileName == expectedAssetName)
                {
                    Debug.Log($"[SimpleLevelController] ✓ 從 AssetDatabase 文件名匹配加載: {path} (文件名: {fileName})");
                    return tempAsset;
                }
                
                // 方法2c: 如果場景名稱包含數字，嘗試匹配
                if (levelNumber > 0 && tempAsset.name.Contains($"Level{levelNumber}"))
                {
                    Debug.Log($"[SimpleLevelController] ✓ 從 AssetDatabase 數字匹配加載: {path} (名稱: {tempAsset.name})");
                    return tempAsset;
                }
            }
        }
        
        // 方法3: 如果還是找不到，嘗試模糊匹配場景名稱
        foreach (string guid in allGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(sceneName))
            {
                LevelDataAsset tempAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
                if (tempAsset != null)
                {
                    Debug.Log($"[SimpleLevelController] ✓ 從 AssetDatabase 路徑匹配加載: {path}");
                    return tempAsset;
                }
            }
        }
        #endif
        
        Debug.LogWarning($"[SimpleLevelController] ⚠️ 無法自動加載場景 {sceneName} 的關卡數據");
        Debug.LogWarning($"[SimpleLevelController] 請確保：");
        Debug.LogWarning($"  1. 使用 Tools > Copy Level Data to Resources 將資源複製到 Resources 文件夾");
        Debug.LogWarning($"  2. 或在 Inspector 中手動設定 Level Data Asset");
        return null;
    }
    
    /// <summary>
    /// 從場景名稱中提取關卡編號
    /// Level1 -> 1, Level2 -> 2, Level1_Desert -> 1
    /// </summary>
    private int ExtractLevelNumber(string sceneName)
    {
        // 使用正則表達式或簡單字符串匹配
        // 匹配 "Level" 後面的數字（1-99）
        for (int i = 99; i >= 1; i--)  // 從大到小匹配，避免 Level1 匹配到 Level10
        {
            string pattern = $"Level{i}";
            if (sceneName.Contains(pattern))
            {
                // 確保不是更大的數字（例如 Level1 不應該匹配到 Level10）
                if (i < 10 || !sceneName.Contains($"Level{i + 1}"))
                {
                    return i;
                }
            }
        }
        
        // 如果沒有匹配到，返回 0
        return 0;
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

        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        isWaveActive = true;

        // 新的時間軸系統不需要停止倒數，時間持續進行

        // 根據使用新系統或舊系統來處理
        if (currentLevelConfig != null)
        {
            // 新系統
            var currentWave = currentLevelConfig.waves[currentWaveIndex];
            Debug.Log($"開始第 {currentWaveIndex + 1} 波，敵人數量: {currentWave.enemies.Length}");
            StartCoroutine(SpawnWaveEnemiesNew(currentWave));
        }
        else if (currentLevelData != null)
        {
            // 舊系統
            var currentWave = currentLevelData.enemyWaves[currentWaveIndex];
            Debug.Log($"開始第 {currentWaveIndex + 1} 波，敵人數量: {currentWave.enemyCount}");
            StartCoroutine(SpawnWaveEnemies(currentWave));
        }
    }
    
    /// <summary>
    /// 新系統：生成波數的所有敵人
    /// </summary>
    private System.Collections.IEnumerator SpawnWaveEnemiesNew(WaveConfig wave)
    {
        for (int i = 0; i < wave.enemies.Length; i++)
        {
            SpawnEnemyNew(wave.enemies[i]);
            enemiesSpawnedInWave++;

            if (i < wave.enemies.Length - 1)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    /// <summary>
    /// 新系統：生成單一敵人
    /// </summary>
    private void SpawnEnemyNew(EnemyConfig config)
    {
        // 根據字串 key 獲取 Prefab
        GameObject prefab = GetEnemyPrefab(config.prefabKey);
        if (prefab == null)
        {
            Debug.LogError($"[SimpleLevelController] 無法生成敵人，Prefab key: {config.prefabKey}");
            return;
        }

        // 獲取生成位置
        Vector3 spawnPosition = GetSpawnPosition(config.spawnPointIndex);

        // 生成敵人
        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy";

        Debug.Log($"[SimpleLevelController] 生成敵人: {config.prefabKey} 在位置 {spawnPosition}");

        // 通知 GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemySpawned();
        }
    }

    /// <summary>
    /// 根據索引獲取生成位置
    /// </summary>
    private Vector3 GetSpawnPosition(int spawnPointIndex)
    {
        if (spawnPoints != null && spawnPointIndex >= 0 && spawnPointIndex < spawnPoints.Length)
        {
            var point = spawnPoints[spawnPointIndex];
            if (point != null)
            {
                return point.position;
            }
        }

        // 如果索引無效，使用隨機位置
        Debug.LogWarning($"[SimpleLevelController] 生成點索引 {spawnPointIndex} 無效，使用隨機位置");
        return new Vector3(
            Random.Range(-15f, 15f),
            0f,
            Random.Range(-15f, 15f)
        );
    }

    /// <summary>
    /// 舊系統：生成波數的所有敵人（兼容舊 LevelData）
    /// </summary>
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
        totalEnemiesKilled++;
        Debug.Log($"敵人被消滅: {enemiesKilledInWave}/{enemiesSpawnedInWave} (總進度: {totalEnemiesKilled}/{totalEnemiesInLevel})");

        // 檢查是否所有敵人都被消滅（勝利條件）
        if (totalEnemiesKilled >= totalEnemiesInLevel)
        {
            Debug.Log("[SimpleLevelController] 所有敵人已被消滅，關卡完成！");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Victory();
            }
            return;
        }

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

        // 如果使用時間控制波次，則不自動觸發下一波（由 CheckAndSpawnTimeDrivenWaves 控制）
        if (useTimeDrivenWaves)
        {
            Debug.Log("[SimpleLevelController] 使用時間控制波次，等待時間觸發下一波");
            // 勝利條件已經在 OnEnemyDestroyed 中檢查，這裡不需要再檢查
            return;
        }

        // 舊系統：事件驅動，打完就生下一波
        currentWaveIndex++;

        // 檢查是否還有下一波
        if (currentWaveIndex < totalWaves)
        {
            float nextWaveDelay = GetNextWaveDelay();
            StartCoroutine(WaitForNextWave(nextWaveDelay));
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

    /// <summary>
    /// 獲取下一波的延遲時間（支援新舊系統）
    /// </summary>
    private float GetNextWaveDelay()
    {
        if (currentLevelConfig != null && currentWaveIndex < currentLevelConfig.waves.Length)
        {
            return currentLevelConfig.waves[currentWaveIndex].waveDelay;
        }
        else if (currentLevelData != null && currentWaveIndex < currentLevelData.enemyWaves.Count)
        {
            return currentLevelData.enemyWaves[currentWaveIndex].waveDelay;
        }
        return 1f; // 預設延遲
    }
    
    private System.Collections.IEnumerator WaitForNextWave(float delay)
    {
        Debug.Log($"[SimpleLevelController] Waiting for next wave... Delay: {delay}s");

        // 新的時間軸系統會自動處理倒數，這裡只需要等待
        // Wave Progress Bar 已經在 Start() 時初始化了整關的時間軸

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
