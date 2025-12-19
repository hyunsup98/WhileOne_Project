using UnityEngine;

public class ActionIdleState : IState
{
    Player _player;
    PlayerAttack _attack;
    PlayerDig _dig;
    public ActionIdleState(Player player)
    {
        _player = player;

        _attack = _player.PlayerAttack;

        _dig = _player.PlayerDig;
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
            Debug.Log("공격 전환");
            _player.ActionState(new AttackState(_player));
        }
        if (_dig.IsDigging == true)
        {
            Debug.Log("땅파기 전환");
            _player.ActionState(new DigState(_player));
        }
        if(_player.IsDamaged == true)
        {
            _player.ActionState(new DamageState(_player));
        }
    }
}
