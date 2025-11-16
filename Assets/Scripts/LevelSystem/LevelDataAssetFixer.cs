using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class LevelDataAssetFixer : EditorWindow
{
    [MenuItem("Tools/Fix LevelDataAsset References")]
    public static void FixLevelDataAssetReferences()
    {
        Debug.Log("開始修復 LevelDataAsset 引用...");
        
        // 找到所有 LevelDataAsset 文件
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        int fixedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            
            // 讀取文件內容
            string content = File.ReadAllText(path);
            
            // 檢查是否有腳本引用問題
            if (content.Contains("m_Script: {fileID: 0}"))
            {
                Debug.Log($"修復文件: {path}");
                
                // 獲取正確的腳本 GUID
                string scriptGUID = GetLevelDataAssetScriptGUID();
                if (!string.IsNullOrEmpty(scriptGUID))
                {
                    // 替換腳本引用
                    content = content.Replace(
                        "m_Script: {fileID: 0}",
                        $"m_Script: {{fileID: 11500000, guid: {scriptGUID}, type: 3}}"
                    );
                    
                    // 寫回文件
                    File.WriteAllText(path, content);
                    fixedCount++;
                }
            }
        }
        
        // 重新導入資源
        AssetDatabase.Refresh();
        
        Debug.Log($"修復完成！共修復了 {fixedCount} 個文件");
    }
    
    private static string GetLevelDataAssetScriptGUID()
    {
        // 直接返回 LevelData.cs 的 GUID
        return "bfabb25406d09484eb7911e491c00f27";
    }
    
    [MenuItem("Tools/Check LevelDataAsset Status")]
    public static void CheckLevelDataAssetStatus()
    {
        Debug.Log("=== LevelDataAsset 狀態檢查 ===");
        
        // 檢查 LevelDataAsset 文件
        CheckLevelDataAssets();
        
        // 檢查所有腳本引用
        CheckAllScriptReferences();
        
        Debug.Log("=== 檢查完成 ===");
    }
    
    [MenuItem("Tools/Simple Check LevelDataAssets")]
    public static void SimpleCheckLevelDataAssets()
    {
        Debug.Log("=== 簡單檢查 LevelDataAsset ===");
        
        // 直接搜索 .asset 文件
        string[] assetFiles = System.IO.Directory.GetFiles("Assets", "*.asset", System.IO.SearchOption.AllDirectories);
        
        int levelDataAssetCount = 0;
        int brokenCount = 0;
        
        foreach (string filePath in assetFiles)
        {
            // 跳過系統文件
            if (filePath.Contains("Library") || filePath.Contains("Packages"))
                continue;
            
            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                
                // 檢查是否為 LevelDataAsset
                if (content.Contains("Assembly-CSharp::LevelDataAsset"))
                {
                    levelDataAssetCount++;
                    Debug.Log($"找到 LevelDataAsset: {filePath}");
                    
                    if (content.Contains("m_Script: {fileID: 0}"))
                    {
                        Debug.LogWarning($"❌ 損壞: {filePath}");
                        brokenCount++;
                    }
                    else
                    {
                        Debug.Log($"✅ 正常: {filePath}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"讀取文件失敗 {filePath}: {e.Message}");
            }
        }
        
        Debug.Log($"LevelDataAsset 總計: {levelDataAssetCount} 個文件，損壞: {brokenCount} 個");
        Debug.Log("=== 簡單檢查完成 ===");
    }
    
    [MenuItem("Tools/Auto Fix All LevelDataAssets")]
    public static void AutoFixAllLevelDataAssets()
    {
        Debug.Log("=== 自動修復所有 LevelDataAsset ===");
        
        // 直接搜索 .asset 文件
        string[] assetFiles = System.IO.Directory.GetFiles("Assets", "*.asset", System.IO.SearchOption.AllDirectories);
        
        int fixedCount = 0;
        
        foreach (string filePath in assetFiles)
        {
            // 跳過系統文件
            if (filePath.Contains("Library") || filePath.Contains("Packages"))
                continue;
            
            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                
                // 檢查是否為 LevelDataAsset 且損壞
                if (content.Contains("Assembly-CSharp::LevelDataAsset") && content.Contains("m_Script: {fileID: 0}"))
                {
                    Debug.Log($"修復損壞的 LevelDataAsset: {filePath}");
                    
                    // 修復腳本引用
                    content = content.Replace("m_Script: {fileID: 0}", 
                        "m_Script: {fileID: 11500000, guid: bfabb25406d09484eb7911e491c00f27, type: 3}");
                    
                    // 寫回文件
                    System.IO.File.WriteAllText(filePath, content);
                    fixedCount++;
                    
                    Debug.Log($"✅ 已修復: {filePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"修復文件失敗 {filePath}: {e.Message}");
            }
        }
        
        // 重新導入資產
        AssetDatabase.Refresh();
        
        Debug.Log($"=== 修復完成！共修復了 {fixedCount} 個文件 ===");
    }
    
    private static void CheckLevelDataAssets()
    {
        Debug.Log("檢查 LevelDataAsset 文件...");
        
        // 使用更準確的搜索方式
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        int totalCount = 0;
        int brokenCount = 0;
        
        Debug.Log($"找到 {guids.Length} 個 GUID");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) 
            {
                Debug.LogWarning($"GUID {guid} 對應的路徑為空");
                continue;
            }
            
            // 跳過系統文件
            if (path.Contains("Library") || path.Contains("Packages"))
            {
                Debug.Log($"跳過系統文件: {path}");
                continue;
            }
            
            Debug.Log($"檢查文件: {path}");
            totalCount++;
            
            // 檢查文件是否存在
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning($"文件不存在: {path}");
                continue;
            }
            
            // 讀取文件內容
            string content = System.IO.File.ReadAllText(path);
            
            if (content.Contains("m_Script: {fileID: 0}"))
            {
                Debug.LogWarning($"❌ 損壞的 LevelDataAsset: {path}");
                brokenCount++;
            }
            else
            {
                Debug.Log($"✅ 正常: {path}");
            }
        }
        
        Debug.Log($"LevelDataAsset 總計: {totalCount} 個文件，損壞: {brokenCount} 個");
    }
    
    private static void CheckAllScriptReferences()
    {
        Debug.Log("檢查所有腳本引用...");
        
        // 找到所有 ScriptableObject 文件
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        int totalCount = 0;
        int brokenCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            
            // 跳過系統文件和 URP 相關文件
            if (path.Contains("Library") || 
                path.Contains("Packages") || 
                path.Contains("Settings/DefaultVolumeProfile") ||
                path.Contains("DefaultVolumeProfile"))
                continue;
            
            totalCount++;
            
            // 讀取文件內容
            string content = File.ReadAllText(path);
            
            if (content.Contains("m_Script: {fileID: 0}"))
            {
                Debug.LogWarning($"❌ 損壞的腳本引用: {path}");
                brokenCount++;
            }
        }
        
        Debug.Log($"所有腳本引用總計: {totalCount} 個文件，損壞: {brokenCount} 個");
    }
    
    [MenuItem("Tools/Recreate LevelDataAssets")]
    public static void RecreateLevelDataAssets()
    {
        Debug.Log("重新創建 LevelDataAsset 文件...");
        
        // 備份現有文件
        string backupPath = "Assets/Scripts/LevelSystem/LevelConfigs/Backup/";
        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }
        
        // 移動現有文件到備份目錄
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("LevelConfigs"))
            {
                string fileName = Path.GetFileName(path);
                string backupFile = backupPath + fileName;
                File.Move(path, backupFile);
                Debug.Log($"備份文件: {fileName}");
            }
        }
        
        // 重新創建關卡數據
        CreateLevel1Data();
        CreateLevel2Data();
        
        AssetDatabase.Refresh();
        Debug.Log("重新創建完成！");
    }
    
    private static void CreateLevel1Data()
    {
        var levelAsset = ScriptableObject.CreateInstance<LevelDataAsset>();
        levelAsset.levelData = new LevelData
        {
            levelName = "關卡 1 - 新手訓練",
            levelDescription = "學習基本操作，消滅所有敵人",
            timeLimit = 100,
            enemyWaves = new System.Collections.Generic.List<EnemyWave>
            {
                new EnemyWave
                {
                    enemyCount = 1,
                    waveDelay = 0,
                    spawnInterval = 1
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    waveDelay = 0,
                    spawnInterval = 1
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = false,
            survivalTime = 60,
            scoreReward = 100,
            experienceReward = 50
        };
        
        AssetDatabase.CreateAsset(levelAsset, "Assets/Scripts/LevelSystem/LevelConfigs/Level1_Data.asset");
    }
    
    private static void CreateLevel2Data()
    {
        var levelAsset = ScriptableObject.CreateInstance<LevelDataAsset>();
        levelAsset.levelData = new LevelData
        {
            levelName = "關卡 2 - 挑戰開始",
            levelDescription = "面對更多敵人，測試你的技能",
            timeLimit = 180,
            enemyWaves = new System.Collections.Generic.List<EnemyWave>
            {
                new EnemyWave
                {
                    enemyCount = 2,
                    waveDelay = 2,
                    spawnInterval = 0
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    waveDelay = 1,
                    spawnInterval = 0
                },
                new EnemyWave
                {
                    enemyCount = 1,
                    waveDelay = 1,
                    spawnInterval = 0
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = false,
            survivalTime = 60,
            scoreReward = 200,
            experienceReward = 100
        };
        
        AssetDatabase.CreateAsset(levelAsset, "Assets/Scripts/LevelSystem/LevelConfigs/Level2_Data.asset");
    }
}
#endif
