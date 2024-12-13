using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/Dijkstra")]
public class DijkstraAlgorithm : PathfindingAlgorithm
{
    private MetricsManager metrics = new MetricsManager(); // Initialize MetricsManager

    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        metrics.StartTracking(); // Start collecting metrics

        // Priority queue setup using a Dictionary for node costs
        Dictionary<PathNode, float> nodeCost = new Dictionary<PathNode, float>();
        Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();
        List<PathNode> visitedNodes = new List<PathNode>();

        // Initialize costs to infinity
        foreach (PathNode node in allNodes)
        {
            nodeCost[node] = Mathf.Infinity;
        }

        // Set start node cost to 0
        nodeCost[startNode] = 0;
        List<PathNode> openSet = new List<PathNode> { startNode };

        while (openSet.Count > 0)
        {
            PathNode currentNode = GetNodeWithLowestCost(openSet, nodeCost);
            openSet.Remove(currentNode);
            visitedNodes.Add(currentNode);
            metrics.NodeExpanded(); // Count expanded node

            // If target node is reached, stop and measure
            if (currentNode == targetNode)
            {
                List<Vector2> finalPath = ReconstructPath(cameFrom, currentNode);
                metrics.StopTracking(finalPath);
                metrics.PrintMetrics("Dijkstra Algorithm"); // Output metrics
                return finalPath;
            }

            // Evaluate neighbors
            foreach (PathNode neighbor in currentNode.neighbors)
            {
                if (neighbor.isBlocked || visitedNodes.Contains(neighbor))
                    continue;

                float tentativeCost = nodeCost[currentNode] + Vector2.Distance(currentNode.nodePosition, neighbor.nodePosition);

                if (tentativeCost < nodeCost[neighbor])
                {
                    nodeCost[neighbor] = tentativeCost;
                    cameFrom[neighbor] = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        metrics.StopTracking(new List<Vector2>()); // Stop tracking for failed path
        metrics.PrintMetrics("Dijkstra Algorithm");
        Debug.LogWarning("Dijkstra: No path found!");
        return new List<Vector2>();
    }

    private PathNode GetNodeWithLowestCost(List<PathNode> openSet, Dictionary<PathNode, float> nodeCost)
    {
        PathNode lowestCostNode = openSet[0];
        float lowestCost = nodeCost[lowestCostNode];

        foreach (PathNode node in openSet)
        {
            if (nodeCost[node] < lowestCost)
            {
                lowestCost = nodeCost[node];
                lowestCostNode = node;
            }
        }

        return lowestCostNode;
    }

    private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode currentNode)
    {
        List<Vector2> path = new List<Vector2>();
        while (cameFrom.ContainsKey(currentNode))
        {
            path.Add(currentNode.nodePosition);
            currentNode = cameFrom[currentNode];
        }
        path.Reverse();
        return path;
    }

    public override void HandleBlockedPath(GameManager manager, PathNode blockedNode)
    {
        Debug.Log("Dijkstra: Blocked path detected! Recalculating...");

        PathNode startNode = manager.FindClosestNodeToPlayer();
        PathNode targetNode = manager.FindClosestNodeToTarget();

        if (startNode != null && targetNode != null)
        {
            List<Vector2> newPath = CalculatePath(startNode, targetNode, manager.allNodes);
            manager.player.SetPath(newPath);
        }
    }
}
