using UnityEngine;

public class ForceStartWave : MonoBehaviour
{
    [Header("強制開始設定")]
    [SerializeField] private bool autoStartOnStart = true;
    [SerializeField] private float startDelay = 1f;
    
    private void Start()
    {
        if (autoStartOnStart)
        {
            StartCoroutine(ForceStart());
        }
    }
    
    private System.Collections.IEnumerator ForceStart()
    {
        yield return new WaitForSeconds(startDelay);
        
        Debug.Log("=== 強制開始波數 ===");
        
        // 檢查 LevelManager
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager 不存在！");
            yield break;
        }
        
        if (LevelManager.Instance.CurrentLevelData == null)
        {
            Debug.LogError("當前關卡數據為空！");
            yield break;
        }
        
        // 檢查 WaveManager
        if (WaveManager.Instance == null)
        {
            Debug.LogError("WaveManager 不存在！");
            yield break;
        }
        
        // 強制初始化關卡
        Debug.Log("強制初始化關卡...");
        WaveManager.Instance.InitializeLevel(LevelManager.Instance.CurrentLevelData);
        
        // 等待一幀
        yield return new WaitForEndOfFrame();
        
        // 強制開始第一波
        Debug.Log("強制開始第一波...");
        WaveManager.Instance.StartNextWave();
        
        Debug.Log("=== 強制開始完成 ===");
    }
    
    [ContextMenu("立即強制開始")]
    public void ForceStartNow()
    {
        StartCoroutine(ForceStart());
    }
    
    [ContextMenu("檢查關卡數據")]
    public void CheckLevelData()
    {
        Debug.Log("=== 關卡數據檢查 ===");
        
        if (LevelManager.Instance != null)
        {
            Debug.Log($"LevelManager 存在: {LevelManager.Instance != null}");
            Debug.Log($"當前關卡數據: {LevelManager.Instance.CurrentLevelData != null}");
            
            if (LevelManager.Instance.CurrentLevelData != null)
            {
                var levelData = LevelManager.Instance.CurrentLevelData;
                Debug.Log($"關卡名稱: {levelData.levelName}");
                Debug.Log($"敵人波數: {levelData.enemyWaves.Count}");
                
                for (int i = 0; i < levelData.enemyWaves.Count; i++)
                {
                    var wave = levelData.enemyWaves[i];
                    Debug.Log($"  波數 {i + 1}: {wave.enemyCount} 個敵人, 預製體: {(wave.enemyPrefab != null ? wave.enemyPrefab.name : "未設定")}");
                }
            }
        }
        else
        {
            Debug.LogError("LevelManager 不存在！");
        }
        
        if (WaveManager.Instance != null)
        {
            Debug.Log($"WaveManager 存在: {WaveManager.Instance != null}");
            Debug.Log($"當前波數索引: {WaveManager.Instance.CurrentWaveIndex}");
            Debug.Log($"總波數: {WaveManager.Instance.TotalWaves}");
            Debug.Log($"波數激活: {WaveManager.Instance.IsWaveActive}");
        }
        else
        {
            Debug.LogError("WaveManager 不存在！");
        }
    }
}
