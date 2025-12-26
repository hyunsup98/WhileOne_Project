using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 복도 길이 검증을 담당하는 클래스
/// </summary>
public static class DungeonCorridorValidator
{
    /// <summary>
    /// 모든 복도의 길이가 최소 길이 이상인지 검증합니다.
    /// </summary>
    /// <param name="dungeonGrid">던전 그리드</param>
    /// <param name="unityGrid">Unity Grid 컴포넌트</param>
    /// <param name="minCorridorLength">최소 복도 길이 (Unity unit)</param>
    /// <param name="minActualLength">실제 최소 복도 길이 (Unity unit, out)</param>
    /// <returns>조정이 필요한지 여부 (false = 모든 복도가 최소 길이 이상, true = 일부 복도가 최소 길이 미만)</returns>
    public static bool ValidateCorridorLengths(
        DungeonGrid dungeonGrid,
        Grid unityGrid,
        float minCorridorLength,
        out float minActualLength)
    {
        minActualLength = float.MaxValue;
        bool needsAdjustment = false;
        
        if (dungeonGrid == null || unityGrid == null)
        {
            return false;
        }
        
        // 모든 방을 순회하면서 연결된 방 찾기
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            // 4방향 모두 확인
            Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            
            foreach (Vector2Int direction in directions)
            {
                // 문이 연결되어 있는지 확인
                if (!room.IsDoorConnected(direction)) continue;
                
                // 인접한 방 위치 계산
                Vector2Int nextPos = position + direction;
                
                // 인접한 방이 실제로 존재하는지 확인
                Room nextRoom = dungeonGrid.GetRoom(nextPos);
                if (nextRoom == null || nextRoom.roomObject == null) continue;
                
                // 두 방 사이의 복도 길이 계산
                float corridorLength = CalculateCorridorLength(
                    room.roomObject, nextRoom.roomObject, direction);
                
                if (corridorLength > 0)
                {
                    minActualLength = Mathf.Min(minActualLength, corridorLength);
                    
                    if (corridorLength < minCorridorLength)
                    {
                        needsAdjustment = true;
                        Debug.LogWarning($"[DungeonCorridorValidator] 복도 길이 부족: " +
                            $"방1({room.roomObject.name}) ↔ 방2({nextRoom.roomObject.name}), " +
                            $"길이: {corridorLength:F2} < 최소: {minCorridorLength:F2}");
                    }
                }
            }
        }
        
        if (minActualLength == float.MaxValue)
        {
            minActualLength = 0f;
        }
        
