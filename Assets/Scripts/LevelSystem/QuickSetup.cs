using UnityEngine;

public class QuickSetup : MonoBehaviour
{
    [Header("快速設置")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("必要組件")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameManager gameManager;
    
    [Header("關卡配置")]
    [SerializeField] private LevelDataAsset[] levelAssets;
    
    [Header("生成點設定")]
    [SerializeField] private Transform[] spawnPoints;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupSystem();
        }
    }
    
    [ContextMenu("快速設置系統")]
    public void SetupSystem()
    {
        Debug.Log("開始快速設置關卡系統...");
        
        // 1. 設置 LevelManager
        SetupLevelManager();
        
        // 2. 設置 WaveManager
        SetupWaveManager();
        
        // 3. 檢查 GameManager
        CheckGameManager();
        
        Debug.Log("快速設置完成！");
    }
    
    private void SetupLevelManager()
    {
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager == null)
            {
                GameObject levelManagerObj = new GameObject("LevelManager");
                levelManager = levelManagerObj.AddComponent<LevelManager>();
                Debug.Log("創建了新的 LevelManager");
            }
        }
        
        // 添加關卡配置
        if (levelAssets != null && levelAssets.Length > 0)
        {
            // 清空現有關卡
            levelManager.ClearLevels();
            
            // 添加新關卡
            foreach (var levelAsset in levelAssets)
            {
                if (levelAsset != null)
                {
                    levelManager.AddLevel(levelAsset);
                    Debug.Log($"添加關卡: {levelAsset.levelData.levelName}");
                }
            }
        }
        
        Debug.Log("LevelManager 設置完成");
    }
    
    private void SetupWaveManager()
    {
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager == null)
            {
                GameObject waveManagerObj = new GameObject("WaveManager");
                waveManager = waveManagerObj.AddComponent<WaveManager>();
                Debug.Log("創建了新的 WaveManager");
            }
        }
        
        // 設置生成點
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // 使用反射來設定 defaultSpawnPoints
            var field = typeof(WaveManager).GetField("defaultSpawnPoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(waveManager, spawnPoints);
                Debug.Log($"設置了 {spawnPoints.Length} 個默認生成點");
            }
        }
        else
        {
            Debug.LogWarning("沒有設定生成點！敵人將使用隨機位置生成。");
        }
        
        Debug.Log("WaveManager 設置完成");
    }
    
    private void CheckGameManager()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        if (gameManager != null)
        {
            Debug.Log("GameManager 已找到");
        }
        else
        {
            Debug.LogWarning("沒有找到 GameManager！");
        }
    }
    
    [ContextMenu("測試關卡系統")]
    public void TestLevelSystem()
    {
        if (levelManager != null && levelManager.TotalLevels > 0)
        {
            Debug.Log("開始測試關卡系統...");
            levelManager.LoadLevel(0);
        }
        else
        {
            Debug.LogError("LevelManager 未正確設置或沒有可用關卡！");
        }
    }
    
    [ContextMenu("檢查系統狀態")]
    public void CheckSystemStatus()
    {
        Debug.Log("=== 系統狀態檢查 ===");
        
        if (levelManager != null)
        {
            Debug.Log($"LevelManager: ✅ (關卡數: {levelManager.TotalLevels})");
        }
        else
        {
            Debug.LogError("LevelManager: ❌");
        }
        
        if (waveManager != null)
        {
            Debug.Log($"WaveManager: ✅");
        }
        else
        {
            Debug.LogError("WaveManager: ❌");
        }
        
        if (gameManager != null)
        {
            Debug.Log($"GameManager: ✅");
        }
        else
        {
            Debug.LogError("GameManager: ❌");
        }
        
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Debug.Log($"生成點: ✅ ({spawnPoints.Length} 個)");
        }
        else
        {
            Debug.LogWarning("生成點: ⚠️ (未設定)");
        }
        
        if (levelAssets != null && levelAssets.Length > 0)
        {
            Debug.Log($"關卡配置: ✅ ({levelAssets.Length} 個)");
        }
        else
        {
            Debug.LogWarning("關卡配置: ⚠️ (未設定)");
        }
    }
}
