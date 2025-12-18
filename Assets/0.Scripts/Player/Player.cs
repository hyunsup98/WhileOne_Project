using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;

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
    private IState currentState;

    public event Action<float,float> _onHpChanged;
    public event Action<float,float> _onStiminaChanged;

    private WaitForSeconds _delay;
    private WaitForSeconds _blinkTime;

    [SerializeField] float time = 0.5f;
    float _finsihTime  = 2;
    float _checkTime = 0;

    SortingGroup _group;

    //외부에서 사용할 프로퍼티
    public float Hp => _hp;
    public float Stamina => _stamina;
    public int MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public int Attack => _attack;
    public int AttackSpeed => _attackSpeed;

    void Start()
    {
        _hp = _maxHp;
        _stamina = _maxStamina;

        _delay = new WaitForSeconds(1f);
        _blinkTime = new WaitForSeconds(time);

        _group = transform.GetChild(0).GetComponent<SortingGroup>();
        Debug.Log(_group.name);

        StartCoroutine(Blink());
    }

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
        



        StopCoroutine(Blink());
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

    public void SetState(IState state)
    {
        currentState?.Exit();
        currentState = state;
        currentState.Enter();
    }

    IEnumerator RestoreCoroutine()
    {
        yield return _delay;
        _stamina += 5;
    }

    IEnumerator Blink()
    {
        _checkTime = 0;

        while (_finsihTime>_checkTime)
        {
            _group.sortingOrder = 0;

            yield return _blinkTime;

            _group.sortingOrder = 1;

            _checkTime += time*Time.deltaTime;

            yield return _blinkTime;
        }
        yield return null;

    }
}
