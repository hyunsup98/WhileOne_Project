using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 그리드를 관리하는 클래스
/// </summary>
public class DungeonGrid
{
    private Dictionary<Vector2Int, Room> grid;
    private int gridSize;
    
    public DungeonGrid(int size)
    {
        grid = new Dictionary<Vector2Int, Room>();
        gridSize = size;
    }
    
    /// <summary>
    /// 그리드 내부인지 확인합니다.
    /// </summary>
    public bool IsInGrid(Vector2Int position)
    {
        return Mathf.Abs(position.x) <= gridSize && Mathf.Abs(position.y) <= gridSize;
    }
    
    /// <summary>
    /// 해당 위치가 비어있는지 확인합니다.
    /// </summary>
    public bool IsEmpty(Vector2Int position)
    {
        return !grid.ContainsKey(position);
    }
    
    /// <summary>
    /// 방을 그리드에 추가합니다.
    /// </summary>
    public void AddRoom(Vector2Int position, Room room)
    {
        if (grid.ContainsKey(position))
        {
            Debug.LogWarning($"위치 {position}에 이미 방이 있습니다.");
            return;
        }
        grid[position] = room;
    }
    
    /// <summary>
    /// 해당 위치의 방을 가져옵니다.
    /// </summary>
    public Room GetRoom(Vector2Int position)
    {
        grid.TryGetValue(position, out Room room);
        return room;
    }
    
    /// <summary>
    /// 모든 방을 반환합니다.
    /// </summary>
    public IEnumerable<Room> GetAllRooms()
    {
        return grid.Values;
    }
    
    /// <summary>
    /// 그리드의 모든 위치를 반환합니다.
    /// </summary>
    public IEnumerable<Vector2Int> GetAllPositions()
    {
        return grid.Keys;
    }
    
    /// <summary>
    /// 방의 개수를 반환합니다.
    /// </summary>
    public int GetRoomCount()
    {
        return grid.Count;
    }
    
    /// <summary>
    /// 두 방 사이의 거리를 계산합니다.
    /// </summary>
    public int GetDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }
}

