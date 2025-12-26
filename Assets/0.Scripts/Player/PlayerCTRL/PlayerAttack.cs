using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Player _player;
    private PlayerDig _dig;
    private TrakingPlayer _traking;

    //플레이어 인풋 관련
    private PlayerInput _input;
    private InputActionMap _actionMap;
    private InputAction _attackAtion;

    //게임오브젝트 이펙트 관련
    //[SerializeField] private GameObject _effect1;
    //[SerializeField] private GameObject _effect2;

    // 이벤트 관련 변수
    public event Action<int> _onSubWeaponAttack;        // 서브 무기로 쳤을 때 내구도 넘겨주기

    bool _timer = false;
    float _attSpeed;
    private bool _isAttacking = false;
    bool _isEffect1 = true; //그냥 토글용 변수
    private bool isUse;

    //public GameObject Effect1 => _effect1;
    //public GameObject Effect2 => _effect2;
    public bool IsAttacking => _isAttacking;

    private void Awake()
    {
        inIt();
    }

    void inIt()
    {
        _player = GetComponent<Player>();

        _input = GetComponent<PlayerInput>();  //최상위 부모의 플레이어 인풋 컴포넌트

        _traking = GetComponentInChildren<TrakingPlayer>();
    }
    void Start()
    {
        _attSpeed = 10f / _player.AttackSpeed; //공격 속도 세팅 (공격속도는 10/n값) 필요하다면 조절가능

        _actionMap = _input.actions.FindActionMap("Player");

        _attackAtion = _actionMap.FindAction("Attack");

        _attackAtion.started += Attaking;//입력 받을 때
        _attackAtion.started += NotDuplication;

        _attackAtion.canceled += ctx => //입력 뗄 때
        {
            _isAttacking = false;
            isUse = false;
        };

        _dig = _player.PlayerDig;

       
    }
    private void Attaking(InputAction.CallbackContext ctx)
    {
        
        if (_dig.IsDigging == false && !_player.Stop.Action)
        {

            if (_timer) //딜레이 중이면 리턴
            {
                return;
            }

            StartCoroutine(AttackSpeed()); //공격속도 딜레이

            _isAttacking = true; //공격 트리거용 변수

            _traking.PlayEffect();

            _timer = true;
           
        }
       
    }
    private void NotDuplication(InputAction.CallbackContext ctx)
    {
        StartCoroutine(Delay());
    }


    IEnumerator AttackSpeed() //공격 속도 딜레이 코루틴
    {
        yield return new WaitForSeconds(_attSpeed);
        _timer = false;
    }
    private void Update()
    {
        _attSpeed = 10f / _player.AttackSpeed; //공격 속도 실시간 세팅
    }

    private void OnDisable()
    {
        _attackAtion.started -= Attaking;
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.3f);
        _isAttacking = false;
    }
}
