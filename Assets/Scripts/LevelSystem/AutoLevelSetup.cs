using UnityEngine;

public class AutoLevelSetup : MonoBehaviour
{
    [Header("自動設定")]
    [SerializeField] private bool setupOnAwake = true;
    
    [Header("關卡配置")]
    [SerializeField] private LevelDataAsset[] levelAssets;
    
    private void Awake()
    {
        if (setupOnAwake)
        {
            SetupLevels();
        }
    }
    
    [ContextMenu("設定關卡")]
    public void SetupLevels()
    {
        // 找到 LevelManager
        var levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("找不到 LevelManager！");
            return;
        }
        
        // 清空現有關卡
        levelManager.ClearLevels();
        
        // 添加關卡
        if (levelAssets != null && levelAssets.Length > 0)
        {
            foreach (var levelAsset in levelAssets)
            {
                if (levelAsset != null)
                {
                    levelManager.AddLevel(levelAsset);
                    Debug.Log($"添加關卡: {levelAsset.levelData.levelName}");
                }
            }
            
            Debug.Log($"總共添加了 {levelAssets.Length} 個關卡");
        }
        else
        {
            Debug.LogWarning("沒有設定關卡配置文件！");
        }
    }
    
    private void Start()
    {
        // 確保關卡已設定後，強制載入第一個關卡
        if (levelAssets != null && levelAssets.Length > 0)
        {
            var levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null && levelManager.TotalLevels > 0)
            {
                levelManager.LoadLevel(0);
                Debug.Log("自動載入第一個關卡");
            }
        }
    }
}
