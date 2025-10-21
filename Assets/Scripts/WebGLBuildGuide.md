# WebGL 構建修復指南

## 🔧 **WebGL 構建問題解決方案**

### **問題1: 序列化錯誤**
```
A scripted object has a different serialization layout when loading
```

**解決方法**：
1. 在場景中添加 `WebGLCompatibility` 腳本
2. 確保所有腳本都是最新版本
3. 清理並重新構建項目

### **問題2: 缺失腳本錯誤**
```
The referenced script on this Behaviour is missing!
```

**解決方法**：
1. 檢查所有 GameObject 的組件引用
2. 移除或修復損壞的組件引用
3. 使用 `WebGLCompatibility` 腳本自動修復

### **問題3: LevelManager 關卡配置問題**
```
LevelManager: 沒有可用的關卡!請確保關卡配置文件已正確設定。
```

**解決方法**：
1. 確保關卡配置文件在 Build Settings 中
2. 檢查 `AutoLevelSetup` 組件配置
3. 使用 `WebGLCompatibility` 腳本修復

### **問題4: 音頻加載問題**
```
Trying to get length of sound which is not loaded yet
```

**解決方法**：
1. 添加 `AudioListenerManager` 腳本
2. 確保只有一個 Audio Listener
3. 使用 WebGL 兼容的音頻設置

## 🛠️ **修復步驟**

### **步驟1: 添加兼容性腳本**
1. 在場景中創建空的 GameObject
2. 添加 `WebGLCompatibility` 腳本
3. 勾選 `Fix On Start` 和 `Show Debug Info`

### **步驟2: 修復關卡配置**
1. 確保 `AutoLevelSetup` 組件正確配置
2. 檢查關卡配置文件是否在 Build Settings 中
3. 運行 `WebGLCompatibility` 的 "手動修復" 功能

### **步驟3: 修復音頻問題**
1. 添加 `AudioListenerManager` 腳本
2. 運行 "修復音頻監聽器" 功能
3. 確保只有一個 Audio Listener

### **步驟4: 構建設置**
1. **Player Settings**：
   - Publishing Settings → Compression Format: Gzip
   - Publishing Settings → Data Caching: Enabled
   - Publishing Settings → Name Files As Hashes: Enabled

2. **Build Settings**：
   - 確保所有場景都在 Build Settings 中
   - 檢查所有腳本和資源都包含在構建中

3. **Quality Settings**：
   - 降低 WebGL 的質量設置以減少內存使用

## 🎯 **WebGL 最佳實踐**

### **性能優化**：
1. 減少 Draw Calls
2. 使用 Texture Atlases
3. 優化 Mesh 和材質
4. 限制同時播放的音頻數量

### **兼容性**：
1. 避免使用不兼容的 Unity 功能
2. 使用 WebGL 兼容的輸入系統
3. 處理瀏覽器的音頻限制

### **調試**：
1. 使用瀏覽器開發者工具
2. 檢查 Console 錯誤信息
3. 使用 `WebGLCompatibility` 腳本調試

## 🚀 **測試步驟**

1. **本地測試**：
   - 在 Unity 編輯器中測試
   - 使用 WebGL 預覽

2. **瀏覽器測試**：
   - 在不同瀏覽器中測試
   - 檢查 Console 錯誤
   - 測試音頻播放

3. **性能測試**：
   - 檢查內存使用
   - 測試加載時間
   - 檢查幀率

## 📝 **常見問題**

### **Q: 為什麼坦克炮管不能轉動？**
A: 可能是輸入系統在 WebGL 中的兼容性問題，檢查 `TankController` 的輸入處理。

### **Q: 為什麼關卡不能跳轉？**
A: 檢查 `SimpleLevelController` 的關卡配置和 `SceneLevelManager` 的設置。

### **Q: 為什麼有音頻問題？**
A: WebGL 對音頻有限制，確保使用 `AudioListenerManager` 修復音頻監聽器問題。

現在您的 WebGL 構建應該可以正常工作了！

