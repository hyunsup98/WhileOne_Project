using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    private Player _player;
    private PlayerDig _dig;
    private PlayerMovement _playerMovement;
    private PlayerDamage _playerDamage;
    private PlayerInput _input;
    private InputActionMap _actionMap;
    private InputAction _dash;
    private Rigidbody2D _rd2D;

    private GameObject _damaged;

    [SerializeField] private float _dashForce = 50; //대쉬 힘
    [SerializeField] private float _dashTime = 0.1f; //코루틴에서 사용할 대쉬 지속 시간

    //[SerializeField] Blink _test;
    TrailRenderer _trail;

    bool _isDash;
    float _size = 10;
    float dashtime = 0.2f;

    WaitForSeconds DashDelay;
    WaitForSeconds DashTime;

    Blink blink;

    public bool IsDash => _isDash;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
        _playerMovement = GetComponent<PlayerMovement>();
        _player = GetComponent<Player>();
        _rd2D = GetComponent<Rigidbody2D>();
        _trail.enabled = false;

        DashDelay = new WaitForSeconds(dashtime);
        DashTime = new WaitForSeconds(_dashTime);
        //for (float i = 1; i <= _size; i++)
        //{
        //    blink = AfterimagePool.Instance.GetObject(_test, AfterimagePool.Instance.transform);
        //    blink.transform.position = transform.position + new Vector3(i / 5, 0, 0);
        //}
    }
    private void Start()
    {

        _dig = _player.PlayerDig;
        _damaged = transform.GetChild(0).gameObject;
        _input = _player.Playerinput;
        _actionMap = _player.ActionMap;
        _dash = _actionMap.FindAction("Dash");

        _dash.performed += OnDash;

    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.phase);
        if (_isDash)
        {
            return;
        }
        if (_player.Stamina <= 0  )
        {
            return;
        }
        if (ctx.performed && ctx.ReadValue<float>() > 0.1f)
        {
            if (_player.Stamina >= 50 && _playerMovement.Move != Vector3.zero)
            {
                StartCoroutine(Dash());
            }
            else
            {
                SoundManager.Instance.PlaySoundEffect("Player_CantDash_FX_001");

            }
        }
    }
    IEnumerator Dash()
    {
        _isDash = true;
        if (!_dig.IsDigging && !_player.Stop.Action)
        {
            _player.UseStamina();
            //_damaged.tag = "Untagged";
            _rd2D.linearVelocity = _playerMovement.Move.normalized * _dashForce;
            //AfterimagePool.Instance.GetObject(_test, AfterimagePool.Instance.transform);
            //blink.transform.position = transform.position;
            SoundManager.Instance.PlaySoundEffect("Player_Dash_FX_001");
            yield return DashTime;
            //AfterimagePool.Instance.TakeObject(blink);
            _rd2D.linearVelocity = Vector2.zero;
            _isDash = false;

            yield return DashDelay;
            //_damaged.tag = "Player";
        }
        else
        {
            _isDash = false;
        }
    }
    private void OnDisable()
    {
        _dash.performed -= OnDash;
    }

}
