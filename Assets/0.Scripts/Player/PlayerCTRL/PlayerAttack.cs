using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Player _player;

    //플레이어 인풋 관련
    private PlayerInput _input;
    private InputActionMap _actionMap;
    private InputAction _attackAtion;


    //플레이어 공격관련 (프로퍼티로 받아 올것들)
    private int _attack;
    private int _attackSpeed;

    //

    private void Awake()
    {
        _player = GetComponent<Player>();
        _input = GetComponent<PlayerInput>();
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
        Debug.Log("Attack");

    }
    void Update()
    {

    }
}
