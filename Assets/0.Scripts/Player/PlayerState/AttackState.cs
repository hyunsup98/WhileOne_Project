using UnityEngine;
using UnityEngine.InputSystem;

public class AttackState : IState
{
    Player _player;
    PlayerAttack _attack;

    public AttackState(Player player)
    {
        _player = player;

        _attack = _player.PlayerAttack;
    }
    
    public void Enter()
    {

    }

    public void Exit()
    {
        
    }

    public void Update()
    {
       if(_attack.IsAttacking == false)
       {
            Debug.Log("일반으로 전환");
            _player.ActionState(new ActionIdleState(_player));
       }
    }
}
