using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/FringeSearch")]
public class FringeSearchAlgorithm : PathfindingAlgorithm
{
    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        float threshold = Vector2.Distance(startNode.nodePosition, targetNode.nodePosition);
        HashSet<PathNode> visitedNodes = new HashSet<PathNode>();
        Queue<PathNode> nowList = new Queue<PathNode>();
        List<PathNode> laterList = new List<PathNode>();
        Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();

        nowList.Enqueue(startNode);
        visitedNodes.Add(startNode);

        int iterationLimit = 5000; // Prevent excessive iterations
        int iterationCount = 0;

        while (nowList.Count > 0)
        {
            iterationCount++;
            if (iterationCount > iterationLimit)
            {
                Debug.LogWarning("Fringe Search: Iteration limit reached. Terminating.");
                break;
            }

            PathNode currentNode = nowList.Dequeue();

            // Target Reached
            if (currentNode == targetNode)
            {
                return ReconstructPath(cameFrom, targetNode);
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
                Debug.Log("Fringe Search: Adjusting threshold due to blocked paths.");
                threshold = AdjustThreshold(threshold, laterList, targetNode);
                nowList = MoveNodesToNowList(laterList);
                laterList.Clear();
            }
        }

        Debug.LogWarning("Fringe Search: No path found.");
        return new List<Vector2>();
    }

    private float AdjustThreshold(float currentThreshold, List<PathNode> laterList, PathNode targetNode)
    {
        // Dynamically adjust threshold based on proximity to target
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
        Debug.Log("Fringe Search: Dynamic obstacle encountered. Adjusting path...");

        // Reset the blocked node to allow reevaluation
        if (blockedNode != null)
        {
            blockedNode.isBlocked = true; // Ensure the node is flagged as blocked
        }

        // Partial recalculation
        PathNode startNode = manager.FindClosestNodeToPlayer();
        PathNode targetNode = manager.FindClosestNodeToTarget();

        if (startNode != null && targetNode != null)
        {
            // Remove blocked nodes from visited set to allow reconsideration
            HashSet<PathNode> resetVisitedNodes = new HashSet<PathNode>();
            foreach (PathNode node in manager.allNodes)
            {
                if (!node.isBlocked)
                {
                    resetVisitedNodes.Add(node);
                }
            }

            // Retry the pathfinding process
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


    private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode currentNode)
    {
        List<Vector2> path = new List<Vector2>();
        HashSet<PathNode> visitedInPath = new HashSet<PathNode>();

        // Prevent circular path reconstruction
        while (cameFrom.ContainsKey(currentNode))
        {
            if (visitedInPath.Contains(currentNode))
            {
                Debug.LogWarning("Fringe Search: Circular path detected during reconstruction.");
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
            if (!node.isBlocked) // Only add unblocked nodes back to the Now List
            {
                nowList.Enqueue(node);
            }
        }
        return nowList;
    }

}
