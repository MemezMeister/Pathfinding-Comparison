using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/FringeSearch")]
public class FringeSearchAlgorithm : PathfindingAlgorithm
{
    private MetricsManager metrics = new MetricsManager();
    private PathNode originalTargetNode; // Store the original target node
    private HashSet<PathNode> visitedNodes;
    private Queue<PathNode> nowList;

    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        metrics.StartTracking();

        if (originalTargetNode == null)
            originalTargetNode = targetNode;

        float threshold = Vector2.Distance(startNode.nodePosition, originalTargetNode.nodePosition);
        visitedNodes = new HashSet<PathNode>();
        nowList = new Queue<PathNode>();
        List<PathNode> laterList = new List<PathNode>();
        Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();

        nowList.Enqueue(startNode);
        visitedNodes.Add(startNode);

        int iterationLimit = 500;
        int iterationCount = 0;

        while (nowList.Count > 0)
        {
            iterationCount++;
            metrics.NodeExpanded();

            if (iterationCount > iterationLimit)
            {
                Debug.LogWarning("Fringe Search: Iteration limit reached. Terminating.");
                break;
            }

            PathNode currentNode = nowList.Dequeue();

            if (currentNode == originalTargetNode)
            {
                List<Vector2> path = ReconstructPath(cameFrom, currentNode, startNode);
                metrics.StopTracking(path);
                metrics.PrintMetrics("Fringe Search");
                return path;
            }

            foreach (PathNode neighbor in currentNode.neighbors)
            {
                if (visitedNodes.Contains(neighbor)) continue;

                if (neighbor.isBlocked)
                {
                    if (!laterList.Contains(neighbor))
                        laterList.Add(neighbor);
                    continue;
                }

                float cost = Vector2.Distance(startNode.nodePosition, neighbor.nodePosition) +
                             Vector2.Distance(neighbor.nodePosition, originalTargetNode.nodePosition);

                if (cost <= threshold)
                {
                    nowList.Enqueue(neighbor);
                    visitedNodes.Add(neighbor);
                    cameFrom[neighbor] = currentNode; // Track path using cameFrom
                }
                else if (!laterList.Contains(neighbor))
                {
                    laterList.Add(neighbor);
                }
            }

            if (nowList.Count == 0 && laterList.Count > 0)
            {
                Debug.Log("Fringe Search: Adjusting threshold.");
                threshold = AdjustThreshold(threshold, laterList, originalTargetNode);
                nowList = MoveNodesToNowList(laterList);
                laterList.Clear();
            }
        }

        Debug.LogWarning("Fringe Search: No path found.");
        metrics.StopTracking(null);
        metrics.PrintMetrics("Fringe Search");
        return new List<Vector2>();
    }

    private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode endNode, PathNode startNode)
    {
        List<PathNode> nodePath = new List<PathNode>();
        HashSet<PathNode> seenNodes = new HashSet<PathNode>();

        while (endNode != null)
        {
            if (seenNodes.Contains(endNode)) break; // Prevent duplicates
            nodePath.Add(endNode);
            if (endNode == startNode) break;
            seenNodes.Add(endNode);
            cameFrom.TryGetValue(endNode, out endNode); // Backtrack using cameFrom dictionary
        }

        nodePath.Reverse();
        List<Vector2> path = new List<Vector2>();
        foreach (PathNode node in nodePath)
        {
            path.Add(node.nodePosition);
        }

        return path;
    }

    private float AdjustThreshold(float currentThreshold, List<PathNode> laterList, PathNode targetNode)
    {
        float minCost = float.MaxValue;
        foreach (PathNode node in laterList)
        {
            float cost = Vector2.Distance(node.nodePosition, targetNode.nodePosition);
            if (cost < minCost) minCost = cost;
        }

        return Mathf.Max(currentThreshold + 10.0f, minCost);
    }

    private Queue<PathNode> MoveNodesToNowList(List<PathNode> laterList)
    {
        Queue<PathNode> nowList = new Queue<PathNode>();
        foreach (PathNode node in laterList)
        {
            if (!node.isBlocked)
                nowList.Enqueue(node);
        }
        return nowList;
    }

    public override void HandleBlockedPath(GameManager manager, PathNode blockedNode)
    {
        Debug.Log("Fringe Search: Dynamic obstacle encountered. Adjusting path...");
        if (blockedNode != null) blockedNode.isBlocked = true;

        PathNode startNode = manager.FindClosestNodeToPlayer();
        PathNode targetNode = originalTargetNode;

        if (startNode != null && targetNode != null)
        {
            // Re-enable visitation of last 10 nodes
            ClearVisitedForLastNodes(10);

            Debug.Log($"Recalculating path from {startNode.nodePosition} to original goal {targetNode.nodePosition}");
            List<Vector2> newPath = CalculatePath(startNode, targetNode, manager.allNodes);

            if (newPath.Count > 0)
            {
                manager.player.SetPath(newPath);
            }
            else
            {
                Debug.LogWarning("Fringe Search: No valid path found during partial recalculation.");
            }
        }
    }

    private void ClearVisitedForLastNodes(int nodesToClear)
    {
        List<PathNode> lastVisited = new List<PathNode>();

        foreach (PathNode node in nowList)
        {
            lastVisited.Add(node);
            if (lastVisited.Count >= nodesToClear) break;
        }

        foreach (PathNode node in lastVisited)
        {
            visitedNodes.Remove(node);
        }

        Debug.Log($"Cleared visited flags for {lastVisited.Count} nodes.");
    }

    public void ResetOriginalTarget()
    {
        originalTargetNode = null;
    }
}
