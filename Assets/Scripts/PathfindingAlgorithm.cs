using UnityEngine;
using System.Collections.Generic;

public abstract class PathfindingAlgorithm : ScriptableObject
{
    public abstract List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes);
    public virtual void HandleBlockedPath(GameManager manager, PathNode blockedNode)
    {
        Debug.Log("This algorithm does not support recalculation.");
    }
    
}
    