using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Player _player;
    private PlayerDig _dig;
    private TrakingPlayer _traking;
    private WeaponChange _weapon;

    //플레이어 인풋 관련
    private PlayerInput _input;
    private InputActionMap _actionMap;
    private InputAction _attackAtion;

    // 이벤트 관련 변수
    public event Action<int> OnSubWeaponAttack;        // 서브 무기로 쳤을 때 내구도 넘겨주기

    WaitForSeconds attackSpeed;
    WaitForSeconds delayTime;

    bool _timer = false;
    float _attSpeed;
    float _delayTime = 0.3f;
    private bool _isAttacking = false;
    private bool _isEffect1 = true; //그냥 토글용 변수
    private bool isUse;

    //public GameObject Effect1 => _effect1;
    //public GameObject Effect2 => _effect2;
    public bool IsAttacking => _isAttacking;
    public bool IsTimer { get; set; }

    private void Awake()
    {
        InIt();
    }

    void InIt()
    {
        _player = GetComponent<Player>();

        _input = GetComponent<PlayerInput>();  //최상위 부모의 플레이어 인풋 컴포넌트

        _traking = GetComponentInChildren<TrakingPlayer>();

        _weapon = GetComponent<WeaponChange>();
    }
    void Start()
    {
        attackSpeed = new WaitForSeconds(_attSpeed);
        delayTime = new WaitForSeconds(_delayTime);
        _attSpeed = 10f / _player.AttackSpeed; //공격 속도 세팅 (공격속도는 10/n값) 필요하다면 조절가능

        _actionMap = _input.actions.FindActionMap("Player");

        _attackAtion = _actionMap.FindAction("Attack");

        _attackAtion.performed += Attaking;//입력 받을 때
        _attackAtion.performed += NotDuplication;

        _attackAtion.canceled += ctx => //입력 뗄 때
        {
            _isAttacking = false;
            isUse = false;
        };

        _dig = _player.PlayerDig;


    }
    private void Attaking(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }
        if (_dig.IsDigging == false && !_player.Stop.Action)
        {
            if (!_timer)
            { 
                Debug.Log(_timer);
            }

            if (_timer) //딜레이 중이면 리턴
            {
                Debug.Log("딜레이중");
                return;
            }
            Debug.Log("공격");
            _timer = true;
            _isAttacking = true; //공격 트리거용 변수

            StartCoroutine(AttackSpeed()); //공격속도 딜레이


            SoundManager.Instance.PlaySoundEffect(DataManager.Instance.SFXData.FindWeaponSound(_weapon.currentweapon.WeaponData.weaponID.ToString()));

            _traking.PlayEffect();
        }

    }
    IEnumerator AttackSpeed() //공격 속도 딜레이 코루틴
    {
        yield return new WaitForSeconds(_attSpeed);
        //yield return attackSpeed;
       _timer = false;
    }
    private void NotDuplication(InputAction.CallbackContext ctx)
    {
        StartCoroutine(Delay());
    }
    private void Update()
    {
        _attSpeed = 10f / _player.AttackSpeed; //공격 속도 실시간 세팅
    }
    IEnumerator Delay()
    {
        yield return delayTime;
        _isAttacking = false;
    }

    private void OnDisable()
    {
        _attackAtion.started -= Attaking;
        _attackAtion.started -= NotDuplication;
    }
}
