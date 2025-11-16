using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// 升級 UI 管理器
/// 顯示在左下角，可用滑鼠點擊或鍵盤數字鍵升級
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI upgradePointsText;

    [Header("Upgrade Buttons")]
    [SerializeField] private UpgradeButton moveSpeedButton;
    [SerializeField] private UpgradeButton bulletSpeedButton;
    [SerializeField] private UpgradeButton fireRateButton;

    [Header("Settings")]
    [SerializeField] private Color canUpgradeColor = new Color(0.2f, 0.8f, 0.3f, 1f);
    [SerializeField] private Color cannotUpgradeColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color maxLevelColor = new Color(1f, 0.8f, 0.2f, 1f);

    private TankStats tankStats;

    void Start()
    {
        Debug.Log("[UpgradeUI] 开始初始化...");
        
        // 延迟查找玩家坦克，因为可能还没生成
        StartCoroutine(InitializeDelayed());
    }
    
    private System.Collections.IEnumerator InitializeDelayed()
    {
        // 等待 0.5 秒让 GameManager 先生成玩家
        yield return new WaitForSeconds(0.5f);
        
        FindPlayerTank();
        
        if (tankStats == null)
        {
            Debug.LogError("[UpgradeUI] ❌ 找不到 TankStats 组件！");
            yield break;
        }

        Debug.Log($"[UpgradeUI] ✓ 成功找到 TankStats");
        Debug.Log($"  - 物件名称: {tankStats.gameObject.name}");
        Debug.Log($"  - InstanceID: {tankStats.GetInstanceID()}");

        // 订阅事件
        tankStats.OnUpgradePointsChanged += UpdateUI;
        tankStats.OnStatUpgraded += OnStatUpgraded;
        
        Debug.Log($"[UpgradeUI] 已订阅 TankStats 事件 (InstanceID: {tankStats.GetInstanceID()})");
        Debug.Log($"[UpgradeUI] upgradePanel = {(upgradePanel != null ? "✓ 已设置" : "❌ null")}");

        // 设置按钮点击事件
        SetupButtons();

        // 初始化 UI
        UpdateUI(0);
    }
    
    private void FindPlayerTank()
    {
        // 方法 1: 从 GameManager 获取（最可靠）
        GameObject playerObj = GameManager.GetPlayerTank();
        if (playerObj != null)
        {
            tankStats = playerObj.GetComponent<TankStats>();
            if (tankStats != null)
            {
                Debug.Log($"[UpgradeUI] ✓ 从 GameManager 获取玩家: {playerObj.name}");
                Debug.Log($"  - InstanceID: {tankStats.GetInstanceID()}");
                Debug.Log($"  - Active: {playerObj.activeInHierarchy}");
                return;
            }
        }
        
        Debug.LogWarning("[UpgradeUI] GameManager 未返回玩家，尝试 Player Tag...");
        
        // 方法 2: 使用 Player Tag 查找
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            tankStats = playerObj.GetComponent<TankStats>();
            if (tankStats != null)
            {
                Debug.Log($"[UpgradeUI] ✓ 通过 Tag 找到玩家: {playerObj.name}");
                Debug.Log($"  - InstanceID: {tankStats.GetInstanceID()}");
                return;
            }
        }
        
        Debug.LogWarning("[UpgradeUI] 未找到 Player Tag，尝试备用方法...");
        
        // 方法 3: 查找活动且名字包含 Clone 的
        TankStats[] allStats = FindObjectsByType<TankStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"[UpgradeUI] 找到 {allStats.Length} 个活动的 TankStats");
        
        foreach (var stats in allStats)
        {
            Debug.Log($"  - {stats.gameObject.name} (InstanceID: {stats.GetInstanceID()}, Active: {stats.gameObject.activeInHierarchy})");
            
            if (stats.gameObject.name.Contains("Clone") && stats.gameObject.activeInHierarchy)
            {
                tankStats = stats;
                Debug.Log($"[UpgradeUI] ✓ 找到玩家 Clone: {stats.gameObject.name}");
                return;
            }
        }
        
        Debug.LogError("[UpgradeUI] ❌ 未找到任何有效的玩家 TankStats");
    }
    
    private void SetupButtons()
    {
        if (moveSpeedButton != null)
        {
            moveSpeedButton.button.onClick.AddListener(() => OnUpgradeButtonClicked(TankStats.StatType.MoveSpeed));
            moveSpeedButton.SetStatName("Move speed");
            moveSpeedButton.SetHotkey("1");
        }

        if (bulletSpeedButton != null)
        {
            bulletSpeedButton.button.onClick.AddListener(() => OnUpgradeButtonClicked(TankStats.StatType.BulletSpeed));
            bulletSpeedButton.SetStatName("Bullet speed");
            bulletSpeedButton.SetHotkey("2");
        }

        if (fireRateButton != null)
        {
            fireRateButton.button.onClick.AddListener(() => OnUpgradeButtonClicked(TankStats.StatType.FireRate));
            fireRateButton.SetStatName("Fire rate");
            fireRateButton.SetHotkey("3");
        }
    }

    void Update()
    {
        if (tankStats == null) return;

        // 鍵盤快捷鍵
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                OnUpgradeButtonClicked(TankStats.StatType.MoveSpeed);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                OnUpgradeButtonClicked(TankStats.StatType.BulletSpeed);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                OnUpgradeButtonClicked(TankStats.StatType.FireRate);
            }
        }
    }

    private void OnUpgradeButtonClicked(TankStats.StatType statType)
    {
        Debug.Log($"[UpgradeUI] 嘗試升級 {statType}");
        Debug.Log($"   升級前點數: {tankStats.GetAvailableUpgradePoints()}");
        
        if (tankStats.TryUpgradeStat(statType))
        {
            Debug.Log($"   ✅ 升級成功！");
            Debug.Log($"   升級後點數: {tankStats.GetAvailableUpgradePoints()}");
            
            // 升級成功，播放音效（可選）
            PlayUpgradeSound();
        }
        else
        {
            Debug.Log($"   ❌ 升級失敗（點數不足或已達最高等級）");
        }
    }

    private void UpdateUI(int availablePoints)
    {
        Debug.Log($"[UpgradeUI] UpdateUI 被調用！升級點數: {availablePoints}");
        
        // 更新升級點數顯示
        if (upgradePointsText != null)
        {
            upgradePointsText.text = $"Upgrade Points: {availablePoints}";
        }

        // 更新各個按鈕狀態
        UpdateButton(moveSpeedButton, TankStats.StatType.MoveSpeed);
        UpdateButton(bulletSpeedButton, TankStats.StatType.BulletSpeed);
        UpdateButton(fireRateButton, TankStats.StatType.FireRate);

        // 顯示/隱藏面板
        if (upgradePanel != null)
        {
            bool shouldShow = availablePoints > 0;
            upgradePanel.SetActive(shouldShow);
            Debug.Log($"[UpgradeUI] upgradePanel.SetActive({shouldShow})");
        }
        else
        {
            Debug.LogError("[UpgradeUI] ❌ upgradePanel 是 null，無法顯示UI！");
        }
    }

    private void UpdateButton(UpgradeButton upgradeButton, TankStats.StatType statType)
    {
        if (upgradeButton == null || tankStats == null) return;

        int currentLevel = 0;
        int maxLevel = 0;
        float currentValue = 0;
        float nextValue = 0;
        string statName = "";
        string hotkey = "";

        switch (statType)
        {
            case TankStats.StatType.MoveSpeed:
                currentLevel = tankStats.GetMoveSpeedLevel();
                maxLevel = tankStats.GetMaxMoveSpeedLevel();
                currentValue = tankStats.GetCurrentMoveSpeed();
                nextValue = currentValue + 0.3f;
                statName = "Move speed";
                hotkey = "1";
                break;
            case TankStats.StatType.BulletSpeed:
                currentLevel = tankStats.GetBulletSpeedLevel();
                maxLevel = tankStats.GetMaxBulletSpeedLevel();
                currentValue = tankStats.GetCurrentBulletSpeed();
                nextValue = currentValue + 0.5f;
                statName = "Bullet speed";
                hotkey = "2";
                break;
            case TankStats.StatType.FireRate:
                currentLevel = tankStats.GetFireRateLevel();
                maxLevel = tankStats.GetMaxFireRateLevel();
                currentValue = tankStats.GetCurrentFireRate();
                nextValue = currentValue + 0.3f;
                statName = "Fire rate";
                hotkey = "3";
                break;
        }

        bool canUpgrade = tankStats.CanUpgrade(statType);
        bool isMaxLevel = currentLevel >= maxLevel;

        // 檢查是否為簡化模式（只有 statNameText）
        bool isSimpleMode = upgradeButton.statNameText != null && 
                           upgradeButton.levelText == null && 
                           upgradeButton.valueText == null;

        if (isSimpleMode)
        {
            // 簡化模式：所有資訊顯示在一個文字中
            string levelInfo = $"Lv.{currentLevel}/{maxLevel}";
            
            upgradeButton.statNameText.text = $" [{hotkey}] {statName} {levelInfo}";
        }
        else
        {
            // 完整模式：分別更新各個文字
            upgradeButton.SetStatName(statName);
            upgradeButton.SetLevel(currentLevel, maxLevel);
            upgradeButton.SetValue(currentValue, nextValue);
            upgradeButton.SetHotkey(hotkey);
        }

        // 更新按鈕顏色和可用性
        if (isMaxLevel)
        {
            upgradeButton.SetColor(maxLevelColor);
            upgradeButton.SetInteractable(false);
        }
        else if (canUpgrade)
        {
            upgradeButton.SetColor(canUpgradeColor);
            upgradeButton.SetInteractable(true);
        }
        else
        {
            upgradeButton.SetColor(cannotUpgradeColor);
            upgradeButton.SetInteractable(false);
        }
    }

    private void OnStatUpgraded(TankStats.StatType statType, int newLevel, int maxLevel)
    {
        // 屬性升級時的回調
        UpdateUI(tankStats.GetAvailableUpgradePoints());
    }

    private void PlayUpgradeSound()
    {
        // 這裡可以加入升級音效
    }

    void OnDestroy()
    {
        // 取消訂閱事件
        if (tankStats != null)
        {
            tankStats.OnUpgradePointsChanged -= UpdateUI;
            tankStats.OnStatUpgraded -= OnStatUpgraded;
        }
    }

    // 測試用：給予升級點數
    [ContextMenu("Add 3 Upgrade Points")]
    public void TestAddPoints()
    {
        if (tankStats != null)
        {
            tankStats.AddUpgradePoints(3);
        }
    }
}

/// <summary>
/// 單個升級按鈕的資料結構
/// </summary>
[System.Serializable]
public class UpgradeButton
{
    public Button button;
    public Image buttonImage;
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI hotkeyText;

    public void SetStatName(string name)
    {
        if (statNameText != null)
            statNameText.text = name;
    }

    public void SetLevel(int current, int max)
    {
        if (levelText != null)
            levelText.text = $"Lv.{current}/{max}";
    }

    public void SetValue(float current, float next)
    {
        if (valueText != null)
        {
            if (current < next)
                valueText.text = $"{current:F1} → {next:F1}";
            else
                valueText.text = $"{current:F1} (MAX)";
        }
    }

    public void SetHotkey(string key)
    {
        if (hotkeyText != null)
            hotkeyText.text = $"[{key}]";
    }

    public void SetColor(Color color)
    {
        if (buttonImage != null)
            buttonImage.color = color;
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }
}
