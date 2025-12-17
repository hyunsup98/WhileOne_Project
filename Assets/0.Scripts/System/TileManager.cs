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
        if(Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0;

            CheckDigSpot(mousePos);
        }
    }

    public void CheckDigSpot(Vector3 pos)
    {
        // 받아온 pos 좌표에 _digSpotLayer 레이어를 가진 콜라이더가 있는지 확인
        Collider2D col = Physics2D.OverlapPoint(pos, _digSpotLayer);

        Debug.Log(col.name);

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
                //이미 발굴이 완료된 타일이라면
                else if(_alreadyDigTile == GetTile(tilemap, Vector3Int.FloorToInt(pos)))
                {
                    //깡 소리를 냄
                    Debug.Log("이미 보물을 획득한 타일");
                }
                else
                {
                    Debug.Log("발굴이 불가능한 타일");
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
