# Unity AI系統使用說明

## 概述
這個AI系統是從Python坦克遊戲移植過來的完整AI解決方案，包含了複雜的行為樹、路徑尋找、閃避系統等。

## 主要組件

### 1. AIConfig.cs
- 定義AI配置參數
- 包含所有AI行為設定
- 支持JSON配置加載

### 2. AdvancedEnemyTank.cs
- 高級AI坦克控制器
- 實現所有AI行為狀態
- 支持多種AI個性

### 3. AStarPathfinding.cs
- A*路徑尋找算法
- 動態障礙物避開
- 支持多種路徑尋找需求

### 4. AIManager.cs
- AI管理器
- 統一管理所有AI坦克
- 提供調試功能

## 使用方法

### 1. 設置AI坦克
```csharp
// 在Inspector中設置
aiPersonality = "brown";  // AI個性
tankUnitType = "brown_tank";  // 坦克類型
```

### 2. 配置AI行為
```csharp
// 通過AIConfigDatabase配置
AIConfig config = AIConfigLoader.GetAIConfig("brown");
```

### 3. 添加路徑尋找
```csharp
// 在場景中添加AStarPathfinding組件
AStarPathfinding pathfinding = gameObject.AddComponent<AStarPathfinding>();
```

## AI行為狀態

### 1. Idle (待機)
- 坦克靜止不動
- 檢測到敵人時轉為其他狀態

### 2. Patrolling (巡邏)
- 在指定範圍內隨機移動
- 到達目標點後等待

### 3. Defending (防禦)
- 朝敵人移動但保持距離
- 準備進入攻擊狀態

### 4. Attacking (攻擊)
- 保持適當距離
- 持續瞄準和射擊

### 5. Dodge (閃避)
- 檢測到危險時自動閃避
- 計算最佳閃避路徑

### 6. Wander (遊蕩)
- 隨機移動
- 用於探索行為

## 配置參數

### 移動設定
- `rotationSpeed`: 旋轉速度
- `aimingAngle`: 瞄準角度誤差
- `rotationMultMax/Min`: 旋轉倍數

### 巡邏設定
- `patrolRadius`: 巡邏半徑
- `distLeavePatrol`: 離開巡邏距離
- `distLeaveDefend`: 離開防禦距離
- `distLeaveAttack`: 離開攻擊距離

### 閃避設定
- `canDodgeProj`: 能否閃避子彈
- `canDodgeMine`: 能否閃避地雷
- `distStartDodge`: 開始閃避距離
- `advancedDodge`: 高級閃避

### 瞄準設定
- `perfectAim`: 完美瞄準
- `advancedTargeting`: 高級瞄準
- `predictiveTargeting`: 預測射擊
- `shootEnemyProjectiles`: 射擊敵方子彈

## 調試功能

### 1. 視覺調試
- 在Scene視圖中顯示AI狀態
- 顯示檢測範圍和路徑
- 顯示當前目標

### 2. 控制台調試
- 顯示AI狀態變化
- 顯示路徑尋找信息
- 顯示碰撞檢測

### 3. 性能監控
- 監控AI更新頻率
- 監控路徑尋找性能
- 監控內存使用

## 擴展功能

### 1. 添加新AI個性
```csharp
// 在AIConfigDatabase中添加新個性
AIPersonalityData newPersonality = new AIPersonalityData();
newPersonality.name = "aggressive";
newPersonality.config = new AIConfig();
// 設置參數...
```

### 2. 自定義行為
```csharp
// 繼承AdvancedEnemyTank並重寫方法
public class CustomAITank : AdvancedEnemyTank
{
    protected override void HandleAttackState()
    {
        // 自定義攻擊行為
    }
}
```

### 3. 添加新狀態
```csharp
// 在AIBehaviorState枚舉中添加新狀態
public enum AIBehaviorState
{
    // 現有狀態...
    CustomState
}
```

## 性能優化

### 1. 更新頻率控制
- 使用AIManager控制更新頻率
- 避免每幀更新所有AI

### 2. 路徑尋找優化
- 使用對象池管理路徑節點
- 緩存常用路徑

### 3. 視覺檢測優化
- 使用LOD系統
- 距離剔除

## 常見問題

### 1. AI不移動
- 檢查路徑尋找設置
- 確認障礙物Layer設置
- 檢查AI狀態

### 2. 性能問題
- 降低AI更新頻率
- 優化路徑尋找算法
- 使用對象池

### 3. 閃避不工作
- 檢查閃避參數設置
- 確認子彈檢測
- 檢查路徑尋找

## 更新日誌

### v1.0.0
- 初始版本
- 基本AI行為
- 路徑尋找系統
- 閃避系統

### 計劃功能
- 多層次AI決策
- 學習系統
- 動態難度調整
- 更多AI個性
