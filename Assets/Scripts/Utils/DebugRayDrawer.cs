using UnityEngine;

public class DebugRayDrawer : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showRays = true;
    
    private EnemyTankAI enemyTankAI;
    
    void Start()
    {
        enemyTankAI = GetComponent<EnemyTankAI>();
        if (enemyTankAI == null)
        {
            Debug.LogError("DebugRayDrawer: EnemyTankAI component not found!");
        }
    }
    
    void Update()
    {
        if (showRays && enemyTankAI != null)
        {
            DrawDebugRays();
        }
    }
    
    private void DrawDebugRays()
    {
        // 這裡可以添加更多的調試射線繪製邏輯
        // 例如：檢測範圍、射擊範圍等
    }
    
    void OnDrawGizmos()
    {
        if (!showRays) return;
        
        // 繪製檢測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f); // detectionRange
        
        // 繪製射擊範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 8f); // shootingRange
    }
}
