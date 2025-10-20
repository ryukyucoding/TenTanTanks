# AI Tank System - 使用指南

## 概述
這個AI系統是從TanksRebirth專案中擷取並適配到Unity的智能坦克系統。它提供了完整的AI行為，包括路徑尋找、障礙物避開、目標追蹤和戰鬥邏輯。

## 文件結構
```
Assets/Scripts/AI/
├── AIParameters.cs          # AI參數配置
├── AIBehavior.cs            # AI行為系統
├── Pathfinding/
│   └── AStarPathfinder.cs   # A*路徑尋找算法
└── Enemy/
    └── EnemyTankAI.cs       # 新的AI坦克實現
```

## 主要功能

### 1. AIParameters (AI參數)
- **移動設置**: 隨機轉角、移動計時器、障礙物感知
- **戰鬥設置**: 射擊計時器、瞄準偏移、砲塔速度
- **地雷設置**: 地雷放置機率、感知範圍
- **感知設置**: 對友軍/敵軍的感知距離
- **高級設置**: 位置預測、智能反彈、子彈偏轉

### 2. AIBehavior (AI行為)
- 管理AI行為的計時器
- 提供行為狀態檢查
- 支援行為重置

### 3. AStarPathfinder (路徑尋找)
- 實現A*算法進行路徑規劃
- 支援8方向移動（包含對角線）
- 防止對角線穿越障礙物

### 4. EnemyTankAI (AI坦克)
- 完整的AI行為實現
- 智能移動和避障
- 目標追蹤和戰鬥邏輯
- 危險檢測和迴避

## 使用方法

### 步驟1: 替換現有腳本
1. 將現有的`EnemyTank`腳本從EnemyTank物件上移除
2. 添加`EnemyTankAI`腳本到EnemyTank物件上
3. 配置所有必要的組件引用（tankBody, turret, firePoint等）

### 步驟2: 配置AI參數
在Inspector中調整`AIParameters`：
- **Movement Settings**: 調整移動行為
- **Combat Settings**: 調整戰鬥行為
- **Mine Settings**: 調整地雷行為
- **Awareness Settings**: 調整感知範圍
- **Advanced Settings**: 調整高級功能

### 步驟3: 設置NavMesh（可選）
如果需要更精確的路徑尋找：
1. 確保場景中的地面和障礙物設置了Navigation Static
2. 使用NavMeshBaker腳本生成NavMesh
3. 在EnemyTankAI中啟用NavMesh路徑尋找

## 主要改進

### 相比原始EnemyTank的優勢：
1. **更智能的移動**: 基於TanksRebirth的移動系統
2. **障礙物避開**: 自動檢測和避開障礙物
3. **危險感知**: 檢測並避開子彈和其他危險
4. **目標預測**: 可選的位置預測功能
5. **可配置性**: 豐富的AI參數配置
6. **路徑尋找**: 支援A*路徑規劃

### 最小化變更：
- 保持原有的IDamageable介面
- 保持原有的組件結構
- 保持原有的GameManager整合
- 只需替換腳本，無需修改其他系統

## 調試功能
- 在Scene視圖中顯示檢測範圍
- 顯示射擊範圍
- 顯示巡邏範圍
- 顯示當前路徑（如果使用路徑尋找）

## 注意事項
1. 確保所有必要的組件都已正確引用
2. 調整AI參數以獲得最佳遊戲體驗
3. 可以為不同類型的敵人創建不同的AI參數配置
4. 系統支援多個AI坦克同時運行

## 擴展建議
1. 可以添加更多AI行為類型
2. 可以實現團隊協作AI
3. 可以添加學習和適應功能
4. 可以整合更複雜的路徑尋找算法
