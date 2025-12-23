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
        Dictionary<RoomType, GameObject> roomPrefabs,
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
            
            Vector2 currentRoomSize = roomSizes[position];
            
            // 십자 규격 정렬: 같은 행/열의 방들이 정확히 같은 x/y 좌표를 가지도록
            Vector3 targetRoomCenterPos = new Vector3(
                colXPositions[position.x],  // 같은 열(x)의 방들은 모두 같은 X 좌표
                rowYPositions[position.y],  // 같은 행(y)의 방들은 모두 같은 Y 좌표
                0f
            );
            
            // 충돌 검사: 같은 행/열이 아닌 방들과의 충돌만 확인 (같은 행/열은 이미 정렬됨)
            targetRoomCenterPos = AdjustRoomPositionForCollisionAligned(
                position, targetRoomCenterPos, currentRoomSize, roomPositions, roomSizes, 
                cellSize, roomSpacingInCells, colXPositions, rowYPositions);
            
            // Grid 하위에 생성 (임시 위치)
            GameObject roomObj = Object.Instantiate(prefab, targetRoomCenterPos, Quaternion.identity, parent);
            
            // RoomCenterMarker를 기준으로 위치 조정
            // RoomCenterMarker가 targetRoomCenterPos에 오도록 전체 방을 오프셋 조정
            AlignRoomToGridCenter(roomObj, targetRoomCenterPos);
            
            // RoomCenterMarker의 최종 위치를 저장 (다음 방 배치 시 참조용)
            Vector3 finalRoomCenterPos = DungeonRoomHelper.GetRoomWorldCenter(roomObj);
            roomPositions[position] = finalRoomCenterPos;
            
            room.roomObject = roomObj;
            
            // 방 스크립트에 문 정보 전달
            BaseRoom roomScript = roomObj.GetComponent<BaseRoom>();
            if (roomScript != null)
            {
                roomScript.InitializeRoom(room);
                roomScript.RefreshDoorStates(); // 생성 직후 문/NoDoor 상태 재정렬
            }
            else
            {
                Debug.LogWarning($"[DungeonRoomPlacer] BaseRoom 컴포넌트가 없습니다: {roomObj.name}");
            }
            
            // 방 타입 텍스트 표시
            if (showRoomTypeLabels)
            {
                // 실제 방 중심(가능하면 RoomCenterMarker 기준)을 사용
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
            
            // X축 충돌 검사: 대각선 상의 방과 충돌하는 경우 X축으로 밀어내기
            float minRequiredX = currentHalfWidth + existingHalfWidth + corridorSpacing;
            if (dx < minRequiredX && dy < currentHalfHeight + existingHalfHeight)
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
            }
            
            // Y축 충돌 검사: 대각선 상의 방과 충돌하는 경우 Y축으로 밀어내기
            float minRequiredY = currentHalfHeight + existingHalfHeight + corridorSpacing;
            if (dy < minRequiredY && dx < currentHalfWidth + existingHalfWidth)
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
    
    private static GameObject GetRoomPrefab(RoomType type, Dictionary<RoomType, GameObject> roomPrefabs)
    {
        if (roomPrefabs.ContainsKey(type) && roomPrefabs[type] != null)
            return roomPrefabs[type];
        
        // Normal 프리팹을 기본값으로 사용
        if (roomPrefabs.ContainsKey(RoomType.Normal))
            return roomPrefabs[RoomType.Normal];
        
        return null;
    }
}
