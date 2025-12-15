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

    //기획자분들 수치 조정하기 편하게 직렬화하기
    [Header("플레이어관련 수치 조정")]


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
            Debug.Log(ctx.ReadValue<Vector2>());
    }
    private void OnStopped(InputAction.CallbackContext ctx)
    {
        _move = Vector3.zero;
        Debug.Log("멈춤호출");
    }
    void FixedUpdate()
    {
        //이동시 방향 전환 버그있음
        
        //if (dir.x < 0)
        //{
        //    transform.localEulerAngles = new Vector3(0, 0, 0);
        //    Debug.Log($"{dir} 방향");
        //}
        //if (dir.x > 0)
        //{
        //    transform.localEulerAngles = new Vector3(0, 180, 0);
        //    Debug.Log($"{dir} 방향");
        //}
        transform.Translate(_move * Time.deltaTime * _player.MoveSpeed);
    }

}
