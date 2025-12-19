using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// 타일 조건 체크 관련 기능을 담당하는 클래스
/// </summary>
public class TileManager : Singleton<TileManager>
{
    [SerializeField] private Tile _digSpotTile;         // 발굴이 가능한 타일
    [SerializeField] private List<Treasure> treasures = new List<Treasure>();
    [SerializeField] private TreasureBar _treasureBar;

    public RoomController CurrentRoom { get; set; }     //현재 플레이어가 진입한 방

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousepos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousepos.z = 0;

            Dig(mousepos);
        }
    }

    /// <summary>
    /// 받아온 좌표에 발굴 가능한 타일이 있는지 체크하는 메서드
    /// </summary>
    /// <param name="pos"> 확인할 좌표 </param>
    /// <returns> 발굴이 가능한지에 대한 여부 </returns>
    public bool CanDig(Vector3Int pos)
    {
        if (CurrentRoom == null) return false;

        return CurrentRoom.DigSpotTileMap.GetTile(pos) == _digSpotTile;
    }

    /// <summary>
    /// 실제로 땅을 파는 메서드
    /// </summary>
    /// <param name="pos"> 확인할 좌표 </param>
    public void Dig(Vector3 pos)
    {
        Vector3Int cellPos = CurrentRoom.DigSpotTileMap.WorldToCell(pos);

        // 땅을 팔 수 있을 때
        if (CanDig(cellPos))
        {
            CurrentRoom.DigSpotTileMap.SetTile(cellPos, null);
            CurrentRoom.FloorTileMap.SetTile(cellPos, CurrentRoom.AfterDigTile);

            //보물 획득
            _treasureBar.AddTreasure(treasures[0]);
        }
        // 땅을 팔 수 없을 때
        else
        {
            if(CurrentRoom.FloorTileMap.GetTile(cellPos) == CurrentRoom.AfterDigTile)
            {
                // 이미 발굴이 완료된 타일일 때
                // todo: 흙 사운드 랜덤 재생
            }
            else
            {
                // 원래부터 땅을 팔 수 없는 타일일 때
                // todo: 깡! 소리가 나는 사운드 랜덤 재생
            }
        }
    }

    /// <summary>
    /// 특정 타일맵의 특정 좌표 위치에 있는 타일을 반환
    /// </summary>
    /// <param name="tilemap"> 검사할 타일맵 </param>
    /// <param name="pos"> 체크할 좌표 위치 </param>
    /// <returns> 받아온 타일 </returns>
    public TileBase GetTile(Tilemap tilemap, Vector3Int pos)
    {
        TileBase tile = tilemap.GetTile(pos);
        return tile;
    }

    /// <summary>
    /// 타일맵의 특정 좌표 위치에 타일을 설치
    /// </summary>
    /// <param name="tilemap"> 설치할 타일맵 </param>
    /// <param name="tile"> 설치할 타일 </param>
    /// <param name="pos"> 설치할 좌표 위치 </param>
    public void SetTile(Tilemap tilemap, TileBase tile, Vector3Int pos)
    {
        tilemap.SetTile(pos, tile);
    }
}
