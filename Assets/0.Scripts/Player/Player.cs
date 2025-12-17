using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Player : MonoBehaviour
{
    [Header("플레이어 수치 조절 컴포넌트")]
    [SerializeField] private float _maxHp = 100;
    [SerializeField] private float _maxStamina = 100;
    [SerializeField] private int _moveSpeed = 3;
    [SerializeField] private int _attack = 5;
    [SerializeField] private int _attackSpeed = 4;

    private float _hp;
    private float _stamina;

    //현재 상태를 담을 인터페이스 변수
    private IPlayerState currentState;


    public event Action<float,float> _onHpChanged;
    public event Action<float,float> _onStiminaChanged;
    
    //외부에서 사용할 프로퍼티
    public float Hp => _hp;
    public float Stamina => _stamina;
    public int MoveSpeed { get; set; }
    public int Attack => _attack;
    public int AttackSpeed => _attackSpeed;


    public float ChangedHealth
    {
        get { return _hp; }
        private set
        {
            _hp = Mathf.Clamp(value, 0, _maxHp);
            if(_hp != value)
            {
                _onHpChanged?.Invoke(_maxHp,ChangedHealth);
            }
        }
           
    }
    public float ChangedStamina
    {
        get { return _stamina; }
        private set
        {
            _stamina = Mathf.Clamp(value, 0, _maxStamina);
            if(_stamina != value)
            {
                _onHpChanged?.Invoke(_maxStamina, ChangedStamina);
            }
        }
           
    }

    public void TakenDamage(float damage)
    {
        _hp -= damage;
    }

    public void RestoreStamina()
    {
        
    }
    public void UseStamina()
    {
        if(_stamina < 50)
        {
            return;
        }
        _stamina -= 50;
    }

    void Start()
    {
        _hp = _maxHp;
        _stamina = _maxStamina;
    }

    public void SetState(IPlayerState state)
    {
        currentState?.OnExit();
        currentState = state;
        currentState.OnEnter();
    }
}
