using UnityEngine;

public class ActionIdleState : IState
{
    Player _player;
    PlayerAttack _attack;
    PlayerDig _dig;
    Animator _animator;
    public ActionIdleState(Player player)
    {
        _player = player;

        _attack = _player.PlayerAttack;

        _dig = _player.PlayerDig;

        _animator = _player.Animator;
    }
    public void Enter()
    {

    }
    public void Exit()
    {

    }
    public void Update()
    {
        if(_attack.IsAttacking == true)
        {
            _animator.SetBool("isAttack", true);
            _player.ActionState(new AttackState(_player));
        }
        if (_dig.IsDigging == true)
        {
            _player.ActionState(new DigState(_player));
        }
        //if(_player.IsDamaged == true)
        //{
        //    _player.ActionState(new DamageState(_player));
        //}
    }
}
