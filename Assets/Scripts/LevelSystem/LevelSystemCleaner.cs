using UnityEngine;

public class LevelSystemCleaner : MonoBehaviour
{
    [Header("清理設定")]
    [SerializeField] private bool autoCleanOnStart = true;
    [SerializeField] private bool disableOtherScripts = true;
    
    private void Start()
    {
        if (autoCleanOnStart)
        {
            CleanLevelSystem();
        }
    }
    
    [ContextMenu("清理關卡系統")]
    public void CleanLevelSystem()
    {
        Debug.Log("=== 開始清理關卡系統 ===");
        
        // 1. 停止所有協程
        StopAllCoroutines();
        
        // 2. 禁用可能衝突的腳本
        if (disableOtherScripts)
        {
            DisableConflictingScripts();
        }
        
        // 3. 清除所有敵人
        ClearAllEnemies();
        
        // 4. 重置所有管理器
        ResetAllManagers();
        
        // 5. 重新初始化系統
        ReinitializeSystem();
        
        Debug.Log("=== 關卡系統清理完成 ===");
    }
    
    private void DisableConflictingScripts()
    {
        Debug.Log("禁用可能衝突的腳本...");
        
        // 禁用多個初始化腳本
        var scriptsToDisable = new string[]
        {
            "AutoLevelSetup",
            "QuickSetup", 
            "ForceStartWave",
            "ForceActivateWave",
            "LevelSystemSetup",
            "WaveDiagnostic"
        };
        
        foreach (var scriptName in scriptsToDisable)
        {
            var scripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var script in scripts)
            {
                if (script.GetType().Name == scriptName)
                {
                    script.enabled = false;
                    Debug.Log($"已禁用: {scriptName} 在 {script.gameObject.name}");
                }
            }
        }
    }
    
    private void ClearAllEnemies()
    {
        Debug.Log("清除所有敵人...");
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                DestroyImmediate(enemy);
            }
        }
        
        Debug.Log($"清除了 {enemies.Length} 個敵人");
    }
    
    private void ResetAllManagers()
    {
        Debug.Log("重置所有管理器...");
        
        // 重置 WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.ResetWaves();
            Debug.Log("WaveManager 已重置");
        }
        
        // 重置 LevelManager
        if (LevelManager.Instance != null)
        {
            // 重新載入當前關卡
            LevelManager.Instance.RestartCurrentLevel();
            Debug.Log("LevelManager 已重置");
        }
    }
    
    private void ReinitializeSystem()
    {
        Debug.Log("重新初始化系統...");
        
        // 等待一幀讓重置完成
        StartCoroutine(DelayedReinitialize());
    }
    
    private System.Collections.IEnumerator DelayedReinitialize()
    {
        yield return new WaitForEndOfFrame();
        
        // 確保只有一個關卡載入
        if (LevelManager.Instance != null && LevelManager.Instance.TotalLevels > 0)
        {
            Debug.Log("載入第一個關卡...");
            LevelManager.Instance.LoadLevel(0);
        }
        
        yield return new WaitForSeconds(1f);
        
        // 檢查系統狀態
        CheckSystemStatus();
    }
    
    private void CheckSystemStatus()
    {
        Debug.Log("=== 系統狀態檢查 ===");
        
        if (LevelManager.Instance != null)
        {
            Debug.Log($"LevelManager: ✅ (關卡數: {LevelManager.Instance.TotalLevels})");
        }
        else
        {
            Debug.LogError("LevelManager: ❌");
        }
        
        if (WaveManager.Instance != null)
        {
            Debug.Log($"WaveManager: ✅ (波數激活: {WaveManager.Instance.IsWaveActive})");
        }
        else
        {
            Debug.LogError("WaveManager: ❌");
        }
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"場景中敵人數量: {enemies.Length}");
    }
    
    [ContextMenu("只保留必要組件")]
    public void KeepOnlyEssentialComponents()
    {
        Debug.Log("=== 只保留必要組件 ===");
        
        // 找到所有關卡系統相關的腳本
        var allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var essentialScripts = new string[] { "GameManager", "LevelManager", "WaveManager" };
        
        foreach (var script in allScripts)
        {
            string scriptName = script.GetType().Name;
            
            if (scriptName.Contains("Level") || scriptName.Contains("Wave") || scriptName.Contains("Game"))
            {
                if (System.Array.Exists(essentialScripts, s => s == scriptName))
                {
                    script.enabled = true;
                    Debug.Log($"保留: {scriptName} 在 {script.gameObject.name}");
                }
                else
                {
                    script.enabled = false;
                    Debug.Log($"禁用: {scriptName} 在 {script.gameObject.name}");
                }
            }
        }
        
        Debug.Log("=== 組件清理完成 ===");
    }
    
    [ContextMenu("強制單一關卡載入")]
    public void ForceSingleLevelLoad()
    {
        Debug.Log("=== 強制單一關卡載入 ===");
        
        // 清除所有敵人
        ClearAllEnemies();
        
        // 重置 WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.ResetWaves();
        }
        
        // 只載入一次關卡
        if (LevelManager.Instance != null && LevelManager.Instance.TotalLevels > 0)
        {
            LevelManager.Instance.LoadLevel(0);
            Debug.Log("關卡載入完成");
        }
    }
}
