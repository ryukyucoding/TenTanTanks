using UnityEngine;

public class WaveDiagnostic : MonoBehaviour
{
    [Header("診斷設定")]
    [SerializeField] private bool enableDiagnostic = true;
    [SerializeField] private float checkInterval = 1f;
    
    private float lastCheckTime;
    private int lastEnemyCount = 0;
    
    private void Start()
    {
        if (enableDiagnostic)
        {
            Debug.Log("=== 波數診斷開始 ===");
            StartCoroutine(DiagnosticLoop());
        }
    }
    
    private System.Collections.IEnumerator DiagnosticLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            
            if (enableDiagnostic)
            {
                CheckCurrentState();
            }
        }
    }
    
    private void CheckCurrentState()
    {
        // 檢查場景中的敵人數量
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int currentEnemyCount = enemies.Length;
        
        if (currentEnemyCount != lastEnemyCount)
        {
            Debug.Log($"敵人數量變化: {lastEnemyCount} -> {currentEnemyCount}");
            lastEnemyCount = currentEnemyCount;
            
            // 列出所有敵人
            for (int i = 0; i < enemies.Length; i++)
            {
                Debug.Log($"  敵人 {i + 1}: {enemies[i].name} 位置: {enemies[i].transform.position}");
            }
        }
        
        // 檢查 WaveManager 狀態
        if (WaveManager.Instance != null)
        {
            Debug.Log($"WaveManager 狀態:");
            Debug.Log($"  當前波數索引: {WaveManager.Instance.CurrentWaveIndex}");
            Debug.Log($"  總波數: {WaveManager.Instance.TotalWaves}");
            Debug.Log($"  當前波敵人數: {WaveManager.Instance.EnemiesInCurrentWave}");
            Debug.Log($"  已生成敵人: {WaveManager.Instance.EnemiesSpawnedInWave}");
            Debug.Log($"  已消滅敵人: {WaveManager.Instance.EnemiesKilledInWave}");
            Debug.Log($"  波數激活: {WaveManager.Instance.IsWaveActive}");
            Debug.Log($"  所有波數完成: {WaveManager.Instance.IsAllWavesComplete}");
        }
        
        // 檢查關卡數據
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
        {
            var levelData = LevelManager.Instance.CurrentLevelData;
            Debug.Log($"關卡數據:");
            Debug.Log($"  關卡名稱: {levelData.levelName}");
            Debug.Log($"  敵人波數: {levelData.enemyWaves.Count}");
            
            for (int i = 0; i < levelData.enemyWaves.Count; i++)
            {
                var wave = levelData.enemyWaves[i];
                Debug.Log($"    波數 {i + 1}: {wave.enemyCount} 個敵人, 間隔: {wave.spawnInterval}秒");
            }
        }
    }
    
    [ContextMenu("立即診斷")]
    public void DiagnoseNow()
    {
        CheckCurrentState();
    }
    
    [ContextMenu("檢查敵人來源")]
    public void CheckEnemySources()
    {
        Debug.Log("=== 敵人來源檢查 ===");
        
        // 檢查所有可能的敵人生成來源
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"場景中敵人總數: {enemies.Length}");
        
        // 檢查是否有其他腳本在生成敵人
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in allMonoBehaviours)
        {
            if (mb.GetType().Name.Contains("Enemy") || mb.GetType().Name.Contains("Spawn"))
            {
                Debug.Log($"找到可能的敵人生成腳本: {mb.GetType().Name} 在 {mb.gameObject.name}");
            }
        }
        
        // 檢查 GameManager 是否在使用舊系統
        if (GameManager.Instance != null)
        {
            var gameManager = GameManager.Instance.GetComponent<GameManager>();
            if (gameManager != null)
            {
                // 使用反射檢查 useLevelSystem 變數
                var field = typeof(GameManager).GetField("useLevelSystem", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    bool useLevelSystem = (bool)field.GetValue(gameManager);
                    Debug.Log($"GameManager 使用關卡系統: {useLevelSystem}");
                    
                    if (!useLevelSystem)
                    {
                        Debug.LogWarning("GameManager 沒有使用關卡系統！可能在使用舊的敵人生成邏輯！");
                    }
                }
            }
        }
    }
    
    [ContextMenu("強制重置波數")]
    public void ForceResetWaves()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.ResetWaves();
            Debug.Log("波數已重置");
        }
        
        // 清除所有敵人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            DestroyImmediate(enemy);
        }
        Debug.Log($"清除了 {enemies.Length} 個敵人");
    }
}
