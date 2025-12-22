using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    //플레이어 자체를 받아오기
    Player _player;

    //인풋 액션 관련
    PlayerInput _playerInput;
    InputActionMap _actionMap;
    InputAction _moveAction;
    MoveStopAnimation _stop;


    //리짓바디, 방향 관련"
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
        _stop = transform.GetChild(0).GetComponent<MoveStopAnimation>();


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
        Move = new Vector3(_dir.x, _dir.y, 0).normalized;
    }
    private void OnStopped(InputAction.CallbackContext ctx)
    {
        Move = Vector3.zero;
    }
    private void Update()
    {
        if (_stop.Action)
        {
            _dir = Vector2.zero;
            return;
        }

        _player.transform.Translate(Move * Time.deltaTime * _player.MoveSpeed);

    }
}
