using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class MetricsManager
{
    float startTime;// To measure execution time
    private int nodesExpanded;   // Count of nodes expanded
    private float pathLength;    // Length of the final path
    private long startMemory;    // Memory usage before pathfinding
    private long endMemory;      // Memory usage after pathfinding
    public float executionTime;

    public void StartTracking()
    {
        ResetMetrics();  
        nodesExpanded = 0;
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        startMemory = System.GC.GetTotalMemory(true);
        startTime = Time.realtimeSinceStartup;
    }

    public void StopTracking(List<Vector2> path)
    {
        float endTime = Time.realtimeSinceStartup;    // End the timer
        endMemory = System.GC.GetTotalMemory(true);
        executionTime = (endTime - startTime) * 1000f;
        // Measure path length
        if (path != null && path.Count > 1)
        {
            pathLength = CalculatePathLength(path);
        }
        else
        {
            pathLength = 0;
        }

    }

    public void NodeExpanded()
    {
        nodesExpanded++;
    }

    public void PrintMetrics(string algorithmName)
    {

        UnityEngine.Debug.Log($"--- Metrics for {algorithmName} ---");
        UnityEngine.Debug.Log($"Execution Time: {executionTime} ms");
        UnityEngine.Debug.Log($"Nodes Expanded: {nodesExpanded}");
        UnityEngine.Debug.Log($"Path Length: {pathLength:F2}");
        UnityEngine.Debug.Log($"Memory Usage: {GetMemoryUsage()} bytes");
    }

    private void ResetMetrics()
    {
        startTime = 0f;
        nodesExpanded = 0;
        pathLength = 0;
        startMemory = 0;
        endMemory = 0;
    }

    private float CalculatePathLength(List<Vector2> path)
    {
        float totalLength = 0f;
        for (int i = 1; i < path.Count; i++)
        {
            totalLength += Vector2.Distance(path[i - 1], path[i]);
        }
        return totalLength;
    }

    private long GetMemoryUsage()
    {
        return endMemory - startMemory;
    }
}
