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
        
        // 加載敵人 prefab（使用 GUID）
        string guid1 = "90166595365399f4e961287e558349aa";
        string guid2 = "b38e840a27da14bf39e56459edb46314";
        GameObject enemyPrefab1 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid1));
        GameObject enemyPrefab2 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid2));
        
        if (enemyPrefab1 == null)
            Debug.LogWarning($"無法找到 GUID {guid1} 對應的敵人 prefab");
        if (enemyPrefab2 == null)
            Debug.LogWarning($"無法找到 GUID {guid2} 對應的敵人 prefab");
        
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
                    enemyPrefab = null,
                    waveDelay = 1,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab1,
                            spawnPointIndex = 0
                        }
                    }
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    enemyPrefab = null,
                    waveDelay = 1,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab1,
                            spawnPointIndex = 2
                        },
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab2,
                            spawnPointIndex = 4
                        }
                    }
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
        
        // 加載敵人 prefab（使用 GUID）
        string guid1 = "7dd4d53b6647040aab6329fc125558b8";
        string guid2 = "90166595365399f4e961287e558349aa";
        string guid3 = "b38e840a27da14bf39e56459edb46314";
        string guid4 = "ba39c04b07fc440869d220d7c670b06d";
        GameObject enemyPrefab1 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid1));
        GameObject enemyPrefab2 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid2));
        GameObject enemyPrefab3 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid3));
        GameObject enemyPrefab4 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid4));
        
        if (enemyPrefab1 == null) Debug.LogWarning($"無法找到 GUID {guid1} 對應的敵人 prefab");
        if (enemyPrefab2 == null) Debug.LogWarning($"無法找到 GUID {guid2} 對應的敵人 prefab");
        if (enemyPrefab3 == null) Debug.LogWarning($"無法找到 GUID {guid3} 對應的敵人 prefab");
        if (enemyPrefab4 == null) Debug.LogWarning($"無法找到 GUID {guid4} 對應的敵人 prefab");
        
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
                    enemyPrefab = null,
                    waveDelay = 1,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab1,
                            spawnPointIndex = 0
                        },
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab2,
                            spawnPointIndex = 3
                        }
                    }
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    enemyPrefab = null,
                    waveDelay = 1,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab1,
                            spawnPointIndex = 1
                        },
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab3,
                            spawnPointIndex = 2
                        }
                    }
                },
                new EnemyWave
                {
                    enemyCount = 1,
                    enemyPrefab = null,
                    waveDelay = 1,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab4,
                            spawnPointIndex = 3
                        }
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
        
        // 加載敵人 prefab（使用 GUID）
        string guid1 = "7dd4d53b6647040aab6329fc125558b8";
        string guid2 = "ba39c04b07fc440869d220d7c670b06d";
        string guid3 = "b38e840a27da14bf39e56459edb46314";
        string guid4 = "90166595365399f4e961287e558349aa";
        GameObject enemyPrefab1 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid1));
        GameObject enemyPrefab2 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid2));
        GameObject enemyPrefab3 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid3));
        GameObject enemyPrefab4 = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(guid4));
        
        if (enemyPrefab1 == null) Debug.LogWarning($"無法找到 GUID {guid1} 對應的敵人 prefab");
        if (enemyPrefab2 == null) Debug.LogWarning($"無法找到 GUID {guid2} 對應的敵人 prefab");
        if (enemyPrefab3 == null) Debug.LogWarning($"無法找到 GUID {guid3} 對應的敵人 prefab");
        if (enemyPrefab4 == null) Debug.LogWarning($"無法找到 GUID {guid4} 對應的敵人 prefab");
        
        levelAsset.levelData = new LevelData
        {
            levelName = "關卡 3 - 生存挑戰",
            levelDescription = "在限定時間內生存下來，敵人會越來越強",
            timeLimit = 120,
            enemyWaves = new System.Collections.Generic.List<EnemyWave>
            {
                new EnemyWave
                {
                    enemyCount = 2,
                    enemyPrefab = null,
                    waveDelay = 1,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab1,
                            spawnPointIndex = 0
                        },
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab1,
                            spawnPointIndex = 1
                        }
                    }
                },
                new EnemyWave
                {
                    enemyCount = 2,
                    enemyPrefab = null,
                    waveDelay = 2,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab2,
                            spawnPointIndex = 2
                        },
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab3,
                            spawnPointIndex = 3
                        }
                    }
                },
                new EnemyWave
                {
                    enemyCount = 3,
                    enemyPrefab = null,
                    waveDelay = 1,
                    spawnInterval = 0,
                    enemyEntries = new EnemySpawnEntry[]
                    {
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab1,
                            spawnPointIndex = 0
                        },
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab3,
                            spawnPointIndex = 1
                        },
                        new EnemySpawnEntry
                        {
                            enemyPrefab = enemyPrefab4,
                            spawnPointIndex = 3
                        }
                    }
                }
            },
            requireAllEnemiesDefeated = true,
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

