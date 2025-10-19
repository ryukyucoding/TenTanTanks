using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameTime = 300f; // �C���ɶ��]��^
    [SerializeField] private int enemyCount = 3;     // �ĤH�ƶq

    [Header("Spawn Settings")]
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private GameObject enemyTankPrefab;
    [SerializeField] private Transform[] spawnPoints; // �ͦ��I

    [Header("UI References")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text enemyCountText;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text gameOverMessage;

    // �C�����A
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

    // �R�A�ޥΡ]��L�}���i�H�X�ݡ^
    public static GameManager Instance;

    void Awake()
    {
        // ��ҼҦ�
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

        // �K�[���a�ͩR�Ȳե�
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
                // �H���ͦ���m
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
            GameOver("�ɶ���I");
        }
    }

    private void UpdateUI()
    {
        // ��s�ɶ����
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentGameTime / 60);
            int seconds = Mathf.FloorToInt(currentGameTime % 60);
            timeText.text = $"�ɶ�: {minutes:00}:{seconds:00}";
        }

        // ��s�ĤH�ƶq
        if (enemyCountText != null)
        {
            enemyCountText.text = $"�ĤH: {remainingEnemies}";
        }

        // ��s���a�ͩR��
        if (healthText != null && playerTank != null)
        {
            PlayerHealth playerHealth = playerTank.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                healthText.text = $"�ͩR��: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
            }
        }
    }

    private void CheckGameConditions()
    {
        // �ˬd���a�O�_���`
        if (playerTank == null)
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null || playerHealth.CurrentHealth <= 0)
            {
                GameOver("�A�Q���ѤF�I");
                return;
            }
        }

        // �ˬd�O�_�����Ҧ��ĤH
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
        // �i�H�b�o�̳B�z���a���˪��S�ĩέ���
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

        // �Ȱ��C��
        Time.timeScale = 0f;

        Debug.Log($"Game Over: {reason}");
    }

    public void Victory()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Victory;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // �Ȱ��C��
        Time.timeScale = 0f;

        Debug.Log("Victory!");
    }

    // UI���s��k
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

    // ���@�ݩ�
    public GameState CurrentState => currentState;
    public float RemainingTime => currentGameTime;
    public int RemainingEnemies => remainingEnemies;
}
