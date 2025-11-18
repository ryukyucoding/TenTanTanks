using UnityEngine;
using UnityEditor;
using System.IO;

public class LevelDataResourceCopier : EditorWindow
{
    [MenuItem("Tools/Copy Level Data to Resources")]
    public static void ShowWindow()
    {
        GetWindow<LevelDataResourceCopier>("Level Data Resource Copier");
    }

    private void OnGUI()
    {
        GUILayout.Label("將 LevelDataAsset 複製到 Resources 文件夾", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        GUILayout.Label("說明：", EditorStyles.boldLabel);
        GUILayout.Label("這個工具會將 LevelConfigs 文件夾中的 LevelDataAsset");
        GUILayout.Label("複製到 Resources/LevelConfigs 文件夾，以便運行時自動加載。");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("複製所有 Level Data 到 Resources", GUILayout.Height(30)))
        {
            CopyAllLevelDataToResources();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("清理 Resources/LevelConfigs", GUILayout.Height(30)))
        {
            CleanResourcesFolder();
        }
    }

    private void CopyAllLevelDataToResources()
    {
        string sourcePath = "Assets/Scripts/LevelSystem/LevelConfigs";
        string targetPath = "Assets/Resources/LevelConfigs";
        
        // 確保目標文件夾存在
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        if (!AssetDatabase.IsValidFolder(targetPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "LevelConfigs");
        }
        
        // 查找所有 LevelDataAsset
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset", new[] { sourcePath });
        
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("錯誤", $"在 {sourcePath} 中沒有找到 LevelDataAsset！", "確定");
            return;
        }
        
        int copiedCount = 0;
        
        foreach (string guid in guids)
        {
            string sourceAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(sourceAssetPath);
            string targetAssetPath = $"{targetPath}/{fileName}";
            
            // 檢查目標文件是否已存在
            if (File.Exists(targetAssetPath))
            {
                // 詢問是否覆蓋
                if (!EditorUtility.DisplayDialog(
                    "文件已存在",
                    $"文件 {fileName} 已存在於 Resources 文件夾。\n是否覆蓋？",
                    "覆蓋",
                    "跳過"))
                {
                    continue;
                }
            }
            
            // 複製文件
            if (AssetDatabase.CopyAsset(sourceAssetPath, targetAssetPath))
            {
                copiedCount++;
                Debug.Log($"✓ 已複製: {fileName}");
            }
            else
            {
                Debug.LogError($"✗ 複製失敗: {fileName}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog(
            "完成",
            $"已複製 {copiedCount}/{guids.Length} 個 LevelDataAsset 到 Resources 文件夾！",
            "確定");
        
        Debug.Log($"=== 複製完成：{copiedCount}/{guids.Length} ===");
    }

    private void CleanResourcesFolder()
    {
        string targetPath = "Assets/Resources/LevelConfigs";
        
        if (!AssetDatabase.IsValidFolder(targetPath))
        {
            EditorUtility.DisplayDialog("提示", "Resources/LevelConfigs 文件夾不存在，無需清理。", "確定");
            return;
        }
        
        if (EditorUtility.DisplayDialog(
            "確認清理",
            "確定要刪除 Resources/LevelConfigs 文件夾中的所有文件嗎？",
            "確定",
            "取消"))
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset", new[] { targetPath });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("完成", "已清理 Resources/LevelConfigs 文件夾！", "確定");
            Debug.Log("=== 清理完成 ===");
        }
    }
}

