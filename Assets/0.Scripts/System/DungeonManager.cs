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
    [field: SerializeField] public WeaponUI WeaponUI { get; private set; }              // 무기 획득 UI
    [field: SerializeField] public GameObject InteractImg { get; private set; }         // 상호작용 키 이미지
    #endregion

    #region 툴팁 관련 변수
    [field: SerializeField] public WeaponTooltip WeaponTooltip { get; private set; }        // 무기 툴팁
    [field: SerializeField] public TreasureTooltip TreasureTooltip { get; private set; }    // 보물 툴팁
    #endregion

    #region 슬롯 관련 변수
    [field: SerializeField] public EquipSlot MainWeaponSlot { get; private set; }       // 메인 무기 슬롯
    [field: SerializeField] public EquipSlot SubWeaponSlot { get; private set; }        // 서브 무기 슬롯
    #endregion

    private void Awake()
    {
        GameManager.Instance.CurrentDungeon = this;
    }

    private void Start()
    {
        _tileManager = new TileManager(this);

        MainWeaponSlot.ChangeIcon(DataManager.Instance.WeaponData.GetWeapon(4001));
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
