using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class AutoLevelConfig : MonoBehaviour
{
    [Header("自動關卡配置")]
    [SerializeField] private bool autoConfigureOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureLevels();
        }
    }
    
    [ContextMenu("配置關卡")]
    public void ConfigureLevels()
    {
        if (showDebugInfo) Debug.Log("=== 開始自動配置關卡 ===");
        
        // 找到 SimpleLevelController
        var levelController = FindFirstObjectByType<SimpleLevelController>();
        if (levelController == null)
        {
            Debug.LogError("找不到 SimpleLevelController！");
            return;
        }
        
        // 找到所有 LevelDataAsset 文件
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        List<LevelDataAsset> levelAssets = new List<LevelDataAsset>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            
            // 跳過系統文件
            if (path.Contains("Library") || path.Contains("Packages"))
                continue;
                
            LevelDataAsset asset = AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
            if (asset != null)
            {
                levelAssets.Add(asset);
                if (showDebugInfo) Debug.Log($"找到關卡數據: {asset.name}");
            }
        }
        
        if (levelAssets.Count == 0)
        {
            Debug.LogWarning("沒有找到任何關卡數據文件！");
            return;
        }
        
        // 使用反射設置 availableLevels
        var field = typeof(SimpleLevelController).GetField("availableLevels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(levelController, levelAssets);
            if (showDebugInfo) Debug.Log($"已配置 {levelAssets.Count} 個關卡到 SimpleLevelController");
            
            // 標記場景為已修改
            EditorUtility.SetDirty(levelController);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogError("無法訪問 SimpleLevelController 的 availableLevels 字段！");
        }
        
        if (showDebugInfo) Debug.Log("=== 關卡配置完成 ===");
    }
    
    [ContextMenu("檢查關卡配置")]
    public void CheckLevelConfiguration()
    {
        Debug.Log("=== 檢查關卡配置 ===");
        
        var levelController = FindFirstObjectByType<SimpleLevelController>();
        if (levelController == null)
        {
            Debug.LogError("找不到 SimpleLevelController！");
            return;
        }
        
        // 使用反射檢查 availableLevels
        var field = typeof(SimpleLevelController).GetField("availableLevels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var availableLevels = field.GetValue(levelController) as List<LevelDataAsset>;
            if (availableLevels != null)
            {
                Debug.Log($"SimpleLevelController 配置了 {availableLevels.Count} 個關卡:");
                for (int i = 0; i < availableLevels.Count; i++)
                {
                    if (availableLevels[i] != null)
                    {
                        Debug.Log($"  - 關卡 {i}: {availableLevels[i].name}");
                    }
                    else
                    {
                        Debug.LogWarning($"  - 關卡 {i}: null");
                    }
                }
            }
            else
            {
                Debug.LogWarning("availableLevels 為 null");
            }
        }
        
        Debug.Log("=== 檢查完成 ===");
    }
}
#endif