        return needsAdjustment;
    }
    
    /// <summary>
    /// 두 방 사이의 복도 길이를 계산합니다.
    /// </summary>
    private static float CalculateCorridorLength(
        GameObject room1,
        GameObject room2,
        Vector2Int direction)
    {
        if (room1 == null || room2 == null) return 0f;
        
        // DoorCenterMarker를 사용하여 문 위치 가져오기
        Vector3 room1DoorPos = DungeonRoomHelper.GetDoorWorldPosition(room1, direction);
        Vector2Int oppositeDirection = Direction.Opposite(direction);
        Vector3 room2DoorPos = DungeonRoomHelper.GetDoorWorldPosition(room2, oppositeDirection);
        
        // DoorCenterMarker가 없으면 0 반환
        if (room1DoorPos == Vector3.zero || room2DoorPos == Vector3.zero)
        {
            return 0f;
        }
        
        // 두 DoorCenterMarker 사이의 거리 계산
        float distance = Vector3.Distance(room1DoorPos, room2DoorPos);
        
        return distance;
    }
    
    /// <summary>
    /// 복도 배치가 올바른지 검증합니다.
    /// 복도가 이상하게 길어지거나 제대로 배치되지 않았는지 확인합니다.
    /// </summary>
    /// <param name="dungeonGrid">던전 그리드</param>
    /// <param name="unityGrid">Unity Grid 컴포넌트</param>
    /// <param name="roomSpacingInCells">방 간격 (칸 수)</param>
    /// <param name="problematicCorridors">문제가 있는 복도 정보 (out)</param>
    /// <param name="maxCorridorLengthMultiplier">예상 복도 길이의 최대 배수 (기본값: 2.0, 예상 길이의 2배 이상이면 문제)</param>
    /// <returns>복도 배치에 문제가 있으면 true, 정상이면 false</returns>
    public static bool ValidateCorridorPlacement(
        DungeonGrid dungeonGrid,
        Grid unityGrid,
        int roomSpacingInCells,
        out List<string> problematicCorridors,
        float maxCorridorLengthMultiplier = 2.0f)
    {
        problematicCorridors = new List<string>();
        
        if (dungeonGrid == null || unityGrid == null)
        {
            return false;
        }
        
        float cellSize = DungeonGridHelper.ResolveCellSize(null, unityGrid);
        bool hasProblem = false;
        
        // 모든 방을 순회하면서 연결된 방 찾기
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            
            foreach (Vector2Int direction in directions)
            {
                if (!room.IsDoorConnected(direction)) continue;
                
                Vector2Int nextPos = position + direction;
                Room nextRoom = dungeonGrid.GetRoom(nextPos);
                if (nextRoom == null || nextRoom.roomObject == null) continue;
                
                // 두 방 사이의 실제 복도 길이 계산
                Vector3 room1DoorPos = DungeonRoomHelper.GetDoorWorldPosition(room.roomObject, direction);
                Vector2Int oppositeDirection = Direction.Opposite(direction);
                Vector3 room2DoorPos = DungeonRoomHelper.GetDoorWorldPosition(nextRoom.roomObject, oppositeDirection);
                
                if (room1DoorPos == Vector3.zero || room2DoorPos == Vector3.zero) continue;
                
                float actualCorridorLength = Vector3.Distance(room1DoorPos, room2DoorPos);
                
                // 예상 복도 길이 계산 (그리드 거리 기준)
                // 그리드 거리가 1칸이면 roomSpacingInCells * cellSize가 예상 길이
                int gridDistance = Mathf.Abs(nextPos.x - position.x) + Mathf.Abs(nextPos.y - position.y);
                float expectedCorridorLength = gridDistance * roomSpacingInCells * cellSize;
                
                // 복도가 예상 길이보다 훨씬 긴지 확인
                if (actualCorridorLength > expectedCorridorLength * maxCorridorLengthMultiplier)
                {
                    hasProblem = true;
                    string problemInfo = $"방1({position}) ↔ 방2({nextPos}): " +
                        $"실제 길이 {actualCorridorLength:F2}, 예상 길이 {expectedCorridorLength:F2} (배수: {actualCorridorLength / expectedCorridorLength:F2})";
                    problematicCorridors.Add(problemInfo);
                    
                    Debug.LogWarning($"[DungeonCorridorValidator] 복도가 이상하게 길어짐: {problemInfo}");
                }
                
                // 복도가 너무 짧은지 확인 (방이 겹치는 경우)
                if (actualCorridorLength < cellSize * 0.5f)
                {
                    hasProblem = true;
                    string problemInfo = $"방1({position}) ↔ 방2({nextPos}): " +
                        $"복도가 너무 짧음 (길이: {actualCorridorLength:F2}), 방이 겹칠 수 있음";
                    problematicCorridors.Add(problemInfo);
                    
                    Debug.LogWarning($"[DungeonCorridorValidator] 복도가 너무 짧음: {problemInfo}");
                }
            }
        }
        
        // 방 겹침 검증 추가
        bool hasRoomOverlap = ValidateRoomOverlaps(dungeonGrid, out List<string> overlappingRooms);
        if (hasRoomOverlap)
        {
            hasProblem = true;
            problematicCorridors.AddRange(overlappingRooms);
        }
        
        return hasProblem;
    }
    
    /// <summary>
    /// 모든 방이 겹치지 않는지 검증합니다.
    /// </summary>
    /// <param name="dungeonGrid">던전 그리드</param>
    /// <param name="overlappingRooms">겹치는 방 정보 (out)</param>
    /// <returns>방이 겹치면 true, 정상이면 false</returns>
    public static bool ValidateRoomOverlaps(
        DungeonGrid dungeonGrid,
        out List<string> overlappingRooms)
    {
        overlappingRooms = new List<string>();
        
        if (dungeonGrid == null)
        {
            return false;
        }
        
        bool hasOverlap = false;
        var positionsList = new List<Vector2Int>(dungeonGrid.GetAllPositions());
        
        // 모든 방 쌍에 대해 겹침 검사
        for (int i = 0; i < positionsList.Count; i++)
        {
            Vector2Int position1 = positionsList[i];
            Room room1 = dungeonGrid.GetRoom(position1);
            if (room1 == null || room1.roomObject == null) continue;
            
            // 방 크기 가져오기
            BaseRoom baseRoom1 = room1.roomObject.GetComponent<BaseRoom>();
            if (baseRoom1 == null) continue;
            
            float width1 = baseRoom1.RoomWidth;
            float height1 = baseRoom1.RoomHeight;
            Vector3 center1 = DungeonRoomHelper.GetRoomWorldCenter(room1.roomObject);
            
            float halfWidth1 = width1 * 0.5f;
            float halfHeight1 = height1 * 0.5f;
            
            // 방1의 경계 상자
            float room1MinX = center1.x - halfWidth1;
            float room1MaxX = center1.x + halfWidth1;
            float room1MinY = center1.y - halfHeight1;
            float room1MaxY = center1.y + halfHeight1;
            
            for (int j = i + 1; j < positionsList.Count; j++)
            {
                Vector2Int position2 = positionsList[j];
                Room room2 = dungeonGrid.GetRoom(position2);
                if (room2 == null || room2.roomObject == null) continue;
                
                // 방 크기 가져오기
                BaseRoom baseRoom2 = room2.roomObject.GetComponent<BaseRoom>();
                if (baseRoom2 == null) continue;
                
                float width2 = baseRoom2.RoomWidth;
                float height2 = baseRoom2.RoomHeight;
                Vector3 center2 = DungeonRoomHelper.GetRoomWorldCenter(room2.roomObject);
                
                float halfWidth2 = width2 * 0.5f;
                float halfHeight2 = height2 * 0.5f;
                
                // 방2의 경계 상자
                float room2MinX = center2.x - halfWidth2;
                float room2MaxX = center2.x + halfWidth2;
                float room2MinY = center2.y - halfHeight2;
                float room2MaxY = center2.y + halfHeight2;
                
                // 경계 상자 겹침 확인
                bool xOverlap = room1MaxX > room2MinX && room1MinX < room2MaxX;
                bool yOverlap = room1MaxY > room2MinY && room1MinY < room2MaxY;
                
                if (xOverlap && yOverlap)
                {
                    hasOverlap = true;
                    
                    // 겹침 영역 계산
                    float overlapX = Mathf.Min(room1MaxX, room2MaxX) - Mathf.Max(room1MinX, room2MinX);
                    float overlapY = Mathf.Min(room1MaxY, room2MaxY) - Mathf.Max(room1MinY, room2MinY);
                    float overlapArea = overlapX * overlapY;
                    
                    string overlapInfo = $"방1({position1}) ↔ 방2({position2}): " +
                        $"방이 겹침 (겹침 영역: {overlapArea:F2}, " +
                        $"방1 중심: {center1}, 방2 중심: {center2})";
                    overlappingRooms.Add(overlapInfo);
                    
                    Debug.LogWarning($"[DungeonCorridorValidator] 방 겹침 감지: {overlapInfo}");
                }
            }
        }
        
        return hasOverlap;
    }
}

