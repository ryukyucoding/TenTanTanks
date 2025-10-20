using UnityEngine;

[System.Serializable]
public class AIBehavior
{
    public string label;
    public float value;

    public AIBehavior(string label)
    {
        this.label = label;
        this.value = 0f;
    }

    public bool IsModOf(float remainder)
    {
        if (remainder == 0)
            return false;
        return value % remainder < Time.deltaTime;
    }

    public void Reset()
    {
        value = 0f;
    }
}

public static class AIBehaviorExtensions
{
    public static AIBehavior FromName(this AIBehavior[] arr, string name)
    {
        foreach (var behavior in arr)
        {
            if (behavior.label == name)
                return behavior;
        }
        return null;
    }
}
