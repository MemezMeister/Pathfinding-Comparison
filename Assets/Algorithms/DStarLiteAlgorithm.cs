using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/DStarLite")]
public class DStarLiteAlgorithm : PathfindingAlgorithm
{
    private Dictionary<PathNode, float> gCost;
    private Dictionary<PathNode, float> rhsCost;
    private PriorityQueue<PathNode> openSet;
    private Dictionary<PathNode, PathNode> cameFrom; // Track parents for path reconstruction
    private PathNode startNode, goalNode;

    private float Heuristic(PathNode a, PathNode b)
    {
        return Mathf.Abs(a.nodePosition.x - b.nodePosition.x) + Mathf.Abs(a.nodePosition.y - b.nodePosition.y); // Manhattan Distance
    }

    private float CalculateKey(PathNode node)
    {
        float minCost = Mathf.Min(gCost[node], rhsCost[node]);
        return minCost + Heuristic(node, goalNode);
    }

    public override List<Vector2> CalculatePath(PathNode start, PathNode goal, List<PathNode> allNodes)
    {
        startNode = start;
        goalNode = goal;

        // Initialize
        gCost = new Dictionary<PathNode, float>();
        rhsCost = new Dictionary<PathNode, float>();
        cameFrom = new Dictionary<PathNode, PathNode>();
        openSet = new PriorityQueue<PathNode>();

        foreach (var node in allNodes)
        {
            gCost[node] = Mathf.Infinity;
            rhsCost[node] = Mathf.Infinity;
        }

        rhsCost[goalNode] = 0;
        openSet.Enqueue(goalNode, CalculateKey(goalNode));

        return ComputeShortestPath();
    }

    private List<Vector2> ComputeShortestPath()
    {
        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet.Dequeue();

            if (currentNode == startNode)
                break;

            foreach (PathNode neighbor in currentNode.neighbors) // Only expand to valid neighbors
            {
                if (neighbor.isBlocked) continue;

                float tentativeCost = gCost[currentNode] + Vector2.Distance(currentNode.nodePosition, neighbor.nodePosition);

                if (tentativeCost < gCost[neighbor])
                {
                    gCost[neighbor] = tentativeCost;
                    rhsCost[neighbor] = tentativeCost;
                    cameFrom[neighbor] = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, CalculateKey(neighbor));
                }
            }
        }

        return ReconstructPath();
    }

    private List<Vector2> ReconstructPath()
    {
        List<Vector2> path = new List<Vector2>();
        PathNode currentNode = startNode;

        while (cameFrom.ContainsKey(currentNode))
        {
            path.Add(currentNode.nodePosition);
            currentNode = cameFrom[currentNode];
        }

        if (path.Count == 0)
        {
            Debug.LogWarning("D*-Lite: ReconstructPath - No valid path found!");
        }

        path.Reverse();
        return path;
    }

    public override void HandleBlockedPath(GameManager manager, PathNode blockedNode)
    {
        Debug.Log("D*-Lite: Handling blocked path...");

        blockedNode.isBlocked = true;

        foreach (PathNode neighbor in blockedNode.neighbors)
        {
            if (!neighbor.isBlocked)
            {
                float cost = gCost[neighbor] + Vector2.Distance(blockedNode.nodePosition, neighbor.nodePosition);
                rhsCost[blockedNode] = Mathf.Min(rhsCost[blockedNode], cost);
            }
        }

        // Recompute path
        List<Vector2> newPath = ComputeShortestPath();

        if (newPath.Count > 0)
        {
            Debug.Log("D*-Lite: Path recalculated.");
            manager.player.SetPath(newPath); // Pass the updated path to player
        }
        else
        {
            Debug.LogWarning("D*-Lite: No valid path found after recalculation.");
        }
    }
}
