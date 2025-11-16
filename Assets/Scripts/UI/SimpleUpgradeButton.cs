using UnityEngine;

/// <summary>
/// 簡化版升級按鈕
/// 只需要一個按鈕和一個文字即可
/// </summary>
public class SimpleUpgradeButton : MonoBehaviour
{
    // 這個腳本僅作為參考，說明如何創建簡化版本的升級按鈕
    // 實際上 UpgradeUI 已經支援完整和簡化兩種模式
    
    /*
     * 簡化版 UI 結構：
     * 
     * Canvas
     * └── UpgradePanel (Vertical Layout Group)
     *     ├── UpgradePointsText (TextMeshProUGUI) "升級點數: 0"
     *     ├── MoveSpeedButton (Button)
     *     │   └── Text (TextMeshProUGUI) "[1] 移動速度 Lv.0/10 (2.5 → 2.8)"
     *     ├── BulletSpeedButton (Button)
     *     │   └── Text (TextMeshProUGUI) "[2] 子彈速度 Lv.0/10 (5.0 → 5.5)"
     *     └── FireRateButton (Button)
     *         └── Text (TextMeshProUGUI) "[3] 射速 Lv.0/10 (1.2 → 1.5)"
     * 
     * 設置 UpgradeUI 時：
     * - 只需要填寫 button 和 statNameText（用來顯示所有資訊）
     * - 其他欄位可以留空
     * - UpgradeUI 會自動將所有資訊顯示在 statNameText 中
     */
}
