using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 방 오브젝트 배치를 담당하는 클래스
/// </summary>
public static class DungeonRoomPlacer
{
    /// <summary>
    /// 방 오브젝트를 생성하고 배치합니다.
    /// </summary>
    public static void CreateRoomObjects(
        DungeonGrid dungeonGrid,
        Transform parent,
        Grid unityGrid,
        int roomSpacingInCells,
        Dictionary<RoomType, GameObject[]> roomPrefabs,
        bool showRoomTypeLabels,
        float roomLabelOffsetX,
        float roomLabelOffsetY,
        float cellSize)
    {
        // 먼저 모든 방의 크기 정보를 수집
        Dictionary<Vector2Int, Vector2> roomSizes = new Dictionary<Vector2Int, Vector2>();
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null) continue;
            
            GameObject prefab = GetRoomPrefab(room.roomType, roomPrefabs);
            if (prefab == null) continue;
            
            BaseRoom prefabBaseRoom = prefab.GetComponent<BaseRoom>();
            if (prefabBaseRoom != null)
            {
                float width = prefabBaseRoom.RoomWidth;
                float height = prefabBaseRoom.RoomHeight;
                roomSizes[position] = new Vector2(width, height);
            }
            else
            {
                // BaseRoom이 없으면 기본값 사용
                roomSizes[position] = new Vector2(roomSpacingInCells, roomSpacingInCells);
            }
        }
        
        // 각 방을 배치하면서 충돌 검사 및 조정
        // roomPositions에는 RoomCenterMarker의 월드 위치를 저장
        Dictionary<Vector2Int, Vector3> roomPositions = new Dictionary<Vector2Int, Vector3>();
        Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
        
        // 십자 규격 정렬: 같은 행/열의 방들이 정확히 같은 x/y 좌표를 가지도록
        // 먼저 모든 방의 기본 위치를 계산 (행/열별로 최대 크기를 고려하여 정렬)
        Dictionary<int, float> rowYPositions = new Dictionary<int, float>(); // 행(y)별 Y 좌표
        Dictionary<int, float> colXPositions = new Dictionary<int, float>(); // 열(x)별 X 좌표
        
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null) continue;
            
            // 기본 위치 계산 (행/열 정렬을 위한 기준 위치)
            Vector3Int baseRoomCenterCell = new Vector3Int(position.x * roomSpacingInCells, position.y * roomSpacingInCells, 0);
            Vector3 baseWorldPosition = unityGrid != null
                ? unityGrid.CellToWorld(baseRoomCenterCell) + cellCenterOffset
                : new Vector3(position.x * roomSpacingInCells * cellSize + cellSize * 0.5f, 
                             position.y * roomSpacingInCells * cellSize + cellSize * 0.5f, 0f);
            
            // 행/열별 위치 저장 (같은 행/열의 방들이 같은 좌표를 가지도록)
            if (!rowYPositions.ContainsKey(position.y))
            {
                rowYPositions[position.y] = baseWorldPosition.y;
            }
            if (!colXPositions.ContainsKey(position.x))
            {
                colXPositions[position.x] = baseWorldPosition.x;
            }
        }
        
        // 모든 방을 먼저 생성 (임시 위치)
        Dictionary<Vector2Int, GameObject> tempRoomObjects = new Dictionary<Vector2Int, GameObject>();
        
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null) continue;
            
            GameObject prefab = GetRoomPrefab(room.roomType, roomPrefabs);
            if (prefab == null)
            {
                Debug.LogWarning($"{room.roomType} 프리펩이 설정되지 않았습니다.");
                continue;
            }
            
            // 십자 규격 정렬: 같은 행/열의 방들이 정확히 같은 x/y 좌표를 가지도록
            Vector3 targetRoomCenterPos = new Vector3(
                colXPositions[position.x],  // 같은 열(x)의 방들은 모두 같은 X 좌표
                rowYPositions[position.y],  // 같은 행(y)의 방들은 모두 같은 Y 좌표
                0f
            );
            
            // Grid 하위에 생성 (임시 위치)
            GameObject roomObj = Object.Instantiate(prefab, targetRoomCenterPos, Quaternion.identity, parent);
            
            // RoomCenterMarker를 기준으로 위치 조정
            AlignRoomToGridCenter(roomObj, targetRoomCenterPos);
            
            // RoomCenterMarker의 최종 위치를 저장
            Vector3 finalRoomCenterPos = DungeonRoomHelper.GetRoomWorldCenter(roomObj);
            roomPositions[position] = finalRoomCenterPos;
            tempRoomObjects[position] = roomObj;
        }
        
        // 충돌 검사 및 조정 (반복적으로 모든 충돌 해결)
        // 십자 규격 정렬을 유지하면서 충돌을 해결하는 것은 복잡하므로,
        // 같은 행/열이 아닌 방들만 충돌 검사하고, 같은 행/열은 roomSpacingInCells로 이미 간격이 확보되어 있다고 가정
        int maxIterations = 10;
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            bool hasCollision = false;
            
            // 모든 방 쌍에 대해 충돌 검사 (같은 행/열이 아닌 경우만)
            var positionsList = new List<Vector2Int>(dungeonGrid.GetAllPositions());
            
            for (int i = 0; i < positionsList.Count; i++)
            {
                Vector2Int position1 = positionsList[i];
                if (!tempRoomObjects.ContainsKey(position1) || !roomSizes.ContainsKey(position1)) continue;
                
                Vector2 size1 = roomSizes[position1];
                Vector3 pos1 = roomPositions[position1];
                float halfWidth1 = size1.x * 0.5f;
                float halfHeight1 = size1.y * 0.5f;
                
                for (int j = i + 1; j < positionsList.Count; j++)
                {
                    Vector2Int position2 = positionsList[j];
                    if (!tempRoomObjects.ContainsKey(position2) || !roomSizes.ContainsKey(position2)) continue;
                    
                    // 같은 행 또는 열에 있는 방은 건너뛰기 (이미 정렬되어 있고 충돌하지 않음)
                    if (position1.x == position2.x || position1.y == position2.y) continue;
                    
                    Vector2 size2 = roomSizes[position2];
                    Vector3 pos2 = roomPositions[position2];
                    float halfWidth2 = size2.x * 0.5f;
                    float halfHeight2 = size2.y * 0.5f;
                    
                    // 두 방 사이의 거리
                    float dx = Mathf.Abs(pos1.x - pos2.x);
                    float dy = Mathf.Abs(pos1.y - pos2.y);
                    
                    // 최소 간격 (복도 간격 포함)
                    float corridorSpacing = roomSpacingInCells * cellSize;
                    float minRequiredX = halfWidth1 + halfWidth2 + corridorSpacing;
                    float minRequiredY = halfHeight1 + halfHeight2 + corridorSpacing;
                    
                    // 경계 상자 겹침 확인
                    bool xOverlap = dx < (halfWidth1 + halfWidth2);
                    bool yOverlap = dy < (halfHeight1 + halfHeight2);
                    
                    if (xOverlap && yOverlap)
                    {
                        hasCollision = true;
                        
                        // X축과 Y축 중 더 큰 겹침을 해결
                        float xGap = (halfWidth1 + halfWidth2) - dx;
                        float yGap = (halfHeight1 + halfHeight2) - dy;
                        
                        // 최대 이동 거리 제한 (한 번에 너무 많이 이동하지 않도록)
                        float maxPush = roomSpacingInCells * cellSize * 0.5f; // 최대 roomSpacingInCells의 절반만 이동
                        
                        if (xGap >= yGap)
                        {
                            // X축으로 분리
                            float pushX = Mathf.Min(minRequiredX - dx, maxPush);
                            float pushXInCells = Mathf.Ceil(pushX / cellSize);
                            float direction = pos2.x > pos1.x ? 1 : -1;
                            
                            // 같은 열의 모든 방 이동
                            int col = position1.x;
                            if (colXPositions.ContainsKey(col))
                            {
                                colXPositions[col] -= direction * pushXInCells * cellSize;
                                
                                // 같은 열의 모든 방 위치 업데이트
                                foreach (var sameColPos in dungeonGrid.GetAllPositions())
                                {
                                    if (sameColPos.x == col && tempRoomObjects.ContainsKey(sameColPos))
                                    {
                                        GameObject sameColRoom = tempRoomObjects[sameColPos];
                                        Vector3 newPos = new Vector3(colXPositions[col], roomPositions[sameColPos].y, 0f);
                                        AlignRoomToGridCenter(sameColRoom, newPos);
                                        roomPositions[sameColPos] = DungeonRoomHelper.GetRoomWorldCenter(sameColRoom);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Y축으로 분리
                            float pushY = Mathf.Min(minRequiredY - dy, maxPush);
                            float pushYInCells = Mathf.Ceil(pushY / cellSize);
                            float direction = pos2.y > pos1.y ? 1 : -1;
                            
                            // 같은 행의 모든 방 이동
                            int row = position1.y;
                            if (rowYPositions.ContainsKey(row))
                            {
                                rowYPositions[row] -= direction * pushYInCells * cellSize;
                                
                                // 같은 행의 모든 방 위치 업데이트
                                foreach (var sameRowPos in dungeonGrid.GetAllPositions())
                                {
                                    if (sameRowPos.y == row && tempRoomObjects.ContainsKey(sameRowPos))
                                    {
                                        GameObject sameRowRoom = tempRoomObjects[sameRowPos];
                                        Vector3 newPos = new Vector3(roomPositions[sameRowPos].x, rowYPositions[row], 0f);
                                        AlignRoomToGridCenter(sameRowRoom, newPos);
                                        roomPositions[sameRowPos] = DungeonRoomHelper.GetRoomWorldCenter(sameRowRoom);
                                    }
                                }
                            }
                        }
                        
                        // 위치 업데이트 후 다시 검사
                        pos1 = roomPositions[position1];
                    }
                }
            }
            
            if (!hasCollision)
            {
                Debug.Log($"[DungeonRoomPlacer] 모든 충돌 해결 완료 (반복 횟수: {iteration + 1})");
                break;
            }
            
            if (iteration == maxIterations - 1)
            {
                Debug.LogWarning($"[DungeonRoomPlacer] 최대 반복 횟수({maxIterations})에 도달했지만 일부 충돌이 남아있을 수 있습니다.");
            }
        }
        
        // 최종 위치로 방 설정 및 초기화
        foreach (var kvp in tempRoomObjects)
        {
            Vector2Int position = kvp.Key;
            GameObject roomObj = kvp.Value;
            Room room = dungeonGrid.GetRoom(position);
            
            if (room == null) continue;
            
            room.roomObject = roomObj;
            
            // 방 스크립트에 문 정보 전달
            BaseRoom roomScript = roomObj.GetComponent<BaseRoom>();
            if (roomScript != null)
            {
                roomScript.InitializeRoom(room);
                roomScript.RefreshDoorStates();
            }
            else
            {
                Debug.LogWarning($"[DungeonRoomPlacer] BaseRoom 컴포넌트가 없습니다: {roomObj.name}");
            }
            
            // 방 타입 텍스트 표시
            if (showRoomTypeLabels)
            {
                Vector3 center = DungeonRoomHelper.GetRoomWorldCenter(roomObj);
                CreateRoomTypeLabel(roomObj, room.roomType, center, roomLabelOffsetX, roomLabelOffsetY);
            }
        }
    }
    
    /// <summary>
    /// 십자 규격 정렬을 유지하면서 충돌을 검사하고 위치를 조정합니다.
    /// 같은 행/열의 방들은 정확히 같은 x/y 좌표를 유지합니다.
    /// </summary>
    private static Vector3 AdjustRoomPositionForCollisionAligned(
        Vector2Int currentPos, 
        Vector3 basePosition, 
        Vector2 currentRoomSize,
        Dictionary<Vector2Int, Vector3> existingPositions,
        Dictionary<Vector2Int, Vector2> roomSizes,
        float cellSize,
        int roomSpacingInCells,
        Dictionary<int, float> colXPositions,
        Dictionary<int, float> rowYPositions)
    {
        float adjustedX = basePosition.x;
        float adjustedY = basePosition.y;
        float corridorSpacing = roomSpacingInCells * cellSize; // 칸 수를 Unity unit으로 변환
        
        // 같은 행/열이 아닌 모든 이미 배치된 방과 충돌 검사
        foreach (var kvp in existingPositions)
        {
            Vector2Int existingPos = kvp.Key;
            Vector3 existingRoomCenterPos = kvp.Value;
            
            // 자기 자신은 건너뛰기
            if (existingPos == currentPos) continue;
            
            // 같은 행 또는 열에 있는 방은 이미 정렬되어 있으므로 건너뛰기
            // (같은 행/열은 충돌하지 않도록 roomSpacingInCells로 이미 간격이 확보됨)
            if (existingPos.x == currentPos.x || existingPos.y == currentPos.y) continue;
            
            // 방 크기 정보 확인
            if (!roomSizes.ContainsKey(existingPos)) continue;
            Vector2 existingSize = roomSizes[existingPos];
            
            // 현재 방과 기존 방의 반경 계산
            float currentHalfWidth = currentRoomSize.x * 0.5f;
            float currentHalfHeight = currentRoomSize.y * 0.5f;
            float existingHalfWidth = existingSize.x * 0.5f;
            float existingHalfHeight = existingSize.y * 0.5f;
            
            // 두 방 사이의 실제 거리 (대각선 거리)
            float dx = Mathf.Abs(adjustedX - existingRoomCenterPos.x);
            float dy = Mathf.Abs(adjustedY - existingRoomCenterPos.y);
            
            // 방 겹침 방지: 두 방의 경계 상자(Bounding Box)가 겹치지 않도록 확인
            // X축과 Y축 모두에서 최소 간격 확보 필요
            
            // X축 최소 간격: 두 방의 반폭 합 + 복도 간격
            float minRequiredX = currentHalfWidth + existingHalfWidth + corridorSpacing;
            // Y축 최소 간격: 두 방의 반높이 합 + 복도 간격
            float minRequiredY = currentHalfHeight + existingHalfHeight + corridorSpacing;
            
            // X축 충돌 검사: X축 거리가 부족하고 Y축에서도 겹칠 가능성이 있는 경우
            bool xCollision = dx < minRequiredX;
            bool yOverlap = dy < (currentHalfHeight + existingHalfHeight);
            
            if (xCollision && yOverlap)
            {
                // X축으로 충분히 밀어내기 (Grid 셀 단위로, 같은 열 정렬 유지)
                float pushX = minRequiredX - dx;
                float pushXInCells = Mathf.Ceil(pushX / cellSize);
                
                // 같은 열의 모든 방들을 함께 이동시켜야 함 (십자 규격 유지)
                float direction = existingRoomCenterPos.x > adjustedX ? 1 : -1;
                float newX = adjustedX + direction * pushXInCells * cellSize;
                adjustedX = newX;
                
                // 열의 X 좌표 업데이트 (같은 열의 모든 방이 함께 이동)
                colXPositions[currentPos.x] = adjustedX;
                
                // X축 조정 후 거리 재계산
                dx = Mathf.Abs(adjustedX - existingRoomCenterPos.x);
            }
            
            // Y축 충돌 검사: Y축 거리가 부족하고 X축에서도 겹칠 가능성이 있는 경우
            bool yCollision = dy < minRequiredY;
            bool xOverlap = dx < (currentHalfWidth + existingHalfWidth);
            
            if (yCollision && xOverlap)
            {
                // Y축으로 충분히 밀어내기 (Grid 셀 단위로, 같은 행 정렬 유지)
                float pushY = minRequiredY - dy;
                float pushYInCells = Mathf.Ceil(pushY / cellSize);
                
                // 같은 행의 모든 방들을 함께 이동시켜야 함 (십자 규격 유지)
                float direction = existingRoomCenterPos.y > adjustedY ? 1 : -1;
                float newY = adjustedY + direction * pushYInCells * cellSize;
                adjustedY = newY;
                
                // 행의 Y 좌표 업데이트 (같은 행의 모든 방이 함께 이동)
                rowYPositions[currentPos.y] = adjustedY;
            }
        }
        
        // 최종 위치 (십자 규격 정렬 유지)
        return new Vector3(adjustedX, adjustedY, basePosition.z);
    }
    
    /// <summary>
    /// 방 프리팹을 Grid 셀 중심에 정렬합니다.
    /// </summary>
    private static void AlignRoomToGridCenter(GameObject roomObj, Vector3 targetCenter)
    {
        if (roomObj == null) return;

        // RoomCenterMarker를 우선 사용
        Transform marker = DungeonRoomHelper.FindRoomCenterMarker(roomObj);
        Vector3 currentCenter = marker != null ? marker.position : roomObj.transform.position;

        Vector3 offset = targetCenter - currentCenter;
        roomObj.transform.position += offset;
    }
    
    /// <summary>
    /// 방 타입을 표시하는 텍스트 라벨을 생성합니다.
    /// </summary>
    private static void CreateRoomTypeLabel(GameObject roomObj, RoomType roomType, Vector3 roomWorldPos, float roomLabelOffsetX, float roomLabelOffsetY)
    {
        // 방 중심 기준 크기 계산 (월드 단위)
        float roomSize = 0f;
        BaseRoom baseRoom = roomObj.GetComponent<BaseRoom>();
        if (baseRoom != null && baseRoom.RoomSize > 0)
        {
            float cellSize = DungeonGridHelper.ResolveCellSize(roomObj, null);
            float tileSize = baseRoom.TileSize > 0f ? baseRoom.TileSize : cellSize;
            roomSize = baseRoom.RoomSize * tileSize;
        }
        else
        {
            // Renderer로부터 크기 추정
            Renderer renderer = roomObj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                roomSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y);
            }
        }

        float halfRoomSize = roomSize > 0 ? roomSize * 0.5f : 0f;

        // 라벨의 월드 위치 계산
        Vector3 worldLabelPos = roomWorldPos + new Vector3(roomLabelOffsetX, halfRoomSize + roomLabelOffsetY, 0f);

        // 방의 로컬 좌표계로 변환
        Vector3 localLabelPos = roomObj.transform.InverseTransformPoint(worldLabelPos);

        // TextMesh 오브젝트 생성
        GameObject labelObj = new GameObject($"RoomTypeLabel_{roomType}");
        labelObj.transform.SetParent(roomObj.transform, false);
        labelObj.transform.localPosition = localLabelPos;

        TextMesh textMesh = labelObj.AddComponent<TextMesh>();

        // 방 타입에 따라 텍스트와 색상 설정
        string roomTypeText = "";
        Color textColor = Color.white;

        switch (roomType)
        {
            case RoomType.Start:
                roomTypeText = "시작 방";
                textColor = Color.green;
                break;
            case RoomType.Exit:
                roomTypeText = "탈출 방";
                textColor = Color.cyan;
                break;
            case RoomType.Event:
                roomTypeText = "이벤트 방";
                textColor = Color.purple;
                break;
            case RoomType.Trap:
                roomTypeText = "함정 방";
                textColor = Color.red;
                break;
            case RoomType.Treasure:
                roomTypeText = "보물 방";
                textColor = Color.yellow;
                break;
            case RoomType.Boss:
                roomTypeText = "보스 방";
                textColor = Color.orange;
                break;
            case RoomType.Normal:
            default:
                roomTypeText = "전투 방";
                textColor = Color.white;
                break;
        }

        // TextMesh 설정
        textMesh.text = roomTypeText;
        textMesh.color = textColor;
        textMesh.fontSize = 40;
        textMesh.characterSize = 0.5f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.offsetZ = -1f;
    }
    
    /// <summary>
    /// 방 타입에 해당하는 프리팹 리스트에서 랜덤하게 하나를 선택하여 반환합니다.
    /// </summary>
    private static GameObject GetRoomPrefab(RoomType type, Dictionary<RoomType, GameObject[]> roomPrefabs)
    {
        // 해당 타입의 프리팹 리스트가 있는지 확인
        if (roomPrefabs.ContainsKey(type) && roomPrefabs[type] != null && roomPrefabs[type].Length > 0)
        {
            GameObject[] prefabs = roomPrefabs[type];
            // null이 아닌 프리팹만 필터링
            var validPrefabs = System.Array.FindAll(prefabs, p => p != null);
            if (validPrefabs.Length > 0)
            {
                // 프리팹 리스트에서 랜덤하게 선택
                return validPrefabs[Random.Range(0, validPrefabs.Length)];
            }
        }
        
        // Normal 프리팹을 기본값으로 사용
        if (roomPrefabs.ContainsKey(RoomType.Normal) && roomPrefabs[RoomType.Normal] != null && roomPrefabs[RoomType.Normal].Length > 0)
        {
            var normalPrefabs = System.Array.FindAll(roomPrefabs[RoomType.Normal], p => p != null);
            if (normalPrefabs.Length > 0)
            {
                return normalPrefabs[Random.Range(0, normalPrefabs.Length)];
            }
        }
        
        return null;
    }
}
