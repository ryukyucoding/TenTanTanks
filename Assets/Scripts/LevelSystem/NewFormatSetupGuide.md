# 新格式 LevelData 設置指南

## 🎯 目標
使用新的 `LevelDataNew` 格式來避免 Unity 6 的序列化問題。

## 📋 設置步驟

### 步驟 1：遷移現有數據（如果有）
1. 運行 `Tools > Migrate to New LevelData Format`
2. 這會創建 `Level1_Data_New.asset` 和 `Level2_Data_New.asset`
3. 運行 `Tools > Test New LevelData` 驗證遷移成功

### 步驟 2：在場景中設置

1. **找到 SimpleLevelController**
   - 在 Hierarchy 中找到包含 `SimpleLevelController` 的 GameObject
   - 通常是 `LevelManager` 或類似的對象

2. **添加 LevelControllerAdapter 組件**
   - 選中該 GameObject
   - 點擊 `Add Component`
   - 搜索並添加 `LevelControllerAdapter`

3. **配置 LevelControllerAdapter**
   - **New Level Data**: 
     - 設置 `Size` 為 2（如果有兩個關卡）
     - 將 `Level1_Data_New` 拖到 Element 0
     - 將 `Level2_Data_New` 拖到 Element 1
   
   - **Spawn Points**:
     - 從 Hierarchy 中拖拽生成點到這裡
     - 例如：SpawnPoint1, SpawnPoint2, SpawnPoint3
   
   - **Enemy Prefab**:
     - 從 Project 中拖拽敵人預製體
     - 例如：EnemyTank

4. **驗證設置**
   - 在 `LevelControllerAdapter` 組件上右鍵
   - 選擇 `顯示當前配置`
   - 查看 Console 確認配置正確

### 步驟 3：運行遊戲

1. 點擊 Play 按鈕
2. 檢查 Console 輸出，應該看到：
   ```
   === LevelControllerAdapter 初始化 ===
   ✅ 轉換關卡: 關卡 1 - 新手訓練
   ✅ 轉換關卡: 關卡 2 - 挑戰開始
   ✅ 已設置 2 個關卡到 SimpleLevelController
   ✅ 已設置 3 個生成點
   ✅ 已設置敵人預製體: EnemyTank
   ```

## 🆕 創建新關卡

1. **創建新的 LevelDataNew 資產**
   - 右鍵點擊 Project 面板
   - `Create > Game > Level Configuration`
   - 命名為 `Level3_Data_New`

2. **配置關卡數據**
   - **Config > Level Name**: 關卡名稱
   - **Config > Level Description**: 關卡描述
   - **Config > Waves**: 添加波數
     - 點擊 `+` 添加新波數
     - 設置敵人數量、延遲時間等
   - **Difficulty**: 設置難度（1-5）

3. **添加到場景**
   - 在 `LevelControllerAdapter` 的 `New Level Data` 列表中增加 Size
   - 拖拽新創建的資產到列表中

## 🔧 調試工具

### 顯示當前配置
- 在 `LevelControllerAdapter` 組件上右鍵
- 選擇 `顯示當前配置`
- 查看 Console 了解當前關卡配置

### 手動轉換和設置
- 在 `LevelControllerAdapter` 組件上右鍵
- 選擇 `轉換並設置關卡`
- 強制重新轉換和設置關卡數據

### 驗證新格式
- 運行 `Tools > Test New LevelData`
- 檢查所有 `LevelDataNew` 資產是否正常

## ⚠️ 注意事項

1. **不要刪除舊的 LevelData.cs**
   - 新格式需要它來轉換為舊格式
   - SimpleLevelController 仍然使用舊格式

2. **每次修改關卡數據後**
   - 不需要手動操作
   - `LevelControllerAdapter` 會在 Start 時自動轉換

3. **Enemy Prefab 和 Spawn Points**
   - 必須在 `LevelControllerAdapter` 中設置
   - 這些不會保存在 `.asset` 文件中

## 🐛 常見問題

### Q: 添加 LevelControllerAdapter 時出錯
A: 確保你添加的是 `LevelControllerAdapter` 而不是 `LevelDataAdapter`（後者不是 MonoBehaviour）

### Q: 遊戲運行時沒有敵人生成
A: 檢查：
1. Enemy Prefab 是否已設置
2. Spawn Points 是否已設置
3. 關卡數據中的 Waves 是否已配置

### Q: 關卡不會自動切換
A: 確保 `SimpleLevelController` 的 `Auto Load Next Level` 已勾選

### Q: 出現 "ExtensionOfNativeClass" 錯誤
A: 這是舊格式的問題，使用新格式就不會出現這個錯誤

## ✅ 檢查清單

- [ ] 已遷移舊數據到新格式
- [ ] 已添加 `LevelControllerAdapter` 組件
- [ ] 已設置 `New Level Data` 列表
- [ ] 已設置 `Spawn Points`
- [ ] 已設置 `Enemy Prefab`
- [ ] 已運行 `顯示當前配置` 驗證
- [ ] 已測試遊戲運行
- [ ] 沒有出現錯誤信息

## 📚 相關文件

- `LevelDataNew.cs` - 新的關卡數據格式
- `LevelControllerAdapter.cs` - 適配器組件（添加到場景中）
- `LevelDataAdapter.cs` - 數據包裝器（僅供內部使用）
- `MigrateToNewFormat.cs` - 遷移工具
- `SimpleLevelController.cs` - 原有的關卡控制器

## 🎉 完成！

設置完成後，您的關卡系統應該可以正常工作，不再出現 Unity 6 的序列化問題！


