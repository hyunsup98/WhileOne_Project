using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 복도 생성을 담당하는 클래스
/// </summary>
public static class DungeonCorridorGenerator
{
    /// <summary>
    /// 복도를 생성합니다.
    /// </summary>
    public static void CreateCorridors(
        DungeonGrid dungeonGrid,
        Transform parent,
        Grid unityGrid,
        GameObject corridorPrefabHorizontal,
        GameObject corridorPrefabVertical)
    {
        if (corridorPrefabHorizontal == null && corridorPrefabVertical == null)
        {
            Debug.LogWarning("[DungeonCorridorGenerator] 복도 프리팹이 설정되지 않아 복도를 생성할 수 없습니다.");
            return;
        }
        
        if (unityGrid == null)
        {
            Debug.LogWarning("[DungeonCorridorGenerator] Grid를 찾을 수 없어 복도를 생성할 수 없습니다.");
            return;
        }
        
        // 모든 방 사이에 직선 복도 생성
        Dictionary<Vector2Int, List<Vector2Int>> roomConnections = new Dictionary<Vector2Int, List<Vector2Int>>();
        
        // 방 연결 정보 수집
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            List<Vector2Int> connectedDirections = new List<Vector2Int>();
            Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            
            foreach (Vector2Int direction in directions)
            {
                if (room.IsDoorConnected(direction))
                {
                    Vector2Int nextPos = position + direction;
                    Room nextRoom = dungeonGrid.GetRoom(nextPos);
                    if (nextRoom != null && nextRoom.roomObject != null)
                    {
                        connectedDirections.Add(direction);
                    }
                }
            }
            
            roomConnections[position] = connectedDirections;
        }
        
        // 복도 중복 생성 방지
        HashSet<string> createdCorridors = new HashSet<string>();
        
