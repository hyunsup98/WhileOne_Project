using UnityEngine;

/// <summary>
/// Grid 및 스페이싱 관련 유틸리티 메서드들
/// </summary>
public static class DungeonGridHelper
{
    private const float DefaultCellSize = 1f; // 타일 한 칸 기본 크기 (PPU 32, Grid cell size 1)
    
    /// <summary>
    /// Grid 오브젝트를 찾거나 생성합니다.
    /// 우선순위: 1. 지정된 gridParent 사용, 2. 씬에서 Grid 찾기, 3. 신규 생성
    /// </summary>
    public static Grid SetupGridParent(Transform gridParent, Transform fallbackParent, out Transform finalGridParent)
    {
        // 1순위: gridParent가 Inspector에서 명시적으로 설정되어 있으면 무조건 사용
        if (gridParent != null)
        {
            Grid unityGrid = gridParent.GetComponent<Grid>();
            // gridParent에 Grid가 없으면 자식에서 찾기
            if (unityGrid == null)
            {
                unityGrid = gridParent.GetComponentInChildren<Grid>();
            }
            
            // Grid 컴포넌트를 찾았으면 사용
            if (unityGrid != null)
            {
                Debug.Log($"[DungeonGridHelper] 지정된 gridParent({gridParent.name})의 Grid를 사용합니다.");
                finalGridParent = gridParent;
                return unityGrid;
            }
            
            // gridParent는 설정되어 있지만 Grid 컴포넌트가 없는 경우
            // gridParent에 Grid 컴포넌트 추가
            unityGrid = gridParent.gameObject.AddComponent<Grid>();
            if (unityGrid != null)
            {
                Debug.Log($"[DungeonGridHelper] 지정된 gridParent({gridParent.name})에 Grid 컴포넌트를 추가했습니다.");
                finalGridParent = gridParent;
                return unityGrid;
            }
            else
            {
                Debug.LogError($"[DungeonGridHelper] 지정된 gridParent({gridParent.name})에 Grid 컴포넌트를 추가할 수 없습니다.");
                finalGridParent = null;
                return null;
            }
        }
        
        // 2순위: 씬에서 Grid 오브젝트 찾기 (gridParent가 설정되지 않은 경우만)
        Grid foundGrid = Object.FindFirstObjectByType<Grid>();
        if (foundGrid != null)
        {
            finalGridParent = foundGrid.transform;
            Debug.Log($"[DungeonGridHelper] 씬에서 발견한 Grid({foundGrid.name})를 사용합니다.");
            return foundGrid;
        }
        
        // 3순위: Grid 오브젝트가 없으면 신규 생성
        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(fallbackParent);
        finalGridParent = gridObj.transform;
        Grid newGrid = gridObj.AddComponent<Grid>();
        
        if (newGrid == null)
        {
            Debug.LogError("[DungeonGridHelper] Grid 컴포넌트 생성에 실패했습니다.");
            return null;
        }
        else
        {
            Debug.LogWarning($"[DungeonGridHelper] Grid 오브젝트를 신규 생성했습니다. ({gridObj.name}) Inspector에서 gridParent를 지정하는 것을 권장합니다.");
        }
        
        return newGrid;
    }
    
    /// <summary>
    /// 방 간격을 자동으로 계산합니다 (칸 수 단위, 4의 배수로 반올림).
    /// </summary>
    public static int CalculateRoomSpacingInCells(GameObject normalRoomPrefab, float minTileSpacing, float cellSize)
    {
        if (normalRoomPrefab == null) return 12; // 기본값 (4의 배수)
        
        BaseRoom baseRoom = normalRoomPrefab.GetComponent<BaseRoom>();
        float roomSize = 0f;
        
        if (baseRoom != null)
        {
            // RoomWidth와 RoomHeight는 이미 Unity unit으로 반환되므로 그대로 사용
            float roomWidth = baseRoom.RoomWidth;
            float roomHeight = baseRoom.RoomHeight;
            // 가로와 세로 중 큰 값을 사용 (방 간격 계산용)
            roomSize = Mathf.Max(roomWidth, roomHeight);
        }
        else
        {
            // BaseRoom이 없으면 프리펩의 크기로 추정
            Renderer renderer = normalRoomPrefab.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                roomSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y);
            }
        }
        
        if (roomSize > 0)
        {
            // 칸 수로 계산
            float roomSizeInCells = roomSize / cellSize;
            float corridorLengthInCells = 3f; // 복도 3칸
            float minSpacingInCells = roomSizeInCells + corridorLengthInCells + minTileSpacing;
            
            // 4의 배수로 올림 (Mathf.CeilToInt로 올림 후 4의 배수로 조정)
            int spacingInCells = Mathf.CeilToInt(minSpacingInCells);
            spacingInCells = ((spacingInCells + 3) / 4) * 4; // 올림하여 4의 배수로 조정
            
            return spacingInCells;
        }
        
        return 12; // 기본값 (4의 배수)
    }

    /// <summary>
    /// 셀 크기를 결정합니다. (기본값 1, PPU 32, Grid cell size 1)
    /// </summary>
    public static float ResolveCellSize(GameObject normalRoomPrefab, Grid unityGrid)
    {
        // BaseRoom tileSize 우선
        BaseRoom prefabBaseRoom = normalRoomPrefab != null ? normalRoomPrefab.GetComponent<BaseRoom>() : null;
        if (prefabBaseRoom != null && prefabBaseRoom.TileSize > 0f)
            return prefabBaseRoom.TileSize;
        
        // Grid cellSize 사용
        if (unityGrid != null && unityGrid.cellSize.x > 0f)
            return unityGrid.cellSize.x;
        
        // 기본값
        return DefaultCellSize;
    }
}
