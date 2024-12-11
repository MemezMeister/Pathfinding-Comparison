using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pathfinding/ThetaStar")]
public class ThetaStarAlgorithm : PathfindingAlgorithm
{
    public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
    {
        // Initialize the cost and data structures
        Dictionary<PathNode, float> gCost = new Dictionary<PathNode, float>();
        Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();
        PriorityQueue<PathNode> openSet = new PriorityQueue<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        foreach (PathNode node in allNodes)
            gCost[node] = Mathf.Infinity;

        gCost[startNode] = 0;
        openSet.Enqueue(startNode, Heuristic(startNode, targetNode));

        while (openSet.Count > 0)
        {
            PathNode current = openSet.Dequeue();

            // Check if target is reached
            if (current == targetNode)
            {
                Debug.Log("Theta*: Target reached. Reconstructing path...");
                return ReconstructPath(cameFrom, targetNode);
            }

            closedSet.Add(current);

            foreach (PathNode neighbor in GetNonDiagonalNeighbors(current))
            {
                if (closedSet.Contains(neighbor) || neighbor.isBlocked)
                    continue;

                PathNode parent = cameFrom.ContainsKey(current) ? cameFrom[current] : current;

                float newCost = gCost[parent] + Vector2.Distance(parent.nodePosition, neighbor.nodePosition);

                if (HasLineOfSight(parent, neighbor))
                {
                    if (newCost < gCost[neighbor])
                    {
                        gCost[neighbor] = newCost;
                        cameFrom[neighbor] = parent;

                        if (!openSet.Contains(neighbor))
                            openSet.Enqueue(neighbor, gCost[neighbor] + Heuristic(neighbor, targetNode));
                    }
                }
                else
                {
                    float neighborCost = gCost[current] + Vector2.Distance(current.nodePosition, neighbor.nodePosition);

                    if (neighborCost < gCost[neighbor])
                    {
                        gCost[neighbor] = neighborCost;
                        cameFrom[neighbor] = current;

                        if (!openSet.Contains(neighbor))
                            openSet.Enqueue(neighbor, gCost[neighbor] + Heuristic(neighbor, targetNode));
                    }
                }
            }
        }

        Debug.LogWarning("Theta*: No valid path found!");
        return new List<Vector2>();
    }

    private List<PathNode> GetNonDiagonalNeighbors(PathNode node)
    {
        List<PathNode> nonDiagonalNeighbors = new List<PathNode>();

        foreach (PathNode neighbor in node.neighbors)
        {
            Vector2 direction = (neighbor.nodePosition - node.nodePosition).normalized;

            // Allow only cardinal (non-diagonal) directions
            if (Mathf.Abs(direction.x) > 0 && Mathf.Abs(direction.y) == 0) // Horizontal
            {
                nonDiagonalNeighbors.Add(neighbor);
            }
            else if (Mathf.Abs(direction.y) > 0 && Mathf.Abs(direction.x) == 0) // Vertical
            {
                nonDiagonalNeighbors.Add(neighbor);
            }
        }

        return nonDiagonalNeighbors;
    }

    private bool HasLineOfSight(PathNode start, PathNode end)
    {
        // Use raycasting to check line-of-sight between nodes
        Vector2 startPos = start.nodePosition;
        Vector2 endPos = end.nodePosition;

        RaycastHit2D hit = Physics2D.Linecast(startPos, endPos);
        return hit.collider == null; // True if nothing blocks the path
    }

    private float Heuristic(PathNode a, PathNode b)
    {
        // Manhattan distance for grid-based movement
        return Mathf.Abs(a.nodePosition.x - b.nodePosition.x) + Mathf.Abs(a.nodePosition.y - b.nodePosition.y);
    }

    private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode currentNode)
    {
        List<Vector2> path = new List<Vector2>();

        while (cameFrom.ContainsKey(currentNode))
        {
            path.Add(currentNode.nodePosition);
            currentNode = cameFrom[currentNode];
        }

        path.Add(currentNode.nodePosition); // Add the start node
        path.Reverse();
        return path;
    }
}
