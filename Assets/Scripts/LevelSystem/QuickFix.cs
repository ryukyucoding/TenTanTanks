using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class QuickFix : EditorWindow
{
    [MenuItem("Tools/Quick Fix - Repair Script References")]
    public static void QuickFixScriptReferences()
    {
        Debug.Log("=== 🔧 快速修復腳本引用 ===");
        
        // 獲取 LevelData.cs 的 GUID
        string scriptPath = "Assets/Scripts/LevelSystem/LevelData.cs";
        string guid = AssetDatabase.AssetPathToGUID(scriptPath);
        
        Debug.Log($"LevelData.cs GUID: {guid}");
        
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError("找不到 LevelData.cs 的 GUID！");
            return;
        }
        
        // 查找所有 .asset 文件
        string[] assetFiles = Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories);
        int fixedCount = 0;
        
        foreach (string filePath in assetFiles)
        {
            if (filePath.Contains("Library") || filePath.Contains("Packages"))
                continue;
            
            try
            {
                string content = File.ReadAllText(filePath);
                
                // 檢查是否為 LevelDataAsset 且損壞
                if (content.Contains("Assembly-CSharp::LevelDataAsset"))
                {
                    bool wasFixed = false;
                    
                    // 修復 fileID: 0
                    if (content.Contains("m_Script: {fileID: 0}"))
                    {
                        content = content.Replace(
                            "m_Script: {fileID: 0}",
                            $"m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}"
                        );
                        wasFixed = true;
                    }
                    
                    // 修復錯誤的 GUID
                    if (content.Contains("m_Script: {fileID: 11500000, guid: bfabb25406d09484eb7911e491c00f27, type: 3}") 
                        && guid != "bfabb25406d09484eb7911e491c00f27")
                    {
                        content = content.Replace(
                            "m_Script: {fileID: 11500000, guid: bfabb25406d09484eb7911e491c00f27, type: 3}",
                            $"m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}"
                        );
                        wasFixed = true;
                    }
                    
                    if (wasFixed)
                    {
                        File.WriteAllText(filePath, content);
                        fixedCount++;
                        Debug.Log($"✅ 已修復: {filePath}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"修復文件失敗 {filePath}: {e.Message}");
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"共修復了 {fixedCount} 個文件");
            AssetDatabase.Refresh();
            
            // 驗證修復結果
            System.Threading.Thread.Sleep(1000);
            VerifyFix();
        }
        else
        {
            Debug.Log("沒有找到需要修復的文件");
        }
        
        Debug.Log("=== ✅ 快速修復完成 ===");
    }
    
    [MenuItem("Tools/Verify LevelDataAssets")]
    public static void VerifyFix()
    {
        Debug.Log("=== 🔍 驗證 LevelDataAssets ===");
        
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        Debug.Log($"找到 {guids.Length} 個 LevelDataAsset GUID");
        
        int successCount = 0;
        int failCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) 
            {
                Debug.LogWarning($"GUID {guid} 沒有對應的路徑");
                continue;
            }
            
            Debug.Log($"檢查: {path}");
            
            LevelDataAsset asset = AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
            if (asset != null)
            {
                string levelName = asset.levelData?.levelName ?? "未命名";
                Debug.Log($"✅ 成功載入: {path}");
                Debug.Log($"   關卡名稱: {levelName}");
                Debug.Log($"   波數數量: {asset.levelData?.enemyWaves?.Count ?? 0}");
                successCount++;
            }
            else
            {
                Debug.LogError($"❌ 載入失敗: {path}");
                failCount++;
                
                // 讀取文件內容檢查
                try
                {
                    string content = File.ReadAllText(path);
                    if (content.Contains("m_Script: {fileID: 0}"))
                    {
                        Debug.LogError($"   原因: 腳本引用為空 (fileID: 0)");
                    }
                }
                catch { }
            }
        }
        
        Debug.Log($"=== 驗證完成: 成功 {successCount} 個，失敗 {failCount} 個 ===");
        
        if (failCount > 0)
        {
            Debug.LogWarning("發現失敗的文件，請運行 'Tools > Quick Fix - Repair Script References' 進行修復");
        }
    }
    
    [MenuItem("Tools/Show LevelData GUID")]
    public static void ShowLevelDataGUID()
    {
        string scriptPath = "Assets/Scripts/LevelSystem/LevelData.cs";
        string guid = AssetDatabase.AssetPathToGUID(scriptPath);
        
        if (!string.IsNullOrEmpty(guid))
        {
            Debug.Log($"LevelData.cs 的 GUID 是: {guid}");
            
            // 檢查 meta 文件
            string metaPath = scriptPath + ".meta";
            if (File.Exists(metaPath))
            {
                string metaContent = File.ReadAllText(metaPath);
                Debug.Log($"Meta 文件內容:\n{metaContent}");
            }
        }
        else
        {
            Debug.LogError("找不到 LevelData.cs 的 GUID！");
        }
    }
}
#endif

