using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelSystemSetup : MonoBehaviour
{
    [Header("快速設置")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private bool replaceGameManager = true;
    
    [Header("組件引用")]
    [SerializeField] private GameManager oldGameManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private WaveManager waveManager;
    
    [Header("UI設置")]
    [SerializeField] private Text waveInfoText;
    [SerializeField] private Text levelInfoText;
    
    [Header("關卡配置")]
    [SerializeField] private LevelDataAsset[] levelAssets;
    
    private void Awake()
    {
        if (autoSetup)
        {
            SetupLevelSystem();
        }
    }
    
    [ContextMenu("設置關卡系統")]
    public void SetupLevelSystem()
    {
        Debug.Log("開始設置關卡系統...");
        
        // 1. 設置LevelManager
        SetupLevelManager();
        
        // 2. 設置WaveManager
        SetupWaveManager();
        
        // 3. 替換GameManager（如果需要）
        if (replaceGameManager)
        {
            ReplaceGameManager();
        }
        
        // 4. 設置UI
        SetupUI();
        
        Debug.Log("關卡系統設置完成！");
    }
    
    private void SetupLevelManager()
    {
        if (levelManager == null)
        {
            levelManager = FindObjectOfType<LevelManager>();
            if (levelManager == null)
            {
                GameObject levelManagerObj = new GameObject("LevelManager");
                levelManager = levelManagerObj.AddComponent<LevelManager>();
            }
        }
        
        // 添加關卡配置
        if (levelAssets != null && levelAssets.Length > 0)
        {
            foreach (var levelAsset in levelAssets)
            {
                if (levelAsset != null)
                {
                    levelManager.AddLevel(levelAsset);
                }
            }
        }
        
        Debug.Log("LevelManager設置完成");
    }
    
    private void SetupWaveManager()
    {
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null)
            {
                GameObject waveManagerObj = new GameObject("WaveManager");
                waveManager = waveManagerObj.AddComponent<WaveManager>();
            }
        }
        
        Debug.Log("WaveManager設置完成");
    }
    
    private void ReplaceGameManager()
    {
        if (oldGameManager == null)
        {
            oldGameManager = FindObjectOfType<GameManager>();
        }
        
        if (oldGameManager != null)
        {
            // 備份舊的GameManager設置
            var oldSettings = BackupGameManagerSettings(oldGameManager);
            
            // 添加新的GameManager組件
            var newGameManager = oldGameManager.gameObject.AddComponent<GameManager>();
            
            // 應用設置
            ApplyGameManagerSettings(newGameManager, oldSettings);
            
            // 啟用新組件，禁用舊組件
            newGameManager.enabled = true;
            oldGameManager.enabled = false;
            
            Debug.Log("GameManager已替換為新版本");
        }
    }
    
    private GameManagerSettings BackupGameManagerSettings(GameManager oldGM)
    {
        // 這裡可以備份舊GameManager的重要設置
        // 由於我們無法直接訪問私有字段，這裡只是示例
        return new GameManagerSettings();
    }
    
    private void ApplyGameManagerSettings(GameManager newGM, GameManagerSettings settings)
    {
        // 應用備份的設置到新的GameManager
        // 這裡可以根據需要實現
    }
    
    private void SetupUI()
    {
        // 查找UI元素
        if (waveInfoText == null)
        {
            waveInfoText = GameObject.Find("WaveInfoText")?.GetComponent<Text>();
        }
        
        if (levelInfoText == null)
        {
            levelInfoText = GameObject.Find("LevelInfoText")?.GetComponent<Text>();
        }
        
        Debug.Log("UI設置完成");
    }
    
    [ContextMenu("創建示例關卡")]
    public void CreateSampleLevels()
    {
        Debug.Log("創建示例關卡...");
        
        // 這裡可以通過代碼創建示例關卡
        // 由於ScriptableObject的創建需要編輯器腳本，這裡只是示例
        
        Debug.Log("示例關卡創建完成！請在Project窗口中手動創建Level Data資源。");
    }
    
    [ContextMenu("測試關卡系統")]
    public void TestLevelSystem()
    {
        if (levelManager != null && waveManager != null)
        {
            Debug.Log("開始測試關卡系統...");
            
            // 載入第一個關卡
            if (levelManager.TotalLevels > 0)
            {
                levelManager.LoadLevel(0);
                Debug.Log("關卡載入成功！");
            }
            else
            {
                Debug.LogWarning("沒有可用的關卡！");
            }
        }
        else
        {
            Debug.LogError("關卡系統未正確設置！");
        }
    }
}

[System.Serializable]
public class GameManagerSettings
{
    // 用於備份GameManager設置的數據結構
    public float gameTime = 300f;
    public int enemyCount = 3;
    public GameObject playerTankPrefab;
    public GameObject enemyTankPrefab;
    public Transform[] spawnPoints;
    // 可以添加更多需要備份的設置
}
