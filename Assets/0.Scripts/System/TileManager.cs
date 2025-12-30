using UnityEngine;

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

        // 땅을 팔 수 있을 때
        if (CanDig(cellPos))
        {
            _dungeonManager.CurrentRoom.DigSpotTileMap.SetTile(cellPos, null);
            _dungeonManager.CurrentRoom.FloorTileMap.SetTile(cellPos, _dungeonManager.CurrentRoom.AfterDigTile);

            _dungeonManager.TreasureBarUI.AddTreasure(DataManager.Instance.TreasureData.PickTreasure());
        }
        // 땅을 팔 수 없을 때
        else
        {
            if(_dungeonManager.CurrentRoom.FloorTileMap.GetTile(cellPos) == _dungeonManager.CurrentRoom.AfterDigTile)
            {
                SoundManager.Instance.PlaySoundEffect("Shovel_PossibleTile_FX_001");
            }
            else
            {
                string[] impossible = { "Shovel_ImpossibleTile_FX_001", "Shovel_ImpossibleTile_FX_002", "Shovel_ImpossibleTile_FX_003" };
                SoundManager.Instance.PlaySoundEffect(impossible);
            }
        }
    }
}
