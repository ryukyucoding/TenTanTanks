# AI個性設定指南

## 如何在Unity中設定12種AI個性

### 步驟1：創建AI個性設定檔案

1. 在Unity編輯器中，右鍵點擊 `Assets/Scripts/AI/` 資料夾
2. 選擇 `Create > AI > AI Personality Setup`
3. 命名為 `AI_Personality_Setup`
4. 在Inspector中點擊 `Initialize All 12 AI Personalities` 按鈕

### 步驟2：12種AI個性詳細設定

#### 1. **Brown (棕色) - 基礎防禦型**
- **特點**: 高級瞄準但無預測射擊
- **設定**: 
  - 旋轉速度: 0.8
  - 瞄準角度: 180度
  - 高級瞄準: 開啟
  - 預測射擊: 關閉

#### 2. **Ash (灰色) - 巡邏型**
- **特點**: 有預測射擊和閃避能力
- **設定**:
  - 巡邏半徑: 250
  - 預測射擊: 開啟 (60%機率)
  - 閃避子彈: 開啟
  - 防禦時間: 2000ms

#### 3. **Marine (海軍) - 精準射擊型**
- **特點**: 完美瞄準但無預測
- **設定**:
  - 完美瞄準: 開啟
  - 瞄準角度: 1度 (極精準)
  - 預測射擊: 關閉
  - 防禦時間: 999999ms (幾乎不離開)

#### 4. **Yellow (黃色) - 隨機移動型**
- **特點**: 可射擊敵方子彈
- **設定**:
  - 隨機移動: 開啟
  - 射擊敵方子彈: 開啟
  - 地雷能力: 開啟
  - 巡邏半徑: 500 (更大範圍)

#### 5. **Pink (粉色) - 高級瞄準型**
- **特點**: 無閃避能力
- **設定**:
  - 高級瞄準: 開啟
  - 閃避子彈: 關閉
  - 預測射擊: 關閉
  - 防禦時間: 200ms

#### 6. **Green (綠色) - 綜合型**
- **特點**: 高級瞄準+預測射擊
- **設定**:
  - 高級瞄準: 開啟
  - 預測射擊: 開啟 (50%機率)
  - 射擊敵方子彈: 開啟
  - 連射冷卻: 150ms

#### 7. **Violet (紫色) - 地雷專家**
- **特點**: 有地雷能力
- **設定**:
  - 地雷機率: 1500
  - 高級瞄準: 開啟
  - 預測射擊: 開啟
  - 射擊敵方子彈: 開啟

#### 8. **White (白色) - 高級閃避型**
- **特點**: 高級閃避+連射
- **設定**:
  - 高級閃避: 開啟
  - 旋轉速度: 2.4 (很快)
  - 連射冷卻: 150ms
  - 射擊敵方子彈: 開啟

#### 9. **Black (黑色) - 最強型**
- **特點**: 完美瞄準+高級閃避+地雷
- **設定**:
  - 完美瞄準: 開啟
  - 高級閃避: 開啟
  - 旋轉速度: 2.0
  - 地雷機率: 1 (極高)
  - 射擊敵方子彈: 開啟

#### 10. **ZBlue (進階藍色)**
- **特點**: 高級閃避+連射
- **設定**:
  - 高級閃避: 開啟
  - 連射冷卻: 150ms
  - 旋轉速度: 2.4
  - 防禦時間: 800ms

#### 11. **ZBrown (進階棕色)**
- **特點**: 快速射擊
- **設定**:
  - 瞄準角度: 5度 (極精準)
  - 連射冷卻: 0ms (無冷卻)
  - 高級瞄準: 開啟
  - 旋轉速度: 0.8

#### 12. **ZAsh (進階灰色)**
- **特點**: 高級閃避+預測射擊
- **設定**:
  - 高級閃避: 開啟
  - 預測射擊: 開啟 (60%機率)
  - 高級瞄準: 開啟
  - 防禦時間: 50ms (極短)

### 步驟3：在遊戲中使用AI個性

```csharp
// 在AdvancedEnemyTank中設定AI個性
public class AdvancedEnemyTank : MonoBehaviour
{
    [SerializeField] private string aiPersonality = "brown"; // 選擇個性
    
    void Start()
    {
        // 載入AI配置
        AIPersonalitySetup setup = Resources.Load<AIPersonalitySetup>("AI/AI_Personality_Setup");
        AIConfig config = setup.GetPersonalityConfig(aiPersonality);
        
        // 應用配置到AI行為
        ApplyAIConfig(config);
    }
}
```

### 步驟4：自定義AI個性

如果你想創建新的AI個性：

```csharp
// 在AIPersonalitySetup中添加新個性
private AIConfig CreateCustomConfig()
{
    AIConfig config = new AIConfig();
    config.rotationSpeed = 1.5f;
    config.aimingAngle = 30f;
    config.advancedTargeting = true;
    config.predictiveTargeting = true;
    config.predictiveTargetingChance = 70f;
    // ... 其他設定
    return config;
}

// 添加到個性列表
AddPersonality("custom", CreateCustomConfig(), "自定義AI個性");
```

### 步驟5：調試AI行為

1. 在Scene視圖中選擇AI坦克
2. 在Inspector中查看AI狀態
3. 使用Gizmos查看AI行為範圍
4. 在Console中查看AI狀態變化

### 常用參數說明

- **rotationSpeed**: 砲塔旋轉速度
- **aimingAngle**: 瞄準角度誤差 (越小越精準)
- **patrolRadius**: 巡邏半徑
- **distLeavePatrol**: 離開巡邏的距離
- **canDodgeProj**: 能否閃避子彈
- **advancedTargeting**: 高級瞄準 (射線檢測)
- **predictiveTargeting**: 預測射擊
- **perfectAim**: 完美瞄準 (無誤差)
- **shootEnemyProjectiles**: 射擊敵方子彈
- **mineChance**: 放置地雷的機率

這樣你就可以在Unity中完整設定和使用這12種AI個性了！
