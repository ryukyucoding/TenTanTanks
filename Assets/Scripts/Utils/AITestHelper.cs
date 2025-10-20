using UnityEngine;

public class AITestHelper : MonoBehaviour
{
    [Header("AI Test Settings")]
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private float debugUpdateInterval = 1f;
    
    private float lastDebugTime;
    private EnemyTankAI[] allAITanks;

    void Start()
    {
        // 找到所有AI坦克
        allAITanks = FindObjectsByType<EnemyTankAI>(FindObjectsSortMode.None);
        Debug.Log($"AITestHelper: Found {allAITanks.Length} AI tanks");
        
        // 為每個AI坦克設置調試模式
        foreach (var aiTank in allAITanks)
        {
            if (aiTank != null)
            {
                Debug.Log($"AI Tank found: {aiTank.name}");
            }
        }
    }

    void Update()
    {
        if (!enableDebugMode) return;
        
        if (Time.time - lastDebugTime >= debugUpdateInterval)
        {
            lastDebugTime = Time.time;
            LogAIStatus();
        }
    }

    private void LogAIStatus()
    {
        if (allAITanks == null) return;
        
        foreach (var aiTank in allAITanks)
        {
            if (aiTank != null)
            {
                // 使用反射來獲取私有字段（僅用於調試）
                var targetTankField = typeof(EnemyTankAI).GetField("targetTank", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var seesTargetField = typeof(EnemyTankAI).GetField("seesTarget", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (targetTankField != null && seesTargetField != null)
                {
                    Transform target = (Transform)targetTankField.GetValue(aiTank);
                    bool seesTarget = (bool)seesTargetField.GetValue(aiTank);
                    
                    if (target != null)
                    {
                        float distance = Vector3.Distance(aiTank.transform.position, target.position);
                        Debug.Log($"AI Tank {aiTank.name}: Target={target.name}, SeesTarget={seesTarget}, Distance={distance:F1}");
                    }
                    else
                    {
                        Debug.Log($"AI Tank {aiTank.name}: No target found");
                    }
                }
            }
        }
    }

    [ContextMenu("Force Find Player")]
    public void ForceFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // 嘗試其他方式尋找玩家
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("player"))
                {
                    player = obj;
                    break;
                }
            }
        }
        
        if (player != null)
        {
            Debug.Log($"Player found: {player.name}");
        }
        else
        {
            Debug.LogWarning("No player found in scene");
        }
    }
}
