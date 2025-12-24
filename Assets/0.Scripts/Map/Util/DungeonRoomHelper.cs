using UnityEngine;

/// <summary>
/// 방 관련 유틸리티 메서드들
/// </summary>
public static class DungeonRoomHelper
{
    /// <summary>
    /// 방 오브젝트 하위에서 RoomCenterMarker 태그를 가진 Transform을 찾습니다.
    /// </summary>
    public static Transform FindRoomCenterMarker(GameObject roomObj)
    {
        if (roomObj == null) return null;

        Transform[] children = roomObj.GetComponentsInChildren<Transform>(true);
        foreach (var t in children)
        {
            if (t.CompareTag("RoomCenterMarker"))
            {
                return t;
            }
        }

        return null;
    }
    
    /// <summary>
    /// 방의 "논리적 중심" 월드 좌표를 반환합니다.
    /// 1순위: RoomCenterMarker 태그가 붙은 자식 Transform 위치
    /// 2순위: roomObj.transform.position
    /// </summary>
    public static Vector3 GetRoomWorldCenter(GameObject roomObj)
    {
        if (roomObj == null)
            return Vector3.zero;

        // RoomCenterMarker를 우선 사용
        Transform marker = FindRoomCenterMarker(roomObj);
        if (marker != null)
            return marker.position;

        // 없으면 프리팹 기준 위치 사용
        return roomObj.transform.position;
    }
    
    /// <summary>
    /// 방의 특정 방향 문의 월드 위치를 반환합니다.
    /// DoorCenterMarker 태그를 우선 사용하고, 없으면 문 Transform의 위치를 사용합니다.
    /// 문을 찾을 수 없으면 Vector3.zero를 반환합니다.
    /// </summary>
    public static Vector3 GetDoorWorldPosition(GameObject roomObj, Vector2Int direction)
    {
        if (roomObj == null)
            return Vector3.zero;
        
        // DoorCenterMarker 태그를 사용하여 문 위치 찾기 (우선)
        Transform doorCenterMarker = FindDoorCenterMarker(roomObj, direction);
        if (doorCenterMarker != null)
        {
            return doorCenterMarker.position;
        }
        
        // DoorCenterMarker가 없으면 기존 방식으로 문 Transform 찾기
        Transform doorTransform = FindDoorTransform(roomObj, direction);
        return doorTransform != null ? doorTransform.position : Vector3.zero;
    }
    
