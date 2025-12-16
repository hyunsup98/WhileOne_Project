using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Player _player;

    //플레이어 인풋 관련
    private PlayerInput _input;
    private InputActionMap _actionMap;
    private InputAction _attackAtion;

    //게임오브젝트 이펙트 관련
    GameObject _effect1;
    GameObject _effect2;
    private void Awake()
    {
        _player = GetComponent<Player>();

        _input = transform.root.GetComponent<PlayerInput>();  //최상위 부모의 플레이어 인풋 컴포넌트

        _effect1 = transform.GetChild(0).gameObject; //자식 이펙트

        _effect2 = transform.GetChild(1).gameObject; //자식 이펙트
    }

    void Start()
    {

        _actionMap = _input.actions.FindActionMap("Player");

        _attackAtion = _actionMap.FindAction("Attack");

        _attackAtion.performed += OnAttack;

        _attackAtion.canceled += ctx =>
        {
            Debug.Log("Attack end");
        };
    }
    void OnAttack(InputAction.CallbackContext context)
    {
        bool isEffect1 = true;

        if (isEffect1)
        {
            _effect1.SetActive(true);
            isEffect1 = false;
            Debug.Log("1");
        }
        else
        {
            _effect1.SetActive(true);
            isEffect1 = true;
            Debug.Log("2");
        }

    }
}
