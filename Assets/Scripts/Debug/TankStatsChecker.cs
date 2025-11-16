using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 检查场景中所有的 TankStats 对象
/// 按 T 键显示详细信息
/// </summary>
public class TankStatsChecker : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            CheckAllTankStats();
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 16;
        style.normal.textColor = Color.yellow;
        style.padding = new RectOffset(10, 10, 10, 10);
        style.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.85f));

        // 查找所有 TankStats
        TankStats[] allStats = FindObjectsByType<TankStats>(FindObjectsSortMode.None);
        
        string info = $"=== TankStats 对象数量: {allStats.Length} ===\n\n";
        
        int index = 1;
        foreach (var ts in allStats)
        {
            info += $"#{index}: {ts.gameObject.name}\n";
            info += $"  InstanceID: {ts.GetInstanceID()}\n";
            info += $"  GameObject ID: {ts.gameObject.GetInstanceID()}\n";
            info += $"  升级点数: {ts.GetAvailableUpgradePoints()}\n";
            info += $"  移动等级: {ts.GetMoveSpeedLevel()}\n";
            info += $"  是 Clone: {ts.gameObject.name.Contains("Clone")}\n";
            info += $"  Active: {ts.gameObject.activeInHierarchy}\n\n";
            index++;
        }
        
        // 检查 UpgradePointManager
        var upm = FindFirstObjectByType<UpgradePointManager>();
        if (upm != null)
        {
            var field = typeof(UpgradePointManager).GetField("playerTankStats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var targetStats = field.GetValue(upm) as TankStats;
                if (targetStats != null)
                {
                    info += $"--- UpgradePointManager ---\n";
                    info += $"目标: {targetStats.gameObject.name}\n";
                    info += $"InstanceID: {targetStats.GetInstanceID()}\n\n";
                }
                else
                {
                    info += "--- UpgradePointManager ---\n目标: NULL\n\n";
                }
            }
        }
        
        // 检查 UpgradeUI
        var ui = FindFirstObjectByType<UpgradeUI>();
        if (ui != null)
        {
            var field = typeof(UpgradeUI).GetField("tankStats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var targetStats = field.GetValue(ui) as TankStats;
                if (targetStats != null)
                {
                    info += $"--- UpgradeUI ---\n";
                    info += $"目标: {targetStats.gameObject.name}\n";
                    info += $"InstanceID: {targetStats.GetInstanceID()}\n\n";
                }
                else
                {
                    info += "--- UpgradeUI ---\n目标: NULL\n\n";
                }
            }
        }
        
        info += "\n按 T 键显示详细控制台日志";

        GUI.Box(new Rect(10, 10, 350, Screen.height - 20), info, style);
    }

    private void CheckAllTankStats()
    {
        Debug.Log("========================================");
        Debug.Log("=== 所有 TankStats 对象检查 ===");
        Debug.Log("========================================");
        
        TankStats[] allStats = FindObjectsByType<TankStats>(FindObjectsSortMode.None);
        Debug.Log($"场景中共有 {allStats.Length} 个 TankStats");
        
        int index = 1;
        foreach (var ts in allStats)
        {
            Debug.Log($"\n--- TankStats #{index} ---");
            Debug.Log($"名称: {ts.gameObject.name}");
            Debug.Log($"InstanceID: {ts.GetInstanceID()}");
            Debug.Log($"GameObject InstanceID: {ts.gameObject.GetInstanceID()}");
            Debug.Log($"升级点数: {ts.GetAvailableUpgradePoints()}");
            Debug.Log($"移动等级: {ts.GetMoveSpeedLevel()}");
            Debug.Log($"子弹等级: {ts.GetBulletSpeedLevel()}");
            Debug.Log($"射速等级: {ts.GetFireRateLevel()}");
            Debug.Log($"是 Clone: {ts.gameObject.name.Contains("Clone")}");
            Debug.Log($"Active: {ts.gameObject.activeInHierarchy}");
            Debug.Log($"Tag: {ts.tag}");
            index++;
        }
        
        Debug.Log("\n========================================");
        
        // 检查 UpgradePointManager
        var upm = FindFirstObjectByType<UpgradePointManager>();
        if (upm != null)
        {
            var field = typeof(UpgradePointManager).GetField("playerTankStats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var targetStats = field.GetValue(upm) as TankStats;
                Debug.Log($"\n[UpgradePointManager] 当前目标:");
                if (targetStats != null)
                {
                    Debug.Log($"  名称: {targetStats.gameObject.name}");
                    Debug.Log($"  InstanceID: {targetStats.GetInstanceID()}");
                }
                else
                {
                    Debug.LogError("  NULL！");
                }
            }
        }
        
        // 检查 UpgradeUI
        var ui = FindFirstObjectByType<UpgradeUI>();
        if (ui != null)
        {
            var field = typeof(UpgradeUI).GetField("tankStats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var targetStats = field.GetValue(ui) as TankStats;
                Debug.Log($"\n[UpgradeUI] 当前目标:");
                if (targetStats != null)
                {
                    Debug.Log($"  名称: {targetStats.gameObject.name}");
                    Debug.Log($"  InstanceID: {targetStats.GetInstanceID()}");
                }
                else
                {
                    Debug.LogError("  NULL！");
                }
            }
        }
        
        Debug.Log("\n========================================");
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
