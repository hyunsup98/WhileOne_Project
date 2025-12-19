using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DungeonManager : MonoBehaviour
{
    [field: SerializeField] public Tile DigSpotTile { get; private set; }       // 발굴이 가능한 타일
    [field: SerializeField] public TreasureBar TreasureBarUI { get; private set; }

    private RoomController _currentRoom;                                        // 현재 플레이어가 존재하는 방
    public RoomController CurrentRoom
    {
        get { return _currentRoom; }
        set
        {
            _currentRoom = value;
            Test = value;
        }
    }

    public TileManager _tileManager { get; private set; }

    public RoomController Test;

    private void Awake()
    {
        GameManager.Instance.CurrentDungeon = this;
    }

    private void Start()
    {
        _tileManager = new TileManager(this);
    }

    private void OnDisable()
    {
        GameManager.Instance.CurrentDungeon = null;
    }
}
