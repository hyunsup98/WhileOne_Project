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
    private GameObject _effect1;
    private GameObject _effect2;

    WaitForSeconds _delay;
    bool isEffect1 = true; //그냥 토글용 변수
    float attSpeed;

    private void Awake()
    {
        _player = transform.root.GetComponent<Player>();

        _input = transform.root.GetComponent<PlayerInput>();  //최상위 부모의 플레이어 인풋 컴포넌트

        _effect1 = transform.GetChild(0).gameObject; //자식 이펙트

        _effect2 = transform.GetChild(1).gameObject; //자식 이펙트
    }

    void Start()
    {
        _delay = new WaitForSeconds(attSpeed);

        _actionMap = _input.actions.FindActionMap("Player");

        _attackAtion = _actionMap.FindAction("Attack");

        Debug.Log(_delay);

        _attackAtion.performed += ctx =>
        {
            if (isEffect1)
            {
                //_effect1.SetActive(true);
                isEffect1 = false;
                Debug.Log("1이펙트");
                StartCoroutine(AttackSpeed());
            }
            else
            {
                //_effect2.SetActive(true);
                isEffect1 = true;
                Debug.Log("2이펙트");
                StartCoroutine(AttackSpeed());
            }
            StopCoroutine(AttackSpeed());
        };
        //_attackAtion.canceled += ctx => StartCoroutine(AttackSpeed());

    }
    //void OnAttack(InputAction.CallbackContext context)
    //{
    //    StartCoroutine(AttackSpeed());
    //}
    IEnumerator AttackSpeed()
    {
        Debug.Log($"{attSpeed}초만큼 딜레이");
        yield return _delay;
    }


    private void Update()
    {
        attSpeed = 10f / _player.AttackSpeed;
    }
}
