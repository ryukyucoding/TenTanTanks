using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor 工具：自動創建 Wave Progress Bar 所需的 Prefab
/// </summary>
public class CreateWaveProgressBarPrefabs : EditorWindow
{
    [MenuItem("Tools/Wave Progress Bar/Create UI Prefabs")]
    public static void CreatePrefabs()
    {
        // 確保 Prefabs/UI 資料夾存在
        string prefabPath = "Assets/Prefabs/UI";
        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }

        // 創建 Tank Icon Prefab
        CreateTankIconPrefab(prefabPath);

        // 創建 Wave Mark Prefab
        CreateWaveMarkPrefab(prefabPath);

        AssetDatabase.Refresh();
        Debug.Log("✓ Wave Progress Bar Prefabs 創建完成！");
    }

    private static void CreateTankIconPrefab(string path)
    {
        // 載入 tank.png sprite
        Sprite tankSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/tank.png");
        if (tankSprite == null)
        {
            Debug.LogError("找不到 tank.png！請確認圖片在 Assets/Sprites/UI/tank.png");
            return;
        }

        // 創建 GameObject
        GameObject tankIcon = new GameObject("TankIcon");
        RectTransform rectTransform = tankIcon.AddComponent<RectTransform>();
        Image image = tankIcon.AddComponent<Image>();

        // 設定 Image
        image.sprite = tankSprite;
        image.preserveAspect = true;

        // 設定大小
        rectTransform.sizeDelta = new Vector2(40, 40); // 可調整大小

        // 儲存為 Prefab
        string prefabPath = path + "/TankIcon.prefab";
        PrefabUtility.SaveAsPrefabAsset(tankIcon, prefabPath);

        // 刪除臨時物件
        DestroyImmediate(tankIcon);

        Debug.Log($"✓ TankIcon Prefab 已創建：{prefabPath}");
    }

    private static void CreateWaveMarkPrefab(string path)
    {
        // 載入 wave_mark.png sprite
        Sprite waveMarkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/wave_mark.png");
        if (waveMarkSprite == null)
        {
            Debug.LogWarning("找不到 wave_mark.png！跳過創建 Wave Mark Prefab");
            return;
        }

        // 創建 GameObject
        GameObject waveMark = new GameObject("WaveMark");
        RectTransform rectTransform = waveMark.AddComponent<RectTransform>();
        Image image = waveMark.AddComponent<Image>();

        // 設定 Image
        image.sprite = waveMarkSprite;
        image.preserveAspect = true;

        // 設定大小
        rectTransform.sizeDelta = new Vector2(30, 30); // 可調整大小

        // 儲存為 Prefab
        string prefabPath = path + "/WaveMark.prefab";
        PrefabUtility.SaveAsPrefabAsset(waveMark, prefabPath);

        // 刪除臨時物件
        DestroyImmediate(waveMark);

        Debug.Log($"✓ WaveMark Prefab 已創建：{prefabPath}");
    }
}
