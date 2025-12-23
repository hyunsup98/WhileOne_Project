using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static FloorSetting;

/// <summary>
/// 던전 방 생성 알고리즘을 담당하는 클래스
/// </summary>
public static class DungeonRoomGenerator
{
    /// <summary>
    /// 방을 생성합니다. (큐 기반 확장 알고리즘)
    /// </summary>
    public static void GenerateRooms(DungeonGrid dungeonGrid, int roomCount, float branchProbability, int maxBranchCount)
    {
        // 시작 방 생성
        Vector2Int startRoomPosition = Vector2Int.zero;
        Room startRoom = new Room(startRoomPosition, RoomType.Start);
        dungeonGrid.AddRoom(startRoomPosition, startRoom);
        
        // 방 생성 루프 (큐 기반으로 여러 방에서 동시에 확장)
        Queue<Vector2Int> expansionQueue = new Queue<Vector2Int>();
        expansionQueue.Enqueue(startRoomPosition);
        int count = 1;
        int maxAttempts = roomCount * 50; // 무한 루프 방지
        int attempts = 0;
        
        while (count < roomCount && expansionQueue.Count > 0 && attempts < maxAttempts)
        {
            attempts++;
            
            Vector2Int currentPos = expansionQueue.Dequeue();
            Room currentRoom = dungeonGrid.GetRoom(currentPos);
            if (currentRoom == null) continue;
            
            // 사용 가능한 방향 수집 (그리드 내부 + 비어있는 위치)
            List<Vector2Int> availableDirections = new List<Vector2Int>();
            foreach (Vector2Int dir in Direction.All)
            {
                Vector2Int nextPos = currentPos + dir;
                if (dungeonGrid.IsInGrid(nextPos) && dungeonGrid.IsEmpty(nextPos))
                {
                    availableDirections.Add(dir);
                }
            }
            
            // 분기 확률에 따라 여러 방향으로 분기
            int branchesToCreate = 1; // 최소 1개는 생성
            if (availableDirections.Count > 0 && Random.Range(0f, 100f) < branchProbability)
            {
                // 여러 방향으로 분기 (최대 maxBranchCount개)
                branchesToCreate = Mathf.Min(availableDirections.Count, maxBranchCount, roomCount - count);
            }
            
            // 선택된 방향으로 방 생성
            List<Vector2Int> createdRooms = new List<Vector2Int>();
            for (int i = 0; i < branchesToCreate && availableDirections.Count > 0 && count < roomCount; i++)
            {
                // 랜덤 방향 선택
                int randomIndex = Random.Range(0, availableDirections.Count);
                Vector2Int direction = availableDirections[randomIndex];
                availableDirections.RemoveAt(randomIndex);
                
                Vector2Int nextPosition = currentPos + direction;
                
                // 새 방 생성
                Room newRoom = new Room(nextPosition, RoomType.Normal);
                dungeonGrid.AddRoom(nextPosition, newRoom);
                
                // 문 연결
                currentRoom.ConnectDoor(direction);
                newRoom.ConnectDoor(Direction.Opposite(direction));
                
                // 새로 생성된 방을 확장 큐에 추가
                expansionQueue.Enqueue(nextPosition);
                createdRooms.Add(nextPosition);
                count++;
            }
            
            // 이미 있는 인접 방과도 연결할 수 있는지 확인 (교차로 생성)
            foreach (Vector2Int dir in Direction.All)
            {
                Vector2Int neighborPos = currentPos + dir;
                Room neighborRoom = dungeonGrid.GetRoom(neighborPos);
                
                // 인접 방이 있고, 아직 연결되지 않은 경우 (확률적으로 연결)
                if (neighborRoom != null && 
                    !currentRoom.IsDoorConnected(dir) && 
                    !createdRooms.Contains(neighborPos) &&
                    Random.Range(0f, 100f) < 30f) // 30% 확률로 추가 연결
                {
                    currentRoom.ConnectDoor(dir);
                    neighborRoom.ConnectDoor(Direction.Opposite(dir));
                }
            }
        }
    }
    
    /// <summary>
    /// 시작 방으로부터의 실제 경로 거리를 계산합니다. (BFS 사용)
    /// </summary>
    public static Dictionary<Vector2Int, int> CalculateDistancesFrom(DungeonGrid dungeonGrid, Vector2Int startPos)
    {
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        // 시작 방 초기화
        queue.Enqueue(startPos);
        visited.Add(startPos);
        distances[startPos] = 0;
        
        // BFS로 실제 경로 거리 계산
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            Room currentRoom = dungeonGrid.GetRoom(current);
            if (currentRoom == null) continue;
            
            // 연결된 모든 방향 확인
            foreach (var kvp in currentRoom.doors)
            {
                if (kvp.Value) // 문이 연결되어 있으면
                {
                    Vector2Int direction = kvp.Key;
                    Vector2Int nextPos = current + direction;
                    
                    if (!visited.Contains(nextPos) && dungeonGrid.GetRoom(nextPos) != null)
                    {
                        visited.Add(nextPos);
                        distances[nextPos] = distances[current] + 1;
                        queue.Enqueue(nextPos);
                    }
                }
            }
        }
        
        return distances;
    }
    
    /// <summary>
    /// 출구 방을 선택합니다. (시작 방에서 가장 먼 방)
    /// </summary>
    public static Vector2Int SelectExitRoom(Dictionary<Vector2Int, int> distances, Vector2Int startPos)
    {
        // 시작 방 제외한 모든 방 중 가장 먼 방 선택
        Vector2Int farthest = startPos;
        int maxDistance = 0;
        
        foreach (var kvp in distances)
        {
            // 시작 방은 제외
            if (kvp.Key == startPos) continue;
            
            if (kvp.Value > maxDistance)
            {
                maxDistance = kvp.Value;
                farthest = kvp.Key;
            }
        }
        
        return farthest;
    }
    
    /// <summary>
    /// 인접한 방들이 모두 서로 문이 연결되도록 보정합니다.
    /// </summary>
    public static void EnsureAdjacentDoorsConnected(DungeonGrid dungeonGrid)
    {
        Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null) continue;
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = position + dir;
                Room neighbor = dungeonGrid.GetRoom(neighborPos);
                if (neighbor == null) continue;
                
                // 양쪽 모두 문 연결 상태 보정
                if (!room.IsDoorConnected(dir) || !neighbor.IsDoorConnected(Direction.Opposite(dir)))
                {
                    room.ConnectDoor(dir);
                    neighbor.ConnectDoor(Direction.Opposite(dir));
                }
            }
        }
    }
}
