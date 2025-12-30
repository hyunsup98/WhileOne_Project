using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("플레이어 수치 조절 컴포넌트")]
    [SerializeField] private float _maxHp = 100;
    [SerializeField] private float _maxStamina = 100;
    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private int _attack = 5;
    [SerializeField] private int _attackSpeed = 4;

    private float _hp;
    private float _stamina;
    

    PlayerInput _input;
    InputActionMap _actionMap;

    Vector3 _mousePosition;

    //현재 상태를 담을 인터페이스 변수
    private IState moveCurrentState;
    private IState actionCurrentState;

    //이벤트 관련 변수
    public event Action<float,float> OnHpChanged;
    public event Action<float,float> OnStaminaChanged;

    //코루틴 변수
    private WaitForSeconds _delay;
    private float _finsihTime  = 0.7f;
    private float _checkTime = 0;
    private float _restoreStamina = 5;
    Coroutine regen;

    [field: SerializeField] public PlayerDamage GetDamage { get; private set; }

    //상태에서 사용할 스크립트
    private PlayerAttack _attackAction;
    PlayerMovement _playerMove;
    PlayerDig _playerDig;
    PlayerDash _dash;
    MoveStopAnimation _stop;
    Animator _animator;

    //외부에서 사용할 프로퍼티
    public float Hp { get { return _hp; } set { _hp = value; } }
    public float MaxHp => _maxHp;
    public float Stamina { get { return _stamina; } set { _stamina = value; } }
    public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public int Attack => _attack;
    public int AttackSpeed => _attackSpeed;
    
    public PlayerInput Playerinput
    {
        get { return _input; }
        set { _input = value; }
    }
    public InputActionMap ActionMap
    {
        get { return _actionMap; }
        set { _actionMap = value; }
    }
    public PlayerMovement PlayerMove
    {
        get { return _playerMove; } 
        set { _playerMove = value; }
    }
    public PlayerAttack PlayerAttack
    {
        get { return _attackAction; }
        set { _attackAction = value; }
    }
    public PlayerDig PlayerDig
    {
        get { return _playerDig; }
        set { _playerDig = value; }
    }
    public PlayerDash PlayerDash
    {
        get { return _dash; }
        set { _dash = value; }
    }
    public MoveStopAnimation Stop
    {
        get { return _stop; }
        set { _stop = value; }
    }
    public Animator Animator
    {
        get { return _animator; }
        set { _animator = value; }
    }

    [field: SerializeField] public WeaponChange Player_WeaponChange { get; private set; }

    private void Awake()
    {
        GameManager.Instance.player = this;

        Component();
        _actionMap = _input.actions.FindActionMap("Player");

        ChangedStamina = _maxStamina;

        float savedHp = DataManager.Instance.CharacterData._playerHp;
        _hp = (savedHp > 0) ? savedHp : _maxHp;

    }
    void Component()
    {
        _playerMove = GetComponent<PlayerMovement>();
        _playerDig = GetComponent<PlayerDig>();
        _input = GetComponent<PlayerInput>();
        _dash = GetComponent<PlayerDash>();
        _attackAction = GetComponent<PlayerAttack>();
        _animator = GetComponentInChildren<Animator>();
        _stop = GetComponentInChildren<MoveStopAnimation>();

    }
    void Start()
    {
        OnStaminaChanged?.Invoke(_maxStamina, ChangedStamina);
        
        MoveState(new IdleState(this));
        ActionState(new ActionIdleState(this));

        _delay = new WaitForSeconds(5f);

        _hp = DataManager.Instance.CharacterData._playerHp;
        OnHpChanged?.Invoke(_maxHp, _hp);
    }
    private void OnEnable()
    {
        regen = StartCoroutine(RestoreStamina());
    }

    public float ChangedHealth
    {
        get { return _hp; }
        set
        {
            _hp = Mathf.Clamp(value, 0, _maxHp);
            if(_hp >= 0f)
            {
                OnHpChanged?.Invoke(_maxHp,ChangedHealth);
                DataManager.Instance.CharacterData._playerHp = _hp;

                if(_hp <= Mathf.Epsilon)
                {
                    GameManager.Instance._CurrentGameState = GameState.Dead;
                }
            }
        }
    }
    public float ChangedStamina
    {
        get { return _stamina; }
        set
        {
            _stamina = Mathf.Clamp(value, 0, _maxStamina);
            DataManager.Instance.CharacterData._playerStamina = _stamina;
            if (_stamina >= 0f)
            {
                OnStaminaChanged?.Invoke(_maxStamina, ChangedStamina);
            }
        }
    }
    //private void UpdateStamina(float currentStamina)
    //{
    //    _stamina = currentStamina;
    //    DataManager.Instance.CharacterData._playerStamina = _stamina;
    //    if (_stamina >= 0f)
    //    {
    //        _onStaminaChanged?.Invoke(_maxStamina, ChangedStamina);
    //    }
    //}

    private void Update()
    {
        PlayerDir();
        moveCurrentState?.Update();
        actionCurrentState?.Update();

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ChangedHealth -= 10f;
        }
    }
    
    void PlayerDir()
    {
        //마우스 좌표값을 월드 좌표값으로 변환
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //플레이어와 마우스 사이 좌표 거리 계산
        Vector3 angle = _mousePosition - transform.position;
        //좌표 거리값을 바탕으로 각도(aTan) 계산 
        float angleZ = Mathf.Atan2(angle.y, angle.x) * Mathf.Rad2Deg; //라디안을 도(°)로 단위 변환

        if (angleZ > 90 || angleZ < -90)
        {
            //transform.localRotation =new Quaternion(0,0,0,0);
            transform.localScale = new Vector3(1, 1, 1);
        }
        //캐릭터를 기준으로 마우스가 오른쪽으로 넘어가면 캐릭터 방향 변경
        else
        {
            //transform.localRotation = new Quaternion(0, 180, 0, 0);
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    IEnumerator RestoreStamina()
    {
       while(_stamina < _maxStamina)
        {
            yield return new WaitForSeconds(0.2f);

            ChangedStamina += _restoreStamina;
        }
        regen = null;
    }
    public void UseStamina()
    {
        _stamina -= 50;
        OnStaminaChanged?.Invoke(_maxStamina, _stamina);
        if (regen == null)
        {
           regen =  StartCoroutine(RestoreStamina());
        }
    }

    public void MoveState(IState state)
    {
        moveCurrentState?.Exit();
        moveCurrentState = state;
        moveCurrentState.Enter();
    }
    public void ActionState(IState state)
    {
        actionCurrentState?.Exit();
        actionCurrentState = state;
        actionCurrentState.Enter();
    }
    private void OnDisable()
    {
        GameManager.Instance.player = null;
        StopAllCoroutines();
    }
}
