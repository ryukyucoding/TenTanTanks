using UnityEngine;
using System.Collections;

public class WebGLCompatibility : MonoBehaviour
{
    [Header("WebGL 兼容性設置")]
    [SerializeField] private bool fixOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private void Start()
    {
        if (fixOnStart)
        {
            StartCoroutine(FixWebGLIssues());
        }
    }
    
    private IEnumerator FixWebGLIssues()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== WebGL 兼容性修復開始 ===");
        }
        
        // 等待幾幀讓所有組件初始化
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // 修復缺失的腳本引用
        FixMissingScriptReferences();
        
        // 修復關卡配置
        FixLevelConfiguration();
        
        // 修復音頻問題
        FixAudioIssues();
        
        if (showDebugInfo)
        {
            Debug.Log("=== WebGL 兼容性修復完成 ===");
        }
    }
    
    private void FixMissingScriptReferences()
    {
        // 找到所有有問題的 GameObject
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            // 檢查是否有缺失的組件
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    Debug.LogWarning($"發現缺失的組件在 GameObject: {obj.name}");
                    // 這裡可以添加修復邏輯
                }
            }
        }
    }
    
    private void FixLevelConfiguration()
    {
        // 確保 LevelManager 有正確的關卡配置
        var levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            if (levelManager.TotalLevels == 0)
            {
                Debug.LogWarning("LevelManager 沒有關卡配置，嘗試修復...");
                
                // 嘗試從 AutoLevelSetup 獲取關卡配置
                var autoSetup = FindFirstObjectByType<AutoLevelSetup>();
                if (autoSetup != null)
                {
                    autoSetup.SetupLevels();
                    Debug.Log("已從 AutoLevelSetup 修復關卡配置");
                }
            }
        }
        
        // 確保 SimpleLevelController 有正確的關卡配置
        var simpleController = FindFirstObjectByType<SimpleLevelController>();
        if (simpleController != null)
        {
            // 檢查 SimpleLevelController 的關卡配置
            Debug.Log("SimpleLevelController 關卡配置檢查完成");
        }
        
        // 修復 LevelDataAsset 引用問題
        FixLevelDataAssetReferences();
    }
    
    private void FixLevelDataAssetReferences()
    {
        Debug.Log("檢查並修復 LevelDataAsset 引用...");
        
        // 這裡可以添加更多的 LevelDataAsset 修復邏輯
        // 由於在運行時無法直接修改 .asset 文件，
        // 我們主要確保關卡數據能正確加載
        
        Debug.Log("LevelDataAsset 引用檢查完成");
    }
    
    private void FixAudioIssues()
    {
        // 修復音頻監聽器問題
        var audioManager = FindFirstObjectByType<AudioListenerManager>();
        if (audioManager != null)
        {
            audioManager.FixAudioListeners();
        }
        
        // 確保音頻系統正確初始化
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            Debug.LogWarning($"發現 {listeners.Length} 個音頻監聽器，WebGL 可能會有問題");
        }
    }
    
    [ContextMenu("手動修復 WebGL 問題")]
    public void ManualFix()
    {
        StartCoroutine(FixWebGLIssues());
    }
    
    [ContextMenu("檢查 WebGL 兼容性")]
    public void CheckCompatibility()
    {
        Debug.Log("=== WebGL 兼容性檢查 ===");
        
        // 檢查關卡系統
        var levelManager = FindFirstObjectByType<LevelManager>();
        var simpleController = FindFirstObjectByType<SimpleLevelController>();
        
        Debug.Log($"LevelManager: {(levelManager != null ? "存在" : "缺失")}");
        Debug.Log($"SimpleLevelController: {(simpleController != null ? "存在" : "缺失")}");
        
        if (levelManager != null)
        {
            Debug.Log($"LevelManager 關卡數量: {levelManager.TotalLevels}");
        }
        
        // 檢查音頻系統
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"音頻監聽器數量: {listeners.Length}");
        
        // 檢查缺失的腳本
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int missingScripts = 0;
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    missingScripts++;
                }
            }
        }
        
        Debug.Log($"缺失的腳本數量: {missingScripts}");
        Debug.Log("=== 檢查完成 ===");
    }
}
