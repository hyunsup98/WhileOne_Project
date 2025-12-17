using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerDig : MonoBehaviour
{
    PlayerInput _input;
    private InputActionMap _actionMap;
    private InputAction _digAction;

    [SerializeField] GameObject test;
    [SerializeField] float offSet;


    Vector3 _mousePosition;
    Vector3 dir;
    bool _isDigging;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();  //최상위 부모의 플레이어 인풋 컴포넌트

    }
    void Start()
    {
        
        _actionMap = _input.actions.FindActionMap("Player");
        _digAction = _actionMap.FindAction("Special");

        _digAction.performed += Dig;
        _digAction.canceled += ctx =>
        {
            test.SetActive(false);
            _isDigging = false;
        };
    }
    void Dig(InputAction.CallbackContext ctx)
    {
        test.SetActive(true);
        _isDigging = true;
    }

    private void Update()
    {
        //마우스 좌표값을 월드 좌표값으로 변환
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //Debug.Log(_mousePosition);

        //플레이어와 마우스 사이 좌표 거리 계산
        Vector3 angle = _mousePosition - transform.position;

       
        Vector3 a = Vector3Int.FloorToInt(transform.position);

        if (angle.x > 1)
        {
            dir.x = a.x+1f;
        }
        else if(angle.x < -1)
        {
            dir.x = a.x - 1f;
        }
        else
        {
            dir.x = a.x;
        }
        Debug.Log(dir.x);
        if (angle.y > 1)
        {
            dir.y = a.y+1f;
        }
        else if(angle.y < -1)
        {
            dir.y = a.y - 1f;
        }
        else
        {
            dir.y = a.y;
        }
        Debug.Log(dir.y);


       
        test.transform.position = new Vector3(dir.x+offSet, dir.y+offSet, 0);
    }
}
