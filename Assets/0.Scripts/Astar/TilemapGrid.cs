using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGrid : MonoBehaviour, IGridMap
{
    private Tilemap wallTilemap;
    private List<Vector2Int> result = new();

    public TilemapGrid(Tilemap wall)
    {
        wallTilemap = wall;
    }

    // 이동이 가능한 cell인지를 검사
    public bool IsWalkable(Vector2Int cellPos) => !wallTilemap.HasTile((Vector3Int)cellPos);

    // 해당 셀의 코스트(G값)를 반환
    public int GetMoveCost(Vector2Int cellPos) => 10;

    // 해당 셀의 이웃한 셀(상하좌우)을 출력
    public Vector2Int[] GetNeighbors(Vector2Int cellPos)
    {
        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
        };

        // 이웃한 타일이 갈 수 있는 타일인지 검사
        foreach (var dir in directions)
        {
            Vector2Int next = cellPos + dir;

            if (!IsWalkable(next))
                continue;

            result.Add(next);
        }
        return result.ToArray();
    }
    
    public void Init() => result.Clear();
}