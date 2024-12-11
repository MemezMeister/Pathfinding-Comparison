using System.Collections.Generic;
using UnityEngine;

public class AutoNeighborManager : MonoBehaviour
{
    [Header("Settings")]
    public float neighborRange = 1.1f; // Max distance to consider as a neighbor
    public bool findNeighborsOnStart = true; // Auto-run on Start
    private List<PathNode> allNodes;

    private void Start()
    {
        if (findNeighborsOnStart)
        {
            FindAllNodes();
            AssignNeighbors();
        }
    }

    // Collect all PathNodes in the scene
    void FindAllNodes()
    {
        allNodes = new List<PathNode>(FindObjectsOfType<PathNode>());
    }

    // Assign neighbors to each PathNode
    void AssignNeighbors()
    {
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError("No PathNodes found in the scene. Ensure all nodes have the PathNode script attached.");
            return;
        }

        foreach (PathNode node in allNodes)
        {
            node.neighbors.Clear(); // Clear any pre-existing neighbors

            foreach (PathNode potentialNeighbor in allNodes)
            {
                if (node == potentialNeighbor) continue; // Skip self

                float distance = Vector2.Distance(node.nodePosition, potentialNeighbor.nodePosition);
                if (distance <= neighborRange && !node.neighbors.Contains(potentialNeighbor))
                {
                    node.neighbors.Add(potentialNeighbor); // Add valid neighbors
                }
            }
        }

        Debug.Log("Neighbor assignment completed successfully!");
    }

    // Manual trigger from Inspector
    [ContextMenu("Assign Neighbors Manually")]
    public void AssignNeighborsManually()
    {
        FindAllNodes();
        AssignNeighbors();
    }
}
