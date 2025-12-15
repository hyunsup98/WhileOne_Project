using UnityEngine;

public interface IGridMap
{
    bool IsWalkable(Vector2Int cell);

    Vector2Int[] GetNeighbors(Vector2Int cell);

    int GetMoveCost(Vector2Int cell);
}
