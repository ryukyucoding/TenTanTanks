using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathfindingNode
{
    public Vector3 position;
    public bool isWalkable = true;
    public float gCost = 0f;
    public float hCost = 0f;
    public float fCost => gCost + hCost;
    public PathfindingNode parent;
    public List<PathfindingNode> neighbors = new List<PathfindingNode>();
    
    public PathfindingNode(Vector3 pos, bool walkable = true)
    {
        position = pos;
        isWalkable = walkable;
    }
    
    public void AddNeighbor(PathfindingNode neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
        }
    }
    
    public void RemoveNeighbor(PathfindingNode neighbor)
    {
        neighbors.Remove(neighbor);
    }
    
    public float DistanceTo(PathfindingNode other)
    {
        return Vector3.Distance(position, other.position);
    }
    
    public void Reset()
    {
        gCost = 0f;
        hCost = 0f;
        parent = null;
    }
}
