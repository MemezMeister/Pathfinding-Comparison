using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public List<PathNode> allNodes; // Assign all nodes in the Inspector
    public GameObject obstaclePrefab; // Prefab to spawn as the obstacle
    public float spawnInterval = 5f; // Interval for obstacle spawning
    public float obstacleLifespan = 10f; // Lifespan of each obstacle

    private List<PathNode> availableNodes;

    private void Start()
    {
        availableNodes = new List<PathNode>(allNodes);
        InvokeRepeating(nameof(SpawnRandomObstacle), 1f, spawnInterval);
    }

    void SpawnRandomObstacle()
    {
        if (availableNodes.Count == 0)
            return;

        int randomIndex = Random.Range(0, availableNodes.Count);
        PathNode selectedNode = availableNodes[randomIndex];

        if (!selectedNode.isBlocked)
        {
            // Use 2D position
            GameObject spawnedObstacle = Instantiate(obstaclePrefab,
                new Vector3(selectedNode.nodePosition.x, selectedNode.nodePosition.y, 0f), Quaternion.identity);

            selectedNode.isBlocked = true;
            spawnedObstacle.transform.parent = selectedNode.transform;

            StartCoroutine(HandleObstacleLifespan(spawnedObstacle, selectedNode));
            availableNodes.RemoveAt(randomIndex);
        }
    }

    System.Collections.IEnumerator HandleObstacleLifespan(GameObject obstacle, PathNode node)
    {
        yield return new WaitForSeconds(obstacleLifespan);

        Destroy(obstacle);
        node.isBlocked = false;
        availableNodes.Add(node);
    }
}
