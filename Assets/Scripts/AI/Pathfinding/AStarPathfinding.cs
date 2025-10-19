using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    public float nodeSpacing = 1f;
    public LayerMask obstacleLayer = 1;
    public bool showDebugGizmos = false;
    
    private List<PathfindingNode> allNodes = new List<PathfindingNode>();
    private List<PathfindingNode> openList = new List<PathfindingNode>();
    private List<PathfindingNode> closedList = new List<PathfindingNode>();
    
    public void InitializePathfinding(Vector3 mapCenter, Vector3 mapSize)
    {
        GenerateNodes(mapCenter, mapSize);
        ConnectNodes();
    }
    
    private void GenerateNodes(Vector3 mapCenter, Vector3 mapSize)
    {
        allNodes.Clear();
        
        int nodesX = Mathf.RoundToInt(mapSize.x / nodeSpacing);
        int nodesZ = Mathf.RoundToInt(mapSize.z / nodeSpacing);
        
        Vector3 startPos = mapCenter - mapSize / 2f;
        
        for (int x = 0; x < nodesX; x++)
        {
            for (int z = 0; z < nodesZ; z++)
            {
                Vector3 nodePos = startPos + new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
                
                // 檢查是否有障礙物
                bool isWalkable = !Physics.CheckSphere(nodePos, nodeSpacing * 0.4f, obstacleLayer);
                
                PathfindingNode node = new PathfindingNode(nodePos, isWalkable);
                allNodes.Add(node);
            }
        }
    }
    
    private void ConnectNodes()
    {
        foreach (PathfindingNode node in allNodes)
        {
            if (!node.isWalkable) continue;
            
            foreach (PathfindingNode otherNode in allNodes)
            {
                if (otherNode == node || !otherNode.isWalkable) continue;
                
                float distance = Vector3.Distance(node.position, otherNode.position);
                if (distance <= nodeSpacing * 1.5f) // 只連接相鄰節點
                {
                    node.AddNeighbor(otherNode);
                }
            }
        }
    }
    
    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
    {
        PathfindingNode startNode = GetClosestNode(startPos);
        PathfindingNode endNode = GetClosestNode(endPos);
        
        if (startNode == null || endNode == null || !startNode.isWalkable || !endNode.isWalkable)
        {
            return new List<Vector3>();
        }
        
        return FindPath(startNode, endNode);
    }
    
    private List<Vector3> FindPath(PathfindingNode startNode, PathfindingNode endNode)
    {
        openList.Clear();
        closedList.Clear();
        
        // 重置所有節點
        foreach (PathfindingNode node in allNodes)
        {
            node.Reset();
        }
        
        openList.Add(startNode);
        
        while (openList.Count > 0)
        {
            PathfindingNode currentNode = GetLowestFCostNode();
            
            if (currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }
            
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            
            foreach (PathfindingNode neighbor in currentNode.neighbors)
            {
                if (!neighbor.isWalkable || closedList.Contains(neighbor))
                    continue;
                
                float newGCost = currentNode.gCost + currentNode.DistanceTo(neighbor);
                
                if (newGCost < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = neighbor.DistanceTo(endNode);
                    neighbor.parent = currentNode;
                    
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }
        
        return new List<Vector3>(); // 沒有找到路徑
    }
    
    private PathfindingNode GetLowestFCostNode()
    {
        PathfindingNode lowestFCostNode = openList[0];
        
        foreach (PathfindingNode node in openList)
        {
            if (node.fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = node;
            }
        }
        
        return lowestFCostNode;
    }
    
    private List<Vector3> RetracePath(PathfindingNode startNode, PathfindingNode endNode)
    {
        List<Vector3> path = new List<Vector3>();
        PathfindingNode currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        return path;
    }
    
    private PathfindingNode GetClosestNode(Vector3 position)
    {
        PathfindingNode closestNode = null;
        float closestDistance = float.MaxValue;
        
        foreach (PathfindingNode node in allNodes)
        {
            if (!node.isWalkable) continue;
            
            float distance = Vector3.Distance(position, node.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }
        
        return closestNode;
    }
    
    public PathfindingNode GetRandomWalkableNode()
    {
        List<PathfindingNode> walkableNodes = new List<PathfindingNode>();
        
        foreach (PathfindingNode node in allNodes)
        {
            if (node.isWalkable)
            {
                walkableNodes.Add(node);
            }
        }
        
        if (walkableNodes.Count > 0)
        {
            return walkableNodes[Random.Range(0, walkableNodes.Count)];
        }
        
        return null;
    }
    
    public List<PathfindingNode> GetNodesInRadius(Vector3 center, float radius)
    {
        List<PathfindingNode> nodesInRadius = new List<PathfindingNode>();
        
        foreach (PathfindingNode node in allNodes)
        {
            if (node.isWalkable && Vector3.Distance(center, node.position) <= radius)
            {
                nodesInRadius.Add(node);
            }
        }
        
        return nodesInRadius;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        foreach (PathfindingNode node in allNodes)
        {
            Gizmos.color = node.isWalkable ? Color.green : Color.red;
            Gizmos.DrawWireSphere(node.position, nodeSpacing * 0.2f);
        }
    }
}
