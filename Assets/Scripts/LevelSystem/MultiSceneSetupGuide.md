# 多場景關卡系統設置指南

## 🎯 概述
這個系統讓您能夠為每個關卡使用不同的地圖場景，實現真正的多場景關卡切換。

## 📁 步驟1: 創建場景結構

### 建議的場景結構：
```
Scenes/
├── MainMenu.unity          # 主菜單
├── Level1_Desert.unity     # 關卡1 - 沙漠地圖
├── Level2_Forest.unity     # 關卡2 - 森林地圖
├── Level3_City.unity       # 關卡3 - 城市地圖
└── GameOver.unity          # 遊戲結束場景
```

## 🛠️ 步驟2: 設置每個關卡場景

### 對於每個關卡場景，您需要：

1. **創建新場景**：
   - File → New Scene
   - 保存為 `Level1_Desert.unity`, `Level2_Forest.unity` 等

2. **在場景中添加必要物件**：
   ```
   Level1_Desert/
   ├── Terrain/DesertTerrain
   ├── SpawnPoints (Empty GameObject)
   │   ├── SpawnPoint1
   │   ├── SpawnPoint2
   │   └── SpawnPoint3
   ├── PlayerSpawnPoint
   ├── SimpleLevelController (GameObject)
   └── Lighting/DesertLighting
   ```

3. **設置 SimpleLevelController**：
   - 添加 `SimpleLevelController` 腳本到場景中的 GameObject
   - 不需要設置關卡數據（會由 SceneLevelManager 自動設置）

## ⚙️ 步驟3: 設置 SceneLevelManager

### 在主場景或持久場景中：

1. **創建 SceneLevelManager GameObject**：
   ```
   SceneLevelManager (GameObject)
   └── SceneLevelManager (Script)
   ```

2. **配置關卡場景列表**：
   - 在 Inspector 中設置 `Level Scenes` 列表
   - 為每個關卡添加 `LevelSceneData`：

   **Level 1 設置**：
   - Level Name: "沙漠關卡"
   - Level Data Asset: 您的 Level1 關卡數據
   - Scene Name: "Level1_Desert"
   - Is Additive: false
   - Spawn Points: 拖拽場景中的生成點
   - Enemy Prefab: 敵人預製體

   **Level 2 設置**：
   - Level Name: "森林關卡"
   - Level Data Asset: 您的 Level2 關卡數據
   - Scene Name: "Level2_Forest"
   - Is Additive: false
   - Spawn Points: 拖拽場景中的生成點
   - Enemy Prefab: 敵人預製體

## 🎮 步驟4: 設置 Build Settings

1. **添加場景到 Build Settings**：
   - File → Build Settings
   - 將所有場景拖拽到 "Scenes In Build" 列表中
   - 確保順序正確

2. **設置場景索引**：
   ```
   0: MainMenu
   1: Level1_Desert
   2: Level2_Forest
   3: Level3_City
   4: GameOver
   ```

## 🔧 步驟5: 場景切換流程

### 工作流程：
1. **遊戲開始** → 載入 Level1_Desert 場景
2. **Level1 完成** → 自動切換到 Level2_Forest 場景
3. **Level2 完成** → 自動切換到 Level3_City 場景
4. **所有關卡完成** → 回到主菜單

## 🎨 步驟6: 自定義每個場景

### 為每個場景創建獨特的地圖：

**Level1_Desert (沙漠地圖)**：
- 使用沙漠地形
- 沙丘和岩石
- 沙漠風格的建築物
- 暖色調光照

**Level2_Forest (森林地圖)**：
- 使用森林地形
- 樹木和草叢
- 木屋建築
- 綠色調光照

**Level3_City (城市地圖)**：
- 使用城市地形
- 建築物和街道
- 現代化建築
- 城市夜景光照

## 🚀 步驟7: 測試和調試

### 測試功能：
1. **場景切換測試**：
   - 確保每個關卡能正確載入對應場景
   - 檢查生成點是否正確設置

2. **關卡完成測試**：
   - 完成 Level1 後是否自動跳轉到 Level2
   - 檢查場景切換是否流暢

3. **調試工具**：
   - 使用 SceneLevelManager 的 Context Menu
   - 檢查 Console 日誌輸出

## 📝 注意事項

1. **場景命名**：確保場景名稱與 SceneLevelManager 中的設置一致

2. **生成點設置**：每個場景都需要設置正確的生成點

3. **預製體引用**：確保敵人預製體在每個場景中都可用

4. **光照設置**：為每個場景設置合適的光照環境

5. **性能優化**：避免在場景中放置過多不必要的物件

## 🎯 完成後的優勢

- ✅ 每個關卡都有獨特的地圖環境
- ✅ 真正的場景切換體驗
- ✅ 靈活的關卡管理
- ✅ 易於擴展新關卡
- ✅ 支持不同的遊戲風格

現在您就可以為每個關卡創建完全不同的地圖場景了！
