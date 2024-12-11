using UnityEngine;
using System.Collections.Generic;

public abstract class PathfindingAlgorithm : ScriptableObject
{
    public abstract List<Vector2> CalculatePath(PathNode startNode, PathNode targetNode, List<PathNode> allNodes);
}
