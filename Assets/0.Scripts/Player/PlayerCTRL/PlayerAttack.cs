using System.Collections;
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
    [SerializeField] private GameObject _effect1;
    [SerializeField] private GameObject _effect2;

    WaitForSeconds _delay;
    bool _isEffect1 = true; //그냥 토글용 변수
    bool _timer = false;
    float _attSpeed;

    private void Awake()
    {
        _player = transform.root.GetComponent<Player>();

        _input = transform.root.GetComponent<PlayerInput>();  //최상위 부모의 플레이어 인풋 컴포넌트

        //_effect1 = transform.GetChild(0).gameObject; //자식 이펙트

        //_effect2 = transform.GetChild(1).gameObject; //자식 이펙트
    }

    void Start()
    {
        _attSpeed = 10f / _player.AttackSpeed; //공격 속도 세팅 (공격속도는 10/n값) 필요하다면 조절가능

        _actionMap = _input.actions.FindActionMap("Player");

        _attackAtion = _actionMap.FindAction("Attack");

        _attackAtion.performed += ctx => //입력 받을 때
        {
            if (_timer) //딜레이 중이면 리턴
            {
                return;
            }

            StartCoroutine(AttackSpeed()); //공격속도 딜레이
            
            if (_isEffect1) //첫번째 이펙트 위에서 아래로 쓸기
            {
                _effect1.SetActive(true);
                _isEffect1 = false;
                _timer = true;
            }
            else //두번째 이펙트 아래에서 위로 쓸기
            {
                _effect2.SetActive(true);
                _isEffect1 = true;
                _timer = true;
            }
        };
    }
    
    IEnumerator AttackSpeed() //공격 속도 딜레이 코루틴
    {
        Debug.Log($"{_attSpeed}초만큼 딜레이");
        yield return new WaitForSeconds(_attSpeed);
        _timer = false;
    }
    private void Update()
    {
        _attSpeed = 10f / _player.AttackSpeed; //공격 속도 실시간 세팅
    }
}
