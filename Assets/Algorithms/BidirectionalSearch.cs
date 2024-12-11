using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/BidirectionalFrontToFront")]
public class BidirectionalAlgorithm : PathfindingAlgorithm
{
    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        HashSet<PathNode> openSetForward = new HashSet<PathNode> { startNode };
        HashSet<PathNode> openSetBackward = new HashSet<PathNode> { targetNode };

        Dictionary<PathNode, PathNode> cameFromForward = new Dictionary<PathNode, PathNode>();
        Dictionary<PathNode, PathNode> cameFromBackward = new Dictionary<PathNode, PathNode>();

        Dictionary<PathNode, float> costForward = new Dictionary<PathNode, float> { { startNode, 0 } };
        Dictionary<PathNode, float> costBackward = new Dictionary<PathNode, float> { { targetNode, 0 } };

        HashSet<PathNode> blockedNodes = new HashSet<PathNode>();
        PathNode meetingNode = null;

        while (openSetForward.Count > 0 && openSetBackward.Count > 0)
        {
            // Forward step
            if (StepSearch(openSetForward, costForward, cameFromForward, openSetBackward, blockedNodes, ref meetingNode))
            {
                return ReconstructPath(cameFromForward, cameFromBackward, meetingNode);
            }

            // Backward step
            if (StepSearch(openSetBackward, costBackward, cameFromBackward, openSetForward, blockedNodes, ref meetingNode))
            {
                return ReconstructPath(cameFromForward, cameFromBackward, meetingNode);
            }
        }

        Debug.LogWarning("Bidirectional Search: No path found!");
        return new List<Vector2>();
    }

    private bool StepSearch(HashSet<PathNode> openSet, Dictionary<PathNode, float> costThis,
                            Dictionary<PathNode, PathNode> cameFrom, HashSet<PathNode> openSetOther,
                            HashSet<PathNode> blockedNodes, ref PathNode meetingNode)
    {
        if (openSet.Count == 0) return false;

        PathNode currentNode = GetNodeWithLowestCost(openSet, costThis);
        openSet.Remove(currentNode);

        foreach (PathNode neighbor in currentNode.neighbors)
        {
            if (neighbor.isBlocked)
            {
                blockedNodes.Add(neighbor); // Mark dynamic obstacles
                continue;
            }

            if (blockedNodes.Contains(neighbor) || costThis.ContainsKey(neighbor))
                continue;

            costThis[neighbor] = costThis[currentNode] + Vector2.Distance(currentNode.nodePosition, neighbor.nodePosition);
            cameFrom[neighbor] = currentNode;
            openSet.Add(neighbor);

            if (openSetOther.Contains(neighbor))
            {
                meetingNode = neighbor;
                return true; // Meeting point found
            }
        }

        return false;
    }

    private PathNode GetNodeWithLowestCost(HashSet<PathNode> openSet, Dictionary<PathNode, float> cost)
    {
        PathNode lowestCostNode = null;
        float lowestCost = Mathf.Infinity;

        foreach (PathNode node in openSet)
        {
            if (cost[node] < lowestCost)
            {
                lowestCost = cost[node];
                lowestCostNode = node;
            }
        }

        return lowestCostNode;
    }

    private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFromForward,
                                          Dictionary<PathNode, PathNode> cameFromBackward, PathNode meetingNode)
    {
        List<Vector2> path = new List<Vector2>();

        // Forward path
        PathNode current = meetingNode;
        while (cameFromForward.ContainsKey(current))
        {
            path.Add(current.nodePosition);
            current = cameFromForward[current];
        }
        path.Reverse();

        path.Add(meetingNode.nodePosition); // Add meeting node

        // Backward path
        current = meetingNode;
        while (cameFromBackward.ContainsKey(current))
        {
            current = cameFromBackward[current];
            path.Add(current.nodePosition);
        }

        return path;
    }

    public override void HandleBlockedPath(GameManager manager, PathNode blockedNode)
    {
        Debug.Log("Bidirectional Search: Adjusting dynamically for blocked path.");

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
                Debug.Log("Bidirectional Search: No new path found after adjusting for blocked nodes.");
            }
        }
    }
}
