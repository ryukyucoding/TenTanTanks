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
    public WaveConfig[] waves;
    public float survivalTime = 60f;  // 關卡時間限制（秒）
}

/// <summary>
/// 波數配置
/// </summary>
[System.Serializable]
public class WaveConfig
{
    public EnemyConfig[] enemies;

    [Tooltip("舊系統：相對延遲時間（從上一波結束後等待）")]
    public float waveDelay = 1f;

    [Tooltip("新系統：絕對生成時間（從關卡開始後第幾秒生成）。如果 > 0，會覆蓋 waveDelay")]
    public float spawnTime = 0f;

    [Tooltip("同一波中，每個敵人生成的間隔秒數")]
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
            survivalTime = 30,
            waves = new[]
            {
                // Wave 1: 1 個土色敵人（關卡開始時立即出現）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 0 }
                    },
                    spawnTime = 0f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 1 個土色 + 1 個綠色（8 秒後出現）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 1 }
                    },
                    spawnTime = 8f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            }
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
            survivalTime = 45,
            waves = new[]
            {
                // Wave 1: 1 個灰色 + 1 個土色（立即出現）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    spawnTime = 0f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 1 個灰色 + 1 個綠色（15 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 }
                    },
                    spawnTime = 8f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 3: 1 個紫色敵人（30 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 3 }
                    },
                    spawnTime = 18f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            }
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
            survivalTime = 60,
            waves = new[]
            {
                // Wave 1: 2 個灰色敵人（立即出現）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 }
                    },
                    spawnTime = 0f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 1 個紫色 + 1 個綠色（15 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 3 }
                    },
                    spawnTime = 8f,
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 3: 1 個灰色 + 1 個綠色 + 1 個土色（30 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    spawnTime = 18f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            }
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
            survivalTime = 90,
            waves = new[]
            {
                // Wave 1: 2 個綠色敵人（立即出現）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 }
                    },
                    spawnTime = 0f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 2 個灰色 + 1 個土色（10 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 3 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 0 }
                    },
                    spawnTime = 10f,
                    waveDelay = 1.5f,
                    spawnInterval = 0f
                },
                // Wave 3: 2 個紫色敵人（25 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 3 }
                    },
                    spawnTime = 20f,
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 4: 混合波（40 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    spawnTime = 35f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            }
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
            survivalTime = 120,
            waves = new[]
            {
                // Wave 1: 3 個灰色敵人（立即出現）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 2 }
                    },
                    spawnTime = 0f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                },
                // Wave 2: 2 個紫色 + 1 個綠色（30 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 3 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 }
                    },
                    spawnTime = 15f,
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 3: 2 個綠色 + 2 個土色（60 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    spawnTime = 30f,
                    waveDelay = 1.5f,
                    spawnInterval = 0f
                },
                // Wave 4: 終極波次 - 所有類型敵人（100 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_GRAY, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_GREEN, spawnPointIndex = 3 }
                    },
                    spawnTime = 50f,
                    waveDelay = 2f,
                    spawnInterval = 0f
                },
                // Wave 5: 最終挑戰（150 秒後）
                new WaveConfig
                {
                    enemies = new[]
                    {
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 0 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 1 },
                        new EnemyConfig { prefabKey = ENEMY_PURPLE, spawnPointIndex = 2 },
                        new EnemyConfig { prefabKey = ENEMY_SOIL, spawnPointIndex = 3 }
                    },
                    spawnTime = 70f,
                    waveDelay = 1f,
                    spawnInterval = 0f
                }
            }
        };
    }
}
