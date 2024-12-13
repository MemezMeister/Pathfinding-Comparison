using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics; // For Stopwatch

[CreateAssetMenu(menuName = "Pathfinding/FringeSearch")]
public class FringeSearchAlgorithm : PathfindingAlgorithm
{
    private MetricsManager metrics = new MetricsManager(); // Metrics Manager Instance

    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        metrics.StartTracking(); // Start metrics tracking

        float threshold = Vector2.Distance(startNode.nodePosition, targetNode.nodePosition);
        HashSet<PathNode> visitedNodes = new HashSet<PathNode>();
        Queue<PathNode> nowList = new Queue<PathNode>();
        List<PathNode> laterList = new List<PathNode>();
        Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();

        nowList.Enqueue(startNode);
        visitedNodes.Add(startNode);

        int iterationLimit = 500; // Prevent excessive iterations
        int iterationCount = 0;

        while (nowList.Count > 0)
        {
            iterationCount++;
            metrics.NodeExpanded(); // Increment nodes expanded

            if (iterationCount > iterationLimit)
            {
                UnityEngine.Debug.LogWarning("Fringe Search: Iteration limit reached. Terminating.");
                break;
            }

            PathNode currentNode = nowList.Dequeue();

            // Target Reached
            if (currentNode == targetNode)
            {
                List<Vector2> path = ReconstructPath(cameFrom, targetNode);
                metrics.StopTracking(path); // Stop metrics and calculate
                metrics.PrintMetrics("Fringe Search");
                return path;
            }

            // Process neighbors
            foreach (PathNode neighbor in currentNode.neighbors)
            {
                if (visitedNodes.Contains(neighbor)) continue;

                // Dynamically handle blocked paths
                if (neighbor.isBlocked)
                {
                    if (!laterList.Contains(neighbor))
                        laterList.Add(neighbor);
                    continue;
                }

                float cost = Vector2.Distance(startNode.nodePosition, neighbor.nodePosition) +
                             Vector2.Distance(neighbor.nodePosition, targetNode.nodePosition);

                if (cost <= threshold)
                {
                    nowList.Enqueue(neighbor);
                    cameFrom[neighbor] = currentNode;
                    visitedNodes.Add(neighbor);
                }
                else if (!laterList.Contains(neighbor))
                {
                    laterList.Add(neighbor);
                }
            }

            // Adjust threshold dynamically if Now List is empty
            if (nowList.Count == 0 && laterList.Count > 0)
            {
                UnityEngine.Debug.Log("Fringe Search: Adjusting threshold due to blocked paths.");
                threshold = AdjustThreshold(threshold, laterList, targetNode);
                nowList = MoveNodesToNowList(laterList);
                laterList.Clear();
            }
        }

        UnityEngine.Debug.LogWarning("Fringe Search: No path found.");
        metrics.StopTracking(null); // Stop metrics (no path)
        metrics.PrintMetrics("Fringe Search");
        return new List<Vector2>();
    }

    private float AdjustThreshold(float currentThreshold, List<PathNode> laterList, PathNode targetNode)
    {
        float minCost = float.MaxValue;

        foreach (PathNode node in laterList)
        {
            float cost = Vector2.Distance(node.nodePosition, targetNode.nodePosition);
            if (cost < minCost) minCost = cost;
        }

        return Mathf.Max(currentThreshold + 1.0f, minCost);
    }

    public override void HandleBlockedPath(GameManager manager, PathNode blockedNode)
    {
        UnityEngine.Debug.Log("Fringe Search: Dynamic obstacle encountered. Adjusting path...");
        if (blockedNode != null) blockedNode.isBlocked = true;

        PathNode startNode = manager.FindClosestNodeToPlayer();
        PathNode targetNode = manager.FindClosestNodeToTarget();

        if (startNode != null && targetNode != null)
        {
            List<Vector2> newPath = CalculatePath(startNode, targetNode, manager.allNodes);
            if (newPath.Count > 0)
            {
                manager.player.SetPath(newPath);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Fringe Search: No valid path found during partial recalculation.");
            }
        }
    }

    private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode currentNode)
    {
        List<Vector2> path = new List<Vector2>();
        HashSet<PathNode> visitedInPath = new HashSet<PathNode>();

        while (cameFrom.ContainsKey(currentNode))
        {
            if (visitedInPath.Contains(currentNode))
            {
                UnityEngine.Debug.LogWarning("Fringe Search: Circular path detected during reconstruction.");
                break;
            }

            visitedInPath.Add(currentNode);
            path.Add(currentNode.nodePosition);
            currentNode = cameFrom[currentNode];
        }

        path.Reverse();
        return path;
    }

    private Queue<PathNode> MoveNodesToNowList(List<PathNode> laterList)
    {
        Queue<PathNode> nowList = new Queue<PathNode>();
        foreach (PathNode node in laterList)
        {
            if (!node.isBlocked)
            {
                nowList.Enqueue(node);
            }
        }
        return nowList;
    }
}
