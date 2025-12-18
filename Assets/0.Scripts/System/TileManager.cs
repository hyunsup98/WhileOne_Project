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
    [SerializeField] private Tile _alreadyDigTile;      // 이미 발굴이 완료된 타일
    [SerializeField] private LayerMask _digSpotLayer;   // 발굴 가능 타일맵이 가질 레이어

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

            CheckDigSpot(mousepos);
        }
    }

    /// <summary>
    /// 발굴 가능 지역을 생성하는 메서드
    /// 실질적인 발굴 가능 지역인지는 digSpotDic 딕셔너리에 추가하여 관리
    /// 타일맵을 통한 타일 그리기는 온전히 시각적인 렌더링 효과만 있음
    /// </summary>
    public void SetDigSpot()
    {

    }

    public void CheckDigSpot(Vector3 pos)
    {
        // 받아온 pos 좌표에 _digSpotLayer 레이어를 가진 콜라이더가 있는지 확인
        Collider2D col = Physics2D.OverlapPoint(pos, _digSpotLayer);

        // 콜라이더가 있다면
        if (col != null)
        {
            Tilemap tilemap = null;

            // 타일맵 컴포넌트가 있다면
            if (col.TryGetComponent(out tilemap))
            {
                // 발굴이 가능한 타일이라면
                if (_digSpotTile == GetTile(tilemap, Vector3Int.FloorToInt(pos)))
                {
                    // 보물 획득 로직
                    // DigSpot 타일을 이미 발굴한 상태의 타일로 바꾸기
                    Debug.Log("보물 획득이 가능한 타일");
                }
                // 발굴 가능한 타일이 없다면 바닥 타일맵에서 일반 바닥, 이미 발굴된 바닥을 체크
                else
                {

                }
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
