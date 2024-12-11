using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/FringeSearch")]
public class FringeSearchAlgorithm : PathfindingAlgorithm
{
    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        float threshold = Vector2.Distance(startNode.nodePosition, targetNode.nodePosition);
        HashSet<PathNode> visitedNodes = new HashSet<PathNode>(); // Track visited nodes

        List<PathNode> nowList = new List<PathNode> { startNode };
        List<PathNode> laterList = new List<PathNode>();
        Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();

        while (nowList.Count > 0)
        {
            List<PathNode> newNowList = new List<PathNode>();

            foreach (PathNode currentNode in nowList)
            {
                // If we reached the target, reconstruct the path
                if (currentNode == targetNode)
                {
                    return ReconstructPath(cameFrom, targetNode);
                }

                foreach (PathNode neighbor in currentNode.neighbors)
                {
                    if (neighbor.isBlocked || visitedNodes.Contains(neighbor)) continue;

                    float cost = Vector2.Distance(startNode.nodePosition, neighbor.nodePosition)
                               + Vector2.Distance(neighbor.nodePosition, targetNode.nodePosition);

                    if (cost <= threshold)
                    {
                        cameFrom[neighbor] = currentNode;
                        newNowList.Add(neighbor);
                        visitedNodes.Add(neighbor); // Mark node as visited
                    }
                    else if (!laterList.Contains(neighbor))
                    {
                        laterList.Add(neighbor); // Avoid duplicates in Later List
                    }
                }
            }

            // Update the Now List
            nowList = newNowList;

            if (nowList.Count == 0 && laterList.Count > 0)
            {
                // Increase threshold and move nodes from Later List to Now List
                threshold += 1.0f; // Adjust increment as needed
                nowList.AddRange(laterList);
                laterList.Clear();

                if (threshold > 1000f) // Safeguard to prevent infinite loop
                {
                    Debug.LogWarning("Fringe Search: Terminating due to threshold limit.");
                    break;
                }
            }
        }

        Debug.LogWarning("Fringe Search: No path found!");
        return new List<Vector2>();
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
}
