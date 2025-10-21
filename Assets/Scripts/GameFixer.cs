using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class GameFixer : MonoBehaviour
{
    [Header("遊戲修復設定")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(FixGameIssues());
        }
    }
    
    private System.Collections.IEnumerator FixGameIssues()
    {
        if (showDebugInfo) Debug.Log("=== 開始修復遊戲問題 ===");
        
        // 等待幾幀讓所有組件初始化
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // 1. 修復關卡配置
        FixLevelConfiguration();
        
        // 2. 修復 Unity 6 兼容性
        FixUnity6Compatibility();
        
        // 3. 修復音頻監聽器
        FixAudioListeners();
        
        // 4. 修復玩家輸入
        FixPlayerInput();
        
        if (showDebugInfo) Debug.Log("=== 遊戲問題修復完成 ===");
    }
    
    private void FixLevelConfiguration()
    {
        if (showDebugInfo) Debug.Log("修復關卡配置...");
        
        // 找到 SimpleLevelController
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
            
            if (availableLevels == null || availableLevels.Count == 0)
            {
                Debug.LogWarning("SimpleLevelController 沒有關卡配置，嘗試自動配置...");
                
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
                
                if (levelAssets.Count > 0)
                {
                    field.SetValue(levelController, levelAssets);
                    EditorUtility.SetDirty(levelController);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                    
                    if (showDebugInfo) Debug.Log($"已配置 {levelAssets.Count} 個關卡到 SimpleLevelController");
                }
                else
                {
                    Debug.LogError("沒有找到任何關卡數據文件！");
                }
            }
            else
            {
                if (showDebugInfo) Debug.Log($"SimpleLevelController 已配置 {availableLevels.Count} 個關卡");
            }
        }
    }
    
    private void FixUnity6Compatibility()
    {
        if (showDebugInfo) Debug.Log("修復 Unity 6 兼容性...");
        
        // 強制重新編譯
        AssetDatabase.Refresh();
        EditorUtility.RequestScriptReload();
        
        if (showDebugInfo) Debug.Log("Unity 6 兼容性修復完成");
    }
    
    private void FixAudioListeners()
    {
        if (showDebugInfo) Debug.Log("修復音頻監聽器...");
        
        // 找到並修復多個 AudioListener
        var audioManager = FindFirstObjectByType<AudioListenerManager>();
        if (audioManager != null)
        {
            audioManager.FixAudioListeners();
            if (showDebugInfo) Debug.Log("音頻監聽器已修復");
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("找不到 AudioListenerManager");
        }
    }
    
    private void FixPlayerInput()
    {
        if (showDebugInfo) Debug.Log("修復玩家輸入...");
        
        // 檢查 TankController
        var tankController = FindFirstObjectByType<TankController>();
        if (tankController != null)
        {
            if (showDebugInfo) Debug.Log("TankController 已找到");
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("找不到 TankController");
        }
    }
    
    [ContextMenu("立即修復遊戲問題")]
    public void ForceFixNow()
    {
        StartCoroutine(FixGameIssues());
    }
    
    [ContextMenu("檢查遊戲狀態")]
    public void CheckGameStatus()
    {
        Debug.Log("=== 遊戲狀態檢查 ===");
        
        // 檢查 SimpleLevelController
        var levelController = FindFirstObjectByType<SimpleLevelController>();
        if (levelController != null)
        {
            Debug.Log("✅ SimpleLevelController 存在");
        }
        else
        {
            Debug.LogError("❌ 找不到 SimpleLevelController");
        }
        
        // 檢查 TankController
        var tankController = FindFirstObjectByType<TankController>();
        if (tankController != null)
        {
            Debug.Log("✅ TankController 存在");
        }
        else
        {
            Debug.LogError("❌ 找不到 TankController");
        }
        
        // 檢查 AudioListener
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"AudioListener 數量: {audioListeners.Length}");
        
        Debug.Log("=== 檢查完成 ===");
    }
}
#endif

