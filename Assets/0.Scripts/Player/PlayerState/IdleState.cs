using UnityEngine;

public class IdleState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    Animator _animator;
    public IdleState(Player player)
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
        _animator.SetBool("isIdle", false);
    }

    public void Update()
    {
        _animator.SetBool("isIdle", true);
        //만약 움직이면 이동상태로
        if (_playerMovement.Move != Vector3.zero)
        {
            _player.MoveState(new MoveState(_player));
            Debug.Log("이동상태 전환");
        }

    }
}
