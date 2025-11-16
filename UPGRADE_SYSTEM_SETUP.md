# 坦克屬性升級系統 - 設置指南

## 系統概述

這個升級系統模仿 diep.io 的風格，允許玩家在每波敵人結束後獲得升級點數，用來提升坦克的三個核心屬性：
1. **移動速度** - 初始 2.5，每次增加 0.3
2. **子彈速度** - 初始 5.0，每次增加 0.5
3. **射速 (Fire Rate)** - 初始 1.2，每次增加 0.3

## 設置步驟

### 1. 在玩家坦克上添加組件

在 Unity 中選擇你的玩家坦克物件（通常是 `Player` 或 `PlayerTank`），添加以下組件：

**a) TankStats 組件**
- 在 Inspector 中點擊 "Add Component"
- 搜索並添加 `TankStats`
- 設定參數（可以使用預設值）：
  - Base Move Speed: 2.5
  - Base Bullet Speed: 5
  - Base Fire Rate: 1.2
  - Move Speed Increment: 0.3
  - Bullet Speed Increment: 0.5
  - Fire Rate Increment: 0.3
  - Max Levels: 10（可調整最大升級次數）

### 2. 創建升級 UI

**a) 創建 Canvas**
1. 在 Hierarchy 中右鍵 → UI → Canvas
2. 設定 Canvas 的 Render Mode 為 "Screen Space - Overlay"

**b) 創建升級面板**
1. 在 Canvas 下創建一個空物件，命名為 `UpgradePanel`
2. 添加 Vertical Layout Group 組件
3. 設置位置在左下角：
   - Anchor: Bottom Left
   - Pivot: (0, 0)
   - Position: (20, 20)

**c) 創建升級點數顯示**
1. 在 UpgradePanel 下創建 TextMeshPro 文字（UI → Text - TextMeshProUGUI）
2. 命名為 `UpgradePointsText`
3. 設定文字大小和顏色

**d) 創建三個升級按鈕**

為每個屬性創建一個按鈕：

1. 在 UpgradePanel 下創建 Button（UI → Button - TextMeshPro）
2. 命名為 `MoveSpeedButton`、`BulletSpeedButton`、`FireRateButton`
3. 為每個按鈕添加子物件：
   - `StatName` (TextMeshProUGUI) - 顯示屬性名稱
   - `Level` (TextMeshProUGUI) - 顯示等級 (Lv.X/10)
   - `Value` (TextMeshProUGUI) - 顯示數值 (5.0 → 5.5)
   - `Hotkey` (TextMeshProUGUI) - 顯示快捷鍵 ([1])

**簡化版本**：如果覺得太複雜，可以只創建三個簡單的按鈕，讓每個按鈕只有一個文字顯示屬性名稱即可。

### 3. 設定 UpgradeUI 組件

1. 在 Canvas 或 UpgradePanel 上添加 `UpgradeUI` 組件
2. 在 Inspector 中設定引用：
   - Upgrade Panel: 拖入 UpgradePanel 物件
   - Upgrade Points Text: 拖入 UpgradePointsText
   - 為每個按鈕設定：
     - Button: 拖入按鈕本身
     - Button Image: 拖入按鈕的 Image 組件
     - Stat Name Text: 拖入對應的文字
     - Level Text: 拖入對應的文字
     - Value Text: 拖入對應的文字
     - Hotkey Text: 拖入對應的文字

### 4. 添加 UpgradePointManager

1. 在場景中創建一個空物件，命名為 `UpgradePointManager`
2. 添加 `UpgradePointManager` 組件
3. 設定：
   - Upgrade Points Per Wave: 3（每波給予 3 點）
   - Player Tank Stats: 拖入玩家坦克的 TankStats 組件（也可以留空，會自動尋找）

### 5. 確認現有組件

確保你的場景中已經有：
- ✅ **WaveManager** - 管理敵人波次（應該已經存在）
- ✅ **EnemyTankAI** 或 **EnemyTank** - 敵人腳本（已自動修改，會通知升級系統）

## 使用方式

### 遊戲中操作

- **滑鼠點擊**：直接點擊升級按鈕
- **鍵盤快捷鍵**：
  - 按 `1` 升級移動速度
  - 按 `2` 升級子彈速度
  - 按 `3` 升級射速

### 測試方式

在編輯模式下測試升級系統：

1. **方法 1**：使用 UpgradePointManager 的測試功能
   - 在 Inspector 中右鍵 `UpgradePointManager` 組件
   - 選擇 "Add 3 Upgrade Points (Test)"
   - 會立即獲得 3 點升級點數

2. **方法 2**：完成一波敵人
   - 消滅場景中的所有敵人
   - 波次完成時會自動獲得升級點數

## 自訂設定

### 調整升級數值

在 `TankStats` 組件中可以調整：
- **Base Stats**: 初始屬性值
- **Increments**: 每次升級增加的數值
- **Max Levels**: 最大升級次數

### 調整獎勵點數

在 `UpgradePointManager` 組件中：
- **Upgrade Points Per Wave**: 每波結束給予的點數（預設 3）

### UI 顏色

在 `UpgradeUI` 組件中可以調整按鈕顏色：
- **Can Upgrade Color**: 可以升級時的顏色（預設綠色）
- **Cannot Upgrade Color**: 無法升級時的顏色（預設灰色）
- **Max Level Color**: 已達最大等級的顏色（預設金色）

## 腳本說明

### 核心腳本

1. **TankStats.cs**
   - 管理坦克的所有可升級屬性
   - 處理升級邏輯
   - 自動更新 TankController 和 TankShooting 的數值

2. **UpgradeUI.cs**
   - 管理升級介面顯示
   - 處理按鈕點擊和鍵盤輸入
   - 更新按鈕狀態和顏色

3. **UpgradePointManager.cs**
   - 追蹤敵人擊殺
   - 在波次完成時給予升級點數
   - 與 WaveManager 整合

### 修改的現有腳本

- **TankController.cs**: 添加了 `SetMoveSpeed()` 方法
- **TankShooting.cs**: 添加了 `SetBulletSpeed()` 和 `SetFireRate()` 方法
- **EnemyTankAI.cs** & **EnemyTank.cs**: 在死亡時通知 UpgradePointManager

## 常見問題

### Q: 升級點數沒有出現？
A: 確認：
1. UpgradePointManager 組件已添加到場景中
2. 已擊殺所有敵人，完成一波
3. WaveManager 正確設置並運行

### Q: 點擊按鈕沒有反應？
A: 確認：
1. UpgradeUI 組件的所有引用都已正確設定
2. TankStats 組件在玩家坦克上
3. 有足夠的升級點數

### Q: 屬性升級後沒有效果？
A: 確認：
1. TankStats 組件與 TankController、TankShooting 在同一個物件上
2. 檢查 Console 是否有錯誤訊息

### Q: 想要修改快捷鍵？
A: 在 `UpgradeUI.cs` 的 `Update()` 方法中修改：
```csharp
if (Keyboard.current.digit1Key.wasPressedThisFrame)  // 改成其他按鍵
```

## 進階擴展

想要添加更多屬性？例如：
- 最大生命值
- 傷害倍率
- 射程
- 子彈反彈次數

只需：
1. 在 `TankStats.cs` 中添加新的屬性和升級邏輯
2. 在 `UpgradeUI.cs` 中添加新的按鈕
3. 在相關腳本中實現 `Set` 方法

---

## 完成！

現在你的坦克遊戲已經有了完整的升級系統！每完成一波敵人，玩家就可以選擇強化自己的坦克，讓遊戲更有深度和策略性。

祝你開發順利！🎮✨
