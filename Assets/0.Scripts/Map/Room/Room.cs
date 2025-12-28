using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전의 방을 나타내는 클래스
/// </summary>
public class Room
{
    public Vector2Int gridPosition;      // 그리드 상의 위치
    public RoomType roomType;            // 방 타입
    public GameObject roomObject;        // 실제 게임 오브젝트
    public Dictionary<Vector2Int, bool> doors; // 문 연결 상태 (방향, 연결됨)
    public EventRoomType? eventRoomType; // 이벤트 방 컨셉 (이벤트 방일 경우에만 사용)
    
    public Room(Vector2Int position, RoomType type)
    {
        gridPosition = position;
        roomType = type;
        doors = new Dictionary<Vector2Int, bool>();
        
        // 4방향 모두 문 초기화 (연결 안됨)
        doors[Direction.Up] = false;
        doors[Direction.Down] = false;
        doors[Direction.Left] = false;
        doors[Direction.Right] = false;
    }
    
    /// <summary>
    /// 특정 방향의 문을 연결합니다.
    /// </summary>
    public void ConnectDoor(Vector2Int direction)
    {
        if (doors.ContainsKey(direction))
        {
            doors[direction] = true;
        }
    }
    
    /// <summary>
    /// 특정 방향의 문이 연결되어 있는지 확인합니다.
    /// </summary>
    public bool IsDoorConnected(Vector2Int direction)
    {
        return doors.ContainsKey(direction) && doors[direction];
    }
    
    /// <summary>
    /// 연결된 문의 개수를 반환합니다.
    /// </summary>
    public int GetConnectedDoorCount()
    {
        int count = 0;
        foreach (var door in doors.Values)
        {
            if (door) count++;
        }
        return count;
    }
}

