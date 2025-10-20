# 關卡系統設置指南

## 修復完成！

所有編譯錯誤已經修復。現在請按照以下步驟完成關卡系統的設置：

## 步驟 1: 創建關卡配置文件

1. 在Unity編輯器中，點擊菜單 `Tools > Create Level Data`
2. 在彈出的窗口中，點擊以下按鈕創建示例關卡：
   - "創建示例關卡 1 - 新手訓練"
   - "創建示例關卡 2 - 挑戰開始" 
   - "創建示例關卡 3 - 生存挑戰"

這些關卡文件會自動保存在 `Assets/Scripts/LevelSystem/LevelConfigs/` 目錄下。

## 步驟 2: 在場景中設置組件

1. **創建LevelManager**：
   - 在Hierarchy中創建一個空的GameObject，命名為 "LevelManager"
   - 添加 `LevelManager` 腳本
   - 在Inspector中，將剛創建的關卡配置文件拖入 "Available Levels" 列表

2. **創建WaveManager**：
   - 在Hierarchy中創建一個空的GameObject，命名為 "WaveManager"
   - 添加 `WaveManager` 腳本
   - 在Inspector中，將敵人生成點拖入 "Default Spawn Points" 列表

3. **更新GameManager**：
   - 找到場景中現有的GameManager
   - 確保 `Use Level System` 選項已勾選
   - 如果需要，可以添加新的UI元素：
     - 創建Text組件用於顯示波數信息 (Wave Info Text)
     - 創建Text組件用於顯示關卡信息 (Level Info Text)

## 步驟 3: 設置敵人預製體

1. 確保你的敵人坦克預製體有正確的標籤 "Enemy"
2. 在關卡配置文件中，將敵人預製體拖入對應的 "Enemy Prefab" 欄位

## 步驟 4: 測試系統

1. 運行遊戲
2. 檢查Console是否有錯誤信息
3. 觀察敵人是否按照關卡配置正確生成
4. 測試波數切換和關卡完成條件

## 自定義關卡

創建新關卡：
1. 在Project窗口中右鍵點擊 `LevelConfigs` 文件夾
2. 選擇 `Create > Game > Level Data`
3. 在Inspector中配置關卡設定：
   - 關卡名稱和描述
   - 時間限制
   - 敵人波數配置
   - 完成條件
   - 獎勵設定

## 故障排除

如果遇到問題：
1. 檢查Console中的錯誤信息
2. 確保所有必要的組件都已正確設置
3. 確保敵人預製體有正確的標籤和腳本
4. 檢查關卡配置文件是否正確引用敵人預製體

## 系統特點

- ✅ 支持多個關卡
- ✅ 每個關卡支持多個敵人波數
- ✅ 每波可以有不同的敵人數量和屬性
- ✅ 敵人屬性可以調整（血量、速度、攻擊力等）
- ✅ 多種關卡完成條件
- ✅ 分數和經驗系統
- ✅ 事件系統供其他腳本使用
- ✅ 向後兼容舊系統

現在你的關卡系統已經準備就緒！可以開始創建你自己的關卡了。
