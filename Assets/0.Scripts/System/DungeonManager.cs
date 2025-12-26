using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DungeonManager : MonoBehaviour
{
    #region 발굴 관련 변수
    [field: SerializeField] public Tile DigSpotTile { get; private set; }       // 발굴이 가능한 타일
    [field: SerializeField] public TreasureBar TreasureBarUI { get; private set; }

    private RoomController _currentRoom;                                        // 현재 플레이어가 존재하는 방
    public RoomController CurrentRoom
    {
        get { return _currentRoom; }
        set
        {
            _currentRoom = value;
        }
    }

    public TileManager _tileManager { get; private set; }

    #endregion

    #region 상자, 무기 관련 변수
    [field: SerializeField] public WeaponUI WeaponUI { get; private set; }
    [field: SerializeField] public GameObject InteractImg { get; private set; }
    #endregion

    private void Awake()
    {
        GameManager.Instance.CurrentDungeon = this;
    }

    private void Start()
    {
        _tileManager = new TileManager(this);
    }

    private void Update()
    {
        if(Keyboard.current.gKey.wasPressedThisFrame)
        {
            TreasureBarUI.AddTreasure(DataManager.Instance.TreasureData.PickTreasure());
        }
    }

    public void SetPosInteractImg(Vector3 pos)
    {
        InteractImg.SetActive(true);
        InteractImg.transform.position = pos;
    }

    private void OnDisable()
    {
        GameManager.Instance.CurrentDungeon = null;
    }
}
