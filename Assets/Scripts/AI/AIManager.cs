using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private bool enableAI = true;
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private int maxAITanks = 10;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool pauseAI = false;
    
    // AI坦克列表
    private List<AdvancedEnemyTank> aiTanks = new List<AdvancedEnemyTank>();
    private List<Transform> allTargets = new List<Transform>();
    
    // 更新計時器
    private float updateTimer = 0f;
    
    // 單例模式
    public static AIManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeAI();
    }
    
    void Update()
    {
        if (!enableAI || pauseAI) return;
        
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateAllAI();
            updateTimer = 0f;
        }
    }
    
    private void InitializeAI()
    {
        // 找到所有AI坦克
        AdvancedEnemyTank[] tanks = FindObjectsByType<AdvancedEnemyTank>(FindObjectsSortMode.None);
        aiTanks.AddRange(tanks);
        
        // 找到所有目標
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            allTargets.Add(player.transform);
        }
        
        Debug.Log($"AI Manager initialized with {aiTanks.Count} AI tanks");
    }
    
    private void UpdateAllAI()
    {
        // 更新所有AI坦克的目標列表
        UpdateTargetLists();
        
        // 更新每個AI坦克
        foreach (AdvancedEnemyTank tank in aiTanks)
        {
            if (tank != null && tank.gameObject.activeInHierarchy)
            {
                // AI更新邏輯在AdvancedEnemyTank中處理
            }
        }
    }
    
    private void UpdateTargetLists()
    {
        // 更新所有目標列表
        allTargets.Clear();
        
        // 添加玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            allTargets.Add(player.transform);
        }
        
        // 添加其他AI坦克（如果它們是敵對的）
        foreach (AdvancedEnemyTank tank in aiTanks)
        {
            if (tank != null && tank.gameObject.activeInHierarchy)
            {
                // 這裡可以添加團隊邏輯
                // 暫時所有AI都將玩家作為目標
            }
        }
    }
    
    public void RegisterAITank(AdvancedEnemyTank tank)
    {
        if (!aiTanks.Contains(tank))
        {
            aiTanks.Add(tank);
            Debug.Log($"Registered new AI tank: {tank.name}");
        }
    }
    
    public void UnregisterAITank(AdvancedEnemyTank tank)
    {
        if (aiTanks.Contains(tank))
        {
            aiTanks.Remove(tank);
            Debug.Log($"Unregistered AI tank: {tank.name}");
        }
    }
    
    public List<Transform> GetAllTargets()
    {
        return new List<Transform>(allTargets);
    }
    
    public List<AdvancedEnemyTank> GetAllAITanks()
    {
        return new List<AdvancedEnemyTank>(aiTanks);
    }
    
    public void SetAIPause(bool pause)
    {
        pauseAI = pause;
        Debug.Log($"AI paused: {pause}");
    }
    
    public void ToggleAI()
    {
        enableAI = !enableAI;
        Debug.Log($"AI enabled: {enableAI}");
    }
    
    public void SpawnAITank(Vector3 position, string personality = "brown", string unitType = "brown_tank")
    {
        if (aiTanks.Count >= maxAITanks)
        {
            Debug.LogWarning("Maximum AI tanks reached!");
            return;
        }
        
        // 這裡需要實現AI坦克的生成邏輯
        // 可以從Prefab實例化
        Debug.Log($"Spawning AI tank at {position} with personality {personality}");
    }
    
    public void RemoveAllAI()
    {
        foreach (AdvancedEnemyTank tank in aiTanks)
        {
            if (tank != null)
            {
                Destroy(tank.gameObject);
            }
        }
        aiTanks.Clear();
        Debug.Log("All AI tanks removed");
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"AI Manager Debug Info");
        GUILayout.Label($"Active AI Tanks: {aiTanks.Count}");
        GUILayout.Label($"Total Targets: {allTargets.Count}");
        GUILayout.Label($"AI Enabled: {enableAI}");
        GUILayout.Label($"AI Paused: {pauseAI}");
        
        if (GUILayout.Button("Toggle AI"))
        {
            ToggleAI();
        }
        
        if (GUILayout.Button("Pause/Resume AI"))
        {
            SetAIPause(!pauseAI);
        }
        
        if (GUILayout.Button("Remove All AI"))
        {
            RemoveAllAI();
        }
        
        GUILayout.EndArea();
    }
}
