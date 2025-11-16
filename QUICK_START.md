# 🎮 坦克升級系統 - 快速開始

## ⚡ 5 分鐘快速設置

### 第一步：玩家坦克設置
1. 選擇你的玩家坦克物件
2. 添加 `TankStats` 組件
3. 完成！（使用預設值即可）

### 第二步：場景設置
1. 創建空物件 → 命名為 `GameManagers`
2. 添加 `UpgradePointManager` 組件
3. 完成！

### 第三步：UI 設置（簡化版）

**最簡單的方法**：
1. 創建 Canvas
2. 創建 Panel（命名為 `UpgradePanel`）
   - 設置 Anchor 為左下角
   - Position: (20, 20)
3. 在 Panel 下創建：
   - 1 個 TextMeshProUGUI（顯示點數）
   - 3 個 Button（每個按鈕有一個子 Text）
4. 在 UpgradePanel 上添加 `UpgradeUI` 組件
5. 拖拽引用：
   - Upgrade Panel → 拖入 Panel
   - Upgrade Points Text → 拖入文字
   - 對於每個按鈕：
     - Button → 拖入按鈕本身
     - Button Image → 拖入按鈕的 Image
     - Stat Name Text → 拖入按鈕的子文字

### 第四步：測試
1. 添加 `UpgradeSystemTester` 到場景任何物件（可選）
2. 播放遊戲
3. 按 `P` 鍵獲得升級點數
4. 按 `1` `2` `3` 鍵或點擊按鈕升級

---

## 📋 完整的組件清單

### 需要的新腳本
- ✅ `TankStats.cs` - 已創建
- ✅ `UpgradeUI.cs` - 已創建
- ✅ `UpgradePointManager.cs` - 已創建
- ✅ `UpgradeSystemTester.cs` - 已創建（測試用）

### 已自動修改的腳本
- ✅ `TankController.cs` - 添加了 SetMoveSpeed()
- ✅ `TankShooting.cs` - 添加了 SetBulletSpeed() 和 SetFireRate()
- ✅ `EnemyTankAI.cs` - 死亡時通知升級系統
- ✅ `EnemyTank.cs` - 死亡時通知升級系統
- ✅ `WaveManager.cs` - 波次完成時給予點數

---

## 🎯 使用方式

### 遊戲中
- **鍵盤**：按 `1` `2` `3` 升級三個屬性
- **滑鼠**：點擊按鈕升級

### 測試中
- 按 `P`：獲得 3 升級點數
- 按 `I`：顯示當前屬性
- 或在 Inspector 中右鍵組件使用測試功能

---

## 🎨 UI 示例結構

```
Canvas
└── UpgradePanel
    ├── PointsText: "升級點數: 3"
    ├── Button1
    │   └── Text: "[1] 移動速度 Lv.0/10 (2.5 → 2.8)"
    ├── Button2
    │   └── Text: "[2] 子彈速度 Lv.0/10 (5.0 → 5.5)"
    └── Button3
        └── Text: "[3] 射速 Lv.0/10 (1.2 → 1.5)"
```

---

## ⚙️ 屬性說明

| 屬性 | 初始值 | 每次增加 | 快捷鍵 |
|------|--------|----------|--------|
| 移動速度 | 2.5 | +0.3 | 1 |
| 子彈速度 | 5.0 | +0.5 | 2 |
| 射速 | 1.2 | +0.3 | 3 |

每完成一波敵人：獲得 **3 點**升級點數

---

## 🔧 自訂設定

### 改變初始值或增量
編輯 `TankStats` 組件的 Inspector 參數

### 改變每波獎勵點數
編輯 `UpgradePointManager` 組件的 `Upgrade Points Per Wave`

### 改變快捷鍵
編輯 `UpgradeUI.cs` 的 Update 方法

---

## ❗ 常見問題

**Q: 為什麼沒有點數？**
A: 需要擊殺所有敵人完成一波，或按 P 鍵測試

**Q: 點擊沒反應？**
A: 檢查 UpgradeUI 的引用是否都設置正確

**Q: 屬性沒變化？**
A: 確保 TankStats 與 TankController、TankShooting 在同一物件上

---

詳細文檔請參考：`UPGRADE_SYSTEM_SETUP.md`
