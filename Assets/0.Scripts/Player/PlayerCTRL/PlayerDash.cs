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
    AfterimagePool obj;

    [SerializeField] private float _dashForce = 10; //대쉬 힘
    [SerializeField] private float _dashTime = 0.1f; //코루틴에서 사용할 대쉬 지속 시간

    bool _isDash;

    public bool IsDash => _isDash;

    private void Awake()
    {
        obj = transform.GetChild(2).GetComponent<AfterimagePool>();
        _playerMovement = GetComponent<PlayerMovement>();
        _player = GetComponent<Player>();
        _rd2D = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        _dig = _player.PlayerDig;

        _input = _player.Playerinput;
        _actionMap = _player.ActionMap;
        _dash = _actionMap.FindAction("Dash");

        _dash.started += OnDash;

    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        StartCoroutine(Dash());
    }
    IEnumerator Dash()
    {
        if (!_dig.IsDigging && !_player.Stop.Action)
        {
            _rd2D.linearVelocity = _playerMovement.Move.normalized * _dashForce;
            yield return new WaitForSeconds(_dashTime);
            //obj.Take();
            _rd2D.linearVelocity = Vector2.zero;

        }

    }

}
