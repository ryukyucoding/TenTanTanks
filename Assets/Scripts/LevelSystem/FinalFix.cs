using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class FinalFix : EditorWindow
{
    [MenuItem("Tools/FINAL FIX - Repair Everything")]
    public static void FinalFixEverything()
    {
        Debug.Log("=== 🔧 最終修復開始 ===");
        
        // 步驟 1: 刪除所有 .meta 文件的緩存
        Debug.Log("步驟 1: 清理緩存...");
        AssetDatabase.ReleaseCachedFileHandles();
        
        // 步驟 2: 強制重新編譯 LevelData.cs
        Debug.Log("步驟 2: 重新編譯 LevelData.cs...");
        string scriptPath = "Assets/Scripts/LevelSystem/LevelData.cs";
        AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        
        // 等待編譯
        System.Threading.Thread.Sleep(2000);
        
        // 步驟 3: 重新生成所有 LevelDataAsset 文件
        Debug.Log("步驟 3: 重新生成 LevelDataAsset 文件...");
        RegenerateAllLevelDataAssets();
        
        // 步驟 4: 刷新資產數據庫
        Debug.Log("步驟 4: 刷新資產數據庫...");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // 步驟 5: 驗證修復結果
        Debug.Log("步驟 5: 驗證修復結果...");
        System.Threading.Thread.Sleep(1000);
        VerifyFix();
        
        Debug.Log("=== ✅ 最終修復完成 ===");
    }
    
    private static void RegenerateAllLevelDataAssets()
    {
        // 查找所有現有的 LevelDataAsset 文件
        string[] assetFiles = Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories);
        
        foreach (string filePath in assetFiles)
        {
            if (filePath.Contains("Library") || filePath.Contains("Packages"))
                continue;
            
            try
            {
                string content = File.ReadAllText(filePath);
                
                // 只處理 LevelDataAsset 文件
                if (content.Contains("Assembly-CSharp::LevelDataAsset"))
                {
                    Debug.Log($"重新生成: {filePath}");
                    
                    // 從原文件讀取數據
                    string levelName = ExtractValue(content, "levelName:");
                    string levelDescription = ExtractValue(content, "levelDescription:");
                    int difficulty = ExtractIntValue(content, "difficulty:");
                    
                    // 創建新的資產
                    LevelDataAsset newAsset = ScriptableObject.CreateInstance<LevelDataAsset>();
                    newAsset.levelData = new LevelData();
                    newAsset.levelData.levelName = UnescapeUnicode(levelName);
                    newAsset.levelData.levelDescription = UnescapeUnicode(levelDescription);
                    newAsset.levelData.enemyWaves = new System.Collections.Generic.List<EnemyWave>();
                    newAsset.difficulty = difficulty;
                    
                    // 添加默認波數
                    newAsset.levelData.enemyWaves.Add(new EnemyWave
                    {
                        enemyCount = 2,
                        waveDelay = 2f,
                        spawnInterval = 1f
                    });
                    
                    // 刪除舊文件
                    AssetDatabase.DeleteAsset(filePath);
                    
                    // 創建新文件
                    AssetDatabase.CreateAsset(newAsset, filePath);
                    
                    Debug.Log($"✅ 已重新生成: {filePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"重新生成文件失敗 {filePath}: {e.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
    }
    
    private static string ExtractValue(string content, string key)
    {
        int startIndex = content.IndexOf(key);
        if (startIndex == -1) return "";
        
        startIndex += key.Length;
        int endIndex = content.IndexOf("\n", startIndex);
        if (endIndex == -1) return "";
        
        string value = content.Substring(startIndex, endIndex - startIndex).Trim();
        // 移除引號
        value = value.Trim('"', ' ');
        return value;
    }
    
    private static int ExtractIntValue(string content, string key)
    {
        string value = ExtractValue(content, key);
        int result;
        if (int.TryParse(value, out result))
            return result;
        return 1;
    }
    
    private static string UnescapeUnicode(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        
        // 處理 Unicode 轉義序列
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\\u([0-9A-Fa-f]{4})");
        return regex.Replace(str, match => 
            ((char)int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)).ToString()
        );
    }
    
    private static void VerifyFix()
    {
        Debug.Log("=== 驗證修復結果 ===");
        
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        Debug.Log($"找到 {guids.Length} 個 LevelDataAsset");
        
        int successCount = 0;
        int failCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            
            LevelDataAsset asset = AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
            if (asset != null)
            {
                Debug.Log($"✅ 成功載入: {path} - {asset.levelData?.levelName}");
                successCount++;
            }
            else
            {
                Debug.LogError($"❌ 載入失敗: {path}");
                failCount++;
            }
        }
        
        Debug.Log($"驗證完成: 成功 {successCount} 個，失敗 {failCount} 個");
    }
}
#endif

