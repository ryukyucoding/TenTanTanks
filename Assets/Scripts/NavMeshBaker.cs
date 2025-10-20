using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    [Header("Build Settings")]
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        if (buildOnStart)
        {
            BuildNavMesh();
        }
    }
    
    [ContextMenu("Build NavMesh")]
    public void BuildNavMesh()
    {
        // 使用Unity 6的簡化方法
        // 獲取所有Navigation Static物件
        var sources = new System.Collections.Generic.List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(transform, 0, NavMeshCollectGeometry.RenderMeshes, 0, new System.Collections.Generic.List<NavMeshBuildMarkup>(), sources);
        
        // 定義NavMesh的邊界
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
        
        // 使用默認設置構建NavMesh
        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
        NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(buildSettings, sources, bounds, Vector3.zero, Quaternion.identity);
        
        if (navMeshData != null)
        {
            // 移除舊的NavMesh
            NavMesh.RemoveAllNavMeshData();
            
            // 添加新的NavMesh
            NavMesh.AddNavMeshData(navMeshData);
            
            if (showDebugInfo)
            {
                Debug.Log("NavMesh built successfully!");
                Debug.Log($"NavMesh bounds: {navMeshData.sourceBounds}");
                Debug.Log($"Sources collected: {sources.Count}");
            }
        }
        else
        {
            Debug.LogError("Failed to build NavMesh! Make sure you have Navigation Static objects in the scene.");
        }
    }
    
    void OnDrawGizmos()
    {
        if (showDebugInfo)
        {
            // 繪製NavMesh邊界
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 1000f);
        }
    }
}
