using UnityEngine;

/// <summary>
/// Fixes fire point references after tank transformations
/// Ensures bullets spawn from correct positions after tank upgrades
/// 
/// USAGE:
/// 1. Attach to any tank that has TankTransformationManager
/// 2. It automatically fixes fire points when transformations are applied
/// 3. Works with both single and multi-turret systems
/// </summary>
public class TankFirePointUpdater : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Component references
    private TankTransformationManager transformationManager;
    private TankController tankController;
    private MultiTurretShooting multiTurretShooting;

    void Start()
    {
        // Find components
        transformationManager = GetComponent<TankTransformationManager>();
        tankController = GetComponent<TankController>();
        multiTurretShooting = GetComponent<MultiTurretShooting>();

        DebugLog("=== TankFirePointUpdater Started ===");
        DebugLog($"TankTransformationManager: {(transformationManager != null ? "✓" : "✗")}");
        DebugLog($"TankController: {(tankController != null ? "✓" : "✗")}");
        DebugLog($"MultiTurretShooting: {(multiTurretShooting != null ? "✓" : "✗")}");

        // Initial fire point update (handles transformations applied before this script runs)
        Invoke("UpdateFirePointsDelayed", 0.1f);
    }

    void OnEnable()
    {
        // Update fire points when object becomes active (handles scene loading)
        Invoke("UpdateFirePointsDelayed", 0.1f);
    }

    /// <summary>
    /// Delayed fire point update to ensure all transformations are complete
    /// </summary>
    private void UpdateFirePointsDelayed()
    {
        UpdateFirePoints();
    }

    /// <summary>
    /// Update fire point references for all shooting systems
    /// Call this after any tank transformation
    /// </summary>
    public void UpdateFirePoints()
    {
        DebugLog("🔄 Updating fire points after tank transformation...");

        // Update TankController fire point
        UpdateTankControllerFirePoint();

        // Update MultiTurretShooting fire points
        UpdateMultiTurretFirePoints();

        DebugLog("✅ Fire point update complete!");
    }

    /// <summary>
    /// Update the main TankController fire point reference
    /// </summary>
    private void UpdateTankControllerFirePoint()
    {
        if (tankController == null)
        {
            DebugLog("No TankController found - skipping single fire point update");
            return;
        }

        // Find the primary fire point in the tank hierarchy
        Transform firePoint = FindBestFirePoint();

        if (firePoint != null)
        {
            // Use reflection to update the private firePoint field
            var firePointField = typeof(TankController).GetField("firePoint",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (firePointField == null)
            {
                // Try WebGLTankController if TankController doesn't have firePoint
                firePointField = typeof(WebGLTankController).GetField("firePoint",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            if (firePointField != null)
            {
                firePointField.SetValue(tankController, firePoint);
                DebugLog($"✅ Updated TankController fire point to: {firePoint.name}");
                DebugLog($"   Position: {firePoint.position}");
            }
            else
            {
                DebugLog("⚠️ Could not access firePoint field via reflection");
                // Alternative: Call a public method if available
                TryUpdateFirePointViaMethod(firePoint);
            }
        }
        else
        {
            DebugLog("❌ No suitable fire point found for TankController");
        }
    }

    /// <summary>
    /// Try to update fire point via public method if available
    /// </summary>
    private void TryUpdateFirePointViaMethod(Transform firePoint)
    {
        // Check if TankController has a SetFirePoint method
        var setFirePointMethod = tankController.GetType().GetMethod("SetFirePoint");
        if (setFirePointMethod != null)
        {
            setFirePointMethod.Invoke(tankController, new object[] { firePoint });
            DebugLog($"✅ Updated fire point via SetFirePoint method");
        }
    }

    /// <summary>
    /// Update MultiTurretShooting fire points
    /// </summary>
    private void UpdateMultiTurretFirePoints()
    {
        if (multiTurretShooting == null)
        {
            DebugLog("No MultiTurretShooting found - skipping multi-turret update");
            return;
        }

        // Call the InitializeFirePoints method to refresh all fire points
        var initMethod = multiTurretShooting.GetType().GetMethod("InitializeFirePoints",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (initMethod != null)
        {
            initMethod.Invoke(multiTurretShooting, null);
            DebugLog("✅ Refreshed MultiTurretShooting fire points");
        }
        else
        {
            DebugLog("⚠️ Could not access InitializeFirePoints method");
        }
    }

    /// <summary>
    /// Find the best fire point in the current tank configuration
    /// </summary>
    private Transform FindBestFirePoint()
    {
        DebugLog("🔍 Searching for fire points...");

        // Strategy 1: Look for objects named "FirePoint" 
        Transform firePoint = transform.Find("FirePoint");
        if (firePoint != null)
        {
            DebugLog($"Found FirePoint: {firePoint.name}");
            return firePoint;
        }

        // Strategy 2: Look in turrets for fire points
        Transform[] turrets = GetComponentsInChildren<Transform>();
        foreach (Transform turret in turrets)
        {
            if (turret.name.ToLower().Contains("turret") || turret.name.ToLower().Contains("cannon"))
            {
                // Look for fire point in this turret
                Transform turretFirePoint = turret.Find("FirePoint");
                if (turretFirePoint != null)
                {
                    DebugLog($"Found FirePoint in turret: {turret.name} -> {turretFirePoint.name}");
                    return turretFirePoint;
                }

                // Look for objects with "fire" in their name
                for (int i = 0; i < turret.childCount; i++)
                {
                    Transform child = turret.GetChild(i);
                    if (child.name.ToLower().Contains("fire"))
                    {
                        DebugLog($"Found fire-related object: {child.name}");
                        return child;
                    }
                }
            }
        }

        // Strategy 3: Create a temporary fire point on the main turret
        Transform mainTurret = transform.Find("Turret");
        if (mainTurret == null)
        {
            // Look for any object with "turret" in the name
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("turret"))
                {
                    mainTurret = child;
                    break;
                }
            }
        }

        if (mainTurret != null)
        {
            // Create a temporary fire point
            GameObject tempFirePoint = new GameObject("TempFirePoint");
            tempFirePoint.transform.SetParent(mainTurret);
            tempFirePoint.transform.localPosition = new Vector3(0, 0, 1); // In front of turret
            DebugLog($"Created temporary fire point on: {mainTurret.name}");
            return tempFirePoint.transform;
        }

        DebugLog("❌ Could not find any suitable turret for fire point");
        return null;
    }

    /// <summary>
    /// Public method to manually trigger fire point update
    /// Can be called by TankTransformationManager after transformations
    /// </summary>
    [ContextMenu("Update Fire Points")]
    public void ManualUpdateFirePoints()
    {
        UpdateFirePoints();
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TankFirePointUpdater] {message}");
        }
    }

    /// <summary>
    /// Check current fire point status (for debugging)
    /// </summary>
    [ContextMenu("Check Fire Point Status")]
    public void CheckFirePointStatus()
    {
        DebugLog("=== FIRE POINT STATUS CHECK ===");

        if (tankController != null)
        {
            Vector3 firePos = tankController.GetFirePointPosition();
            Vector3 fireDir = tankController.GetFireDirection();
            DebugLog($"TankController fire position: {firePos}");
            DebugLog($"TankController fire direction: {fireDir}");
        }

        // List all fire-related objects in the tank
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        DebugLog("Fire-related objects found:");
        foreach (Transform child in allChildren)
        {
            if (child.name.ToLower().Contains("fire") || child.name.ToLower().Contains("turret"))
            {
                DebugLog($"  - {child.name} at {child.position}");
            }
        }
    }
}