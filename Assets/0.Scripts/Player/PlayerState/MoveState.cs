using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MoveState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    PlayerAttack _playerAttack;
    PlayerDig _playerDig;

    public MoveState(Player player)
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
        //_player.transform.Translate(_playerMovement.Move * Time.deltaTime * _player.MoveSpeed);
        //만약 움직임이 멈췄다면 정지 상태로
        if (_playerMovement.Move == Vector3.zero)
        {
            _player.SetState(new IdleState(_player));
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
