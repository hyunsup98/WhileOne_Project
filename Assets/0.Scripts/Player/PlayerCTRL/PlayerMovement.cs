using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    //플레이어 자체를 받아오기
    Player _player;

    //인풋 액션 관련
    PlayerInput _playerInput;
    InputActionMap _actionMap;
    InputAction _moveAction;

    //리짓바디, 방향 관련"
    Rigidbody _playerRigid;
    Vector2 _dir;
    Vector3 _move;

    //마우스 좌표를 기억하는 변수
    Vector3 _mousePosition;

    //외부에서 접근 가능하게 만들 프로퍼티
    public Vector3 Move => _move;



    //Awake에 Player가 가지고 있는 컴포넌트 불러오기
    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();
        //playerRigid = GetComponent<Rigidbody>();

    }
    void Start()
    {
        _actionMap = _playerInput.actions.FindActionMap("Player");

        _moveAction = _actionMap.FindAction("Move");

        _moveAction.performed += OnMove; //입력키 눌렀을 때 이동

        _moveAction.canceled += OnStopped;
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
            _dir = ctx.ReadValue<Vector2>();
            _move = new Vector3(_dir.x, _dir.y, 0).normalized;
        Debug.Log(_move);
    }
    private void OnStopped(InputAction.CallbackContext ctx)
    {
        _move = Vector3.zero;
    }
    void FixedUpdate()
    {
        //마우스 좌표값을 월드 좌표값으로 변환
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //플레이어와 마우스 사이 좌표 거리 계산
        Vector3 angle = _mousePosition - transform.position;
        //좌표 거리값을 바탕으로 각도(aTan) 계산 
        float angleZ = Mathf.Atan2(angle.y, angle.x) * Mathf.Rad2Deg; //라디안을 도(°)로 단위 변환

        if (angleZ > 90 || angleZ<-90)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        //캐릭터를 기준으로 마우스가 오른쪽으로 넘어가면 캐릭터 방향 변경
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

            transform.Translate(_move * Time.deltaTime * _player.MoveSpeed);
    }

}
