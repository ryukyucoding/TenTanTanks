using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 關卡控制器適配器
/// 這個組件可以添加到 GameObject 上，用於管理新格式的關卡數據
/// </summary>
public class LevelControllerAdapter : MonoBehaviour
{
    [Header("關卡配置（使用新格式 LevelDataNew）")]
    [Tooltip("將創建的 LevelDataNew 資產拖到這裡")]
    [SerializeField] private List<LevelDataNew> newLevelData = new List<LevelDataNew>();
    
    [Header("生成設定")]
    [Tooltip("敵人生成點")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("設定")]
    [Tooltip("自動在 Start 時載入第一個關卡")]
    [SerializeField] private bool autoLoadFirstLevel = true;
    
    [Tooltip("關卡完成後自動載入下一關")]
    [SerializeField] private bool autoLoadNextLevel = true;
    
    private SimpleLevelController controller;
    private List<LevelDataAsset> convertedAssets = new List<LevelDataAsset>();
    
    private void Awake()
    {
        Debug.Log("=== LevelControllerAdapter 初始化 ===");
        
        // 獲取或創建 SimpleLevelController
        controller = GetComponent<SimpleLevelController>();
        if (controller == null)
        {
            Debug.LogWarning("找不到 SimpleLevelController，請確保它在同一個 GameObject 上");
            return;
        }
    }
    
    private void Start()
    {
        ConvertAndSetupLevels();
    }
    
    [ContextMenu("轉換並設置關卡")]
    public void ConvertAndSetupLevels()
    {
        if (controller == null)
        {
            Debug.LogError("SimpleLevelController 未找到！");
            return;
        }
        
        // 將新格式轉換為舊格式
        if (newLevelData.Count > 0)
        {
            Debug.Log($"LevelControllerAdapter: 載入 {newLevelData.Count} 個關卡");
            
            convertedAssets.Clear();
            
            foreach (var newData in newLevelData)
            {
                if (newData != null)
                {
                    // 創建臨時的 LevelDataAsset
                    LevelDataAsset tempAsset = ScriptableObject.CreateInstance<LevelDataAsset>();
                    tempAsset.levelData = newData.ToLegacyFormat();
                    convertedAssets.Add(tempAsset);
                    
                    Debug.Log($"✅ 轉換關卡: {newData.config.levelName}");
                }
                else
                {
                    Debug.LogWarning("發現 null 的 LevelDataNew，跳過");
                }
            }
            
            // 使用反射設置到 SimpleLevelController
            var field = typeof(SimpleLevelController).GetField("availableLevels", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(controller, convertedAssets);
                Debug.Log($"✅ 已設置 {convertedAssets.Count} 個關卡到 SimpleLevelController");
            }
            else
            {
                Debug.LogError("無法訪問 SimpleLevelController.availableLevels 字段！");
            }
            
            // 設置生成點
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                controller.SetSpawnPoints(spawnPoints);
                Debug.Log($"✅ 已設置 {spawnPoints.Length} 個生成點");
            }
            else
            {
                Debug.LogWarning("沒有設置生成點！");
            }
            
            Debug.Log("=== LevelControllerAdapter 設置完成 ===");
        }
        else
        {
            Debug.LogWarning("LevelControllerAdapter: 沒有配置任何關卡數據！");
        }
    }
    
    [ContextMenu("顯示當前配置")]
    public void ShowCurrentConfiguration()
    {
        Debug.Log("=== LevelControllerAdapter 當前配置 ===");
        Debug.Log($"關卡數量: {newLevelData.Count}");
        
        for (int i = 0; i < newLevelData.Count; i++)
        {
            if (newLevelData[i] != null)
            {
                Debug.Log($"  {i + 1}. {newLevelData[i].config.levelName} (難度: {newLevelData[i].difficulty})");
                Debug.Log($"     波數: {newLevelData[i].config.waves.Count}");
            }
            else
            {
                Debug.LogWarning($"  {i + 1}. [NULL]");
            }
        }
        
        Debug.Log($"生成點數量: {(spawnPoints != null ? spawnPoints.Length : 0)}");
        Debug.Log("=== 配置顯示完成 ===");
    }
    
    // 公開方法：動態添加關卡
    public void AddLevel(LevelDataNew levelData)
    {
        if (levelData != null && !newLevelData.Contains(levelData))
        {
            newLevelData.Add(levelData);
            Debug.Log($"添加關卡: {levelData.config.levelName}");
            ConvertAndSetupLevels();
        }
    }
    
    // 公開方法：清除所有關卡
    public void ClearLevels()
    {
        newLevelData.Clear();
        convertedAssets.Clear();
        Debug.Log("已清除所有關卡");
    }
}


