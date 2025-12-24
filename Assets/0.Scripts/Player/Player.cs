using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [Header("플레이어 수치 조절 컴포넌트")]
    [SerializeField] private float _maxHp = 100;
    [SerializeField] private float _maxStamina = 100;
    [SerializeField] private int _moveSpeed = 3;
    [SerializeField] private int _attack = 5;
    [SerializeField] private int _attackSpeed = 4;

    [SerializeField] private Rigidbody2D _rg2d;

    private float _hp;
    private float _stamina;

    private bool _isDamage;

    PlayerInput _input;
    InputActionMap _actionMap;

    Vector3 _mousePosition;

    //현재 상태를 담을 인터페이스 변수
    private IState moveCurrentState;
    private IState actionCurrentState;

    //이벤트 관련 변수
    public event Action<float,float> _onHpChanged;
    public event Action<float,float> _onStaminaChanged;

    //코루틴 변수
    private WaitForSeconds _delay;
    private WaitForSeconds _blinkTime;
    [SerializeField] float time = 0.12f;
    private float _finsihTime  = 0.7f;
    private float _checkTime = 0;
    
    //레이어 관련 변수
    private SortingGroup _group;

    //상태에서 사용할 스크립트
    [SerializeField] private PlayerAttack _attackAction;
    PlayerMovement _playerMove;
    PlayerDig _playerDig;
    PlayerDash _dash;
    MoveStopAnimation _stop;
    Animator _animator;

    //외부에서 사용할 프로퍼티
    public float Hp => _hp;
    public float MaxHp => _maxHp; // 잠시만추가할게요
    public float Stamina => _stamina;
    public int MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public int Attack => _attack;
    public int AttackSpeed => _attackSpeed;
    public bool IsDamaged { get { return _isDamage; } set { _isDamage = value; } }
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

    private void Awake()
    {
        Component();
        _actionMap = _input.actions.FindActionMap("Player");

        GameManager.Instance.player = this;
    }
    void Component()
    {
        _playerMove = GetComponent<PlayerMovement>();
        _playerDig = GetComponent<PlayerDig>();
        _input = GetComponent<PlayerInput>();
        _rg2d = GetComponent<Rigidbody2D>();
        _dash = GetComponent<PlayerDash>();
        _animator = transform.GetChild(0).GetComponent<Animator>();
        _stop = transform.GetChild(0).GetComponent<MoveStopAnimation>();

    }
    void Start()
    {
        _hp = _maxHp;
        _stamina = _maxStamina;

        MoveState(new IdleState(this));
        ActionState(new ActionIdleState(this));

        _delay = new WaitForSeconds(1f);
        _blinkTime = new WaitForSeconds(time);

        _group = transform.GetChild(0).GetComponent<SortingGroup>();

        StartCoroutine(Blink());
    }

    public float ChangedHealth
    {
        get { return _hp; }
        set
        {
            _hp = Mathf.Clamp(value, 0, _maxHp);
            if(_hp >= 0f)
            {
                _onHpChanged?.Invoke(_maxHp,ChangedHealth);

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
        private set
        {
            _stamina = Mathf.Clamp(value, 0, _maxStamina);
            if(_stamina <= 0)
            {
                _onHpChanged?.Invoke(_maxStamina, ChangedStamina);
            }
        }
           
    }
    private void Update()
    {
        PlayerDir();
        moveCurrentState?.Update();
        actionCurrentState?.Update();

        //if(Keyboard.current.qKey.wasPressedThisFrame)
        //{
        //    TakenDamage(10, Vector2.zero);
        //}
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
            transform.localScale = new Vector3(1, 1, 1);
        }
        //캐릭터를 기준으로 마우스가 오른쪽으로 넘어가면 캐릭터 방향 변경
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    public void TakenDamage(float damage, Vector2 target)
    {
        IsDamaged = true;
        ChangedHealth -= damage;
        StartCoroutine(KnockBack(target));
        //StartCoroutine(Blink());
    }



    public void RestoreStamina()
    {
       if(_stamina <= 99)
        {
            StartCoroutine(RestoreCoroutine());
        }
    }
    public void UseStamina()
    {
        if(_stamina < 50)
        {
            return;
        }
        _stamina -= 50;
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

    IEnumerator RestoreCoroutine()
    {
        yield return _delay;
        _stamina += 5;
    }

    IEnumerator Blink()
    {
        _checkTime = 0;
        gameObject.layer = LayerMask.NameToLayer("PlayerDamage");
       
        while (_finsihTime>_checkTime)
        {
            _group.sortingOrder = 0;

            yield return _blinkTime;

            _group.sortingOrder = 3;

            _checkTime += time;

            yield return _blinkTime;
        }
        gameObject.layer = LayerMask.NameToLayer("Player");
        _isDamage = false;
        yield return null;

    }
    IEnumerator KnockBack(Vector2 target)
    {
        int dirx = transform.position.x - target.x > 0 ? 1 : -1;
        int diry = transform.position.y - target.y > 0 ? 1 : -1;
        _rg2d.linearVelocityX = dirx * 1f;
        _rg2d.linearVelocityY = diry * 1f;
        yield return new WaitForSeconds(0.5f);
        _rg2d.linearVelocity = Vector2.zero;
    }

    private void OnDisable()
    {
        GameManager.Instance.player = null;
    }
}
