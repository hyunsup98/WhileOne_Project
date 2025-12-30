using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

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

    /// <summary>
    /// 타일매니저에서 읽기 전용으로 사용할 수 있도록 프로퍼티를 제공합니다.
    /// </summary>
    public Tilemap FloorTileMap => floorTileMap;
    public Tilemap DigSpotTileMap => digSpotTileMap;
    
    private void Awake()
    {
        // Box2DCollider 자동 추가
        SetupTriggerCollider();
    }
    
    /// <summary>
    /// 방의 크기에 맞게 Box2DCollider를 자동으로 추가하고 설정합니다.
    /// </summary>
    private void SetupTriggerCollider()
    {
        // 이미 Collider2D가 있으면 추가하지 않음
        Collider2D existingCollider = GetComponent<Collider2D>();
        if (existingCollider != null)
        {
            // 이미 있으면 isTrigger만 확인
            if (!existingCollider.isTrigger)
            {
                existingCollider.isTrigger = true;
            }
            return;
        }
        
        // BaseRoom에서 방 크기 가져오기
        BaseRoom baseRoom = GetComponent<BaseRoom>();
        float roomWidth = 15f; // 기본값
        float roomHeight = 15f; // 기본값
        
        if (baseRoom != null)
        {
            roomWidth = baseRoom.RoomWidth;
            roomHeight = baseRoom.RoomHeight;
        }
        
        // BoxCollider2D 추가
        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(roomWidth, roomHeight);
        boxCollider.isTrigger = true;
        // 위치는 RoomCenterMarker에 맞춤
        Transform centerMarker = transform.Find("RoomCenterMarker");
        if (centerMarker != null)
        {
            boxCollider.offset = centerMarker.localPosition;
        }

        //Debug.Log($"[RoomController] Box2DCollider 자동 추가 완료 - 크기: {roomWidth}x{roomHeight}, isTrigger: true");
    }

    /// <summary>
    /// 플레이어가 이 방의 트리거 영역에 들어왔을 때 호출됩니다.
    /// (RoomController가 붙어 있는 오브젝트에 2D Collider + isTrigger=true 필요)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어만 처리
        if(other.CompareTag("Player"))
        {
            GameManager.Instance.CurrentDungeon.CurrentRoom = this;
        }
    }

    /// <summary>
    /// 플레이어가 이 방의 트리거 영역에서 나갔을 때 호출됩니다.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.Instance.CurrentDungeon.CurrentRoom = null;
        }
    }

}


