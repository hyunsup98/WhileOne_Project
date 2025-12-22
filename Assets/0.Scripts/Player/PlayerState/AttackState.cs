using UnityEngine;
using UnityEngine.InputSystem;

public class AttackState : IState
{
    Player _player;
    PlayerAttack _attack;
    Animator _animator;

    public AttackState(Player player)
    {
        _player = player;

        _attack = _player.PlayerAttack;

        _animator = _player.Animator;
    }
    
    public void Enter()
    {
        
    }

    public void Exit()
    {
        _animator.SetBool("isAttack", false);
    }

    public void Update()
    {

       if (_attack.IsAttacking == false)
       {
            _player.ActionState(new ActionIdleState(_player));
       }
    }
}
