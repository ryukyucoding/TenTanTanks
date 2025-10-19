using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Config Database", menuName = "AI/AI Config Database")]
public class AIConfigDatabase : ScriptableObject
{
    [Header("AI Personalities")]
    public List<AIPersonalityData> personalities = new List<AIPersonalityData>();
    
    [Header("Tank Units")]
    public List<TankUnitData> tankUnits = new List<TankUnitData>();
    
    public AIConfig GetAIConfig(string personalityName)
    {
        foreach (var personality in personalities)
        {
            if (personality.name == personalityName)
            {
                return personality.config;
            }
        }
        
        Debug.LogWarning($"AI personality '{personalityName}' not found, using default");
        return new AIConfig();
    }
    
    public TankUnitConfig GetTankUnitConfig(string unitName)
    {
        foreach (var unit in tankUnits)
        {
            if (unit.name == unitName)
            {
                return unit.config;
            }
        }
        
        Debug.LogWarning($"Tank unit '{unitName}' not found, using default");
        return new TankUnitConfig();
    }
}

[System.Serializable]
public class TankUnitData
{
    public string name;
    public TankUnitConfig config;
    public string description;
    public GameObject prefab;
}
