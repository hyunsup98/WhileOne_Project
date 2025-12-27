using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerDig : MonoBehaviour
{
    Player _player;
    PlayerAttack _attack;
    PlayerInput _input;
    private InputActionMap _actionMap;
    private InputAction _digAction;
    public bool _isDigging;

    [SerializeField] GameObject test;
    [SerializeField] float offSet;

    [SerializeField] private Transform pivotTrans;

    Vector3 _mousePosition;
    Vector3 dir;
    Vector3 cursorPos = Vector3.zero;  //타일 커서 벡터값

    public GameObject Test => test;
    public float OffSet => offSet;
    public Vector3 Dir => dir;

    public bool IsDigging => _isDigging;

    // 타일 커서 관련 변수들
    [SerializeField] private SpriteRenderer tileCursor;
    [SerializeField] private Color blue;
    [SerializeField] private Color red;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _input = GetComponent<PlayerInput>();

    }
    void Start()
    {
        _actionMap = _input.actions.FindActionMap("Player");
        _digAction = _actionMap.FindAction("Special");

        _digAction.performed += Dig;
        _digAction.canceled += ctx =>
        {
            tileCursor.gameObject.SetActive(false);
            test.SetActive(false);
            _isDigging = false;
            GameManager.Instance.CurrentDungeon._tileManager.Dig(cursorPos);
        };
        _attack = _player.PlayerAttack;
    }
    void Dig(InputAction.CallbackContext ctx)
    {

        if (!_attack.IsAttacking )
        {
            tileCursor.gameObject.SetActive(true);
            test.SetActive(true);
            _isDigging = true;

        }
    }

    private void Update()
    {
        //마우스 좌표값을 월드 좌표값으로 변환
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //플레이어와 마우스 사이 좌표 거리 계산
        Vector3 angle = _mousePosition - pivotTrans.position;

       
        cursorPos = Vector3Int.FloorToInt(pivotTrans.position);

        if (angle.x > 1)
        {
            cursorPos.x += 1f;
            dir.x = cursorPos.x;
        }
        else if(angle.x < -1)
        {
            cursorPos.x -= 1f;
            dir.x = cursorPos.x;
        }
        else
        {
            dir.x = cursorPos.x;
        }
        if (angle.y > 1)
        {
            cursorPos.y += 1f;
            dir.y = cursorPos.y;
        }
        else if(angle.y < -1)
        {
            cursorPos.y -= 1f;
            dir.y = cursorPos.y;
        }
        else
        {
            dir.y = cursorPos.y;
        }

        cursorPos = new Vector3(cursorPos.x + 0.5f, cursorPos.y + 0.5f, 0f);
        CheckTileCursor();
    }
    public void CheckTileCursor()
    {
        if (GameManager.Instance.CurrentDungeon == null || !_isDigging) return;

        tileCursor.transform.position = cursorPos;
        tileCursor.color = GameManager.Instance.CurrentDungeon._tileManager.CanDig(cursorPos)
            ? blue : red;
    }
   


}
