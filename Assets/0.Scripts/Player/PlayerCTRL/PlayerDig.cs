using Unity.VisualScripting;
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
    
    Vector3 _mousePosition;
    Vector3 dir;

    public GameObject Test => test;
    public float OffSet => offSet;
    public Vector3 Dir => dir;

    public bool IsDigging => _isDigging;

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
            test.SetActive(false);

            _isDigging = false;
        };
        _attack = _player.PlayerAttack;
    }
    void Dig(InputAction.CallbackContext ctx)
    {
        if (!_attack.IsAttacking)
        {
            test.SetActive(true);
             _isDigging = true;

        }
    }

    private void Update()
    {
        //마우스 좌표값을 월드 좌표값으로 변환
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //플레이어와 마우스 사이 좌표 거리 계산
        Vector3 angle = _mousePosition - transform.position;

       
        Vector3 a = Vector3Int.FloorToInt(transform.position);

        if (angle.x > 1)
        {
            dir.x = a.x+1f;
        }
        else if(angle.x < -1)
        {
            dir.x = a.x - 1f;
        }
        else
        {
            dir.x = a.x;
        }
        if (angle.y > 1)
        {
            dir.y = a.y+1f;
        }
        else if(angle.y < -1)
        {
            dir.y = a.y - 1f;
        }
        else
        {
            dir.y = a.y;
        }
    }
}
