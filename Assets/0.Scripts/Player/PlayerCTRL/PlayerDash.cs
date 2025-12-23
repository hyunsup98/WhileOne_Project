using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    Player _player;
    PlayerDig _dig;
    PlayerMovement _playerMovement;
    PlayerInput _input;
    InputActionMap _actionMap;
    InputAction _dash;
    Rigidbody2D _rd2D;

    [SerializeField] private float _dashForce = 10; //대쉬 힘
    [SerializeField] private float _dashTime = 0.1f; //코루틴에서 사용할 대쉬 지속 시간

    [SerializeField] Blink _test;
    TrailRenderer _trail;

    bool _isDash;
    float _size = 10;

    Blink blink;

    public bool IsDash => _isDash;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
        _playerMovement = GetComponent<PlayerMovement>();
        _player = GetComponent<Player>();
        _rd2D = GetComponent<Rigidbody2D>();

        _trail.enabled = false;
        //for (float i = 1; i <= _size; i++)
        //{
        //    blink = AfterimagePool.Instance.GetObject(_test, AfterimagePool.Instance.transform);
        //    blink.transform.position = transform.position + new Vector3(i / 5, 0, 0);
        //}
    }
    private void Start()
    {

        _dig = _player.PlayerDig;

        _input = _player.Playerinput;
        _actionMap = _player.ActionMap;
        _dash = _actionMap.FindAction("Dash");

        _dash.performed += OnDash;

    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.phase);
        if (_player.Stamina <= 0)
        {
            return;
        }
        if (ctx.performed && ctx.ReadValue<float>() > 0.1f)
        {
            StartCoroutine(Dash());
        }
    }
    IEnumerator Dash()
    {
        _player.UseStamina();
        if (!_dig.IsDigging && !_player.Stop.Action)
        {
            _rd2D.linearVelocity = _playerMovement.Move.normalized * _dashForce;
            //AfterimagePool.Instance.GetObject(_test, AfterimagePool.Instance.transform);
            //blink.transform.position = transform.position;
            yield return new WaitForSeconds(_dashTime);
            //AfterimagePool.Instance.TakeObject(blink);
            _rd2D.linearVelocity = Vector2.zero;

        }

    }
    private void OnDisable()
    {
        _dash.performed -= OnDash;
    }

}
