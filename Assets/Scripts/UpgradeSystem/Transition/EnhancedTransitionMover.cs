using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enhanced Transition Mover - Integrates with upgrade system
/// Pauses and triggers upgrade wheel during specific transitions (Level2¡÷3, Level4¡÷5)
/// Fully compatible with existing TransitionMover functionality
/// </summary>
public class EnhancedTransitionMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float targetX = 12f;
    [SerializeField] private string nextScene = "Level1";

    [Header("Upgrade Trigger Settings")]
    [SerializeField] private float upgradePositionX = 0f; // X position to trigger upgrade
    [SerializeField] private float upgradeDetectionRange = 1f; // Detection range
    [SerializeField] private bool enableUpgrades = true; // Whether to enable upgrade functionality

    [Header("Scenes Requiring Upgrades")]
    [SerializeField] private string[] upgradeScenes = { "Level3", "Level5" }; // Target scenes that need upgrades

    private UpdatedTransitionUpgradeManager upgradeManager;
    private bool hasTriggeredUpgrade = false;
    private bool isUpgradeInProgress = false;
    private bool canMove = true;

    void Start()
    {
        InitializeTransition();
        FindUpgradeManager();
    }

    void Update()
    {
        if (!canMove) return;

        Vector3 pos = transform.position;

        // Check if need to trigger upgrade
        if (!hasTriggeredUpgrade && enableUpgrades && ShouldTriggerUpgrade(pos.x))
        {
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
    /// Find upgrade manager
    /// </summary>
    private void FindUpgradeManager()
    {
        upgradeManager = FindFirstObjectByType<UpdatedTransitionUpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogWarning("[EnhancedTransitionMover] UpdatedTransitionUpgradeManager not found, upgrade functionality will be disabled");
            enableUpgrades = false;
        }
        else
        {
            Debug.Log("[EnhancedTransitionMover] Found UpdatedTransitionUpgradeManager, upgrade functionality enabled");
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
        canMove = false;

        Debug.Log("[EnhancedTransitionMover] Triggered upgrade at position " + transform.position.x + ", target scene: " + nextScene);

        if (upgradeManager != null)
        {
            upgradeManager.TriggerTransitionUpgrade();
        }
        else
        {
            Debug.LogError("[EnhancedTransitionMover] Upgrade manager is null, skipping upgrade and continuing");
            ResumeMovement();
        }
    }

    /// <summary>
    /// Resume movement (called by UpdatedTransitionUpgradeManager)
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
        Debug.Log("Upgrade manager: " + (upgradeManager != null ? "exists" : "not found"));
    }

    // Public methods for other scripts to use
    public bool IsUpgradeInProgress => isUpgradeInProgress;
    public bool HasTriggeredUpgrade => hasTriggeredUpgrade;
    public bool CanMove => canMove;
    public string NextScene => nextScene;
    public float CurrentProgress => Mathf.Abs(transform.position.x - upgradePositionX) / Mathf.Abs(targetX - upgradePositionX);
}