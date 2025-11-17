using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyWave
{
    [Header("波數設定")]
    [Tooltip("敵人數量")]
    public int enemyCount = 1;
    
    [Tooltip("整波共用的預設敵人預製體（如果 per-enemy 沒有指定時使用）")]
    public GameObject enemyPrefab;
    
    [Tooltip("波數間隔時間（秒）")]
    public float waveDelay = 2f;
    
    [Tooltip("敵人生成間隔時間（秒）")]
    public float spawnInterval = 1f;
    
    [Header("進階：每一隻敵人的個別設定（可選）")]
    [Tooltip("如果填寫，會依序使用這裡的設定生成每一隻敵人")]
    public EnemySpawnEntry[] enemyEntries;
}

[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("這一隻敵人的預製體（留空則使用波數的 enemyPrefab）")]
    public GameObject enemyPrefab;
    
    [Tooltip("這一隻敵人要使用的生成點索引（對應 SimpleLevelController / LevelControllerAdapter 上的 spawnPoints 陣列；-1 表示不指定）")]
    public int spawnPointIndex = -1;
}

[System.Serializable]
public class LevelData
{
    [Header("關卡基本設定")]
    [Tooltip("關卡名稱")]
    public string levelName = "關卡 1";
    
    [Tooltip("關卡描述")]
    public string levelDescription = "";
    
    [Tooltip("關卡時間限制（秒，0表示無限制）")]
    public float timeLimit = 0f;
    
    [Header("敵人波數設定")]
    [Tooltip("敵人波數列表")]
    public List<EnemyWave> enemyWaves = new List<EnemyWave>();
    
    [Header("關卡完成條件")]
    [Tooltip("需要消滅所有敵人才能通關")]
    public bool requireAllEnemiesDefeated = true;
    
    [Tooltip("需要存活指定時間才能通關")]
    public bool requireSurviveTime = false;
    
    [Tooltip("存活時間要求（秒）")]
    public float survivalTime = 60f;
    
    [Header("獎勵設定")]
    [Tooltip("通關獎勵分數")]
    public int scoreReward = 100;
    
    [Tooltip("通關獎勵經驗")]
    public int experienceReward = 50;
}

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelDataAsset : ScriptableObject
{
    [Header("關卡數據")]
    public LevelData levelData;
    
    [Header("關卡預覽")]
    [Tooltip("關卡預覽圖片")]
    public Sprite levelPreview;
    
    [Tooltip("關卡難度（1-5星）")]
    [Range(1, 5)]
    public int difficulty = 1;
    
    [Tooltip("關卡解鎖條件")]
    public string unlockCondition = "";
}

