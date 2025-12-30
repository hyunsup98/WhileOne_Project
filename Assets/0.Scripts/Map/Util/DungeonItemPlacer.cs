using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 던전에 아이템을 배치하는 클래스
/// </summary>
public static class DungeonItemPlacer
{
    /// <summary>
    /// 일반 전투 방에 Dig Spot을 배치합니다.
    /// </summary>
    public static void PlaceDigSpots(
        DungeonGrid dungeonGrid,
        float digSpotSpawnChance,
        Grid unityGrid)
    {
        Tile digSpotTile = GameManager.Instance.CurrentDungeon.DigSpotTile;

        if (digSpotTile == null)
        {
            Debug.LogWarning("[DungeonItemPlacer] digSpotTile이 지정되지 않아 Dig Spot을 생성할 수 없습니다.");
            return;
        }
        
        // 모든 일반 전투 방 확인
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            // 일반 전투 방만 처리
            if (room.roomType != RoomType.Normal) continue;
            
            // 확률로 생성
            if (Random.Range(0f, 100f) >= digSpotSpawnChance) continue;
            
            // Dig Spot 배치
            PlaceDigSpotInRoom(room, digSpotTile, unityGrid);
        }
    }
    
    /// <summary>
    /// 보물 방에 Treasure Chest를 배치합니다.
    /// </summary>
    public static void PlaceTreasureChests(
        DungeonGrid dungeonGrid,
        GameObject treasureChestPrefab,
        Grid unityGrid)
    {
        if (treasureChestPrefab == null)
        {
            Debug.LogWarning("[DungeonItemPlacer] treasureChestPrefab이 지정되지 않아 treasure chest를 생성할 수 없습니다.");
            return;
        }

        // 모든 보물 방 확인
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;

            // 보물 방만 처리
            if (room.roomType != RoomType.Treasure) continue;

            // treasure chest 배치
            PlaceTreasureChestInRoom(room, treasureChestPrefab, unityGrid);
        }
    }
    
    /// <summary>
    /// 방에 Dig Spot을 배치합니다.
    /// </summary>
    private static void PlaceDigSpotInRoom(Room room, Tile digSpotTile, Grid unityGrid)
    {
        if (room.roomObject == null) return;
        
        // 방 내부의 Tilemap 찾기
        Tilemap roomTilemap = room.roomObject.GetComponentInChildren<Tilemap>();
        if (roomTilemap == null)
        {
            Debug.LogWarning($"[DungeonItemPlacer] 방({room.roomObject.name})에 Tilemap을 찾을 수 없습니다.");
            return;
        }
        
        // Grid 컴포넌트 찾기
        Grid targetGrid = roomTilemap.GetComponentInParent<Grid>();
        if (targetGrid == null)
        {
            targetGrid = unityGrid;
        }
        
        if (targetGrid == null)
        {
            Debug.LogWarning($"[DungeonItemPlacer] Grid를 찾을 수 없습니다.");
            return;
        }
        
        // 중앙 기준: RoomCenterMarker가 있으면 그 위치를 사용, 없으면 타일맵 중앙 셀 사용
        Transform centerMarker = DungeonRoomHelper.FindRoomCenterMarker(room.roomObject);
        Vector3Int centerCell = centerMarker != null
            ? targetGrid.WorldToCell(centerMarker.position)
            : DungeonRoomHelper.GetRoomCenterCell(roomTilemap);
        
        // 중앙 셀에 타일이 있는지 확인
        Vector3Int digSpotCell = centerCell;
        TileBase centerTile = roomTilemap.GetTile(centerCell);
        
        // 중앙에 타일이 없으면 가장 가까운 타일이 있는 셀 찾기
        if (centerTile == null)
        {
            digSpotCell = DungeonRoomHelper.FindNearestTileCell(roomTilemap, centerCell, targetGrid);
            if (digSpotCell == Vector3Int.zero && centerCell != Vector3Int.zero)
            {
                digSpotCell = centerCell;
            }
        }
        
        // TODO: 프리팹 설치를 타일 맵 설치로 리팩토링
        // Dig Spot 타일 배치 (현재는 구현되지 않음)
    }
    
    /// <summary>
    /// 보물 방에 보물 상자를 배치합니다.
    /// </summary>
    private static void PlaceTreasureChestInRoom(Room room, GameObject treasureChestPrefab, Grid unityGrid)
    {
        if (room.roomObject == null) return;

        // 방 내부의 Tilemap 찾기
        Tilemap roomTilemap = room.roomObject.GetComponentInChildren<Tilemap>();
        if (roomTilemap == null)
        {
            Debug.LogWarning($"[DungeonItemPlacer] 방({room.roomObject.name})에 Tilemap을 찾을 수 없습니다.");
            return;
        }

        // Grid 컴포넌트 찾기
        Grid targetGrid = roomTilemap.GetComponentInParent<Grid>();
        if (targetGrid == null)
        {
            targetGrid = unityGrid;
        }

        if (targetGrid == null)
        {
            Debug.LogWarning($"[DungeonItemPlacer] Grid를 찾을 수 없습니다.");
            return;
        }

        // 중앙 기준: RoomCenterMarker가 있으면 그 위치를 사용, 없으면 타일맵 중앙 셀 사용
        Transform centerMarker = DungeonRoomHelper.FindRoomCenterMarker(room.roomObject);
        Vector3Int centerCell = centerMarker != null
            ? targetGrid.WorldToCell(centerMarker.position)
            : DungeonRoomHelper.GetRoomCenterCell(roomTilemap);

        // 중앙 셀에 타일이 있는지 확인
        Vector3Int treasureChestCell = centerCell;
        TileBase centerTile = roomTilemap.GetTile(centerCell);

        // 중앙에 타일이 없으면 가장 가까운 타일이 있는 셀 찾기
        if (centerTile == null)
        {
            treasureChestCell = DungeonRoomHelper.FindNearestTileCell(roomTilemap, centerCell, targetGrid);
            if (treasureChestCell == Vector3Int.zero && centerCell != Vector3Int.zero)
            {
                treasureChestCell = centerCell;
            }
        }

        // 셀 좌표를 월드 좌표로 변환
        Vector3 treasureChestWorldPos = targetGrid.CellToWorld(treasureChestCell);

        // 셀 중심으로 보정 (타일맵의 tileAnchor 고려) + Y 방향으로 1칸 아래 오프셋
        Vector3 cellSize = targetGrid.cellSize;
        Vector3 tileAnchor = roomTilemap.tileAnchor;
        treasureChestWorldPos += new Vector3(cellSize.x * tileAnchor.x,
                                       cellSize.y * tileAnchor.y - cellSize.y, // 1칸 아래
                                       0f);

        // Interactive 오브젝트 찾기
        Transform interactiveParent = DungeonRoomHelper.FindInteractiveParent(room.roomObject.transform);
        if (interactiveParent == null)
        {
            //Debug.LogWarning($"[DungeonItemPlacer] 방({room.roomObject.name})에 Interactive 오브젝트를 찾을 수 없어 방의 직접 자식으로 배치합니다.");
            interactiveParent = room.roomObject.transform;
        }

        // Treasure Chest 프리팹 생성 (Interactive의 자식으로)
        Object.Instantiate(treasureChestPrefab, treasureChestWorldPos, Quaternion.identity, interactiveParent);
    }
}
