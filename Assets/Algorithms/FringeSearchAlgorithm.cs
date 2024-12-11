public override List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes)
{
    float threshold = Vector2.Distance(startNode.nodePosition, targetNode.nodePosition);
    HashSet<PathNode> visitedNodes = new HashSet<PathNode>(); // Track visited nodes
    Queue<PathNode> nowList = new Queue<PathNode>();
    List<PathNode> laterList = new List<PathNode>();
    Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();

    nowList.Enqueue(startNode);
    visitedNodes.Add(startNode);

    int maxNodeCount = 5000; // Node limit safeguard

    while (nowList.Count > 0)
    {
        // Safeguard: Stop if too many nodes have been visited
        if (visitedNodes.Count > maxNodeCount)
        {
            Debug.LogWarning("Fringe Search: Node limit exceeded. Terminating search.");
            break;
        }

        PathNode currentNode = nowList.Dequeue();

        // Target reached, reconstruct path
        if (currentNode == targetNode)
        {
            return ReconstructPath(cameFrom, targetNode);
        }

        // Check neighbors
        foreach (PathNode neighbor in currentNode.neighbors)
        {
            if (!neighbor.isBlocked && !visitedNodes.Contains(neighbor))
            {
                float cost = Vector2.Distance(startNode.nodePosition, neighbor.nodePosition) +
                             Vector2.Distance(neighbor.nodePosition, targetNode.nodePosition);

                if (cost <= threshold)
                {
                    cameFrom[neighbor] = currentNode;
                    nowList.Enqueue(neighbor);
                    visitedNodes.Add(neighbor); // Mark node as visited
                }
                else if (!laterList.Contains(neighbor))
                {
                    laterList.Add(neighbor); // Avoid duplicates
                }
            }
        }

        // Increase threshold if Now List is empty
        if (nowList.Count == 0 && laterList.Count > 0)
        {
            threshold += 1.0f; // Increase threshold
            nowList = new Queue<PathNode>(laterList);
            laterList.Clear();
        }
    }

    Debug.LogWarning("Fringe Search: No path found!");
    return new List<Vector2>();
}
