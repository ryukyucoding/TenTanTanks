using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 適配器：讓 SimpleLevelController 可以使用新格式的 LevelDataNew
/// 注意：這個類不是 MonoBehaviour，只是一個數據包裝器
/// </summary>
[System.Serializable]
public class LevelDataAssetWrapper
{
    public LevelDataNew newFormat;  // 新格式
    public LevelDataAsset oldFormat;  // 舊格式（兼容）
    
    public LevelData GetLevelData()
    {
        if (newFormat != null)
        {
            return newFormat.ToLegacyFormat();
        }
        else if (oldFormat != null)
        {
            return oldFormat.levelData;
        }
        
        Debug.LogError("LevelDataAssetWrapper: 沒有有效的關卡數據！");
        return new LevelData();
    }
    
    public bool IsValid()
    {
        return newFormat != null || oldFormat != null;
    }
}

// LevelControllerAdapter 已移動到單獨的文件 LevelControllerAdapter.cs

