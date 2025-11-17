using UnityEngine;
using System.Collections.Generic;

// 完全重寫的 LevelData 系統，避免 Unity 6 序列化問題

[System.Serializable]
public class EnemyWaveData
{
    [Header("波數設定")]
    public int enemyCount = 1;
    public GameObject enemyPrefab;
    public float waveDelay = 2f;
    public float spawnInterval = 1f;
}

[System.Serializable]
public class LevelConfiguration
{
    [Header("關卡基本設定")]
    public string levelName = "關卡 1";
    public string levelDescription = "";
    public float timeLimit = 0f;
    
    [Header("敵人波數設定")]
    public List<EnemyWaveData> waves = new List<EnemyWaveData>();
    
    [Header("關卡完成條件")]
    public bool requireAllEnemiesDefeated = true;
    public bool requireSurviveTime = false;
    public float survivalTime = 60f;
    
    [Header("獎勵設定")]
    public int scoreReward = 100;
    public int experienceReward = 50;
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Configuration")]
public class LevelDataNew : ScriptableObject
{
    [Header("關卡配置")]
    public LevelConfiguration config = new LevelConfiguration();
    
    [Header("關卡預覽")]
    public Sprite levelPreview;
    
    [Range(1, 5)]
    public int difficulty = 1;
    
    public string unlockCondition = "";
    
    // 初始化
    private void OnEnable()
    {
        if (config == null)
        {
            config = new LevelConfiguration();
        }
        
        if (config.waves == null)
        {
            config.waves = new List<EnemyWaveData>();
        }
    }
    
    // 轉換為舊格式（兼容性）
    public LevelData ToLegacyFormat()
    {
        LevelData legacy = new LevelData();
        legacy.levelName = config.levelName;
        legacy.levelDescription = config.levelDescription;
        legacy.timeLimit = config.timeLimit;
        legacy.requireAllEnemiesDefeated = config.requireAllEnemiesDefeated;
        legacy.requireSurviveTime = config.requireSurviveTime;
        legacy.survivalTime = config.survivalTime;
        legacy.scoreReward = config.scoreReward;
        legacy.experienceReward = config.experienceReward;
        
        legacy.enemyWaves = new List<EnemyWave>();
        foreach (var wave in config.waves)
        {
            EnemyWave legacyWave = new EnemyWave();
            legacyWave.enemyCount = wave.enemyCount;
            legacyWave.enemyPrefab = wave.enemyPrefab;
            legacyWave.waveDelay = wave.waveDelay;
            legacyWave.spawnInterval = wave.spawnInterval;
            
            legacy.enemyWaves.Add(legacyWave);
        }
        
        return legacy;
    }
}


