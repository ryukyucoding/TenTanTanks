using UnityEngine;

public class ForceActivateWave : MonoBehaviour
{
    [Header("強制激活設定")]
    [SerializeField] private bool autoActivateOnStart = true;
    [SerializeField] private float activationDelay = 2f;
    
    private void Start()
    {
        if (autoActivateOnStart)
        {
            StartCoroutine(ForceActivate());
        }
    }
    
    private System.Collections.IEnumerator ForceActivate()
    {
        yield return new WaitForSeconds(activationDelay);
        
        Debug.Log("=== 強制激活波數 ===");
        
        // 檢查所有組件
        CheckComponents();
        
        // 強制初始化關卡
        ForceInitializeLevel();
        
        // 等待一幀
        yield return new WaitForEndOfFrame();
        
        // 強制開始第一波
        ForceStartFirstWave();
        
        Debug.Log("=== 強制激活完成 ===");
    }
    
    private void CheckComponents()
    {
        Debug.Log("=== 組件檢查 ===");
        
        // 檢查 LevelManager
        if (LevelManager.Instance != null)
        {
            Debug.Log($"✅ LevelManager 存在");
            Debug.Log($"   總關卡數: {LevelManager.Instance.TotalLevels}");
            Debug.Log($"   當前關卡索引: {LevelManager.Instance.CurrentLevelIndex}");
            Debug.Log($"   關卡激活: {LevelManager.Instance.IsLevelActive}");
            
            if (LevelManager.Instance.CurrentLevelData != null)
            {
                var levelData = LevelManager.Instance.CurrentLevelData;
                Debug.Log($"   關卡名稱: {levelData.levelName}");
                Debug.Log($"   敵人波數: {levelData.enemyWaves.Count}");
            }
            else
            {
                Debug.LogError("❌ 當前關卡數據為空！");
            }
        }
        else
        {
            Debug.LogError("❌ LevelManager 不存在！");
        }
        
        // 檢查 WaveManager
        if (WaveManager.Instance != null)
        {
            Debug.Log($"✅ WaveManager 存在");
            Debug.Log($"   當前波數索引: {WaveManager.Instance.CurrentWaveIndex}");
            Debug.Log($"   總波數: {WaveManager.Instance.TotalWaves}");
            Debug.Log($"   波數激活: {WaveManager.Instance.IsWaveActive}");
            Debug.Log($"   所有波數完成: {WaveManager.Instance.IsAllWavesComplete}");
        }
        else
        {
            Debug.LogError("❌ WaveManager 不存在！");
        }
    }
    
    private void ForceInitializeLevel()
    {
        Debug.Log("=== 強制初始化關卡 ===");
        
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
        {
            if (WaveManager.Instance != null)
            {
                Debug.Log("重新初始化關卡...");
                WaveManager.Instance.InitializeLevel(LevelManager.Instance.CurrentLevelData);
            }
            else
            {
                Debug.LogError("WaveManager 不存在，無法初始化關卡！");
            }
        }
        else
        {
            Debug.LogError("無法獲取關卡數據！");
        }
    }
    
    private void ForceStartFirstWave()
    {
        Debug.Log("=== 強制開始第一波 ===");
        
        if (WaveManager.Instance != null)
        {
            Debug.Log("強制開始第一波...");
            WaveManager.Instance.StartNextWave();
        }
        else
        {
            Debug.LogError("WaveManager 不存在，無法開始波數！");
        }
    }
    
    [ContextMenu("立即強制激活")]
    public void ForceActivateNow()
    {
        StartCoroutine(ForceActivate());
    }
    
    [ContextMenu("檢查關卡初始化")]
    public void CheckLevelInitialization()
    {
        Debug.Log("=== 關卡初始化檢查 ===");
        
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
        {
            var levelData = LevelManager.Instance.CurrentLevelData;
            Debug.Log($"關卡數據存在: {levelData.levelName}");
            
            if (WaveManager.Instance != null)
            {
                Debug.Log("重新初始化關卡...");
                WaveManager.Instance.InitializeLevel(levelData);
                Debug.Log("關卡初始化完成");
            }
        }
        else
        {
            Debug.LogError("關卡數據不存在！");
        }
    }
    
    [ContextMenu("強制開始波數")]
    public void ForceStartWave()
    {
        if (WaveManager.Instance != null)
        {
            Debug.Log("強制開始波數...");
            WaveManager.Instance.StartNextWave();
        }
        else
        {
            Debug.LogError("WaveManager 不存在！");
        }
    }
}
