using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameTime = 300f; // 遊戲時間（秒）
    [SerializeField] private int enemyCount = 3;     // 敵人數量

    [Header("Spawn Settings")]
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private GameObject enemyTankPrefab;
    [SerializeField] private Transform[] spawnPoints; // 生成點

    [Header("UI References")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text enemyCountText;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text gameOverMessage;

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

    // 靜態引用（其他腳本可以訪問）
    public static GameManager Instance;

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

        currentGameTime = gameTime;
        remainingEnemies = enemyCount;
    }

    void Start()
    {
        SpawnPlayer();
        SpawnEnemies();
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

        Vector3 spawnPosition = spawnPoints.Length > 0 ? spawnPoints[0].position : Vector3.zero;
        playerTank = Instantiate(playerTankPrefab, spawnPosition, Quaternion.identity);
        playerTank.tag = "Player";

        // 添加玩家生命值組件
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
            enemyCountText.text = $"敵人: {remainingEnemies}";
        }

        // 更新玩家生命值
        if (healthText != null && playerTank != null)
        {
            PlayerHealth playerHealth = playerTank.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                healthText.text = $"生命值: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
            }
        }
    }

    private void CheckGameConditions()
    {
        // 檢查玩家是否死亡
        if (playerTank == null)
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null || playerHealth.CurrentHealth <= 0)
            {
                GameOver("你被擊敗了！");
                return;
            }
        }

        // 檢查是否消滅所有敵人
        if (remainingEnemies <= 0)
        {
            Victory();
        }
    }

    public void OnEnemyDestroyed()
    {
        remainingEnemies--;
        Debug.Log($"Enemy destroyed. Remaining: {remainingEnemies}");
    }

    public void OnPlayerDamaged(int currentHealth, int maxHealth)
    {
        // 可以在這裡處理玩家受傷的特效或音效
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

    // UI按鈕方法
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

    // 公共屬性
    public GameState CurrentState => currentState;
    public float RemainingTime => currentGameTime;
    public int RemainingEnemies => remainingEnemies;
}
