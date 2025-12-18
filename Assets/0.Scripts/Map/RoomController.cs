using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 각 방마다 붙는 RoomController.
/// - 타일매니저에서 사용할 방 전용 타일 설정을 들고 있습니다.
/// - 플레이어가 방 안/밖으로 드나드는 시점을 OnTriggerEnter2D / OnTriggerExit2D 로 제공합니다.
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Room Tiles")]
    [SerializeField] [Tooltip("이 방의 기본 바닥 타일맵")]
    private Tilemap floorTileMap;

    [SerializeField] [Tooltip("Dig Spot(도굴 지점)에 사용할 타일맵")]
    private Tilemap digSpotTileMap;

    [SerializeField] [Tooltip("Dig 후 파헤쳐진 자리(Dug Spot)에 사용할 타일맵")]
    private Tilemap dugSpotTileMap;

    /// <summary>
    /// 타일매니저에서 읽기 전용으로 사용할 수 있도록 프로퍼티를 제공합니다.
    /// </summary>
    public Tilemap FloorTileMap => floorTileMap;
    public Tilemap DigSpotTileMap => digSpotTileMap;
    public Tilemap DugSpotTileMap => dugSpotTileMap;

    /// <summary>
    /// 플레이어가 이 방의 트리거 영역에 들어왔을 때 호출됩니다.
    /// (RoomController가 붙어 있는 오브젝트에 2D Collider + isTrigger=true 필요)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어만 처리
        // 플레이어인지 구분하는 걸 플레이어컨트롤러로 확인합니다. 혹시 더 좋은 방안이 있다면 공유 부탁드립니다...
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // 플레이어가 현재 방 정보를 알 수 있도록 전달
        player.SetCurrentRoom(this);

        // TODO: 나중에 TileManager에서 사용할 추가 로직이 있으면 여기에
        // 예) TileManager.Instance.OnEnterRoom(this, player);
    }

    /// <summary>
    /// 플레이어가 이 방의 트리거 영역에서 나갔을 때 호출됩니다.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // 방에서 나갈 때 현재 방 정보 해제
        player.ClearCurrentRoom(this);

        // TODO: 나중에 TileManager에서 사용할 추가 로직이 있으면 여기에
        // 예) TileManager.Instance.OnExitRoom(this, player);
    }

    /// <summary>
    /// 주어진 월드 좌표에 DigSpot 타일이 있으면 DugSpot으로 변경합니다.
    /// (마우스 우클릭 시 TileManager에서 호출하기 좋은 형태)
    /// </summary>
    public bool TryDigAtWorldPosition(Vector3 worldPosition)
    {
        if (digSpotTileMap == null || dugSpotTileMap == null)
        {
            Debug.LogWarning($"[RoomController] DigSpot/DugSpot Tilemap 이 설정되지 않았습니다. room:{name}");
            return false;
        }

        Vector3Int cell = digSpotTileMap.WorldToCell(worldPosition);
        return TryDigAtCell(cell);
    }

    /// <summary>
    /// 주어진 셀 좌표에 DigSpot 타일이 있으면 DugSpot으로 변경합니다.
    /// </summary>
    public bool TryDigAtCell(Vector3Int cellPosition)
    {
        if (digSpotTileMap == null || dugSpotTileMap == null)
        {
            Debug.LogWarning($"[RoomController] DigSpot/DugSpot Tilemap 이 설정되지 않았습니다. room:{name}");
            return false;
        }

        TileBase current = digSpotTileMap.GetTile(cellPosition);
        Debug.Log($"[RoomController] TryDigAtCell - cell:{cellPosition}, currentTile:{current?.name}");
        if (current == null)
        {
            // DigSpot 타일이 아님
            return false;
        }
        
        // DigSpot 타일 제거
        digSpotTileMap.SetTile(cellPosition, null);

        // DugSpot 타일 선택 (지정되어 있지 않으면 기존 DigSpot 타일을 재사용)
        //TileBase targetDugTile = current;
        //dugSpotTileMap.SetTile(cellPosition, targetDugTile);

        return true;
    }
}