        foreach (var kvp in roomConnections)
        {
            Vector2Int position = kvp.Key;
            List<Vector2Int> connectedDirections = kvp.Value;
            
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            foreach (Vector2Int direction in connectedDirections)
            {
                Vector2Int nextPos = position + direction;
                
                Room nextRoom = dungeonGrid.GetRoom(nextPos);
                if (nextRoom == null || nextRoom.roomObject == null) continue;
                
                // 복도 키 생성 (중복 방지)
                string corridorKey;
                if (position.x < nextPos.x || (position.x == nextPos.x && position.y < nextPos.y))
                {
                    corridorKey = $"{position.x}_{position.y}_{nextPos.x}_{nextPos.y}";
                }
                else
                {
                    corridorKey = $"{nextPos.x}_{nextPos.y}_{position.x}_{position.y}";
                }
                
                if (createdCorridors.Contains(corridorKey)) continue;
                createdCorridors.Add(corridorKey);
                
                // 두 방의 실제 문 위치를 확인하여 복도 유형 결정
                Vector3 room1DoorPos = DungeonRoomHelper.GetDoorWorldPosition(room.roomObject, direction);
                Vector2Int oppositeDirection = Direction.Opposite(direction);
                Vector3 room2DoorPos = DungeonRoomHelper.GetDoorWorldPosition(nextRoom.roomObject, oppositeDirection);
                
                if (room1DoorPos == Vector3.zero || room2DoorPos == Vector3.zero)
                {
                    Debug.LogWarning($"[DungeonCorridorGenerator] 문 위치를 찾을 수 없어 복도를 생성할 수 없습니다.");
                    continue;
                }
                
                // 두 문이 같은 축에 있는지 확인 (직선 복도만 지원)
                bool sameAxis = (Mathf.Abs(room1DoorPos.x - room2DoorPos.x) < 0.01f) || 
                                (Mathf.Abs(room1DoorPos.y - room2DoorPos.y) < 0.01f);
                
                if (!sameAxis)
                {
                    // 문이 같은 축에 있지 않으면 직선 복도를 생성할 수 없습니다.
                    Debug.LogWarning($"[DungeonCorridorGenerator] 두 문의 축이 일치하지 않아 직선 복도를 생성할 수 없습니다. " +
                                     $"Room1: {position}, Room2: {nextPos}, Door1: {room1DoorPos}, Door2: {room2DoorPos}");
                    continue;
                }
                
                // 같은 축: 직선 복도 생성
                CreateStraightCorridor(
                    room.roomObject, nextRoom.roomObject, direction,
                    corridorPrefabHorizontal, corridorPrefabVertical, parent, unityGrid, position, nextPos);
            }
        }
    }
    
    /// <summary>
    /// 두 방 사이에 직선 복도를 생성합니다.
    /// 두 방의 문이 같은 축에 있을 때 사용됩니다.
    /// 복도 길이 = |방 A 문 위치 - 방 B 문 위치|
    /// </summary>
    private static List<GameObject> CreateStraightCorridor(
        GameObject room1, 
        GameObject room2, 
        Vector2Int direction,
        GameObject corridorPrefabHorizontal,
        GameObject corridorPrefabVertical,
        Transform parent,
        Grid unityGrid,
        Vector2Int room1Pos,
        Vector2Int room2Pos)
    {
        // DoorCenterMarker를 사용하여 문 위치 가져오기
        Vector3 room1DoorPos = DungeonRoomHelper.GetDoorWorldPosition(room1, direction);
        Vector2Int oppositeDirection = Direction.Opposite(direction);
        Vector3 room2DoorPos = DungeonRoomHelper.GetDoorWorldPosition(room2, oppositeDirection);
        
        // DoorCenterMarker가 없으면 경고 후 스킵
        if (room1DoorPos == Vector3.zero)
        {
            Debug.LogWarning($"[DungeonCorridorGenerator] 방1({room1.name})의 {direction} 방향 DoorCenterMarker를 찾을 수 없어 복도를 생성할 수 없습니다.");
            return new List<GameObject>();
        }
        if (room2DoorPos == Vector3.zero)
        {
            Debug.LogWarning($"[DungeonCorridorGenerator] 방2({room2.name})의 {oppositeDirection} 방향 DoorCenterMarker를 찾을 수 없어 복도를 생성할 수 없습니다.");
            return new List<GameObject>();
        }
        
        // cellSize 가져오기 (오프셋 계산용)
        float cellSize = DungeonGridHelper.ResolveCellSize(null, unityGrid);
        
        // DoorCenterMarker에서 복도가 뻗어나가는 방향으로 +1칸 이동한 위치 계산
        Vector3 corridorStartPos = room1DoorPos;
        Vector3 corridorEndPos = room2DoorPos;
        
        // 방향 벡터 계산 (정규화)
        Vector3 directionOffset = Vector3.zero;
        if (direction == Direction.Up)
            directionOffset = Vector3.up * cellSize;
        else if (direction == Direction.Down)
            directionOffset = Vector3.down * cellSize;
        else if (direction == Direction.Left)
            directionOffset = Vector3.left * cellSize;
        else if (direction == Direction.Right)
            directionOffset = Vector3.right * cellSize;
        
        // 복도 시작 위치: DoorCenterMarker에서 +1칸 이동
        corridorStartPos = room1DoorPos + directionOffset;
        // 복도 끝 위치: 반대 방향 DoorCenterMarker에서 반대 방향으로 +1칸 이동
        corridorEndPos = room2DoorPos - directionOffset;
        
        // 수평/수직 판단 (십자 규격 정렬로 항상 수평 또는 수직만 가능)
        bool isHorizontal = (direction == Direction.Left || direction == Direction.Right);
        GameObject corridorPrefab = isHorizontal ? corridorPrefabHorizontal : corridorPrefabVertical;
        
        if (corridorPrefab == null)
        {
            Debug.LogWarning($"[DungeonCorridorGenerator] {(isHorizontal ? "가로" : "세로")} 복도 프리팹이 설정되지 않아 복도를 생성할 수 없습니다.");
            return new List<GameObject>();
        }
        
        // 복도 길이 계산: |복도 시작 위치 - 복도 끝 위치|
        float targetDistance = Vector3.Distance(corridorStartPos, corridorEndPos);
        
        // 방향 벡터 계산
        Vector3 directionVector = room2DoorPos - room1DoorPos;
        Vector3 directionNormalized = directionVector.normalized;
        
        // 복도 프리팹 생성하여 실제 길이 측정
        // 임시 복도를 생성 위치에 배치하여 측정 (로컬 위치 기준으로 측정해야 함)
        GameObject tempCorridor = Object.Instantiate(corridorPrefab, Vector3.zero, Quaternion.identity);
        Transform[] tempMarkers = FindCorridorDoorCenterMarkers(tempCorridor, isHorizontal, direction, room1Pos, room2Pos);
        float actualCorridorLength = 0f;
        
        if (tempMarkers[0] != null && tempMarkers[1] != null)
        {
            // 복도 프리팹의 실제 DoorCenterMarker 사이 거리 (로컬 위치 기준)
            // world position이 아닌 local position을 사용하여 프리팹 자체의 길이 측정
            Vector3 localStart = tempMarkers[0].localPosition;
            Vector3 localEnd = tempMarkers[1].localPosition;
            actualCorridorLength = Vector3.Distance(localStart, localEnd);
            
            // local position이 0에 가까우면 world position으로 대체 (부모가 없는 경우)
            if (actualCorridorLength < 0.001f)
            {
                actualCorridorLength = Vector3.Distance(tempMarkers[0].position, tempMarkers[1].position);
            }
        }
        else
        {
            // DoorCenterMarker가 없으면 기본값 사용 (4칸)
            const int corridorTileSize = 4;
            actualCorridorLength = corridorTileSize;
        }
        Object.DestroyImmediate(tempCorridor);
        
        // 복도 길이가 0이면 오류
        if (actualCorridorLength < 0.001f)
        {
            Debug.LogError($"[DungeonCorridorGenerator] 복도 프리팹({corridorPrefab.name})의 길이를 측정할 수 없습니다. DoorCenterMarker 설정을 확인하세요.");
            return new List<GameObject>();
        }
        
        // 필요한 복도 프리팹 개수 계산 (올림하여 충분히 연결, 4의 배수가 아니어도 겹쳐서 연결)
        int corridorCount = Mathf.CeilToInt(targetDistance / actualCorridorLength);
        if (corridorCount < 1) corridorCount = 1; // 최소 1개
        
        // 복도 프리팹들을 배치 (끊기지 않게 연결, 필요시 겹침 허용)
        List<GameObject> corridorTiles = new List<GameObject>();
        
        // 첫 번째 복도: 복도 시작 위치에 맞추기 (DoorCenterMarker에서 +1칸 이동한 위치)
        GameObject firstCorridor = Object.Instantiate(corridorPrefab, corridorStartPos, Quaternion.identity, parent);
        Transform[] firstMarkers = FindCorridorDoorCenterMarkers(firstCorridor, isHorizontal, direction, room1Pos, room2Pos);
        
        if (firstMarkers[0] != null)
        {
            Vector3 firstStartPos = firstMarkers[0].position;
            Vector3 offset = corridorStartPos - firstStartPos;
            firstCorridor.transform.position += offset;
        }
        corridorTiles.Add(firstCorridor);
        
        // 이후 복도들: 이전 복도의 끝에 정확히 연결
        for (int i = 1; i < corridorCount; i++)
        {
            GameObject prevCorridor = corridorTiles[i - 1];
            Transform[] prevMarkers = FindCorridorDoorCenterMarkers(prevCorridor, isHorizontal, direction, room1Pos, room2Pos);
            Transform prevEndMarker = prevMarkers[1];
            
            if (prevEndMarker == null)
            {
                Debug.LogWarning($"[DungeonCorridorGenerator] 이전 복도({prevCorridor.name})의 끝 DoorCenterMarker를 찾을 수 없습니다. 복도 생성이 중단됩니다.");
                break;
            }
            
            // 이전 복도의 끝 위치 (world position)
            Vector3 prevEndPos = prevEndMarker.position;
            
            // 새 복도 생성 (일단 이전 복도 끝 위치에 생성)
            GameObject corridor = Object.Instantiate(corridorPrefab, prevEndPos, Quaternion.identity, parent);
            Transform[] corridorMarkers = FindCorridorDoorCenterMarkers(corridor, isHorizontal, direction, room1Pos, room2Pos);
            Transform corridorStartMarker = corridorMarkers[0];
            
            if (corridorStartMarker != null)
            {
                // 현재 복도의 시작 DoorCenterMarker를 이전 복도의 끝 DoorCenterMarker에 정확히 맞추기
                Vector3 currentStartPos = corridorStartMarker.position;
                Vector3 offset = prevEndPos - currentStartPos;
                corridor.transform.position += offset;
            }
            else
            {
                Debug.LogWarning($"[DungeonCorridorGenerator] 새 복도의 시작 DoorCenterMarker를 찾을 수 없습니다.");
            }
            
            corridorTiles.Add(corridor);
        }
        
        // 마지막 복도의 끝 DoorCenterMarker를 복도 끝 위치에 정확히 맞추기 (DoorCenterMarker에서 +1칸 이동한 위치)
        // 거리가 4의 배수가 아니면 복도들이 일부 겹치게 됨 (끊기지 않고 연결)
        if (corridorTiles.Count > 0)
        {
            GameObject lastCorridor = corridorTiles[corridorTiles.Count - 1];
            Transform[] lastMarkers = FindCorridorDoorCenterMarkers(lastCorridor, isHorizontal, direction, room1Pos, room2Pos);
            Transform lastEndMarker = lastMarkers[1];
            
            if (lastEndMarker != null)
            {
                Vector3 lastEndPos = lastEndMarker.position;
                Vector3 offset = corridorEndPos - lastEndPos;
                
                // 마지막 복도만 이동하여 복도 끝 위치에 정확히 맞추기
                // 이렇게 하면 복도들이 일부 겹칠 수 있지만, 끊기지 않고 완전히 연결됨
                lastCorridor.transform.position += offset;
            }
        }
        
        // 디버그: 복도 배치 확인
        //Debug.Log($"[DungeonCorridorGenerator] 복도 생성 완료 - 방1: {room1.name}, 방2: {room2.name}, 복도 개수: {corridorTiles.Count}/{corridorCount}\n" + "방1 DoorCenterMarker: {room1DoorPos}, 복도 시작 위치: {corridorStartPos}, 방2 DoorCenterMarker: {room2DoorPos}, 복도 끝 위치: {corridorEndPos}, 거리: {targetDistance:F2}, 실제 프리팹 길이: {actualCorridorLength:F2}");
        
        return corridorTiles;
    }
    
    /// <summary>
    /// 복도 프리팹에서 양쪽 끝의 DoorCenterMarker를 찾습니다.
    /// 반환값: [0] = 시작쪽 DoorCenterMarker (room1 쪽), [1] = 끝쪽 DoorCenterMarker (room2 쪽)
    /// </summary>
    private static Transform[] FindCorridorDoorCenterMarkers(
        GameObject corridorObj, 
        bool isHorizontal, 
        Vector2Int direction,
        Vector2Int room1Pos,
        Vector2Int room2Pos)
    {
        Transform[] markers = new Transform[2] { null, null };
        
        if (corridorObj == null) return markers;
        
        // DoorCenterMarker 태그를 가진 모든 오브젝트 찾기
        Transform[] allChildren = corridorObj.GetComponentsInChildren<Transform>(true);
        List<Transform> foundMarkers = new List<Transform>();
        
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("DoorCenterMarker"))
            {
                foundMarkers.Add(child);
            }
        }
        
        if (foundMarkers.Count >= 2)
        {
            // 방향에 따라 시작과 끝 결정
            if (isHorizontal)
            {
                // 가로 복도: X 좌표 기준 정렬
                foundMarkers.Sort((a, b) => a.localPosition.x.CompareTo(b.localPosition.x));
                
                // Left 방향이면 room1이 오른쪽, room2가 왼쪽 → 오른쪽이 시작, 왼쪽이 끝
                // Right 방향이면 room1이 왼쪽, room2가 오른쪽 → 왼쪽이 시작, 오른쪽이 끝
                if (direction == Direction.Left)
                {
                    // room1이 room2보다 오른쪽에 있으므로, 오른쪽 마커가 시작
                    markers[0] = foundMarkers[foundMarkers.Count - 1]; // X 큰 값 (오른쪽)
                    markers[1] = foundMarkers[0]; // X 작은 값 (왼쪽)
                }
                else // Direction.Right
                {
                    // room1이 room2보다 왼쪽에 있으므로, 왼쪽 마커가 시작
                    markers[0] = foundMarkers[0]; // X 작은 값 (왼쪽)
                    markers[1] = foundMarkers[foundMarkers.Count - 1]; // X 큰 값 (오른쪽)
                }
            }
            else
            {
                // 세로 복도: Y 좌표 기준 정렬
                foundMarkers.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));
                
                // Up 방향이면 room1이 아래, room2가 위 → 아래가 시작, 위가 끝
                // Down 방향이면 room1이 위, room2가 아래 → 위가 시작, 아래가 끝
                if (direction == Direction.Up)
                {
                    // room1이 room2보다 아래에 있으므로, 아래 마커가 시작
                    markers[0] = foundMarkers[0]; // Y 작은 값 (아래)
                    markers[1] = foundMarkers[foundMarkers.Count - 1]; // Y 큰 값 (위)
                }
                else // Direction.Down
                {
                    // room1이 room2보다 위에 있으므로, 위 마커가 시작
                    markers[0] = foundMarkers[foundMarkers.Count - 1]; // Y 큰 값 (위)
                    markers[1] = foundMarkers[0]; // Y 작은 값 (아래)
                }
            }
        }
        else if (foundMarkers.Count == 1)
        {
            // 하나만 있으면 양쪽 모두 같은 마커 사용
            markers[0] = foundMarkers[0];
            markers[1] = foundMarkers[0];
        }
        
        return markers;
    }
}
