using UnityEngine;

/// <summary>
/// Automatically loads and applies saved tank transformations when Level scenes start
/// This ensures tank appearance persists across scene transitions until next upgrade
/// 
/// USAGE:
/// 1. Attach this script to any GameObject in Level scenes (Level1, Level2, Level3, Level4, Level5)
/// 2. It will automatically find PlayerDataManager and TankTransformationManager
/// 3. Applies saved transformation immediately when scene starts
/// </summary>
public class LevelSceneTankLoader : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Auto-Detection")]
    [SerializeField] private bool autoFindComponents = true;

    [Header("Manual References (Optional)")]
    [SerializeField] private PlayerDataManager playerDataManager;

    void Start()
    {
        DebugLog("=== LevelSceneTankLoader Started ===");

        if (autoFindComponents)
        {
            FindComponents();
        }

        ApplySavedTankTransformation();
    }

    /// <summary>
    /// Automatically find required components in the scene
    /// </summary>
    private void FindComponents()
    {
        // Find PlayerDataManager (persistent across scenes)
        if (playerDataManager == null)
        {
            playerDataManager = PlayerDataManager.Instance;
        }

        DebugLog("Component Detection Results:");
        DebugLog($"  - PlayerDataManager: {(playerDataManager != null ? "✓ Found" : "✗ Missing")}");
    }

    /// <summary>
    /// Apply saved tank transformation using PlayerDataManager's built-in method
    /// </summary>
    private void ApplySavedTankTransformation()
    {
        // Check if we have PlayerDataManager
        if (playerDataManager == null)
        {
            DebugLog("❌ Cannot apply transformation: PlayerDataManager not found");
            return;
        }

        // Use PlayerDataManager's existing LoadTankTransformation method
        // This method already handles finding TankTransformationManager and applying the correct transformation
        string savedTransformation = playerDataManager.GetCurrentTankTransformation();

        if (string.IsNullOrEmpty(savedTransformation) || savedTransformation == "Basic")
        {
            DebugLog("ℹ️ No saved tank transformation found or using Basic appearance");
            return;
        }

        DebugLog($"🔄 Loading saved tank transformation: {savedTransformation}");

        // PlayerDataManager.LoadTankTransformation() already does all the work!
        playerDataManager.LoadTankTransformation();

        DebugLog("✅ Tank transformation loaded via PlayerDataManager");

        // 🔧 FIX: Update fire points after transformation to ensure bullets spawn correctly
        FixFirePointsAfterTransformation();
    }

    /// <summary>
    /// Fix fire point references after tank transformation
    /// This ensures bullets spawn from the correct positions
    /// </summary>
    private void FixFirePointsAfterTransformation()
    {
        DebugLog("🔧 Fixing fire points after transformation...");

        // Find TankFirePointUpdater or create one if needed
        var firePointUpdater = FindFirstObjectByType<TankFirePointUpdater>();
        if (firePointUpdater == null)
        {
            // Look for player tank and add the updater
            var playerTank = GameObject.FindGameObjectWithTag("Player");
            if (playerTank != null)
            {
                firePointUpdater = playerTank.AddComponent<TankFirePointUpdater>();
                DebugLog("✅ Added TankFirePointUpdater to player tank");
            }
        }

        if (firePointUpdater != null)
        {
            // Update fire points with a small delay to ensure transformation is complete
            firePointUpdater.Invoke("UpdateFirePoints", 0.2f);
            DebugLog("✅ Scheduled fire point update");
        }
        else
        {
            DebugLog("⚠️ Could not find or create TankFirePointUpdater");
        }
    }

    /// <summary>
    /// Public method to manually reload transformation (for debugging or external calls)
    /// </summary>
    public void ReloadTankTransformation()
    {
        DebugLog("🔄 Manual tank transformation reload requested");
        FindComponents();
        ApplySavedTankTransformation();
    }

    /// <summary>
    /// Check current transformation status (for debugging)
    /// </summary>
    [ContextMenu("Check Transformation Status")]
    public void CheckTransformationStatus()
    {
        DebugLog("=== TRANSFORMATION STATUS CHECK ===");

        if (playerDataManager != null)
        {
            string currentTransformation = playerDataManager.GetCurrentTankTransformation();
            DebugLog($"Saved transformation: {currentTransformation}");
        }
        else
        {
            DebugLog("PlayerDataManager not available");
        }
    }

    /// <summary>
    /// Test the automatic loading system
    /// </summary>
    [ContextMenu("Test Load Transformation")]
    public void TestLoadTransformation()
    {
        if (playerDataManager != null)
        {
            playerDataManager.LoadTankTransformation();
            DebugLog("🧪 Test: Called PlayerDataManager.LoadTankTransformation()");
        }
        else
        {
            DebugLog("🧪 Test: PlayerDataManager not found");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[LevelSceneTankLoader] {message}");
        }
    }

    /// <summary>
    /// Handle component finding when objects are enabled later in the frame
    /// </summary>
    void OnEnable()
    {
        // If PlayerDataManager is missing, try to find it again
        if (playerDataManager == null && autoFindComponents)
        {
            Invoke("DelayedComponentFind", 0.1f);
        }
    }

    private void DelayedComponentFind()
    {
        FindComponents();

        // Only apply transformation if we found PlayerDataManager now
        if (playerDataManager != null)
        {
            DebugLog("🔄 Delayed component detection successful - applying transformation");
            ApplySavedTankTransformation();
        }
    }
}