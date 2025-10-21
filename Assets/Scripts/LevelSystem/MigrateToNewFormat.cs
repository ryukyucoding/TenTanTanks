using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
public class MigrateToNewFormat : EditorWindow
{
    [MenuItem("Tools/Migrate to New LevelData Format")]
    public static void MigrateLevelData()
    {
        Debug.Log("=== 🔄 遷移到新的 LevelData 格式 ===");
        
        // 查找所有舊的 .asset 文件
        string[] assetFiles = Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories);
        int migratedCount = 0;
        
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
                    Debug.Log($"遷移文件: {filePath}");
                    
                    // 解析數據
                    string levelName = ExtractValue(content, "levelName:");
                    string levelDescription = ExtractValue(content, "levelDescription:");
                    int difficulty = ExtractIntValue(content, "difficulty:");
                    
                    // 創建新的 LevelDataNew 資產
                    LevelDataNew newAsset = ScriptableObject.CreateInstance<LevelDataNew>();
                    newAsset.config = new LevelConfiguration();
                    newAsset.config.levelName = UnescapeUnicode(levelName);
                    newAsset.config.levelDescription = UnescapeUnicode(levelDescription);
                    newAsset.config.waves = new List<EnemyWaveData>();
                    newAsset.difficulty = difficulty;
                    
                    // 添加默認波數
                    EnemyWaveData wave1 = new EnemyWaveData();
                    wave1.enemyCount = 2;
                    wave1.waveDelay = 2f;
                    wave1.spawnInterval = 1f;
                    newAsset.config.waves.Add(wave1);
                    
                    EnemyWaveData wave2 = new EnemyWaveData();
                    wave2.enemyCount = 3;
                    wave2.waveDelay = 3f;
                    wave2.spawnInterval = 0.8f;
                    newAsset.config.waves.Add(wave2);
                    
                    // 生成新文件路徑
                    string newPath = filePath.Replace(".asset", "_New.asset");
                    
                    // 創建新資產
                    AssetDatabase.CreateAsset(newAsset, newPath);
                    migratedCount++;
                    
                    Debug.Log($"✅ 已遷移到: {newPath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"遷移文件失敗 {filePath}: {e.Message}");
            }
        }
        
        if (migratedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"=== ✅ 遷移完成！共遷移了 {migratedCount} 個文件 ===");
            Debug.Log("新文件已創建，請在 Inspector 中重新配置 Enemy Prefab 和 Spawn Points");
        }
        else
        {
            Debug.Log("沒有找到需要遷移的文件");
        }
    }
    
    private static string ExtractValue(string content, string key)
    {
        int startIndex = content.IndexOf(key);
        if (startIndex == -1) return "";
        
        startIndex += key.Length;
        int endIndex = content.IndexOf("\n", startIndex);
        if (endIndex == -1) return "";
        
        string value = content.Substring(startIndex, endIndex - startIndex).Trim();
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
        
        System.Text.RegularExpressions.Regex regex = 
            new System.Text.RegularExpressions.Regex(@"\\u([0-9A-Fa-f]{4})");
        return regex.Replace(str, match => 
            ((char)int.Parse(match.Groups[1].Value, 
                System.Globalization.NumberStyles.HexNumber)).ToString()
        );
    }
    
    [MenuItem("Tools/Test New LevelData")]
    public static void TestNewLevelData()
    {
        Debug.Log("=== 🧪 測試新的 LevelData 格式 ===");
        
        // 查找所有新格式的文件
        string[] guids = AssetDatabase.FindAssets("t:LevelDataNew");
        Debug.Log($"找到 {guids.Length} 個 LevelDataNew 文件");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            
            LevelDataNew asset = AssetDatabase.LoadAssetAtPath<LevelDataNew>(path);
            if (asset != null)
            {
                Debug.Log($"✅ 成功載入: {path}");
                Debug.Log($"   關卡名稱: {asset.config.levelName}");
                Debug.Log($"   波數數量: {asset.config.waves.Count}");
                Debug.Log($"   難度: {asset.difficulty}");
            }
            else
            {
                Debug.LogError($"❌ 載入失敗: {path}");
            }
        }
        
        Debug.Log("=== 測試完成 ===");
    }
}
#endif

