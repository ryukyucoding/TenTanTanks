using UnityEngine;
using UnityEngine.SceneManagement;

// **************************
// LINE 183 IS TESTING
// **************************
/// <summary>
/// Enhanced Transition Mover - Standalone version that works with TransitionWheelUpgrade
/// Can work independently or with upgrade system
/// Fully compatible with existing TransitionMover functionality
/// </summary>
public class EnhancedTransitionMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float targetX = 12f;
    [SerializeField] private string nextScene = "Level1";

    [Header("Upgrade Trigger Settings")]
    [SerializeField] private float upgradePositionX = -5f; // X position to trigger upgrade
    [SerializeField] private float upgradeDetectionRange = 10f; // Detection range
    [SerializeField] private bool enableUpgrades = true; // Whether to enable upgrade functionality
    [SerializeField] private bool pauseForUpgrade = true; // Whether to pause for upgrade

    [Header("Scenes Requiring Upgrades")]
    [SerializeField] private string[] upgradeScenes = { "Level2", "Level4" }; // Target scenes that need upgrades

    [Header("Transition Upgrade Testing")]
    [SerializeField] private TransitionWheelUpgrade transitionUpgrade; // Optional transition upgrade component

    private bool hasTriggeredUpgrade = false;
    private bool isUpgradeInProgress = false;
    private bool canMove = true;

    void Start()
    {
        InitializeTransition();
        FindUpgradeComponents();
    }

    void Update()
    {
        if (!canMove) return;

        Vector3 pos = transform.position;

        // Debug: 每秒输出一次位置信息
        if (Time.frameCount % 60 == 0 && enableUpgrades)
        {
            Debug.Log($"[EnhancedTransitionMover] Current X: {pos.x:F2}, Target X: {targetX:F2}, Upgrade X: {upgradePositionX:F2}, Distance: {Mathf.Abs(pos.x - upgradePositionX):F2}, Scene: {nextScene}, Is Upgrade Scene: {IsUpgradeScene(nextScene)}");
        }

        // Check if need to trigger upgrade
        if (!hasTriggeredUpgrade && enableUpgrades && ShouldTriggerUpgrade(pos.x))
        {
            Debug.Log($"[EnhancedTransitionMover] 🎯 TRIGGER DETECTED at X={pos.x:F2}");
            TriggerUpgrade();
            return;
        }

        // Normal movement logic (same as original TransitionMover)
        if (pos.x < targetX)
        {
            float step = speed * Time.deltaTime;
            pos.x = Mathf.MoveTowards(pos.x, targetX, step);
            transform.position = pos;
        }
        else
        {
            LoadNext();
        }
    }

    /// <summary>
    /// Initialize transition settings (same as original TransitionMover)
    /// </summary>
    private void InitializeTransition()
    {
        // Prioritize scene set by SceneTransitionManager
        string transitionScene = SceneTransitionManager.GetNextSceneName();
        if (!string.IsNullOrEmpty(transitionScene))
        {
            nextScene = transitionScene;
            Debug.Log("[EnhancedTransitionMover] Using scene set by SceneTransitionManager: " + nextScene);
        }
        else
        {
            Debug.Log("[EnhancedTransitionMover] Using default scene: " + nextScene);
        }

        // Check PlayerDataManager
        CheckPlayerDataManager();

        // Apply health bonus for specific level transitions (same as original)
        ApplyHealthBonus(nextScene);
    }

    /// <summary>
    /// Find upgrade components
    /// </summary>
    private void FindUpgradeComponents()
    {
        // Try to find transition wheel upgrade component first
        if (transitionUpgrade == null)
            transitionUpgrade = FindFirstObjectByType<TransitionWheelUpgrade>();

        if (transitionUpgrade != null)
        {
            Debug.Log("[EnhancedTransitionMover] Found TransitionWheelUpgrade, upgrade functionality enabled");
        }
        else if (enableUpgrades)
        {
            Debug.LogWarning("[EnhancedTransitionMover] No upgrade components found, but upgrades are enabled");
        }
    }

    /// <summary>
    /// Check if should trigger upgrade
    /// </summary>
    private bool ShouldTriggerUpgrade(float currentX)
    {
        // Don't trigger if upgrade functionality is disabled
        if (!enableUpgrades) return false;

        // Don't trigger if target scene is not in upgrade scene list
        if (!IsUpgradeScene(nextScene)) return false;

        // Check if reached upgrade trigger position
        float distanceToUpgradePoint = Mathf.Abs(currentX - upgradePositionX);
        return distanceToUpgradePoint <= upgradeDetectionRange;
    }

    /// <summary>
    /// Check if scene requires upgrade
    /// </summary>
    private bool IsUpgradeScene(string sceneName)
    {
        foreach (string upgradeScene in upgradeScenes)
        {
            if (upgradeScene == sceneName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Trigger upgrade process
    /// </summary>
    private void TriggerUpgrade()
    {
        hasTriggeredUpgrade = true;
        isUpgradeInProgress = true;

        if (pauseForUpgrade)
            canMove = false;

        Debug.Log("[EnhancedTransitionMover] Triggered upgrade at position " + transform.position.x + ", target scene: " + nextScene);

        // Try different upgrade systems
        if (TryTriggerTransitionUpgrade())
        {
            Debug.Log("[EnhancedTransitionMover] Using TransitionWheelUpgrade");
        }
        else if (TryTriggerAdvancedUpgrade())
        {
            Debug.Log("[EnhancedTransitionMover] Using advanced upgrade system");
        }
        else
        {
            Debug.LogWarning("[EnhancedTransitionMover] No upgrade system available, continuing without upgrade");
            ResumeMovement();
        }
    }

    /// <summary>
    /// Try to trigger transition wheel upgrade system
    /// </summary>
    private bool TryTriggerTransitionUpgrade()
    {
        if (transitionUpgrade != null)
        {
            string transitionType = "Level 1 to Level 2"; // default

            if (nextScene == "Level2")
            {
                // Tier 1 upgrade (Basic → Heavy/Rapid/Balanced)
                transitionType = "Level 1 to Level 2";
                Debug.Log("[EnhancedTransitionMover] Level1→Level2: Showing Tier 1 options (Heavy/Rapid/Balanced)");
                
                // Tier 1 wheel show
                transitionUpgrade.ShowUpgradePanel(transitionType);
                return true;
            }
            else if (nextScene == "Level4")
            {
                // Tier 2 upgrade
                transitionType = "Level 3 to Level 4";
                Debug.Log("[EnhancedTransitionMover] Level3→Level4: Checking for Tier 2 options");

                // ✅ 獲取保存的 Tier 1 選擇
                string savedTier1 = "";
                if (PlayerDataManager.Instance != null)
                {
                    savedTier1 = PlayerDataManager.Instance.GetCurrentTankTransformation();
                    Debug.Log($"[EnhancedTransitionMover] Saved Tier 1 transformation: {savedTier1}");
                }
                else
                {
                    Debug.LogError("[EnhancedTransitionMover] ❌ PlayerDataManager not found!");
                }

                // ✅ 如果有 Tier 1 變形，顯示對應的 Tier 2 選項
                if (!string.IsNullOrEmpty(savedTier1) && savedTier1.ToLower() != "basic")
                {
                    Debug.Log($"[EnhancedTransitionMover] ✅ Showing Tier 2 options for parent: {savedTier1}");

                    // ★★★ 修復：使用 FindObjectsOfType 替代 FindFirstObjectByType ★★★
                    Debug.Log("[EnhancedTransitionMover] Searching for UpgradeWheelUI...");
                    UpgradeWheelUI[] allUpgradeWheels = FindObjectsOfType<UpgradeWheelUI>(true);
                    Debug.Log($"[EnhancedTransitionMover] Found {allUpgradeWheels.Length} UpgradeWheelUI objects");

                    UpgradeWheelUI upgradeWheelUI = null;
                    if (allUpgradeWheels.Length > 0)
                    {
                        upgradeWheelUI = allUpgradeWheels[0];
                        Debug.Log($"[EnhancedTransitionMover] Using UpgradeWheelUI: {upgradeWheelUI.name}");
                        Debug.Log($"[EnhancedTransitionMover] UpgradeWheelUI GameObject active: {upgradeWheelUI.gameObject.activeInHierarchy}");
                        Debug.Log($"[EnhancedTransitionMover] UpgradeWheelUI Component enabled: {upgradeWheelUI.enabled}");
                    }

                    if (upgradeWheelUI != null)
                    {
                        Debug.Log($"[EnhancedTransitionMover] ✅ Found UpgradeWheelUI, setting to Tier 2 mode");
                        Debug.Log($"[EnhancedTransitionMover] Parent upgrade: {savedTier1}");

                        // 設置為 Tier 2 模式
                        upgradeWheelUI.SetTransitionMode(2, savedTier1);
                        Debug.Log("[EnhancedTransitionMover] ✅ SetTransitionMode called successfully");
                    }
                    else
                    {
                        Debug.LogError("[EnhancedTransitionMover] ❌ No UpgradeWheelUI available!");
                    }

                    // 顯示輪盤
                    transitionUpgrade.ShowUpgradePanel(transitionType);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[EnhancedTransitionMover] ⚠️ No valid Tier 1 transformation found (got: {savedTier1}), skipping Tier 2 upgrade");
                    ResumeMovement();
                    return false;
                }
            }
            else
            {
                Debug.Log($"[EnhancedTransitionMover] Scene {nextScene} doesn't require upgrade");
                return false;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Try to trigger advanced upgrade system
    /// </summary>
    private bool TryTriggerAdvancedUpgrade()
    {
        // Try to find TransitionUpgradeManager
        var upgradeManager = FindFirstObjectByType<MonoBehaviour>();
        if (upgradeManager != null && upgradeManager.GetType().Name.Contains("TransitionUpgradeManager"))
        {
            // Use reflection to call TriggerTransitionUpgrade if it exists
            var method = upgradeManager.GetType().GetMethod("TriggerTransitionUpgrade");
            if (method != null)
            {
                method.Invoke(upgradeManager, null);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Resume movement (called by upgrade systems)
    /// </summary>
    public void ResumeMovement()
    {
        isUpgradeInProgress = false;
        canMove = true;

        Debug.Log("[EnhancedTransitionMover] Resuming movement");
    }

    /// <summary>
    /// Pause movement
    /// </summary>
    public void PauseMovement()
    {
        canMove = false;
        Debug.Log("[EnhancedTransitionMover] Pausing movement");
    }

    /// <summary>
    /// Force skip upgrade and continue movement
    /// </summary>
    public void SkipUpgradeAndContinue()
    {
        hasTriggeredUpgrade = true;
        ResumeMovement();
        Debug.Log("[EnhancedTransitionMover] Skipping upgrade, continuing movement");
    }

    /// <summary>
    /// Check PlayerDataManager and report status (same as original)
    /// </summary>
    private void CheckPlayerDataManager()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("[EnhancedTransitionMover] PlayerDataManager.Instance is null! Make sure PlayerDataManager object exists in scene.");
        }
        else
        {
            Debug.Log("[EnhancedTransitionMover] PlayerDataManager exists, current health: " + PlayerDataManager.Instance.GetCurrentHealth());
        }
    }

    /// <summary>
    /// Apply health bonus for specific level transitions (exactly same as original TransitionMover)
    /// </summary>
    private void ApplyHealthBonus(string targetScene)
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("[EnhancedTransitionMover] PlayerDataManager.Instance is null, cannot apply health bonus!");
            return;
        }

        Debug.Log("[EnhancedTransitionMover] Checking target scene: " + targetScene);

        if (targetScene == "Level3")
        {
            int beforeHealth = PlayerDataManager.Instance.GetCurrentHealth();
            PlayerDataManager.Instance.AddHealth(1);
            int afterHealth = PlayerDataManager.Instance.GetCurrentHealth();
            Debug.Log("[EnhancedTransitionMover] Entering Level3, gained extra life +1 (before: " + beforeHealth + ", after: " + afterHealth + ")");
        }
        else if (targetScene == "Level5")
        {
            int beforeHealth = PlayerDataManager.Instance.GetCurrentHealth();
            PlayerDataManager.Instance.AddHealth(1);
            int afterHealth = PlayerDataManager.Instance.GetCurrentHealth();
            Debug.Log("[EnhancedTransitionMover] Entering Level5, gained extra life +1 (before: " + beforeHealth + ", after: " + afterHealth + ")");
        }
        else
        {
            Debug.Log("[EnhancedTransitionMover] Target scene '" + targetScene + "' does not require health bonus");
        }
    }

    /// <summary>
    /// Load next scene (same as original TransitionMover)
    /// </summary>
    private void LoadNext()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        // Clear scene name in SceneTransitionManager
        SceneTransitionManager.ClearNextSceneName();

        Debug.Log("[EnhancedTransitionMover] Loading scene: " + nextScene);
        SceneManager.LoadScene(nextScene);
        enabled = false; // Prevent multiple calls
    }

    // Debug methods
    [ContextMenu("Force Trigger Upgrade")]
    public void DebugTriggerUpgrade()
    {
        if (!hasTriggeredUpgrade)
        {
            TriggerUpgrade();
        }
        else
        {
            Debug.Log("[EnhancedTransitionMover] Upgrade has already been triggered");
        }
    }

    [ContextMenu("Skip Upgrade and Continue")]
    public void DebugSkipUpgrade()
    {
        SkipUpgradeAndContinue();
    }

    [ContextMenu("Resume Movement")]
    public void DebugResumeMovement()
    {
        ResumeMovement();
    }

    [ContextMenu("Check Current Status")]
    public void DebugCheckStatus()
    {
        Debug.Log("=== EnhancedTransitionMover Status ===");
        Debug.Log("Target scene: " + nextScene);
        Debug.Log("Current position: " + transform.position.x);
        Debug.Log("Target position: " + targetX);
        Debug.Log("Upgrade trigger position: " + upgradePositionX);
        Debug.Log("Has triggered upgrade: " + hasTriggeredUpgrade);
        Debug.Log("Upgrade in progress: " + isUpgradeInProgress);
        Debug.Log("Can move: " + canMove);
        Debug.Log("Is upgrade scene: " + IsUpgradeScene(nextScene));
        Debug.Log("Transition upgrade found: " + (transitionUpgrade != null));
    }

    // Public methods for other scripts to use
    public bool IsUpgradeInProgress => isUpgradeInProgress;
    public bool HasTriggeredUpgrade => hasTriggeredUpgrade;
    public bool CanMove => canMove;
    public string NextScene => nextScene;
    public float CurrentProgress => Mathf.Abs(transform.position.x - upgradePositionX) / Mathf.Abs(targetX - upgradePositionX);
}