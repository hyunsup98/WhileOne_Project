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
        
        // 1단계: 모든 방 사이에 일반 복도 생성
        Dictionary<Vector2Int, List<Vector2Int>> roomConnections = new Dictionary<Vector2Int, List<Vector2Int>>();
        Dictionary<string, CorridorSegment> corridorSegments = new Dictionary<string, CorridorSegment>(); // 복도 세그먼트 저장
        
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
        
        // 복도 생성 및 경로 추적
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
                
                // 복도 생성 및 경로 저장
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
                    // 교차로(ㄱ, T, +)는 더 이상 사용하지 않으므로,
                    // 문이 같은 축에 있지 않으면 복도를 생성하지 않습니다.
                    Debug.LogWarning($"[DungeonCorridorGenerator] 두 문의 축이 일치하지 않아 직선 복도를 생성할 수 없습니다. " +
                                     $"Room1: {position}, Room2: {nextPos}, Door1: {room1DoorPos}, Door2: {room2DoorPos}");
                    continue;
                }
                
                // 같은 축: 직선 복도만 생성
                List<GameObject> corridorTiles = CreateStraightCorridor(
                    room.roomObject, nextRoom.roomObject, direction,
                    corridorPrefabHorizontal, corridorPrefabVertical, parent, unityGrid, position, nextPos);
                
                if (corridorTiles != null && corridorTiles.Count > 0)
                {
                    // 복도 경로 정보 저장 (교차점 찾기용)
                    bool isHorizontal = (direction == Direction.Left || direction == Direction.Right);
                    
                    corridorSegments[corridorKey] = new CorridorSegment
                    {
                        StartPos = room1DoorPos,
                        EndPos = room2DoorPos,
                        IsHorizontal = isHorizontal,
                        Tiles = corridorTiles
                    };
                }
            }
        }
    }
    
    /// <summary>
    /// 교차로 프리팹의 중심 마커를 찾습니다.
    /// </summary>
    private static Transform FindJunctionCenter(GameObject junctionObj)
    {
        if (junctionObj == null) return null;
        
        // RoomCenterMarker 또는 DoorCenterMarker를 중심으로 사용
        Transform[] allChildren = junctionObj.GetComponentsInChildren<Transform>(true);
        
        // RoomCenterMarker 우선
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("RoomCenterMarker"))
            {
                return child;
            }
        }
        
        // DoorCenterMarker 중 하나를 중심으로 사용 (가장 중앙에 가까운 것)
        List<Transform> doorMarkers = new List<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("DoorCenterMarker"))
            {
                doorMarkers.Add(child);
            }
        }
        
        if (doorMarkers.Count > 0)
        {
            // 모든 DoorCenterMarker의 평균 위치 계산
            Vector3 avgPos = Vector3.zero;
            foreach (Transform marker in doorMarkers)
            {
                avgPos += marker.localPosition;
            }
            avgPos /= doorMarkers.Count;
            
            // 평균 위치에 가장 가까운 마커 반환
            Transform closest = doorMarkers[0];
            float minDist = Vector3.Distance(closest.localPosition, avgPos);
            foreach (Transform marker in doorMarkers)
            {
                float dist = Vector3.Distance(marker.localPosition, avgPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = marker;
                }
            }
            return closest;
        }
        
        // 마커가 없으면 교차로 자체의 transform 반환
        return junctionObj.transform;
    }
    
    /// <summary>
    /// 복도 세그먼트 정보를 저장하는 클래스
    /// </summary>
    private class CorridorSegment
    {
        public Vector3 StartPos;
        public Vector3 EndPos;
        public bool IsHorizontal;
        public List<GameObject> Tiles;
    }
    
    /// <summary>
    /// 교차로 정보를 저장하는 클래스
    /// </summary>
    private class JunctionInfo
    {
        public List<Vector2Int> ConnectedDirections = new List<Vector2Int>();
        public List<string> IntersectingCorridors = new List<string>();
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
        SetCorridorTilePassable(firstCorridor);
        
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
            SetCorridorTilePassable(corridor);
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
        Debug.Log($"[DungeonCorridorGenerator] 복도 생성 완료 - 방1: {room1.name}, 방2: {room2.name}, 복도 개수: {corridorTiles.Count}/{corridorCount}\n" +
            $"방1 DoorCenterMarker: {room1DoorPos}, 복도 시작 위치: {corridorStartPos}, 방2 DoorCenterMarker: {room2DoorPos}, 복도 끝 위치: {corridorEndPos}, 거리: {targetDistance:F2}, 실제 프리팹 길이: {actualCorridorLength:F2}");
        
        return corridorTiles;
    }
    
    /// <summary>
    /// 두 방 사이에 ㄱ자 복도(L자형)를 생성합니다.
    /// 두 방의 문이 다른 축에 있을 때 사용됩니다.
    /// A방 문 -> 한 칸 이동 -> 방향 전환 -> B방 문
    /// 복도 길이 = |방 A 문 위치 - 전환점| + |전환점 - 방 B 문 위치|
    /// </summary>
    private static List<GameObject> CreateLShapedCorridor(
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
        
        if (room1DoorPos == Vector3.zero || room2DoorPos == Vector3.zero)
        {
            Debug.LogWarning($"[DungeonCorridorGenerator] 문 위치를 찾을 수 없어 ㄱ자 복도를 생성할 수 없습니다.");
            return new List<GameObject>();
        }
        
        // 방향 벡터 계산
        Vector3 directionVector = room2DoorPos - room1DoorPos;
        float dx = directionVector.x;
        float dy = directionVector.y;
        
        // 첫 번째 축 결정 (더 긴 거리로 먼저 이동)
        bool firstAxisIsHorizontal = Mathf.Abs(dx) >= Mathf.Abs(dy);
        
        // 전환점 계산 (한 칸 이동 후 방향 전환)
        float cellSize = DungeonGridHelper.ResolveCellSize(null, unityGrid);
        Vector3 turnPoint;
        
        if (firstAxisIsHorizontal)
        {
            // 가로로 먼저 이동 (한 칸), 그 다음 세로로 이동
            // room1DoorPos에서 가로로 한 칸 이동한 후, 세로로 room2DoorPos.y까지 이동
            float moveX = dx > 0 ? cellSize : -cellSize;
            turnPoint = new Vector3(room1DoorPos.x + moveX, room2DoorPos.y, room1DoorPos.z);
        }
        else
        {
            // 세로로 먼저 이동 (한 칸), 그 다음 가로로 이동
            // room1DoorPos에서 세로로 한 칸 이동한 후, 가로로 room2DoorPos.x까지 이동
            float moveY = dy > 0 ? cellSize : -cellSize;
            turnPoint = new Vector3(room2DoorPos.x, room1DoorPos.y + moveY, room1DoorPos.z);
        }
        
        List<GameObject> allCorridorTiles = new List<GameObject>();
        
        // 첫 번째 구간: room1DoorPos -> turnPoint
        Vector3 firstSegmentStart = room1DoorPos;
        Vector3 firstSegmentEnd = turnPoint;
        float firstSegmentLength = Vector3.Distance(firstSegmentStart, firstSegmentEnd);
        
        if (firstSegmentLength > 0.01f)
        {
            bool firstIsHorizontal = firstAxisIsHorizontal;
            GameObject firstPrefab = firstIsHorizontal ? corridorPrefabHorizontal : corridorPrefabVertical;
            Vector2Int firstDirection = firstIsHorizontal ? 
                (directionVector.x > 0 ? Direction.Right : Direction.Left) :
                (directionVector.y > 0 ? Direction.Up : Direction.Down);
            
            List<GameObject> firstSegment = CreateCorridorSegment(
                firstSegmentStart, firstSegmentEnd, firstPrefab, firstIsHorizontal, firstDirection,
                parent, unityGrid, room1Pos, room2Pos);
            
            if (firstSegment != null)
            {
                allCorridorTiles.AddRange(firstSegment);
            }
        }
        
        // 두 번째 구간: turnPoint -> room2DoorPos
        Vector3 secondSegmentStart = turnPoint;
        Vector3 secondSegmentEnd = room2DoorPos;
        float secondSegmentLength = Vector3.Distance(secondSegmentStart, secondSegmentEnd);
        
        if (secondSegmentLength > 0.01f)
        {
            bool secondIsHorizontal = !firstAxisIsHorizontal;
            GameObject secondPrefab = secondIsHorizontal ? corridorPrefabHorizontal : corridorPrefabVertical;
            Vector2Int secondDirection = secondIsHorizontal ?
                (directionVector.x > 0 ? Direction.Right : Direction.Left) :
                (directionVector.y > 0 ? Direction.Up : Direction.Down);
            
            List<GameObject> secondSegment = CreateCorridorSegment(
                secondSegmentStart, secondSegmentEnd, secondPrefab, secondIsHorizontal, secondDirection,
                parent, unityGrid, room1Pos, room2Pos);
            
            if (secondSegment != null)
            {
                allCorridorTiles.AddRange(secondSegment);
            }
        }
        
        // 전체 복도 길이 계산: |방 A 문 위치 - 전환점| + |전환점 - 방 B 문 위치|
        float totalLength = firstSegmentLength + secondSegmentLength;
        
        Debug.Log($"[DungeonCorridorGenerator] ㄱ자 복도 생성 완료 - 방1: {room1.name}, 방2: {room2.name}\n" +
            $"방1 문: {room1DoorPos}, 전환점: {turnPoint}, 방2 문: {room2DoorPos}\n" +
            $"첫 구간 길이: {firstSegmentLength:F2}, 두 번째 구간 길이: {secondSegmentLength:F2}, 총 길이: {totalLength:F2}");
        
        return allCorridorTiles;
    }
    
    /// <summary>
    /// 두 지점 사이에 복도 세그먼트를 생성합니다.
    /// </summary>
    private static List<GameObject> CreateCorridorSegment(
        Vector3 startPos,
        Vector3 endPos,
        GameObject corridorPrefab,
        bool isHorizontal,
        Vector2Int direction,
        Transform parent,
        Grid unityGrid,
        Vector2Int room1Pos,
        Vector2Int room2Pos)
    {
        if (corridorPrefab == null)
        {
            Debug.LogWarning($"[DungeonCorridorGenerator] 복도 프리팹이 설정되지 않았습니다.");
            return new List<GameObject>();
        }
        
        // 복도 길이 계산: |시작 위치 - 끝 위치|
        float targetDistance = Vector3.Distance(startPos, endPos);
        
        // 복도 프리팹 길이 측정
        GameObject tempCorridor = Object.Instantiate(corridorPrefab, Vector3.zero, Quaternion.identity);
        Transform[] tempMarkers = FindCorridorDoorCenterMarkers(tempCorridor, isHorizontal, direction, room1Pos, room2Pos);
        float actualCorridorLength = 0f;
        
        if (tempMarkers[0] != null && tempMarkers[1] != null)
        {
            Vector3 localStart = tempMarkers[0].localPosition;
            Vector3 localEnd = tempMarkers[1].localPosition;
            actualCorridorLength = Vector3.Distance(localStart, localEnd);
            
            if (actualCorridorLength < 0.001f)
            {
                actualCorridorLength = Vector3.Distance(tempMarkers[0].position, tempMarkers[1].position);
            }
        }
        else
        {
            float cellSize = DungeonGridHelper.ResolveCellSize(null, unityGrid);
            const int corridorTileSize = 4;
            actualCorridorLength = corridorTileSize * cellSize;
        }
        Object.DestroyImmediate(tempCorridor);
        
        if (actualCorridorLength < 0.001f)
        {
            Debug.LogError($"[DungeonCorridorGenerator] 복도 프리팹({corridorPrefab.name})의 길이를 측정할 수 없습니다.");
            return new List<GameObject>();
        }
        
        // 필요한 복도 프리팹 개수 계산
        int corridorCount = Mathf.CeilToInt(targetDistance / actualCorridorLength);
        if (corridorCount < 1) corridorCount = 1;
        
        List<GameObject> corridorTiles = new List<GameObject>();
        
        // 첫 번째 복도: 시작 위치에 맞추기
        GameObject firstCorridor = Object.Instantiate(corridorPrefab, startPos, Quaternion.identity, parent);
        Transform[] firstMarkers = FindCorridorDoorCenterMarkers(firstCorridor, isHorizontal, direction, room1Pos, room2Pos);
        
        if (firstMarkers[0] != null)
        {
            Vector3 firstStartPos = firstMarkers[0].position;
            Vector3 offset = startPos - firstStartPos;
            firstCorridor.transform.position += offset;
        }
        corridorTiles.Add(firstCorridor);
        SetCorridorTilePassable(firstCorridor);
        
        // 이후 복도들: 이전 복도의 끝에 정확히 연결
        for (int i = 1; i < corridorCount; i++)
        {
            GameObject prevCorridor = corridorTiles[i - 1];
            Transform[] prevMarkers = FindCorridorDoorCenterMarkers(prevCorridor, isHorizontal, direction, room1Pos, room2Pos);
            Transform prevEndMarker = prevMarkers[1];
            
            if (prevEndMarker == null)
            {
                Debug.LogWarning($"[DungeonCorridorGenerator] 이전 복도의 끝 DoorCenterMarker를 찾을 수 없습니다.");
                break;
            }
            
            Vector3 prevEndPos = prevEndMarker.position;
            GameObject corridor = Object.Instantiate(corridorPrefab, prevEndPos, Quaternion.identity, parent);
            Transform[] corridorMarkers = FindCorridorDoorCenterMarkers(corridor, isHorizontal, direction, room1Pos, room2Pos);
            Transform corridorStartMarker = corridorMarkers[0];
            
            if (corridorStartMarker != null)
            {
                Vector3 currentStartPos = corridorStartMarker.position;
                Vector3 offset = prevEndPos - currentStartPos;
                corridor.transform.position += offset;
            }
            
            corridorTiles.Add(corridor);
            SetCorridorTilePassable(corridor);
        }
        
        // 마지막 복도의 끝을 목표 위치에 맞추기
        if (corridorTiles.Count > 0)
        {
            GameObject lastCorridor = corridorTiles[corridorTiles.Count - 1];
            Transform[] lastMarkers = FindCorridorDoorCenterMarkers(lastCorridor, isHorizontal, direction, room1Pos, room2Pos);
            Transform lastEndMarker = lastMarkers[1];
            
            if (lastEndMarker != null)
            {
                Vector3 lastEndPos = lastEndMarker.position;
                Vector3 offset = endPos - lastEndPos;
                lastCorridor.transform.position += offset;
            }
        }
        
        return corridorTiles;
    }
    
    /// <summary>
    /// 복도 교차점을 찾습니다.
    /// </summary>
    private static Dictionary<Vector3, JunctionInfo> FindCorridorIntersections(
        Dictionary<string, CorridorSegment> corridorSegments,
        Grid unityGrid)
    {
        Dictionary<Vector3, JunctionInfo> intersections = new Dictionary<Vector3, JunctionInfo>();
        float cellSize = DungeonGridHelper.ResolveCellSize(null, unityGrid);
        float tolerance = cellSize * 0.5f; // 교차점 판정 허용 오차
        
        // 모든 복도 쌍을 비교하여 교차점 찾기
        var segmentList = new List<KeyValuePair<string, CorridorSegment>>(corridorSegments);
        
        for (int i = 0; i < segmentList.Count; i++)
        {
            var seg1 = segmentList[i];
            if (seg1.Value == null) continue;
            
            for (int j = i + 1; j < segmentList.Count; j++)
            {
                var seg2 = segmentList[j];
                if (seg2.Value == null) continue;
                
                // 가로 복도와 세로 복도만 교차 가능
                if (seg1.Value.IsHorizontal == seg2.Value.IsHorizontal) continue;
                
                // 교차점 계산
                Vector3? intersection = CalculateLineIntersection(
                    seg1.Value.StartPos, seg1.Value.EndPos,
                    seg2.Value.StartPos, seg2.Value.EndPos);
                
                if (intersection.HasValue)
                {
                    Vector3 intersectionPos = intersection.Value;
                    
                    // 교차점이 두 복도 세그먼트 내부에 있는지 확인
                    if (IsPointOnSegment(intersectionPos, seg1.Value.StartPos, seg1.Value.EndPos, tolerance) &&
                        IsPointOnSegment(intersectionPos, seg2.Value.StartPos, seg2.Value.EndPos, tolerance))
                    {
                        // 교차점 정보 저장 또는 업데이트
                        Vector3 roundedPos = RoundToGrid(intersectionPos, cellSize);
                        
                        if (!intersections.ContainsKey(roundedPos))
                        {
                            intersections[roundedPos] = new JunctionInfo();
                        }
                        
                        JunctionInfo junction = intersections[roundedPos];
                        
                        // 연결된 방향 추가
                        if (seg1.Value.IsHorizontal)
                        {
                            // 가로 복도: 좌우 방향
                            if (!junction.ConnectedDirections.Contains(Direction.Left))
                                junction.ConnectedDirections.Add(Direction.Left);
                            if (!junction.ConnectedDirections.Contains(Direction.Right))
                                junction.ConnectedDirections.Add(Direction.Right);
                        }
                        else
                        {
                            // 세로 복도: 상하 방향
                            if (!junction.ConnectedDirections.Contains(Direction.Up))
                                junction.ConnectedDirections.Add(Direction.Up);
                            if (!junction.ConnectedDirections.Contains(Direction.Down))
                                junction.ConnectedDirections.Add(Direction.Down);
                        }
                        
                        if (seg2.Value.IsHorizontal)
                        {
                            if (!junction.ConnectedDirections.Contains(Direction.Left))
                                junction.ConnectedDirections.Add(Direction.Left);
                            if (!junction.ConnectedDirections.Contains(Direction.Right))
                                junction.ConnectedDirections.Add(Direction.Right);
                        }
                        else
                        {
                            if (!junction.ConnectedDirections.Contains(Direction.Up))
                                junction.ConnectedDirections.Add(Direction.Up);
                            if (!junction.ConnectedDirections.Contains(Direction.Down))
                                junction.ConnectedDirections.Add(Direction.Down);
                        }
                        
                        // 교차하는 복도 추가
                        if (!junction.IntersectingCorridors.Contains(seg1.Key))
                            junction.IntersectingCorridors.Add(seg1.Key);
                        if (!junction.IntersectingCorridors.Contains(seg2.Key))
                            junction.IntersectingCorridors.Add(seg2.Key);
                    }
                }
            }
        }
        
        return intersections;
    }
    
    /// <summary>
    /// 두 선분의 교차점을 계산합니다.
    /// </summary>
    private static Vector3? CalculateLineIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // 2D 평면에서 교차점 계산 (Z는 무시)
        float x1 = p1.x, y1 = p1.y;
        float x2 = p2.x, y2 = p2.y;
        float x3 = p3.x, y3 = p3.y;
        float x4 = p4.x, y4 = p4.y;
        
        float denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        
        if (Mathf.Abs(denom) < 0.0001f) return null; // 평행한 경우
        
        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;
        
        // 교차점이 두 선분 위에 있는지 확인
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            float x = x1 + t * (x2 - x1);
            float y = y1 + t * (y2 - y1);
            return new Vector3(x, y, p1.z);
        }
        
        return null;
    }
    
    /// <summary>
    /// 점이 선분 위에 있는지 확인합니다.
    /// </summary>
    private static bool IsPointOnSegment(Vector3 point, Vector3 segStart, Vector3 segEnd, float tolerance)
    {
        float distToStart = Vector3.Distance(point, segStart);
        float distToEnd = Vector3.Distance(point, segEnd);
        float segLength = Vector3.Distance(segStart, segEnd);
        
        // 점이 선분의 양 끝점 근처에 있거나 선분 위에 있는지 확인
        return Mathf.Abs(distToStart + distToEnd - segLength) < tolerance;
    }
    
    /// <summary>
    /// 위치를 그리드에 맞춰 반올림합니다.
    /// </summary>
    private static Vector3 RoundToGrid(Vector3 pos, float cellSize)
    {
        return new Vector3(
            Mathf.Round(pos.x / cellSize) * cellSize,
            Mathf.Round(pos.y / cellSize) * cellSize,
            pos.z
        );
    }
    
    /// <summary>
    /// 교차점에 교차로를 배치합니다.
    /// </summary>
    private static void CreateJunctionAtIntersection(
        Vector3 intersectionPos,
        JunctionInfo junctionInfo,
        GameObject junctionPrefab,
        Dictionary<string, CorridorSegment> corridorSegments,
        Transform parent,
        Grid unityGrid)
    {
        if (junctionPrefab == null) return;
        
        // 교차로 생성
        GameObject junction = Object.Instantiate(junctionPrefab, intersectionPos, Quaternion.identity, parent);
        
        // 교차로의 중심 마커를 찾아서 교차점에 맞추기
        Transform junctionCenter = FindJunctionCenter(junction);
        if (junctionCenter != null)
        {
            Vector3 offset = intersectionPos - junctionCenter.position;
            junction.transform.position += offset;
        }
        
        SetCorridorTilePassable(junction);
        
        Debug.Log($"[DungeonCorridorGenerator] 교차로 생성 완료 - 위치: {intersectionPos}, 연결 방향: {junctionInfo.ConnectedDirections.Count}개");
    }
    
    /// <summary>
    /// 복도 타일을 플레이어가 통과 가능하도록 설정합니다.
    /// </summary>
    private static void SetCorridorTilePassable(GameObject corridorTile)
    {
        // 복도 타일의 모든 충돌체를 Trigger로 설정
        Collider2D[] colliders = corridorTile.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            // 바닥 타일은 통과 가능하게 (벽은 제외할 수도 있음)
            // 이름에 "Wall"이 포함되어 있지 않으면 통과 가능하게 설정
            if (!collider.name.Contains("Wall") && !collider.name.Contains("wall"))
            {
                collider.isTrigger = true;
            }
        }
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
