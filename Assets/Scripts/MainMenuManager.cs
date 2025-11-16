using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using WheelUpgradeSystem;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button tankConfigButton;  // For wheel upgrade system
    [SerializeField] private Button quitButton;

    [Header("Tank Configuration System (Wheel)")]
    [SerializeField] private GameObject wheelUpgradePanel;
    [SerializeField] private TextMeshProUGUI currentTankConfigText;
    [SerializeField] private Button openUpgradeWheelButton;

    [Header("In-Game Upgrade Stats Display")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI upgradePointsText;
    [SerializeField] private TextMeshProUGUI tankStatsText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;

    // Wheel upgrade system (pre-game tank configuration)
    private TankUpgradeSystem wheelUpgradeSystem;
    private UpgradeWheelUI upgradeWheelUI;

    // In-game upgrade system (upgrade points)
    private TankStats playerTankStats;

    void Start()
    {
        SetupButtons();
        InitializeBothUpgradeSystems();
        UpdateAllDisplays();
    }

    void OnEnable()
    {
        // Subscribe to wheel upgrade events
        if (wheelUpgradeSystem != null)
        {
            // Note: You'll need to create this event in TankUpgradeSystem if it doesn't exist
            // TankUpgradeSystem.OnWheelUpgradeChanged += OnWheelConfigurationChanged;
        }

        // Subscribe to in-game upgrade events
        if (playerTankStats != null)
        {
            playerTankStats.OnUpgradePointsChanged += OnUpgradePointsChanged;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from events
        if (wheelUpgradeSystem != null)
        {
            // TankUpgradeSystem.OnWheelUpgradeChanged -= OnWheelConfigurationChanged;
        }

        if (playerTankStats != null)
        {
            playerTankStats.OnUpgradePointsChanged -= OnUpgradePointsChanged;
        }
    }

    private void SetupButtons()
    {
        // Main navigation buttons
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Tank configuration buttons
        if (tankConfigButton != null)
            tankConfigButton.onClick.AddListener(ToggleTankConfiguration);

        if (openUpgradeWheelButton != null)
            openUpgradeWheelButton.onClick.AddListener(OpenUpgradeWheel);
    }

    private void InitializeBothUpgradeSystems()
    {
        // Initialize wheel upgrade system (pre-game configuration)
        wheelUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();
        upgradeWheelUI = FindFirstObjectByType<UpgradeWheelUI>();

        // Try to find player tank stats (in-game upgrade system)
        // This might not exist in main menu, which is fine
        playerTankStats = FindFirstObjectByType<TankStats>();

        // Load saved wheel configuration
        LoadWheelConfiguration();

        Debug.Log($"Initialized upgrade systems:");
        Debug.Log($"  - Wheel System: {(wheelUpgradeSystem != null ? "✓" : "✗")}");
        Debug.Log($"  - Stats System: {(playerTankStats != null ? "✓" : "✗")}");
    }

    // ========== MAIN MENU ACTIONS ==========
    public void StartGame()
    {
        PlayButtonSound();

        // Save current wheel configuration before starting
        SaveWheelConfiguration();

        // Load the transition scene (which will apply tank configuration)
        Debug.Log("Starting game with current tank configuration...");
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        PlayButtonSound();

        Debug.Log("Quitting game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ========== WHEEL UPGRADE SYSTEM (PRE-GAME CONFIGURATION) ==========
    public void ToggleTankConfiguration()
    {
        PlayButtonSound();

        if (wheelUpgradePanel != null)
        {
            bool isActive = !wheelUpgradePanel.activeSelf;
            wheelUpgradePanel.SetActive(isActive);

            if (isActive)
                UpdateWheelConfigurationDisplay();
        }
        else
        {
            // If no panel, directly open wheel
            OpenUpgradeWheel();
        }
    }

    public void OpenUpgradeWheel()
    {
        PlayButtonSound();

        if (upgradeWheelUI != null)
        {
            upgradeWheelUI.ShowWheel();
        }
        else
        {
            Debug.LogWarning("Upgrade Wheel UI not found! Make sure the wheel system is in the scene.");
        }
    }

    public void CloseTankConfiguration()
    {
        PlayButtonSound();

        if (wheelUpgradePanel != null)
            wheelUpgradePanel.SetActive(false);
    }

    private void UpdateWheelConfigurationDisplay()
    {
        if (wheelUpgradeSystem == null) return;

        string currentConfig = wheelUpgradeSystem.GetCurrentUpgradePath();

        // Update tank configuration display
        if (currentTankConfigText != null)
        {
            currentTankConfigText.text = $"當前配置: {currentConfig}";
        }

        // You can add more detailed display here if needed
        Debug.Log($"Current wheel configuration: {currentConfig}");
    }

    private void OnWheelConfigurationChanged(string newConfiguration)
    {
        UpdateWheelConfigurationDisplay();
        SaveWheelConfiguration();
        Debug.Log($"Wheel configuration changed to: {newConfiguration}");
    }

    // ========== IN-GAME UPGRADE SYSTEM (STATS & POINTS) ==========
    private void UpdateStatsDisplay()
    {
        if (playerTankStats == null)
        {
            // Hide stats panel if no tank stats available (normal in main menu)
            if (statsPanel != null)
                statsPanel.SetActive(false);
            return;
        }

        // Show stats panel
        if (statsPanel != null)
            statsPanel.SetActive(true);

        // Update upgrade points display
        if (upgradePointsText != null)
        {
            int availablePoints = playerTankStats.GetAvailableUpgradePoints();
            upgradePointsText.text = $"升級點數: {availablePoints}";
        }

        // Update tank stats display
        if (tankStatsText != null)
        {
            string statsInfo = "";
            statsInfo += $"移動速度 Lv.{playerTankStats.GetMoveSpeedLevel()}\n";
            statsInfo += $"子彈速度 Lv.{playerTankStats.GetBulletSpeedLevel()}\n";
            statsInfo += $"射擊速度 Lv.{playerTankStats.GetFireRateLevel()}";

            tankStatsText.text = statsInfo;
        }
    }

    private void OnUpgradePointsChanged(int newPoints)
    {
        UpdateStatsDisplay();
        Debug.Log($"Upgrade points changed: {newPoints}");
    }

    // ========== SAVE/LOAD SYSTEM ==========
    private void SaveWheelConfiguration()
    {
        if (wheelUpgradeSystem != null)
        {
            string currentPath = wheelUpgradeSystem.GetCurrentUpgradePath();
            PlayerPrefs.SetString("WheelUpgradePath", currentPath);
            PlayerPrefs.Save();
            Debug.Log($"Saved wheel configuration: {currentPath}");
        }
    }

    private void LoadWheelConfiguration()
    {
        if (wheelUpgradeSystem != null)
        {
            string savedPath = PlayerPrefs.GetString("WheelUpgradePath", "Basic");
            wheelUpgradeSystem.ApplyUpgrade(savedPath);
            Debug.Log($"Loaded wheel configuration: {savedPath}");
        }
    }

    // ========== UTILITIES ==========
    private void UpdateAllDisplays()
    {
        UpdateWheelConfigurationDisplay();
        UpdateStatsDisplay();
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    // ========== DEBUG METHODS ==========
    [ContextMenu("Reset All Upgrades")]
    public void ResetAllUpgrades()
    {
        // Reset wheel configuration
        PlayerPrefs.DeleteKey("WheelUpgradePath");

        if (wheelUpgradeSystem != null)
        {
            wheelUpgradeSystem.ApplyUpgrade("Basic");
        }

        // Reset in-game stats (if available)
        if (playerTankStats != null)
        {
            // You'd need to add a reset method to TankStats
            Debug.Log("In-game stats reset would need to be implemented in TankStats");
        }

        PlayerPrefs.Save();
        UpdateAllDisplays();

        Debug.Log("All upgrades reset to default");
    }

    [ContextMenu("Test Both Systems")]
    public void TestBothSystems()
    {
        Debug.Log("=== TESTING BOTH UPGRADE SYSTEMS ===");

        // Test wheel system
        if (wheelUpgradeSystem != null)
        {
            Debug.Log($"Wheel System: {wheelUpgradeSystem.GetCurrentUpgradePath()}");
        }
        else
        {
            Debug.Log("Wheel System: NOT FOUND");
        }

        // Test stats system
        if (playerTankStats != null)
        {
            Debug.Log($"Stats System: {playerTankStats.GetAvailableUpgradePoints()} points available");
        }
        else
        {
            Debug.Log("Stats System: NOT FOUND (normal in main menu)");
        }
    }
}