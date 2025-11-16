using UnityEngine;
using TMPro;

/// <summary>
/// 即時顯示坦克當前屬性
/// </summary>
public class TankStatsDisplay : MonoBehaviour
{
    [SerializeField] private TankStats tankStats;
    [SerializeField] private TankController tankController;
    [SerializeField] private TankShooting tankShooting;
    
    private TextMeshProUGUI displayText;
    private float updateInterval = 0.5f;
    private float nextUpdateTime = 0f;

    void Start()
    {
        // 自動尋找組件
        if (tankStats == null)
            tankStats = FindFirstObjectByType<TankStats>();
        if (tankController == null)
            tankController = FindFirstObjectByType<TankController>();
        if (tankShooting == null)
            tankShooting = FindFirstObjectByType<TankShooting>();
    }

    void OnGUI()
    {
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + updateInterval;

        if (tankStats == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 20;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.cyan;
        style.padding = new RectOffset(15, 15, 15, 15);
        style.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.85f));

        string info = "<b>=== 坦克屬性狀態 ===</b>\n\n";
        info += $"<color=yellow>升級點數: {tankStats.GetAvailableUpgradePoints()}</color>\n\n";
        
        info += $"<b>移動速度</b>\n";
        info += $"  等級: Lv.{tankStats.GetMoveSpeedLevel()}/{tankStats.GetMaxMoveSpeedLevel()}\n";
        info += $"  <color=lime>數值: {tankStats.GetCurrentMoveSpeed():F2}</color>\n\n";
        
        info += $"<b>子彈速度</b>\n";
        info += $"  等級: Lv.{tankStats.GetBulletSpeedLevel()}/{tankStats.GetMaxBulletSpeedLevel()}\n";
        info += $"  <color=lime>數值: {tankStats.GetCurrentBulletSpeed():F2}</color>\n\n";
        
        info += $"<b>射速</b>\n";
        info += $"  等級: Lv.{tankStats.GetFireRateLevel()}/{tankStats.GetMaxFireRateLevel()}\n";
        info += $"  <color=lime>數值: {tankStats.GetCurrentFireRate():F2}</color>\n\n";
        
        info += $"<color=orange>按 P: 獲得點數 | 按 1/2/3: 升級</color>";

        GUI.Box(new Rect(10, Screen.height - 360, 350, 350), info, style);
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
