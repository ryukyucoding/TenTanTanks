using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelSceneData
{
    [Header("關卡設定")]
    public string levelName;
    public LevelDataAsset levelDataAsset;
    
    [Header("場景設定")]
    public string sceneName;
    public bool isAdditive = false; // 是否為附加場景
    
    [Header("生成設定")]
    public Transform[] spawnPoints;
    public GameObject enemyPrefab;
}

public class SceneLevelManager : MonoBehaviour
{
    [Header("關卡場景配置")]
    [SerializeField] private List<LevelSceneData> levelScenes = new List<LevelSceneData>();
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private bool autoLoadNextLevel = true;
    [SerializeField] private float levelTransitionDelay = 2f;
    
    [Header("場景管理")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private bool keepLevelManagerPersistent = true;
    
    // 事件
    public System.Action<LevelSceneData> OnLevelStarted;
    public System.Action<LevelSceneData, bool> OnLevelCompleted;
    public System.Action<string> OnSceneLoading;
    public System.Action<string> OnSceneLoaded;
    
    // 單例
    public static SceneLevelManager Instance { get; private set; }
    
    // 屬性
    public LevelSceneData CurrentLevelScene => 
        (currentLevelIndex >= 0 && currentLevelIndex < levelScenes.Count) ? levelScenes[currentLevelIndex] : null;
    public int CurrentLevelIndex => currentLevelIndex;
    public int TotalLevels => levelScenes.Count;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (keepLevelManagerPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 延遲載入第一個關卡
        StartCoroutine(DelayedLevelLoad());
    }
    
    private IEnumerator DelayedLevelLoad()
    {
        // 等待幾幀讓場景完全載入
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        if (levelScenes.Count > 0)
        {
            LoadLevel(currentLevelIndex);
        }
        else
        {
            Debug.LogWarning("SceneLevelManager: 沒有設定關卡場景！");
        }
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelScenes.Count)
        {
            Debug.LogError($"無效的關卡索引: {levelIndex}");
            return;
        }
        
        currentLevelIndex = levelIndex;
        var levelScene = levelScenes[levelIndex];
        
        Debug.Log($"載入關卡: {levelScene.levelName} (場景: {levelScene.sceneName})");
        
        // 觸發場景載入事件
        OnSceneLoading?.Invoke(levelScene.sceneName);
        
        // 載入場景
        StartCoroutine(LoadLevelScene(levelScene));
    }
    
    private IEnumerator LoadLevelScene(LevelSceneData levelScene)
    {
        // 如果是附加場景，載入附加場景
        if (levelScene.isAdditive)
        {
            yield return SceneManager.LoadSceneAsync(levelScene.sceneName, LoadSceneMode.Additive);
        }
        else
        {
            // 載入新場景（替換當前場景）
            yield return SceneManager.LoadSceneAsync(levelScene.sceneName);
        }
        
        // 等待場景載入完成
        yield return new WaitForEndOfFrame();
        
        // 觸發場景載入完成事件
        OnSceneLoaded?.Invoke(levelScene.sceneName);
        
        // 觸發關卡開始事件
        OnLevelStarted?.Invoke(levelScene);
        
        // 初始化關卡控制器
        InitializeLevelController(levelScene);
    }
    
    private void InitializeLevelController(LevelSceneData levelScene)
    {
        // 尋找場景中的 SimpleLevelController
        var levelController = FindFirstObjectByType<SimpleLevelController>();
        if (levelController != null)
        {
            // 設定關卡數據
            levelController.SetLevelData(levelScene.levelDataAsset);
            levelController.SetSpawnPoints(levelScene.spawnPoints);
            levelController.SetEnemyPrefab(levelScene.enemyPrefab);
            
            Debug.Log($"關卡控制器已初始化: {levelScene.levelName}");
        }
        else
        {
            Debug.LogWarning($"在場景 {levelScene.sceneName} 中找不到 SimpleLevelController！");
        }
    }
    
    public void LoadNextLevel()
    {
        if (currentLevelIndex + 1 < levelScenes.Count)
        {
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
            Debug.Log("所有關卡完成！遊戲結束！");
            LoadMainMenu();
        }
    }
    
    public void LoadMainMenu()
    {
        Debug.Log("載入主菜單");
        SceneManager.LoadScene(mainMenuScene);
    }
    
    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }
    
    public void CompleteLevel(bool success)
    {
        var currentLevel = CurrentLevelScene;
        if (currentLevel == null) return;
        
        Debug.Log($"關卡完成: {currentLevel.levelName} (成功: {success})");
        
        // 觸發關卡完成事件
        OnLevelCompleted?.Invoke(currentLevel, success);
        
        // 自動載入下一關
        if (success && autoLoadNextLevel)
        {
            Invoke(nameof(LoadNextLevel), levelTransitionDelay);
        }
    }
    
    public void AddLevelScene(LevelSceneData levelScene)
    {
        if (levelScene != null && !levelScenes.Contains(levelScene))
        {
            levelScenes.Add(levelScene);
        }
    }
    
    public void RemoveLevelScene(LevelSceneData levelScene)
    {
        if (levelScene != null && levelScenes.Contains(levelScene))
        {
            levelScenes.Remove(levelScene);
        }
    }
    
    public void ClearLevelScenes()
    {
        levelScenes.Clear();
    }
    
    // 獲取關卡進度信息
    public string GetLevelProgressInfo()
    {
        return $"關卡 {currentLevelIndex + 1}/{TotalLevels}: {CurrentLevelScene?.levelName ?? "無"}";
    }
    
    [ContextMenu("載入下一關")]
    public void ForceNextLevel()
    {
        LoadNextLevel();
    }
    
    [ContextMenu("重新開始當前關卡")]
    public void ForceRestartLevel()
    {
        RestartCurrentLevel();
    }
}
