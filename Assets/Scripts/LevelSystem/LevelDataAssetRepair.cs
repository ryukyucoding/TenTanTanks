using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class LevelDataAssetRepair : EditorWindow
{
    [MenuItem("Tools/Repair LevelDataAsset System")]
    public static void RepairLevelDataAssetSystem()
    {
        Debug.Log("=== 開始修復 LevelDataAsset 系統 ===");
        
        // 1. 強制重新編譯
        Debug.Log("步驟 1: 強制重新編譯...");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // 2. 等待編譯完成
        System.Threading.Thread.Sleep(1000);
        
        // 3. 檢查腳本 GUID
        Debug.Log("步驟 2: 檢查腳本 GUID...");
        string levelDataPath = "Assets/Scripts/LevelSystem/LevelData.cs";
        string metaPath = levelDataPath + ".meta";
        
        if (File.Exists(metaPath))
        {
            string metaContent = File.ReadAllText(metaPath);
            Debug.Log($"LevelData.cs 的 Meta 文件內容:\n{metaContent}");
        }
        else
        {
            Debug.LogError($"找不到 Meta 文件: {metaPath}");
        }
        
        // 4. 重新導入所有 LevelDataAsset
        Debug.Log("步驟 3: 重新導入所有 LevelDataAsset...");
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        Debug.Log($"找到 {guids.Length} 個 LevelDataAsset GUID");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log($"重新導入: {path}");
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            }
        }
        
        // 5. 驗證所有 LevelDataAsset
        Debug.Log("步驟 4: 驗證所有 LevelDataAsset...");
        VerifyLevelDataAssets();
        
        // 6. 最後刷新
        AssetDatabase.Refresh();
        
        Debug.Log("=== 修復完成 ===");
    }
    
    private static void VerifyLevelDataAssets()
    {
        // 使用文件系統直接搜索
        string[] assetFiles = Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories);
        
        int totalCount = 0;
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (string filePath in assetFiles)
        {
            if (filePath.Contains("Library") || filePath.Contains("Packages"))
                continue;
            
            try
            {
                string content = File.ReadAllText(filePath);
                
                if (content.Contains("Assembly-CSharp::LevelDataAsset"))
                {
                    totalCount++;
                    
                    // 檢查腳本引用
                    if (content.Contains("m_Script: {fileID: 11500000, guid: bfabb25406d09484eb7911e491c00f27, type: 3}"))
                    {
                        validCount++;
                        Debug.Log($"✅ 有效: {filePath}");
                        
                        // 嘗試加載資產
                        LevelDataAsset asset = AssetDatabase.LoadAssetAtPath<LevelDataAsset>(filePath);
                        if (asset != null)
                        {
                            Debug.Log($"  └─ 資產成功加載，關卡名稱: {asset.levelData?.levelName ?? "null"}");
                        }
                        else
                        {
                            Debug.LogWarning($"  └─ 資產加載失敗！");
                        }
                    }
                    else if (content.Contains("m_Script: {fileID: 0}"))
                    {
                        invalidCount++;
                        Debug.LogWarning($"❌ 損壞（fileID: 0）: {filePath}");
                    }
                    else
                    {
                        invalidCount++;
                        Debug.LogWarning($"⚠️ 未知狀態: {filePath}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"檢查文件時出錯 {filePath}: {e.Message}");
            }
        }
        
        Debug.Log($"驗證完成: 總計 {totalCount} 個，有效 {validCount} 個，無效 {invalidCount} 個");
    }
    
    [MenuItem("Tools/Force Reimport LevelData.cs")]
    public static void ForceReimportLevelDataScript()
    {
        Debug.Log("=== 強制重新導入 LevelData.cs ===");
        
        string scriptPath = "Assets/Scripts/LevelSystem/LevelData.cs";
        
        if (File.Exists(scriptPath))
        {
            // 強制重新導入腳本
            AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            Debug.Log($"已重新導入: {scriptPath}");
            
            // 等待編譯完成
            System.Threading.Thread.Sleep(2000);
            
            // 刷新資產數據庫
            AssetDatabase.Refresh();
            
            Debug.Log("重新導入完成");
        }
        else
        {
            Debug.LogError($"找不到文件: {scriptPath}");
        }
        
        Debug.Log("=== 完成 ===");
    }
    
    [MenuItem("Tools/Delete and Recreate LevelDataAssets")]
    public static void DeleteAndRecreateLevelDataAssets()
    {
        if (!EditorUtility.DisplayDialog("警告", 
            "這將刪除所有現有的 LevelDataAsset 並重新創建。\n確定要繼續嗎？", 
            "是", "否"))
        {
            return;
        }
        
        Debug.Log("=== 刪除並重新創建 LevelDataAssets ===");
        
        // 1. 刪除所有現有的 LevelDataAsset
        string[] assetFiles = Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories);
        int deletedCount = 0;
        
        foreach (string filePath in assetFiles)
        {
            if (filePath.Contains("Library") || filePath.Contains("Packages"))
                continue;
            
            try
            {
                string content = File.ReadAllText(filePath);
                
                if (content.Contains("Assembly-CSharp::LevelDataAsset"))
                {
                    Debug.Log($"刪除: {filePath}");
                    AssetDatabase.DeleteAsset(filePath);
                    deletedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"刪除文件時出錯 {filePath}: {e.Message}");
            }
        }
        
        Debug.Log($"已刪除 {deletedCount} 個文件");
        
        // 2. 刷新並等待
        AssetDatabase.Refresh();
        System.Threading.Thread.Sleep(1000);
        
        // 3. 創建新的 LevelDataAsset
        CreateNewLevelDataAsset("Level1_Data", "關卡 1 - 新手訓練", 1);
        CreateNewLevelDataAsset("Level2_Data", "關卡 2 - 挑戰開始", 2);
        
        Debug.Log("=== 完成 ===");
    }
    
    private static void CreateNewLevelDataAsset(string fileName, string levelName, int difficulty)
    {
        Debug.Log($"創建新的 LevelDataAsset: {fileName}");
        
        LevelDataAsset asset = ScriptableObject.CreateInstance<LevelDataAsset>();
        asset.levelData = new LevelData();
        asset.levelData.levelName = levelName;
        asset.levelData.enemyWaves = new System.Collections.Generic.List<EnemyWave>();
        asset.difficulty = difficulty;
        
        string path = $"Assets/Scripts/LevelSystem/LevelConfigs/{fileName}.asset";
        
        // 確保目錄存在
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"已創建: {path}");
    }
}
#endif

