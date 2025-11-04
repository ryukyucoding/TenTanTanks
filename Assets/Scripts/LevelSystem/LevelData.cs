using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyWave
{
    [Header("波數設定")]
    [Tooltip("敵人數量")]
    public int enemyCount = 1;
    
    [Tooltip("敵人預製體")]
    public GameObject enemyPrefab;
    
    [Tooltip("波數間隔時間（秒）")]
    public float waveDelay = 2f;
    
    [Tooltip("敵人生成間隔時間（秒）")]
    public float spawnInterval = 1f;
    
    [Tooltip("敵人生成位置（如果為空則使用隨機位置）")]
    public Transform[] spawnPoints;
    
    [Tooltip("敵人屬性調整")]
    public EnemyStatsModifier statsModifier = new EnemyStatsModifier();
}

[System.Serializable]
public class EnemyStatsModifier
{
    [Tooltip("血量倍數")]
    public float healthMultiplier = 1f;
    
    [Tooltip("移動速度倍數")]
    public float speedMultiplier = 1f;
    
    [Tooltip("攻擊力倍數")]
    public float damageMultiplier = 1f;
    
    [Tooltip("攻擊速度倍數")]
    public float fireRateMultiplier = 1f;
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

