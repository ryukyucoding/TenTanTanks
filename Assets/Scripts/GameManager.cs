using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameTime = 300f; // 遊戲時間限制（如果關卡沒有設定時間限制則使用此值）
    [SerializeField] private int enemyCount = 3;     // 敵人數量（舊系統，現在由關卡系統管理）

    [Header("Spawn Settings")]
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private GameObject enemyTankPrefab;
    [SerializeField] private Transform[] spawnPoints; // 生成點

    [Header("UI References")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text enemyCountText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text waveInfoText; // 新增：波數信息顯示
    [SerializeField] private Text levelInfoText; // 新增：關卡信息顯示
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text gameOverMessage;

    [Header("Level System")]
    [SerializeField] private bool useLevelSystem = true; // 是否使用新的關卡系統

    // 遊戲狀態
    public enum GameState
    {
        Playing,
        GameOver,
        Victory
    }

    private GameState currentState = GameState.Playing;
    private float currentGameTime;
    private int remainingEnemies;
    private GameObject playerTank;

    // 單例模式（保持原有功能）
    public static GameManager Instance;
    
    /// <summary>
    /// 获取当前玩家坦克对象
    /// </summary>
    public static GameObject GetPlayerTank()
    {
        if (Instance != null)
        {
            return Instance.playerTank;
        }
        return null;
    }

    void Awake()
    {
        // 單例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化時間設定
        if (useLevelSystem && LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
        {
            var levelData = LevelManager.Instance.CurrentLevelData;
            currentGameTime = levelData.timeLimit > 0 ? levelData.timeLimit : gameTime;
        }
        else
        {
            currentGameTime = gameTime;
        }

        remainingEnemies = enemyCount;
    }

    void Start()
    {
        SpawnPlayer();
        
        if (useLevelSystem)
        {
            // 使用新的關卡系統，不直接生成敵人
            // 敵人由WaveManager管理
            SubscribeToLevelSystemEvents();
        }
        else
        {
            // 使用舊系統直接生成敵人
            SpawnEnemies();
        }
        
        UpdateUI();
    }

    void Update()
    {
        if (currentState != GameState.Playing) return;

        UpdateGameTime();
        UpdateUI();
        CheckGameConditions();
    }

    private void SubscribeToLevelSystemEvents()
    {
        // 訂閱關卡系統事件
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelCompleted += OnLevelCompleted;
            LevelManager.Instance.OnScoreChanged += OnScoreChanged;
            LevelManager.Instance.OnExperienceChanged += OnExperienceChanged;
        }

        // 訂閱波數系統事件
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStarted += OnWaveStarted;
            WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;
            WaveManager.Instance.OnEnemyKilled += OnEnemyKilled;
        }
    }

    private void OnLevelCompleted(LevelData levelData, bool success)
    {
        if (success)
        {
            Victory();
        }
        else
        {
            GameOver("關卡失敗");
        }
    }

    private void OnScoreChanged(int newScore)
    {
        // 可以在這裡處理分數變化
        Debug.Log($"分數更新: {newScore}");
    }

    private void OnExperienceChanged(int newExperience)
    {
        // 可以在這裡處理經驗變化
        Debug.Log($"經驗更新: {newExperience}");
    }

    private void OnWaveStarted(int waveIndex, int totalWaves)
    {
        Debug.Log($"第 {waveIndex + 1} 波開始！");
    }

    private void OnWaveCompleted(int waveIndex, int totalWaves)
    {
        Debug.Log($"第 {waveIndex + 1} 波完成！");
    }

    private void OnEnemyKilled(int killed, int total)
    {
        remainingEnemies = total - killed;
        Debug.Log($"敵人被消滅: {killed}/{total}");
    }

    private void SpawnPlayer()
    {
        if (playerTankPrefab == null)
        {
            Debug.LogError("Player tank prefab not assigned!");
            return;
        }

        Vector3 spawnPosition = spawnPoints.Length > 0 ? spawnPoints[0].position : Vector3.zero;
        playerTank = Instantiate(playerTankPrefab, spawnPosition, Quaternion.identity);
        playerTank.tag = "Player";

        // 添加玩家血量組件
        PlayerHealth playerHealth = playerTank.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = playerTank.AddComponent<PlayerHealth>();
        }
    }

    private void SpawnEnemies()
    {
        if (enemyTankPrefab == null)
        {
            Debug.LogError("Enemy tank prefab not assigned!");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition;

            if (spawnPoints.Length > i + 1)
            {
                spawnPosition = spawnPoints[i + 1].position;
            }
            else
            {
                // 隨機生成位置
                spawnPosition = new Vector3(
                    Random.Range(-15f, 15f),
                    0f,
                    Random.Range(-15f, 15f)
                );
            }

            GameObject enemy = Instantiate(enemyTankPrefab, spawnPosition, Quaternion.identity);
            enemy.tag = "Enemy";
        }
    }

    private void UpdateGameTime()
    {
        currentGameTime -= Time.deltaTime;

        if (currentGameTime <= 0)
        {
            currentGameTime = 0;
            GameOver("時間到！");
        }
    }

    private void UpdateUI()
    {
        // 更新時間顯示
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentGameTime / 60);
            int seconds = Mathf.FloorToInt(currentGameTime % 60);
            timeText.text = $"時間: {minutes:00}:{seconds:00}";
        }

        // 更新敵人數量
        if (enemyCountText != null)
        {
            if (useLevelSystem && WaveManager.Instance != null)
            {
                enemyCountText.text = $"敵人: {WaveManager.Instance.EnemiesKilledInWave}/{WaveManager.Instance.EnemiesInCurrentWave}";
            }
            else
            {
                enemyCountText.text = $"敵人: {remainingEnemies}";
            }
        }

        // 更新波數信息
        if (waveInfoText != null && WaveManager.Instance != null)
        {
            waveInfoText.text = WaveManager.Instance.GetCurrentWaveInfo();
        }

        // 更新關卡信息
        if (levelInfoText != null && LevelManager.Instance != null)
        {
            levelInfoText.text = LevelManager.Instance.GetLevelProgressInfo();
        }

        // 更新玩家血量
        if (healthText != null && playerTank != null)
        {
            PlayerHealth playerHealth = playerTank.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                healthText.text = $"血量: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
            }
        }
    }

    private void CheckGameConditions()
    {
        // 檢查玩家是否死亡
        if (playerTank == null)
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth == null || playerHealth.CurrentHealth <= 0)
            {
                GameOver("你被擊敗了！");
                return;
            }
        }

        // 檢查是否消滅所有敵人（舊系統）
        if (!useLevelSystem && remainingEnemies <= 0)
        {
            Victory();
        }
    }

    public void OnEnemyDestroyed()
    {
        if (useLevelSystem)
        {
            // 新系統中，敵人消滅由WaveManager處理
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnEnemyDestroyed();
            }
            else
            {
                // 如果沒有WaveManager，嘗試找SimpleLevelController
                var simpleController = FindFirstObjectByType<SimpleLevelController>();
                if (simpleController != null)
                {
                    simpleController.OnEnemyDestroyed();
                }
            }
        }
        else
        {
            // 舊系統
            remainingEnemies--;
            Debug.Log($"Enemy destroyed. Remaining: {remainingEnemies}");
        }
    }

    public void OnPlayerDamaged(int currentHealth, int maxHealth)
    {
        // 可以在這裡處理玩家受傷的額外邏輯
        Debug.Log($"Player health: {currentHealth}/{maxHealth}");
    }

    public void GameOver(string reason)
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.GameOver;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverMessage != null)
            gameOverMessage.text = reason;

        // 暫停遊戲
        Time.timeScale = 0f;

        Debug.Log($"Game Over: {reason}");
    }

    public void Victory()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Victory;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // 暫停遊戲
        Time.timeScale = 0f;

        Debug.Log("Victory!");
    }

    // UI控制方法
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    // 關卡系統相關方法
    public void LoadNextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }

    public void RestartLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
        }
    }

    // 屬性訪問器
    public GameState CurrentState => currentState;
    public float RemainingTime => currentGameTime;
    public int RemainingEnemies => remainingEnemies;
}