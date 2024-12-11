using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public bool isBlocked = false; // Whether the node is blocked
    public Vector2 nodePosition;   // Position of the node (2D, world space)
    public List<PathNode> neighbors = new List<PathNode>(); // Neighboring nodes

    private void Awake()
    {
        // Always update nodePosition to match the current world position
        nodePosition = new Vector2(transform.position.x, transform.position.y);
    }

    private void OnDrawGizmos()
    {
        // For debugging, show the position visually in the scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

    private void Update()
    {
        // Dynamically keep world position updated
        nodePosition = new Vector2(transform.position.x, transform.position.y);
    }

    // Optional: Automatically reset position on start
    private void Start()
    {
        nodePosition = new Vector2(transform.position.x, transform.position.y);
    }

    // For debug purposes to see clicks
    private void OnMouseDown()
    {
        Debug.Log($"Node clicked at: {nodePosition}");
        if (!isBlocked)
        {
            GameManager.Instance?.MovePlayerToNode(this);
        }
    }

// Automatically find neighbors based on distance
public void FindNeighbors(List<PathNode> allNodes, float gridSize)
    {
        neighbors.Clear(); // Reset neighbors

        foreach (PathNode node in allNodes)
        {
            if (node != this && Vector2.Distance(nodePosition, node.nodePosition) <= gridSize)
            {
                neighbors.Add(node); // Add as a neighbor if within gridSize
            }
        }
    }
}
