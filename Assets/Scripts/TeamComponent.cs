using UnityEngine;

public class TeamComponent : MonoBehaviour
{
    [Header("Team Settings")]
    [SerializeField] public int team = 0;
    [SerializeField] private string teamName = "Neutral";
    [SerializeField] private Color teamColor = Color.white;
    
    [Header("Debug")]
    [SerializeField] private bool showTeamInfo = false;
    
    public int Team => team;
    public string TeamName => teamName;
    public Color TeamColor => teamColor;
    
    public void SetTeam(int newTeam, string newTeamName = "", Color newTeamColor = default)
    {
        team = newTeam;
        
        if (!string.IsNullOrEmpty(newTeamName))
        {
            teamName = newTeamName;
        }
        
        if (newTeamColor != default)
        {
            teamColor = newTeamColor;
        }
        
        // 更新視覺效果
        UpdateTeamVisuals();
    }
    
    private void UpdateTeamVisuals()
    {
        // 更新坦克顏色或其他視覺元素
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material != null)
            {
                renderer.material.color = teamColor;
            }
        }
    }
    
    public bool IsEnemy(TeamComponent other)
    {
        if (other == null) return false;
        return team != other.team && team != 0 && other.team != 0;
    }
    
    public bool IsAlly(TeamComponent other)
    {
        if (other == null) return false;
        return team == other.team && team != 0;
    }
    
    public bool IsNeutral(TeamComponent other)
    {
        if (other == null) return true;
        return team == 0 || other.team == 0;
    }
    
    void OnDrawGizmos()
    {
        if (!showTeamInfo) return;
        
        // 繪製團隊信息
        Gizmos.color = teamColor;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // 繪製團隊標籤
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Team {team}: {teamName}");
        #endif
    }
}
