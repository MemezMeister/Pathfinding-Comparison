using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/BidirectionalFrontToFront")]
public class BidirectionalAlgorithm : PathfindingAlgorithm
{
    private MetricsManager metrics = new MetricsManager();
    private PathNode originalTargetNode; // Store the original target node

public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
{
    metrics.StartTracking(); // Start metrics tracking

    // Reset the original target node for each new calculation
    originalTargetNode = targetNode;

    Debug.Log($"Bidirectional Search: Starting Pathfinding from {startNode.nodePosition} to {originalTargetNode.nodePosition}");

    // Initialize search sets and cost dictionaries
    HashSet<PathNode> openSetForward = new HashSet<PathNode> { startNode };
    HashSet<PathNode> openSetBackward = new HashSet<PathNode> { originalTargetNode };

    Dictionary<PathNode, PathNode> cameFromForward = new Dictionary<PathNode, PathNode>();
    Dictionary<PathNode, PathNode> cameFromBackward = new Dictionary<PathNode, PathNode>();

    Dictionary<PathNode, float> costForward = new Dictionary<PathNode, float> { { startNode, 0 } };
    Dictionary<PathNode, float> costBackward = new Dictionary<PathNode, float> { { originalTargetNode, 0 } };

    HashSet<PathNode> blockedNodes = new HashSet<PathNode>();
    PathNode meetingNode = null;

    while (openSetForward.Count > 0 && openSetBackward.Count > 0)
    {
        // Forward step
        if (StepSearch(openSetForward, costForward, cameFromForward, openSetBackward, blockedNodes, ref meetingNode, "Forward"))
        {
            var path = ReconstructPath(cameFromForward, cameFromBackward, meetingNode);
            metrics.StopTracking(path);
            metrics.PrintMetrics("Bidirectional Search");
            return path;
        }

        // Backward step
        if (StepSearch(openSetBackward, costBackward, cameFromBackward, openSetForward, blockedNodes, ref meetingNode, "Backward"))
        {
            var path = ReconstructPath(cameFromForward, cameFromBackward, meetingNode);
            metrics.StopTracking(path);
            metrics.PrintMetrics("Bidirectional Search");
            return path;
        }
    }

    Debug.LogWarning("Bidirectional Search: No path found!");
    metrics.StopTracking(null); // Stop tracking with no path
    metrics.PrintMetrics("Bidirectional Search");
    return new List<Vector2>();
}


    private bool StepSearch(HashSet<PathNode> openSet,
                            Dictionary<PathNode, float> costThis,
                            Dictionary<PathNode, PathNode> cameFrom,
                            HashSet<PathNode> openSetOther,
                            HashSet<PathNode> blockedNodes,
                            ref PathNode meetingNode,
                            string direction)
    {
        if (openSet.Count == 0) return false;

        PathNode currentNode = GetNodeWithLowestCost(openSet, costThis);
        openSet.Remove(currentNode);

        // Debug the node being processed
        Debug.Log($"[{direction}] Processing Node: {currentNode.nodePosition}");
        metrics.NodeExpanded(); // Track node expansion

        foreach (PathNode neighbor in currentNode.neighbors)
        {
            if (neighbor.isBlocked || blockedNodes.Contains(neighbor))
            {
                Debug.Log($"[{direction}] Node {neighbor.nodePosition} is blocked. Skipping...");
                continue;
            }

            if (costThis.ContainsKey(neighbor))
                continue; // Skip already visited nodes

            // Update cost and path
            costThis[neighbor] = costThis[currentNode] + Vector2.Distance(currentNode.nodePosition, neighbor.nodePosition);
            cameFrom[neighbor] = currentNode;
            openSet.Add(neighbor);

            // Check if the forward and backward searches meet
            if (openSetOther.Contains(neighbor))
            {
                meetingNode = neighbor;
                Debug.Log($"[{direction}] Meeting Node Found: {neighbor.nodePosition}");
                return true; // Path found
            }
        }

        return false; // Continue searching
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
                                          Dictionary<PathNode, PathNode> cameFromBackward,
                                          PathNode meetingNode)
    {
        List<Vector2> path = new List<Vector2>();

        Debug.Log($"Reconstructing Path: Meeting Node at {meetingNode.nodePosition}");

        // Reconstruct forward path from start to meeting node
        PathNode current = meetingNode;
        while (cameFromForward.ContainsKey(current))
        {
            path.Add(current.nodePosition);
            current = cameFromForward[current];
        }
        path.Reverse();

        // Add meeting node to the path
        path.Add(meetingNode.nodePosition);

        // Reconstruct backward path from meeting node to goal
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
        Debug.Log("Bidirectional Search: Adjusting dynamically for blocked path...");

        blockedNode.isBlocked = true; // Mark the node as blocked

        // Find the player's current position
        PathNode startNode = manager.FindClosestNodeToPlayer();

        if (startNode != null && originalTargetNode != null)
        {
            Debug.Log($"Recalculating path from {startNode.nodePosition} to original goal {originalTargetNode.nodePosition}");

            // Recalculate the path using the stored original target node
            List<Vector2> newPath = CalculatePath(startNode, originalTargetNode, manager.allNodes);

            if (newPath.Count > 0)
            {
                Debug.Log("Bidirectional Search: Path recalculated successfully.");
                manager.player.SetPath(newPath);
            }
            else
            {
                Debug.LogWarning("Bidirectional Search: No path found after handling blocked nodes.");
            }
        }
    }
}
