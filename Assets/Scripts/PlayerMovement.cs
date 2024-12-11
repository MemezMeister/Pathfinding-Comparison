using UnityEngine;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f; // Movement speed
    private List<Vector2> currentPath = new List<Vector2>();
    private int currentTargetIndex;

    private Vector2 previousPosition; // To track movement direction
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        previousPosition = transform.position;

        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on the player object!");
        }
    }

    void Update()
    {
        if (currentPath.Count > 0)
        {
            CheckAndMoveAlongPath();
        }
    }

    public void SetPath(List<Vector2> path)
    {
        currentPath = path;
        currentTargetIndex = 0;
    }
    public Vector2 currentTargetPosition
    {
        get
        {
            if (currentPath.Count > 0 && currentTargetIndex < currentPath.Count)
            {
                return currentPath[currentTargetIndex];
            }
            return transform.position; // Return current position if no path
        }
    }

    void CheckAndMoveAlongPath()
    {
        if (currentTargetIndex >= currentPath.Count) return;

        // Fetch the next node position
        Vector2 targetPosition = currentPath[currentTargetIndex];
        PathNode nextNode = FindClosestNode(targetPosition);

        // Check if the next node is blocked
        if (nextNode != null && nextNode.isBlocked)
        {
            Debug.Log("Obstacle detected! Recalculating path...");
            RecalculatePath();
            return; // Stop further movement this frame
        }

        // Move towards the target position
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Flip the character based on movement direction
        FlipCharacter(transform.position - (Vector3)previousPosition);
        previousPosition = transform.position;

        // Check if the player has reached the current target position
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= currentPath.Count)
            {
                currentPath.Clear();
            }
        }
    }

    void RecalculatePath()
    {
        PathNode startNode = GameManager.Instance.FindClosestNodeToPlayer();
        PathNode targetNode = GameManager.Instance.FindClosestNodeToTarget();

        if (startNode != null && targetNode != null)
        {
            List<Vector2> newPath = GameManager.Instance.currentAlgorithm.CalculatePath(startNode, targetNode, GameManager.Instance.allNodes);

            if (newPath.Count > 0)
            {
                SetPath(newPath);
            }
            else
            {
                Debug.Log("No valid path found after recalculation!");
            }
        }
    }


    void FlipCharacter(Vector2 movementDirection)
    {
        if (movementDirection.x > 0.01f)
        {
            spriteRenderer.flipX = false; // Facing right
        }
        else if (movementDirection.x < -0.01f)
        {
            spriteRenderer.flipX = true; // Facing left
        }
    }

    // Helper method to find the closest node to a given position
    PathNode FindClosestNode(Vector2 position)
    {
        float minDistance = Mathf.Infinity;
        PathNode closestNode = null;

        foreach (PathNode node in GameManager.Instance.allNodes)
        {
            float distance = Vector2.Distance(position, node.nodePosition);
            if (distance < minDistance)
            {
                closestNode = node;
                minDistance = distance;
            }
        }

        return closestNode;
    }
}
