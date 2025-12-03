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
    [Tooltip("玩家坦克的出生點")]
    [SerializeField] private Transform playerSpawnPoint; // 專門給玩家用的出生點

    [Header("UI References")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text enemyCountText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text waveInfoText; // 新增：波數信息顯示
    [SerializeField] private Text levelInfoText; // 新增：關卡信息顯示
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text gameOverMessage;

    [Header("Level Flow")]
    [Tooltip("勝利後是否自動載入下一個關卡")]
    [SerializeField] private bool autoLoadNextScene = true;
    [Tooltip("從 Victory 到載入下一關的延遲時間（秒）")]
    [SerializeField] private float nextSceneDelay = 2f;
    [Tooltip("指定下一個場景名稱（留空則按照 Build Settings 的下一個場景）")]
    [SerializeField] private string nextSceneName;

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

        // 初始化時間設定（目前只用 GameManager 自己的設定）
        currentGameTime = gameTime;

        remainingEnemies = enemyCount;
    }

    void Start()
    {
        SpawnPlayer();
        
        // 初始化當前場景的敵人數量（場景中已存在的敵人）
        CountEnemiesInScene();
        
        UpdateUI();
    }

    void Update()
    {
        if (currentState != GameState.Playing) return;

        UpdateGameTime();
        UpdateUI();
        CheckGameConditions();
    }

    private void SpawnPlayer()
    {
        if (playerTankPrefab == null)
        {
            Debug.LogError("Player tank prefab not assigned!");
            return;
        }

        Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
        playerTank = Instantiate(playerTankPrefab, spawnPosition, Quaternion.identity);
        playerTank.tag = "Player";

        // 添加玩家血量組件
        PlayerHealth playerHealth = playerTank.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = playerTank.AddComponent<PlayerHealth>();
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

        // 更新敵人數量（使用簡單計數，或顯示場上敵人數）
        if (enemyCountText != null)
        {
            enemyCountText.text = $"敵人: {remainingEnemies}";
        }
        
        // 不再從 WaveManager / LevelManager 讀取資訊

        // 更新玩家血量
        if (healthText != null && playerTank != null)
        {
            PlayerHealth playerHealth = playerTank.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                healthText.text = $"血量: {playerHealth.CurrentHealth}";
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

        // 不再用「場上敵人數歸零」來自動判斷通關，
        // 關卡是否完成改由 SimpleLevelController（或其他關卡控制器）主動呼叫 Victory()。
    }

    /// <summary>
    /// 掃描場景中目前有多少敵人，初始化 remainingEnemies。
    /// </summary>
    private void CountEnemiesInScene()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        remainingEnemies = enemies.Length;
    }
    
    /// <summary>
    /// 供生成系統（例如 SimpleLevelController）呼叫，在生成敵人時增加敵人數量。
    /// </summary>
    public void OnEnemySpawned()
    {
        remainingEnemies++;
    }
    
    public void OnEnemyDestroyed()
    {
        // 單純統計場上敵人數
        remainingEnemies--;
        Debug.Log($"Enemy destroyed. Remaining: {remainingEnemies}");

        // 通知簡化關卡控制器，用於統計「本波已擊殺數量」，決定何時啟動下一波 / 通關
        var simpleController = FindFirstObjectByType<SimpleLevelController>();
        if (simpleController != null)
        {
            simpleController.OnEnemyDestroyed();
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

        // 保存玩家當前生命值
        if (playerTank != null && PlayerDataManager.Instance != null)
        {
            PlayerHealth playerHealth = playerTank.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                PlayerDataManager.Instance.SavePlayerHealth(playerHealth.CurrentHealth);
            }
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Debug.Log("Victory!");

        // 自動載入下一關或由玩家手動操作
        if (autoLoadNextScene)
        {
            StartCoroutine(LoadNextSceneRoutine());
        }
        else
        {
            // 不自動換關時，停住遊戲讓玩家看 Victory 畫面
            Time.timeScale = 0f;
        }
    }

    private System.Collections.IEnumerator LoadNextSceneRoutine()
    {
        // 使用實際時間等待，以免受 Time.timeScale 影響
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(nextSceneDelay);

        // 確定下一個場景名稱
        string targetScene = null;
        
        // 如果有指定下一個場景名稱，就用名稱載入
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            targetScene = nextSceneName;
        }
        else
        {
            // 否則就按照 Build Settings 的順序載入下一個場景
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                // 通過場景路徑獲取場景名稱
                string scenePath = SceneUtility.GetScenePathByBuildIndex(nextIndex);
                targetScene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            }
            else
            {
                Debug.Log("沒有下一關可以載入（Build Settings 中已是最後一個場景）");
                // 這裡也可以選擇回主選單，例如：
                // targetScene = "Menu";
                yield break;
            }
        }
        
        // 通過 Transition 場景進行轉場
        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"[GameManager] 準備通過 Transition 加載場景: {targetScene}");
            SceneTransitionManager.LoadSceneWithTransition(targetScene);
        }
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

    // 屬性訪問器
    public GameState CurrentState => currentState;
    public float RemainingTime => currentGameTime;
    public int RemainingEnemies => remainingEnemies;
}