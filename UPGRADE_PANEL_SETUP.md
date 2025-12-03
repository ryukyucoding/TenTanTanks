# 升級面板視覺更新指南

## 已完成的設置

### 1. 圖片位置
- `upgrade_points.png` 已移動到：[Assets/UI/Images/upgrade_points.png](Assets/UI/Images/upgrade_points.png)

### 2. 創建的 Editor 工具

已創建以下工具來幫助您完成設置：

1. **[SetupUpgradePanelTexture.cs](Assets/Editor/SetupUpgradePanelTexture.cs)** - 設定圖片導入設置
2. **[SetupUpgradePanelComplete.cs](Assets/Editor/SetupUpgradePanelComplete.cs)** - 完整設置面板（推薦使用）
3. **[AdjustUpgradePanelSize.cs](Assets/Editor/AdjustUpgradePanelSize.cs)** - 只調整面板大小
4. **[ApplyUpgradePanelImage.cs](Assets/Editor/ApplyUpgradePanelImage.cs)** - 只套用圖片

## 推薦方法：使用完整設置工具 ⭐

### 步驟 1: 設定圖片導入設置
1. 打開 Unity Editor
2. 點擊頂部選單：`Tools > Setup upgrade_points Texture Import Settings`
3. 這會自動將圖片設定為 Sprite (2D and UI) 模式

### 步驟 2: 使用完整設置工具
1. 點擊頂部選單：`Tools > Setup Upgrade Panel (Complete)`
2. 在打開的視窗中：
   - 點擊 **"Find Panel Sprite"** 按鈕（自動找到 upgrade_points.png）
   - 點擊 **"Find Mojangles Font"** 按鈕（自動找到 Mojangles 字體）
3. 調整設置（可選）：
   - **Panel Settings**: 面板大小、位置、縮放
   - **Text Settings**: 文字位置、大小、字體大小
4. 使用預設佈局（可選）：
   - 點擊 **"Use Default Layout"** 或 **"Use Centered Layout"**
5. 點擊 **"Apply to All Level Scenes"** 按鈕

完成！工具會自動更新所有關卡（Level1-5）中的：
- ✅ 面板圖片和大小
- ✅ 所有文字的字體（Mojangles）
- ✅ 所有文字的位置和大小
- ✅ 文字對齊方式

## 完整設置工具的功能

**[SetupUpgradePanelComplete.cs](Assets/Editor/SetupUpgradePanelComplete.cs)** 可以設置：

### 面板設置
- **Panel Sprite**: 面板背景圖片
- **Panel Size**: 面板的寬度和高度（像素）
- **Panel Position**: 面板距離螢幕左下角的位置
- **Panel Scale**: 整體縮放比例

### 文字設置
- **Font**: 所有文字的字體（Mojangles）
- **Title Text**: 標題文字（"Upgrade Points: X"）的位置、大小、字體大小
- **Button Texts**: 三個按鈕文字的位置、大小、字體大小
  - Button 1: Move speed
  - Button 2: Bullet speed
  - Button 3: Fire rate

### 預設佈局
- **Default Layout**: 標準佈局（400x300 面板）
- **Centered Layout**: 置中佈局（450x350 面板）

## 文字位置設置說明

在 **[SetupUpgradePanelComplete.cs](Assets/Editor/SetupUpgradePanelComplete.cs)** 工具中：

1. **Title Text Position**: 標題相對於面板中心的位置
   - 預設：(0, 120) = 面板中心偏上

2. **Button Text Positions**: 按鈕文字相對於面板中心的位置
   - Button 1: (0, 40) = 稍微偏上
   - Button 2: (0, -20) = 中間偏下
   - Button 3: (0, -80) = 下方

所有位置都是相對於面板的中心點，可以直接在工具視窗中調整。

## 程式碼位置

升級 UI 的相關程式碼：
- UI 控制器：[Assets/Scripts/UI/UpgradeUI.cs](Assets/Scripts/UI/UpgradeUI.cs:14)
- 點數管理：[Assets/Scripts/Game/UpgradePointManager.cs](Assets/Scripts/Game/UpgradePointManager.cs)

## 疑難排解

### 如果圖片沒有顯示
1. 確認圖片已正確導入為 Sprite
2. 在 Unity Project 視窗中選擇 upgrade_points.png
3. 在 Inspector 中確認：
   - Texture Type = Sprite (2D and UI)
   - Sprite Mode = Single
4. 如果設定不正確，再次執行步驟 1

### 如果找不到圖片
- 確認檔案路徑：`Assets/UI/Images/upgrade_points.png`
- 在 Unity 中重新整理資源（Ctrl/Cmd + R）

## 注意事項

- 這些工具會直接修改場景文件
- 修改前會自動保存場景
- 建議在執行前先備份專案或使用版本控制