    /// <summary>
    /// 방 오브젝트에서 특정 방향의 DoorCenterMarker를 찾습니다.
    /// </summary>
    public static Transform FindDoorCenterMarker(GameObject roomObj, Vector2Int direction)
    {
        if (roomObj == null) return null;
        
        // 방향 이름 매핑
        string directionName = "";
        if (direction == Direction.Up) directionName = "North";
        else if (direction == Direction.Down) directionName = "South";
        else if (direction == Direction.Left) directionName = "West";
        else if (direction == Direction.Right) directionName = "East";
        else return null;
        
        // DoorCenterMarker 태그를 가진 모든 오브젝트 찾기
        Transform[] allChildren = roomObj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            // DoorCenterMarker 태그 확인
            if (child.CompareTag("DoorCenterMarker"))
            {
                // 부모가 해당 방향의 Door인지 확인
                Transform parent = child.parent;
                if (parent != null)
                {
                    string parentName = parent.name;
                    if (parentName.Contains(directionName) || parentName.EndsWith("_" + directionName))
                    {
                        return child;
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 방 오브젝트에서 특정 방향의 문 Transform을 찾습니다.
    /// </summary>
    public static Transform FindDoorTransform(GameObject roomObj, Vector2Int direction)
    {
        if (roomObj == null) return null;
        
        // 방향 이름 매핑
        string directionName = "";
        if (direction == Direction.Up) directionName = "North";
        else if (direction == Direction.Down) directionName = "South";
        else if (direction == Direction.Left) directionName = "West";
        else if (direction == Direction.Right) directionName = "East";
        else return null;
        
        // Door 컨테이너 찾기
        Transform doorContainer = null;
        foreach (Transform child in roomObj.transform)
        {
            if (child.name.Contains("Door") && !child.name.Contains("_"))
            {
                doorContainer = child;
                break;
            }
        }
        
        if (doorContainer == null) return null;
        
        // Door 컨테이너의 자식에서 방향별 Door 찾기
        foreach (Transform child in doorContainer)
        {
            string childName = child.name;
            if (childName.Contains(directionName) || childName.EndsWith("_" + directionName))
            {
                return child;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 방 타입에 맞는 프리펩을 반환합니다.
    /// </summary>
    public static GameObject GetRoomPrefab(RoomType type, GameObject normalRoomPrefab, GameObject startRoomPrefab, 
        GameObject exitRoomPrefab, GameObject eventRoomPrefab, GameObject trapRoomPrefab, 
        GameObject treasureRoomPrefab, GameObject bossRoomPrefab)
    {
        switch (type)
        {
            case RoomType.Start:
                return startRoomPrefab != null ? startRoomPrefab : normalRoomPrefab;
            case RoomType.Exit:
                return exitRoomPrefab != null ? exitRoomPrefab : normalRoomPrefab;
            case RoomType.Event:
                return eventRoomPrefab != null ? eventRoomPrefab : normalRoomPrefab;
            case RoomType.Trap:
                return trapRoomPrefab != null ? trapRoomPrefab : normalRoomPrefab;
            case RoomType.Treasure:
                return treasureRoomPrefab != null ? treasureRoomPrefab : normalRoomPrefab;
            case RoomType.Boss:
                return bossRoomPrefab != null ? bossRoomPrefab : normalRoomPrefab;
            default:
                return normalRoomPrefab;
        }
    }
    
    /// <summary>
    /// 타일맵에서 실제로 타일이 채워져 있는 영역의 중앙 셀을 반환합니다.
    /// </summary>
    public static Vector3Int GetRoomCenterCell(UnityEngine.Tilemaps.Tilemap tilemap)
    {
        if (tilemap == null)
            return Vector3Int.zero;

        var bounds = tilemap.cellBounds;
        bool hasTile = false;

        Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, 0);
        Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, 0);

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(cell);
            if (tile == null) continue;

            hasTile = true;

            if (cell.x < min.x) min.x = cell.x;
            if (cell.y < min.y) min.y = cell.y;
            if (cell.x > max.x) max.x = cell.x;
            if (cell.y > max.y) max.y = cell.y;
        }

        if (!hasTile)
        {
            // 타일이 하나도 없으면 bounds의 중앙을 사용 (예외 상황)
            return new Vector3Int(
                bounds.xMin + bounds.size.x / 2,
                bounds.yMin + bounds.size.y / 2,
                0
            );
        }

        Vector3Int size = max - min;
        // 실제 타일 영역의 중앙 셀 (짝수 크기인 경우 왼쪽/아래쪽에 더 가깝게 정수 나눗셈)
        return new Vector3Int(
            min.x + size.x / 2,
            min.y + size.y / 2,
            0
        );
    }
    
    /// <summary>
    /// 주어진 셀에서 가장 가까운 타일이 있는 셀을 찾습니다.
    /// </summary>
    public static Vector3Int FindNearestTileCell(UnityEngine.Tilemaps.Tilemap tilemap, Vector3Int centerCell, Grid grid)
    {
        if (tilemap == null || grid == null) return Vector3Int.zero;
        
        // BoundsInt로 타일맵의 범위 가져오기
        var bounds = tilemap.cellBounds;
        
        // 중심에서 시작하여 점점 멀어지는 원형으로 검색
        int maxRadius = Mathf.Max(bounds.size.x, bounds.size.y);
        
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            // 반경 내의 모든 셀 확인
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // 원형 범위 내인지 확인
                    if (x * x + y * y > radius * radius) continue;
                    
                    Vector3Int checkCell = centerCell + new Vector3Int(x, y, 0);
                    
                    // 타일맵 범위 내인지 확인
                    if (!bounds.Contains(checkCell)) continue;
                    
                    // 타일이 있는지 확인
                    var tile = tilemap.GetTile(checkCell);
                    if (tile != null)
                    {
                        return checkCell;
                    }
                }
            }
        }
        
        return Vector3Int.zero;
    }
    
    /// <summary>
    /// 방 오브젝트에서 Interactive 자식 오브젝트를 찾습니다.
    /// </summary>
    public static Transform FindInteractiveParent(Transform roomTransform)
    {
        if (roomTransform == null) return null;
        
        // 직접 자식에서 찾기
        foreach (Transform child in roomTransform)
        {
            if (child.name.Contains("Interactive") || child.name.Contains("interactive"))
            {
                return child;
            }
        }
        
        // 재귀적으로 찾기
        return FindInteractiveParentRecursive(roomTransform);
    }
    
    /// <summary>
    /// 재귀적으로 Interactive 오브젝트를 찾습니다.
    /// </summary>
    private static Transform FindInteractiveParentRecursive(Transform parent)
    {
        if (parent == null) return null;
        
        foreach (Transform child in parent)
        {
            if (child.name.Contains("Interactive") || child.name.Contains("interactive"))
            {
                return child;
            }
            
            Transform found = FindInteractiveParentRecursive(child);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
}
