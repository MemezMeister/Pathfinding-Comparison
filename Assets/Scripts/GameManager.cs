using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerMovement player;
    public List<PathNode> allNodes;
    public PathfindingAlgorithm currentAlgorithm; // Assign Scriptable Object

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void MovePlayerToNode(PathNode targetNode)
    {
        PathNode startNode = FindClosestNodeToPlayer();
        List<Vector2> path = currentAlgorithm.CalculatePath(startNode, targetNode, allNodes);
        player.SetPath(path);
    }

    public PathNode FindClosestNodeToPlayer()
    {
        PathNode closest = null;
        float minDistance = Mathf.Infinity;

        foreach (PathNode node in allNodes)
        {
            float distance = Vector2.Distance(player.transform.position, node.nodePosition);
            if (distance < minDistance && !node.isBlocked)
            {
                closest = node;
                minDistance = distance;
            }
        }

        return closest;
    }
    public void SetupNodeNeighbors(float gridSize)
    {
        foreach (PathNode node in allNodes)
        {
            node.FindNeighbors(allNodes, gridSize);
        }
    }
    public PathNode FindClosestNodeToTarget()
    {
        // Replace with your logic for finding the target node
        PathNode targetNode = null;
        float minDistance = Mathf.Infinity;

        foreach (PathNode node in allNodes)
        {
            // Replace targetPosition with your goal node position
            float distance = Vector2.Distance(node.nodePosition, player.currentTargetPosition);
            if (distance < minDistance && !node.isBlocked)
            {
                targetNode = node;
                minDistance = distance;
            }
        }

        return targetNode;
    }

}
