using UnityEngine;

public class IdleState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    PlayerDig _playerDig;
    PlayerAttack _playerAttack;
    public IdleState(Player player)
    {
        _player = player;
    }
    public void Enter()
    {
       
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        //만약 움직이면 이동상태로 
        if(_playerMovement.Move != Vector3.zero)
        {
            _player.SetState(new MoveState(_player));
        }

        if(_playerAttack._isAttacking == true)
        {
            _player.SetState(new AttackState(_player));
        }
        
        if(_playerDig._isDigging == true)
        {
            _player.SetState(new DigState(_player));
        }

    }
}
