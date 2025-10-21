using UnityEngine;
using UnityEditor;

public class LevelDataCreator : EditorWindow
{
    [MenuItem("Tools/Create Level Data")]
    public static void ShowWindow()
    {
        GetWindow<LevelDataCreator>("Level Data Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("關卡數據創建器", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("創建示例關卡 1 - 新手訓練"))
        {
            CreateLevel1();
        }
        
        if (GUILayout.Button("創建示例關卡 2 - 挑戰開始"))
        {
            CreateLevel2();
        }
        
        if (GUILayout.Button("創建示例關卡 3 - 生存挑戰"))
        {
            CreateLevel3();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("使用說明：", EditorStyles.boldLabel);
        GUILayout.Label("1. 點擊上面的按鈕創建示例關卡");
        GUILayout.Label("2. 關卡文件會保存在 Assets/Scripts/LevelSystem/LevelConfigs/ 目錄下");
        GUILayout.Label("3. 創建後可以在Project窗口中編輯關卡設定");
    }

    private void CreateLevel1()
    {
        var levelAsset = CreateInstance<LevelDataAsset>();
        levelAsset.levelData = new LevelData
        {
            levelName = "關卡 1 - 新手訓練",
            levelDescription = "學習基本操作，消滅所有敵人",
            timeLimit = 0,
            enemyWaves = new System.Collections.Generic.List<EnemyWave>
            {
                new EnemyWave
                {
                    enemyCount = 1,
                    waveDelay = 2,
                    spawnInterval = 1,
                    statsModifier = new EnemyStatsModifier()
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    waveDelay = 3,
                    spawnInterval = 0.5f,
                    statsModifier = new EnemyStatsModifier()
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = false,
            survivalTime = 60,
            scoreReward = 100,
            experienceReward = 50
        };
        levelAsset.difficulty = 1;
        levelAsset.unlockCondition = "";

        SaveLevelAsset(levelAsset, "Level1_Data");
    }

    private void CreateLevel2()
    {
        var levelAsset = CreateInstance<LevelDataAsset>();
        levelAsset.levelData = new LevelData
        {
            levelName = "關卡 2 - 挑戰開始",
            levelDescription = "面對更多敵人，測試你的技能",
            timeLimit = 180,
            enemyWaves = new System.Collections.Generic.List<EnemyWave>
            {
                new EnemyWave
                {
                    enemyCount = 2,
                    waveDelay = 2,
                    spawnInterval = 1,
                    statsModifier = new EnemyStatsModifier
                    {
                        healthMultiplier = 1.2f
                    }
                },
                new EnemyWave
                {
                    enemyCount = 3,
                    waveDelay = 4,
                    spawnInterval = 0.8f,
                    statsModifier = new EnemyStatsModifier
                    {
                        healthMultiplier = 1.2f,
                        speedMultiplier = 1.1f,
                        damageMultiplier = 1.1f
                    }
                },
                new EnemyWave
                {
                    enemyCount = 1,
                    waveDelay = 3,
                    spawnInterval = 1,
                    statsModifier = new EnemyStatsModifier
                    {
                        healthMultiplier = 1.5f,
                        speedMultiplier = 1.2f,
                        damageMultiplier = 1.3f,
                        fireRateMultiplier = 1.2f
                    }
                }
            },
            requireAllEnemiesDefeated = true,
            requireSurviveTime = false,
            survivalTime = 60,
            scoreReward = 200,
            experienceReward = 100
        };
        levelAsset.difficulty = 2;
        levelAsset.unlockCondition = "完成關卡 1";

        SaveLevelAsset(levelAsset, "Level2_Data");
    }

    private void CreateLevel3()
    {
        var levelAsset = CreateInstance<LevelDataAsset>();
        levelAsset.levelData = new LevelData
        {
            levelName = "關卡 3 - 生存挑戰",
            levelDescription = "在限定時間內生存下來，敵人會越來越強",
            timeLimit = 120,
            enemyWaves = new System.Collections.Generic.List<EnemyWave>
            {
                new EnemyWave
                {
                    enemyCount = 1,
                    waveDelay = 1,
                    spawnInterval = 1,
                    statsModifier = new EnemyStatsModifier()
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    waveDelay = 2,
                    spawnInterval = 0.5f,
                    statsModifier = new EnemyStatsModifier
                    {
                        healthMultiplier = 1.3f,
                        speedMultiplier = 1.1f,
                        damageMultiplier = 1.2f,
                        fireRateMultiplier = 1.1f
                    }
                },
                new EnemyWave
                {
                    enemyCount = 3,
                    waveDelay = 2,
                    spawnInterval = 0.3f,
                    statsModifier = new EnemyStatsModifier
                    {
                        healthMultiplier = 1.5f,
                        speedMultiplier = 1.2f,
                        damageMultiplier = 1.4f,
                        fireRateMultiplier = 1.3f
                    }
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    waveDelay = 1,
                    spawnInterval = 0.2f,
                    statsModifier = new EnemyStatsModifier
                    {
                        healthMultiplier = 2f,
                        speedMultiplier = 1.3f,
                        damageMultiplier = 1.6f,
                        fireRateMultiplier = 1.5f
                    }
                }
            },
            requireAllEnemiesDefeated = false,
            requireSurviveTime = true,
            survivalTime = 120,
            scoreReward = 300,
            experienceReward = 150
        };
        levelAsset.difficulty = 3;
        levelAsset.unlockCondition = "完成關卡 2";

        SaveLevelAsset(levelAsset, "Level3_Data");
    }

    private void SaveLevelAsset(LevelDataAsset levelAsset, string fileName)
    {
        string path = "Assets/Scripts/LevelSystem/LevelConfigs/";
        
        // 確保目錄存在
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/LevelSystem", "LevelConfigs");
        }
        
        string fullPath = path + fileName + ".asset";
        AssetDatabase.CreateAsset(levelAsset, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"關卡數據已創建: {fullPath}");
        
        // 選中新創建的資源
        Selection.activeObject = levelAsset;
        EditorGUIUtility.PingObject(levelAsset);
    }
}

