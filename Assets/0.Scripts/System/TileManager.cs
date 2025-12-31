using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 타일 조건 체크 관련 기능을 담당하는 클래스
/// </summary>
public class TileManager
{
    private DungeonManager _dungeonManager;

    public TileManager(DungeonManager dungeonManager)
    {
        _dungeonManager = dungeonManager;
    }

    /// <summary>
    /// 받아온 좌표에 발굴 가능한 타일이 있는지 체크하는 메서드
    /// </summary>
    /// <param name="pos"> 확인할 좌표 </param>
    /// <returns> 발굴이 가능한지에 대한 여부 </returns>
    public bool CanDig(Vector3Int pos)
    {
        if (_dungeonManager.CurrentRoom == null) return false;

        return _dungeonManager.CurrentRoom.DigSpotTileMap.GetTile(pos) == _dungeonManager.DigSpotTile;
    }

    public bool CanDig(Vector3 pos)
    {
        if (_dungeonManager.CurrentRoom == null) return false;

        Vector3Int cellPos = _dungeonManager.CurrentRoom.DigSpotTileMap.WorldToCell(pos);
        return _dungeonManager.CurrentRoom.DigSpotTileMap.GetTile(cellPos) == _dungeonManager.DigSpotTile;
    }

    /// <summary>
    /// 실제로 땅을 파는 메서드
    /// </summary>
    /// <param name="pos"> 확인할 좌표 </param>
    public void Dig(Vector3 pos)
    {
        if (_dungeonManager.CurrentRoom == null) return;

        Vector3Int cellPos = _dungeonManager.CurrentRoom.DigSpotTileMap.WorldToCell(pos);

        Tile afterDigTile = _dungeonManager.AfterDigTile;

        // 땅을 팔 수 있을 때
        if (CanDig(cellPos))
        {
            // DigSpotTileMap에서 타일 제거
            _dungeonManager.CurrentRoom.DigSpotTileMap.SetTile(cellPos, null);
            SoundManager.Instance.PlaySoundEffect("Shovel_PossibleTile_FX_001");
            if (afterDigTile != null)
            {
                // DigSpotTileMap에 AfterDigTile 설정
                // FloorTileMap이 아닌 DigSpotTileMap에 해야 Ground 위에 보임 (Floor에 할시 검은 바탕까지 보여서 이상해짐)
                _dungeonManager.CurrentRoom.DigSpotTileMap.SetTile(cellPos, afterDigTile);
            }
            else
            {
                Debug.LogWarning("[TileManager] DungeonGenerator의 AfterDigTile을 찾을 수 없습니다.");
            }

            // DiggingRoom인지 확인
            DiggingRoom diggingRoom = _dungeonManager.CurrentRoom.GetComponent<DiggingRoom>();
            if (diggingRoom != null)
            {
                diggingRoom.OnDigSpotDug(cellPos);
                return;
            }

            Treasure treasure = DataManager.Instance.TreasureData.PickTreasure();
            _dungeonManager.PlayDigSound("Shovel_PossibleTile_FX_001");
            _dungeonManager.TreasureBarUI.AddTreasure(treasure);
            DataManager.Instance.CharacterData.AddTreasureData(treasure);
        }
        // 땅을 팔 수 없을 때
        else
        {
            if(_dungeonManager.CurrentRoom.FloorTileMap.GetTile(cellPos) == afterDigTile)
            {
                _dungeonManager.PlayDigSound("Shovel_PossibleTile_FX_001");
            }
            else
            {
                _dungeonManager.PlayDigSound("Shovel_ImpossibleTile_FX_001", "Shovel_ImpossibleTile_FX_002", "Shovel_ImpossibleTile_FX_003");
            }
        }
    }
}
