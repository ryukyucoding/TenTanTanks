using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 簡化的關卡數據結構（不依賴 ScriptableObject）
/// </summary>
[System.Serializable]
public class LevelDataConfig
{
    public string levelName;
    public string levelDescription;
    public float timeLimit;
    public WaveConfig[] waves;
    public bool requireAllEnemiesDefeated = true;
    public bool requireSurviveTime = false;
    public float survivalTime = 60f;
    public int scoreReward = 100;
    public int experienceReward = 50;
}

/// <summary>
/// 波數配置
/// </summary>
[System.Serializable]
public class WaveConfig
{
    public EnemyConfig[] enemies;
    public float waveDelay = 1f;
    public float spawnInterval = 0f;
}

/// <summary>
/// 單一敵人配置
/// </summary>
[System.Serializable]
public class EnemyConfig
{
    public string prefabKey;        // 敵人 Prefab 的字串 key
    public int spawnPointIndex;     // 生成點索引
}

/// <summary>
/// 關卡數據庫 - 硬編碼所有關卡配置
/// 不依賴 ScriptableObject Asset，避免檔案損壞問題
/// </summary>
public static class LevelDatabase
{
    // 敵人 Prefab Key 常數定義
    public const string ENEMY_GREEN = "EnemyTankGreen";
    public const string ENEMY_GRAY = "EnemyTankGray";
    public const string ENEMY_SOIL = "EnemyTankSoil";
    public const string ENEMY_PURPLE = "EnemyTankPurple";

    /// <summary>
    /// 根據關卡編號獲取關卡數據
    /// </summary>
    public static LevelDataConfig GetLevelData(int levelNumber)
    {
        switch (levelNumber)
        {
            case 1:
                return CreateLevel1();
            case 2:
                return CreateLevel2();
            case 3:
                return CreateLevel3();
            case 4:
                return CreateLevel4();
            case 5:
                return CreateLevel5();
            default:
                Debug.LogWarning($"未定義的關卡編號: {levelNumber}");
                return null;
        }
    }

    /// <summary>
    /// 關卡 1 - 新手訓練
    /// </summary>
    private static LevelDataConfig CreateLevel1()
    {
        return new LevelDataConfig
        {
            levelName = "關卡 1 - 新手訓練",
            levelDescription = "學習基本操作，消滅所有敵人",
            timeLimit = 0,
            waves = new[]
            {
                // Wave 1: 1 個土色敵人
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 0 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 1 個土色 + 1 個綠色
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 1 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = false,
            survivalTime = 60,
            scoreReward = 100,
            experienceReward = 50
        };
    }

    /// <summary>
    /// 關卡 2 - 挑戰開始
    /// </summary>
    private static LevelDataConfig CreateLevel2()
    {
        return new LevelDataConfig
        {
            levelName = "關卡 2 - 挑戰開始",
            levelDescription = "面對更多敵人，測試你的技能",
            timeLimit = 180,
            waves = new[]
            {
                // Wave 1: 1 個灰色 + 1 個土色
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 1 個灰色 + 1 個綠色
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 3: 1 個紫色敵人（較強）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 3 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = false,
            survivalTime = 60,
            scoreReward = 200,
            experienceReward = 100
        };
    }

    /// <summary>
    /// 關卡 3 - 生存挑戰
    /// </summary>
    private static LevelDataConfig CreateLevel3()
    {
        return new LevelDataConfig
        {
            levelName = "關卡 3 - 生存挑戰",
            levelDescription = "在限定時間內生存下來，敵人會越來越強",
            timeLimit = 120,
            waves = new[]
            {
                // Wave 1: 2 個灰色敵人
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 1 個紫色 + 1 個綠色
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 3 }
                    },
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 3: 1 個灰色 + 1 個綠色 + 1 個土色（最終波）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = true,
            survivalTime = 120,
            scoreReward = 300,
            experienceReward = 150
        };
    }

    /// <summary>
    /// 關卡 4 - 進階戰鬥
    /// </summary>
    private static LevelDataConfig CreateLevel4()
    {
        return new LevelDataConfig
        {
            levelName = "關卡 4 - 進階戰鬥",
            levelDescription = "更多波次的敵人，考驗你的戰鬥技巧",
            timeLimit = 200,
            waves = new[]
            {
                // Wave 1: 2 個綠色敵人
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 2 個灰色 + 1 個土色
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 3 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 0 }
                    },
                    waveDelay = 1.5f,
                    spawnInterval = 0f
                },
                // Wave 3: 2 個紫色敵人
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 3 }
                    },
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 4: 混合波（最終挑戰）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = false,
            survivalTime = 60,
            scoreReward = 400,
            experienceReward = 200
        };
    }

    /// <summary>
    /// 關卡 5 - 終極考驗
    /// </summary>
    private static LevelDataConfig CreateLevel5()
    {
        return new LevelDataConfig
        {
            levelName = "關卡 5 - 終極考驗",
            levelDescription = "最強大的敵人組合，只有真正的高手才能通過",
            timeLimit = 250,
            waves = new[]
            {
                // Wave 1: 3 個灰色敵人
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 2 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 2 個紫色 + 1 個綠色
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 3 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 }
                    },
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 3: 2 個綠色 + 2 個土色
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    waveDelay = 1.5f,
                    spawnInterval = 0f
                },
                // Wave 4: 終極波次 - 所有類型敵人
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 3 }
                    },
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 5: 最終挑戰
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = true,
            survivalTime = 250,
            scoreReward = 500,
            experienceReward = 300
        };
    }
}
