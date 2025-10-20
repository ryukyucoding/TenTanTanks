using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("關卡管理設定")]
    [SerializeField] private List<LevelDataAsset> availableLevels = new List<LevelDataAsset>();
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private bool autoLoadNextLevel = true;
    
    [Header("關卡狀態")]
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private bool isLevelActive = false;
    [SerializeField] private float levelStartTime;
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int currentExperience = 0;
    
    // 事件
    public System.Action<LevelData> OnLevelStarted;
    public System.Action<LevelData, bool> OnLevelCompleted; // (levelData, success)
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnExperienceChanged;
    
    // 單例
    public static LevelManager Instance { get; private set; }
    
    // 屬性
    public LevelData CurrentLevelData => currentLevelData;
    public bool IsLevelActive => isLevelActive;
    public int CurrentLevelIndex => currentLevelIndex;
    public int CurrentScore => currentScore;
    public int CurrentExperience => currentExperience;
    public int TotalLevels => availableLevels.Count;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 訂閱波數管理器事件
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnAllWavesCompleted += OnAllWavesCompleted;
        }
        
        // 訂閱遊戲管理器事件
        if (GameManager.Instance != null)
        {
            // 這裡可以訂閱GameManager的相關事件
        }
        
        // 延遲載入關卡，讓其他腳本有時間設定關卡列表
        StartCoroutine(DelayedLevelLoad());
    }
    
    private System.Collections.IEnumerator DelayedLevelLoad()
    {
        // 等待幾幀讓其他腳本完成初始化
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // 如果有可用關卡，自動載入第一個（強制使用索引0）
        if (availableLevels.Count > 0)
        {
            currentLevelIndex = 0; // 確保從索引0開始
            LoadLevel(currentLevelIndex);
            Debug.Log($"載入關卡索引: {currentLevelIndex}, 關卡名稱: {availableLevels[currentLevelIndex].levelData.levelName}");
        }
        else
        {
            Debug.LogWarning("LevelManager: 沒有可用的關卡！請確保關卡配置文件已正確設定。");
        }
    }
    
    private void OnDestroy()
    {
        // 取消訂閱事件
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnAllWavesCompleted -= OnAllWavesCompleted;
        }
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
        
        Debug.Log($"載入關卡: {currentLevelData.levelName}");
        
        // 重置關卡狀態
        isLevelActive = true;
        levelStartTime = Time.time;
        currentScore = 0;
        currentExperience = 0;
        
        // 通知波數管理器初始化關卡
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.InitializeLevel(currentLevelData);
        }
        
        OnLevelStarted?.Invoke(currentLevelData);
    }
    
    public void LoadNextLevel()
    {
        if (currentLevelIndex + 1 < availableLevels.Count)
        {
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
            Debug.Log("沒有更多關卡了！");
            // 可以在這裡處理遊戲結束或回到主菜單
        }
    }
    
    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }
    
    public void CompleteLevel(bool success)
    {
        if (!isLevelActive) return;
        
        isLevelActive = false;
        
        // 計算關卡分數和經驗
        if (success)
        {
            currentScore += currentLevelData.scoreReward;
            currentExperience += currentLevelData.experienceReward;
            
            OnScoreChanged?.Invoke(currentScore);
            OnExperienceChanged?.Invoke(currentExperience);
            
            Debug.Log($"關卡完成！獲得分數: {currentLevelData.scoreReward}, 經驗: {currentLevelData.experienceReward}");
        }
        
        OnLevelCompleted?.Invoke(currentLevelData, success);
        
        // 自動載入下一關
        if (success && autoLoadNextLevel)
        {
            Invoke(nameof(LoadNextLevel), 2f); // 2秒後載入下一關
        }
    }
    
    private void OnAllWavesCompleted()
    {
        Debug.Log("所有波數完成！");
        
        // 檢查關卡完成條件
        bool levelCompleted = CheckLevelCompletion();
        CompleteLevel(levelCompleted);
    }
    
    private bool CheckLevelCompletion()
    {
        if (currentLevelData == null) return false;
        
        // 檢查是否需要消滅所有敵人
        if (currentLevelData.requireAllEnemiesDefeated)
        {
            // 檢查是否還有敵人存在
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length > 0)
            {
                Debug.Log($"還有 {enemies.Length} 個敵人未消滅");
                return false;
            }
        }
        
        // 檢查是否需要存活指定時間
        if (currentLevelData.requireSurviveTime)
        {
            float elapsedTime = Time.time - levelStartTime;
            if (elapsedTime < currentLevelData.survivalTime)
            {
                Debug.Log($"需要存活 {currentLevelData.survivalTime} 秒，目前: {elapsedTime:F1} 秒");
                return false;
            }
        }
        
        return true;
    }
    
    public void AddScore(int score)
    {
        currentScore += score;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void AddExperience(int experience)
    {
        currentExperience += experience;
        OnExperienceChanged?.Invoke(currentExperience);
    }
    
    public LevelDataAsset GetLevelAsset(int index)
    {
        if (index >= 0 && index < availableLevels.Count)
        {
            return availableLevels[index];
        }
        return null;
    }
    
    public void AddLevel(LevelDataAsset levelAsset)
    {
        if (levelAsset != null && !availableLevels.Contains(levelAsset))
        {
            availableLevels.Add(levelAsset);
        }
    }
    
    public void RemoveLevel(LevelDataAsset levelAsset)
    {
        if (levelAsset != null && availableLevels.Contains(levelAsset))
        {
            availableLevels.Remove(levelAsset);
        }
    }
    
    public void ClearLevels()
    {
        availableLevels.Clear();
    }
    
    // 獲取關卡進度信息
    public string GetLevelProgressInfo()
    {
        if (currentLevelData == null) return "無關卡數據";
        
        string info = $"關卡: {currentLevelData.levelName}\n";
        info += $"分數: {currentScore}\n";
        info += $"經驗: {currentExperience}\n";
        
        if (WaveManager.Instance != null)
        {
            info += $"波數: {WaveManager.Instance.GetCurrentWaveInfo()}\n";
        }
        
        return info;
    }
}
