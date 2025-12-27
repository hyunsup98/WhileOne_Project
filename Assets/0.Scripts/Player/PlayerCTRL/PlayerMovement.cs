using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //플레이어 자체를 받아오기
    Player _player;

    //인풋 액션 관련
    PlayerInput _playerInput;
    InputActionMap _actionMap;
    InputAction _moveAction;
    PlayerDash _dash;
    PlayerDamage _damage;


    //리짓바디, 방향 관련"
    Rigidbody2D _rb;
    Vector2 _dir;
    Vector3 _move;

    //마우스 좌표를 기억하는 변수
    Vector3 _mousePosition;

    //외부에서 접근 가능하게 만들 프로퍼티
    public Vector3 Move
    {
        get; set;
    }
    //Awake에 Player가 가지고 있는 컴포넌트 불러오기
    private void Awake()
    {
        _player = GetComponent<Player>();
       
        _rb = GetComponent<Rigidbody2D>();

        _dash = GetComponent<PlayerDash>();

        _damage = GetComponentInChildren<PlayerDamage>();

        //playerRigid = GetComponent<Rigidbody>();

    }
    private void Start()
    {

        _playerInput = _player.Playerinput;

        _actionMap = _player.ActionMap;

        _moveAction = _actionMap.FindAction("Move");

        _moveAction.performed += OnMove; //입력키 눌렀을 때 이동

        _moveAction.canceled += OnStopped;
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        _dir = ctx.ReadValue<Vector2>();
        Move = (Vector3)_dir.normalized;
    }
    private void OnStopped(InputAction.CallbackContext ctx)
    {
        Move = Vector3.zero;
    }
    private void FixedUpdate()
    {
        if (_damage.IsKnock)
        {
            Debug.Log("넉백 중 이동 차단");
            return;
        }
        if (_dash.IsDash)
        {
            return;
        }
        if (_player.Stop.Action)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }
        //_player.transform.Translate(_player.MoveSpeed * Time.deltaTime * Move);
        _rb.linearVelocity = new Vector2(Move.x * _player.MoveSpeed, Move.y * _player.MoveSpeed);

    }
}
