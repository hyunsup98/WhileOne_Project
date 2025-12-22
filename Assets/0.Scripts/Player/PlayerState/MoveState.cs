using UnityEngine;
using UnityEngine.InputSystem;

public class MoveState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    Animator _animator;


    //리짓바디, 방향 관련"
    Vector2 _dir;
    Vector3 _move;

    private bool _isMoving;


    public MoveState(Player player)
    {
        _player = player;

        _playerMovement = _player.PlayerMove;

        _animator = _player.Animator;
    }

    public void Enter()
    {

    }

    public void Exit()
    {
        _animator.SetBool("isMoving", false);
    }

    public void Update()
    {
        _animator.SetBool("isMoving", true);

        if (_playerMovement.Move == Vector3.zero)
        {
            _player.MoveState(new IdleState(_player));
        }
    }

}
