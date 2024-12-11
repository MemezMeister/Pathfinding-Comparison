using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/Bidirectional")]
public class BidirectionalAlgorithm : PathfindingAlgorithm
{
    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        // Two open sets for forward and backward search
        Queue<PathNode> openSetForward = new Queue<PathNode>();
        Queue<PathNode> openSetBackward = new Queue<PathNode>();

        // Parent tracking
        Dictionary<PathNode, PathNode> cameFromForward = new Dictionary<PathNode, PathNode>();
        Dictionary<PathNode, PathNode> cameFromBackward = new Dictionary<PathNode, PathNode>();

        // Visited nodes for intersection check
        HashSet<PathNode> visitedForward = new HashSet<PathNode>();
        HashSet<PathNode> visitedBackward = new HashSet<PathNode>();

        // Start both searches
        openSetForward.Enqueue(startNode);
        openSetBackward.Enqueue(targetNode);

        visitedForward.Add(startNode);
        visitedBackward.Add(targetNode);

        // Main search loop
        while (openSetForward.Count > 0 && openSetBackward.Count > 0)
        {
            // Forward search
            if (StepSearch(openSetForward, visitedForward, visitedBackward, cameFromForward, out PathNode meetingNode))
            {
                return ReconstructPath(cameFromForward, cameFromBackward, meetingNode);
            }

            // Backward search
            if (StepSearch(openSetBackward, visitedBackward, visitedForward, cameFromBackward, out meetingNode))
            {
                return ReconstructPath(cameFromForward, cameFromBackward, meetingNode);
            }
        }

        Debug.LogWarning("Bidirectional Search: No path found!");
        return new List<Vector2>();
    }

    private bool StepSearch(Queue<PathNode> openSet, HashSet<PathNode> visitedThis, HashSet<PathNode> visitedOther,
                            Dictionary<PathNode, PathNode> cameFrom, out PathNode meetingNode)
    {
        meetingNode = null;

        if (openSet.Count == 0) return false;

        PathNode currentNode = openSet.Dequeue();

        foreach (PathNode neighbor in currentNode.neighbors)
        {
            if (neighbor.isBlocked || visitedThis.Contains(neighbor))
                continue; // Skip blocked or already visited nodes

            visitedThis.Add(neighbor);
            openSet.Enqueue(neighbor);
            cameFrom[neighbor] = currentNode;

            if (visitedOther.Contains(neighbor))
            {
                meetingNode = neighbor; // Meeting point found
                return true;
            }
        }
        return false;
    }

    private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFromForward,
                                          Dictionary<PathNode, PathNode> cameFromBackward, PathNode meetingNode)
    {
        List<Vector2> path = new List<Vector2>();

        // Reconstruct forward path
        PathNode current = meetingNode;
        while (cameFromForward.ContainsKey(current))
        {
            path.Add(current.nodePosition);
            current = cameFromForward[current];
        }
        path.Reverse();

        // Add meeting node
        path.Add(meetingNode.nodePosition);

        // Reconstruct backward path
        current = meetingNode;
        while (cameFromBackward.ContainsKey(current))
        {
            current = cameFromBackward[current];
            path.Add(current.nodePosition);
        }

        return path;
    }
}
