# 關卡系統使用說明

## 概述
這個關卡系統允許你創建多個關卡，每個關卡可以有多個敵人波數，每波可以有不同的敵人數量和屬性設定。

## 系統組件

### 1. LevelData.cs
- **EnemyWave**: 定義每一波的敵人設定
- **EnemyStatsModifier**: 敵人屬性調整器
- **LevelData**: 關卡基本數據
- **LevelDataAsset**: Unity ScriptableObject，用於創建關卡配置文件

### 2. WaveManager.cs
- 管理敵人波數的生成和完成
- 處理波數間的時間間隔
- 提供波數狀態查詢

### 3. LevelManager.cs
- 管理關卡的載入和切換
- 處理關卡完成條件檢查
- 管理分數和經驗系統

### 4. GameManager_New.cs
- 更新後的遊戲管理器
- 支持新的關卡系統
- 向後兼容舊系統

## 使用方法

### 創建新關卡

1. 在Unity編輯器中，右鍵點擊Project窗口
2. 選擇 `Create > Game > Level Data`
3. 設定關卡名稱和描述
4. 配置敵人波數：
   - 每波敵人數量
   - 敵人生成間隔
   - 波數間延遲
   - 敵人屬性調整

### 配置敵人波數

每個關卡可以有多個波數，每波可以設定：

- **enemyCount**: 敵人數量
- **enemyPrefab**: 敵人預製體
- **waveDelay**: 波數開始前的延遲時間
- **spawnInterval**: 敵人生成間隔
- **spawnPoints**: 敵人生成位置（可選）
- **statsModifier**: 敵人屬性調整

### 敵人屬性調整

可以調整敵人的：
- **healthMultiplier**: 血量倍數
- **speedMultiplier**: 移動速度倍數
- **damageMultiplier**: 攻擊力倍數
- **fireRateMultiplier**: 攻擊速度倍數

### 關卡完成條件

支援多種完成條件：
- **requireAllEnemiesDefeated**: 需要消滅所有敵人
- **requireSurviveTime**: 需要存活指定時間
- **timeLimit**: 關卡時間限制

## 示例關卡配置

### 關卡 1 - 新手訓練
- 第1波：1個敵人（基礎屬性）
- 第2波：2個敵人（基礎屬性）
- 完成條件：消滅所有敵人

### 關卡 2 - 挑戰開始
- 第1波：2個敵人（血量+20%）
- 第2波：3個敵人（血量+20%，速度+10%，攻擊+10%）
- 第3波：1個敵人（血量+50%，速度+20%，攻擊+30%，攻擊速度+20%）
- 時間限制：180秒

### 關卡 3 - 生存挑戰
- 第1波：1個敵人
- 第2波：2個敵人（屬性增強）
- 第3波：3個敵人（屬性大幅增強）
- 第4波：2個敵人（屬性極強）
- 完成條件：存活120秒

## 在場景中設置

1. 將 `LevelManager` 腳本添加到場景中的GameObject
2. 將 `WaveManager` 腳本添加到場景中的GameObject
3. 將 `GameManager_New` 腳本替換原有的GameManager
4. 在LevelManager中拖入關卡配置文件
5. 在WaveManager中設定默認生成點

## UI更新

新的GameManager支持額外的UI元素：
- **waveInfoText**: 顯示當前波數信息
- **levelInfoText**: 顯示關卡進度信息

## 事件系統

系統提供豐富的事件供其他腳本訂閱：
- 波數開始/完成事件
- 敵人生成/消滅事件
- 關卡開始/完成事件
- 分數/經驗變化事件

## 向後兼容

新的GameManager支持向後兼容：
- 可以通過 `useLevelSystem` 開關選擇使用新系統或舊系統
- 舊的敵人生成邏輯仍然保留
- 現有的UI元素繼續工作

## 擴展建議

1. 可以添加更多敵人類型
2. 可以實現關卡解鎖系統
3. 可以添加關卡評分系統
4. 可以實現關卡存檔系統
5. 可以添加關卡預覽功能
