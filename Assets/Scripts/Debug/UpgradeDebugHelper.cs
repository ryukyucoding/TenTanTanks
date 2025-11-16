using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 升級系統調試助手 - 顯示詳細的按鍵和狀態資訊
/// </summary>
public class UpgradeDebugHelper : MonoBehaviour
{
    private TankStats tankStats;
    private UpgradeUI upgradeUI;

    void Start()
    {
        Debug.Log("========== 升級系統診斷開始 ==========");
        
        // 使用更強力的搜尋方式
        tankStats = FindFirstObjectByType<TankStats>(FindObjectsInactive.Include);
        upgradeUI = FindFirstObjectByType<UpgradeUI>(FindObjectsInactive.Include);

        if (tankStats == null)
        {
            Debug.LogError("❌ 找不到 TankStats！請確保玩家坦克上有此組件。");
            
            // 搜尋所有 TankController 來找玩家
            var controllers = FindObjectsByType<TankController>(FindObjectsSortMode.None);
            Debug.Log($"場景中找到 {controllers.Length} 個 TankController");
            foreach (var ctrl in controllers)
            {
                Debug.Log($"   - {ctrl.gameObject.name} (Tag: {ctrl.tag}) (Active: {ctrl.gameObject.activeInHierarchy})");
                var stats = ctrl.GetComponent<TankStats>();
                if (stats == null)
                {
                    Debug.LogWarning($"     ⚠️ 此物件缺少 TankStats 組件！");
                }
                else
                {
                    Debug.Log($"     ✓ 找到 TankStats！");
                    tankStats = stats; // 使用找到的第一個
                }
            }

            // 如果還是找不到，搜尋所有 GameObject
            if (tankStats == null)
            {
                Debug.Log("嘗試直接搜尋帶有 Player tag 的物件...");
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    tankStats = playerObj.GetComponent<TankStats>();
                    if (tankStats != null)
                    {
                        Debug.Log($"✅ 在 Player tag 物件上找到 TankStats！({playerObj.name})");
                    }
                    else
                    {
                        Debug.LogError($"❌ Player 物件 ({playerObj.name}) 上沒有 TankStats 組件！");
                    }
                }
            }
        }
        else
        {
            Debug.Log($"✅ 找到 TankStats (在物件: {tankStats.gameObject.name})");
            Debug.Log($"   - 當前點數: {tankStats.GetAvailableUpgradePoints()}");
            Debug.Log($"   - 移動速度等級: {tankStats.GetMoveSpeedLevel()}");
        }

        if (upgradeUI == null)
        {
            Debug.LogError("❌ 找不到 UpgradeUI！請確保場景中有此組件。");
        }
        else
        {
            Debug.Log($"✅ 找到 UpgradeUI (在物件: {upgradeUI.gameObject.name})");
        }

        // 檢查 Input System
        if (Keyboard.current == null)
        {
            Debug.LogError("❌ Keyboard.current 是 null！Input System 可能未正確設置。");
        }
        else
        {
            Debug.Log("✅ Input System Keyboard 正常");
        }

        Debug.Log("========== 診斷完成 ==========");
    }

    private string lastKeyPressed = "";
    private float lastKeyTime = 0f;
    private bool anyKeyPressed = false;

    void Update()
    {
        if (tankStats == null) return;

        // 檢測按鍵（只使用新 Input System）
        if (Keyboard.current != null)
        {
            // 檢測是否有任何按鍵
            anyKeyPressed = Keyboard.current.anyKey.isPressed;

            // P 鍵：添加升級點數（測試用）
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("🟢 [調試] 按下 P 鍵 - 添加 3 升級點數");
                lastKeyPressed = "按下 P 鍵！";
                lastKeyTime = Time.time;
                tankStats.AddUpgradePoints(3);
            }

            // I 鍵：顯示當前屬性
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                Debug.Log("========== 當前坦克屬性 ==========");
                Debug.Log($"升級點數: {tankStats.GetAvailableUpgradePoints()}");
                Debug.Log($"移動速度: Lv.{tankStats.GetMoveSpeedLevel()} = {tankStats.GetCurrentMoveSpeed():F2}");
                Debug.Log($"子彈速度: Lv.{tankStats.GetBulletSpeedLevel()} = {tankStats.GetCurrentBulletSpeed():F2}");
                Debug.Log($"射速: Lv.{tankStats.GetFireRateLevel()} = {tankStats.GetCurrentFireRate():F2}");
                Debug.Log("==================================");
                lastKeyPressed = "按下 I 鍵 - 查看屬性";
                lastKeyTime = Time.time;
            }
        }
    }

    void OnGUI()
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.normal.textColor = Color.white;
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.8f));

        // 左上角：系統狀態
        string statusInfo = "=== 升級系統狀態 ===\n";
        statusInfo += tankStats != null ? "TankStats: ✓ 找到\n" : "TankStats: ✗ 缺失\n";
        statusInfo += upgradeUI != null ? "UpgradeUI: ✓ 找到\n" : "UpgradeUI: ✗ 缺失\n";
        statusInfo += Keyboard.current != null ? "Keyboard: ✓ 正常\n" : "Keyboard: ✗ Null\n";
        statusInfo += anyKeyPressed ? "Input: ✓ 有按鍵\n" : "Input: 無按鍵\n";
        
        if (Time.time - lastKeyTime < 2f)
        {
            statusInfo += $"\n最後按鍵:\n{lastKeyPressed}";
        }

        GUI.Box(new Rect(10, 10, 300, 180), statusInfo, boxStyle);

        // 右上角：屬性狀態
        if (tankStats != null)
        {
            string info = "=== 坦克屬性 ===\n";
            info += $"升級點數: {tankStats.GetAvailableUpgradePoints()}\n\n";
            info += $"移動速度: Lv.{tankStats.GetMoveSpeedLevel()}/{tankStats.GetMaxMoveSpeedLevel()}\n";
            info += $"  值: {tankStats.GetCurrentMoveSpeed():F2}\n";
            info += $"子彈速度: Lv.{tankStats.GetBulletSpeedLevel()}/{tankStats.GetMaxBulletSpeedLevel()}\n";
            info += $"  值: {tankStats.GetCurrentBulletSpeed():F2}\n";
            info += $"射速: Lv.{tankStats.GetFireRateLevel()}/{tankStats.GetMaxFireRateLevel()}\n";
            info += $"  值: {tankStats.GetCurrentFireRate():F2}\n";

            GUI.Box(new Rect(Screen.width - 310, 10, 300, 200), info, boxStyle);
        }

        // 底部：操作提示
        GUIStyle hintStyle = new GUIStyle(boxStyle);
        hintStyle.fontSize = 18;
        hintStyle.normal.textColor = Color.yellow;
        string hint = "按 P 鍵: 獲得 3 點 | 按 1/2/3: 升級屬性 | 按 I: 查看 Console";
        GUI.Box(new Rect(Screen.width / 2 - 350, Screen.height - 50, 700, 40), hint, hintStyle);
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
