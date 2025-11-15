using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button quitButton;

    [Header("Upgrade System (Optional)")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI currentTankNameText;
    [SerializeField] private TextMeshProUGUI currentTankStatsText;
    [SerializeField] private Button openUpgradeWheelButton;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;

    // 升級系統組件（會自動查找）
    private UpgradeWheelUI upgradeWheel;
    private TankUpgradeSystem upgradeSystem;

    void Start()
    {
        SetupButtons();
        InitializeUpgradeSystem();
        UpdateTankDisplay();
    }

    void OnEnable()
    {
        // 訂閱升級系統事件（如果存在）
        if (upgradeSystem != null)
        {
            TankUpgradeSystem.OnTankUpgraded += OnTankUpgraded;
        }
    }

    void OnDisable()
    {
        // 取消訂閱事件
        if (upgradeSystem != null)
        {
            TankUpgradeSystem.OnTankUpgraded -= OnTankUpgraded;
        }
    }

    private void SetupButtons()
    {
        // 原有按鈕
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // 新增按鈕
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(ToggleUpgradePanel);

        if (openUpgradeWheelButton != null)
            openUpgradeWheelButton.onClick.AddListener(OpenUpgradeWheel);
    }

    private void InitializeUpgradeSystem()
    {
        // 自動查找升級系統組件
        upgradeSystem = FindObjectOfType<TankUpgradeSystem>();
        upgradeWheel = FindObjectOfType<UpgradeWheelUI>();

        // 如果找到升級系統，載入保存的配置
        if (upgradeSystem != null)
        {
            LoadUpgradeState();
            Debug.Log("升級系統已初始化");
        }

        // 根據是否有升級系統來顯示/隱藏相關 UI
        UpdateUpgradeUI();
    }

    private void UpdateUpgradeUI()
    {
        bool hasUpgradeSystem = (upgradeSystem != null || upgradeWheel != null);

        // 顯示/隱藏升級相關 UI
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(hasUpgradeSystem);

        if (upgradePanel != null && !hasUpgradeSystem)
            upgradePanel.SetActive(false);
    }

    // ========== 原有功能 ==========
    public void StartGame()
    {
        PlayButtonSound();

        // 保存當前升級狀態
        SaveUpgradeState();

        // 載入遊戲場景
        Debug.Log("StartGame 被調用了！");
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        PlayButtonSound();

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ========== 新增升級功能 ==========
    public void ToggleUpgradePanel()
    {
        PlayButtonSound();

        if (upgradePanel != null)
        {
            bool isActive = !upgradePanel.activeSelf;
            upgradePanel.SetActive(isActive);

            if (isActive)
                UpdateTankDisplay();
        }
        else
        {
            // 如果沒有升級面板，直接開啟升級輪盤
            OpenUpgradeWheel();
        }
    }

    public void OpenUpgradeWheel()
    {
        PlayButtonSound();

        if (upgradeWheel != null)
        {
            upgradeWheel.ShowWheel();
        }
        else
        {
            Debug.LogWarning("UpgradeWheelUI 未找到！請確認場景中有升級系統。");
        }
    }

    public void CloseUpgradePanel()
    {
        PlayButtonSound();

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    private void OnTankUpgraded(TankStats newStats)
    {
        UpdateTankDisplay();
        SaveUpgradeState();
        Debug.Log("坦克配置已更新！");
    }

    private void UpdateTankDisplay()
    {
        if (upgradeSystem == null) return;

        string currentPath = upgradeSystem.GetCurrentUpgradePath();
        TankStats currentStats = upgradeSystem.GetCurrentStats();

        // 更新坦克名稱顯示
        if (currentTankNameText != null)
        {
            currentTankNameText.text = $"當前配置: {currentPath}";
        }

        // 更新屬性顯示
        if (currentTankStatsText != null && currentStats != null)
        {
            string statsText = "";
            statsText += $"傷害: <color=yellow>{currentStats.damage}</color>\n";
            statsText += $"射速: <color=cyan>{currentStats.fireRate}/秒</color>\n";
            statsText += $"子彈大小: <color=orange>{currentStats.bulletSize}</color>\n";
            statsText += $"移動速度: <color=green>{currentStats.moveSpeed}</color>\n";
            statsText += $"血量: <color=red>{currentStats.maxHealth}</color>";

            currentTankStatsText.text = statsText;
        }
    }

    private void SaveUpgradeState()
    {
        if (upgradeSystem != null)
        {
            string currentPath = upgradeSystem.GetCurrentUpgradePath();
            PlayerPrefs.SetString("CurrentUpgradePath", currentPath);
            PlayerPrefs.Save();
        }
    }

    private void LoadUpgradeState()
    {
        if (upgradeSystem != null)
        {
            string savedPath = PlayerPrefs.GetString("CurrentUpgradePath", "Basic");
            upgradeSystem.ApplyUpgrade(savedPath);
        }
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    // ========== Debug 功能 ==========
    [ContextMenu("重置升級")]
    public void ResetUpgrades()
    {
        PlayerPrefs.DeleteKey("CurrentUpgradePath");
        PlayerPrefs.Save();

        if (upgradeSystem != null)
        {
            upgradeSystem.ApplyUpgrade("Basic");
            UpdateTankDisplay();
        }

        Debug.Log("升級已重置為基礎配置");
    }

    [ContextMenu("測試升級系統")]
    public void TestUpgradeSystem()
    {
        if (upgradeSystem != null)
        {
            Debug.Log($"升級系統狀態: {upgradeSystem.GetCurrentUpgradePath()}");

            // 測試一些升級
            string[] testUpgrades = { "Heavy", "Rapid", "Balanced" };
            foreach (string upgrade in testUpgrades)
            {
                upgradeSystem.ApplyUpgrade(upgrade);
                Debug.Log($"測試升級: {upgrade}");
            }

            // 恢復基礎配置
            upgradeSystem.ApplyUpgrade("Basic");
            UpdateTankDisplay();
        }
        else
        {
            Debug.LogWarning("沒有找到升級系統！");
        }
    }

    // ========== 公開方法供 UI 調用 ==========
    public void OnUpgradeButtonHover()
    {
        // 可以在這裡添加按鈕懸停效果
        Debug.Log("升級按鈕懸停");
    }
}