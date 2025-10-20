using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
    // 8方向移動（包含對角線）
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // 下
        new Vector2Int(0, -1),  // 上
        new Vector2Int(1, 0),   // 右
        new Vector2Int(-1, 0),  // 左
        new Vector2Int(1, 1),   // 右下
        new Vector2Int(1, -1),  // 右上
        new Vector2Int(-1, 1),  // 左下
        new Vector2Int(-1, -1)  // 左上
    };

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, System.Func<Vector2Int, bool> isWalkable)
    {
        var startNode = new AStarNode(start);
        startNode.gCost = 0;
        startNode.hCost = ManhattanDistance(start, end);

        var openList = new List<AStarNode> { startNode };
        var closedList = new HashSet<Vector2Int>();

        while (openList.Count > 0)
        {
            var currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || 
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            if (currentNode.position == end)
            {
                return RetracePath(startNode, currentNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.position);

            foreach (var direction in Directions)
            {
                Vector2Int neighborPos = currentNode.position + direction;

                // 禁止對角線穿越
                if (IsDiagonalMovementBlocked(currentNode.position, direction, isWalkable))
                    continue;

                if (!isWalkable(neighborPos) || closedList.Contains(neighborPos))
                    continue;

                // 對角線移動成本更高
                float newMovementCostToNeighbor = currentNode.gCost +
                    (direction.x != 0 && direction.y != 0 ? 1.414f : 1);

                AStarNode existingNeighbor = null;
                foreach (var node in openList)
                {
                    if (node.position == neighborPos)
                    {
                        existingNeighbor = node;
                        break;
                    }
                }

                if (existingNeighbor == null)
                {
                    var neighborNode = new AStarNode(neighborPos)
                    {
                        gCost = newMovementCostToNeighbor,
                        hCost = ManhattanDistance(neighborPos, end),
                        parent = currentNode
                    };
                    openList.Add(neighborNode);
                }
                else if (newMovementCostToNeighbor < existingNeighbor.gCost)
                {
                    existingNeighbor.gCost = newMovementCostToNeighbor;
                    existingNeighbor.parent = currentNode;
                }
            }
        }

        // 找不到路徑
        return new List<Vector2Int>();
    }

    private static bool IsDiagonalMovementBlocked(Vector2Int currentPos, Vector2Int direction, System.Func<Vector2Int, bool> isWalkable)
    {
        // 如果是對角線移動，檢查相鄰的水平和垂直格子
        if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 2)
            return false;

        Vector2Int horizontalNeighbor = new Vector2Int(currentPos.x + direction.x, currentPos.y);
        Vector2Int verticalNeighbor = new Vector2Int(currentPos.x, currentPos.y + direction.y);

        return !isWalkable(horizontalNeighbor) || !isWalkable(verticalNeighbor);
    }

    private static float ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static List<Vector2Int> RetracePath(AStarNode startNode, AStarNode endNode)
    {
        var path = new List<Vector2Int>();
        var currentNode = endNode;

        while (currentNode != null && currentNode.position != startNode.position)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}

public class AStarNode
{
    public Vector2Int position;
    public float gCost;
    public float hCost;
    public AStarNode parent;

    public float fCost => gCost + hCost;

    public AStarNode(Vector2Int position)
    {
        this.position = position;
    }
}
