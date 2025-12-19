using UnityEngine;

public class DamageState : IState
{
    Player _player;

    public DamageState(Player player)
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
        if(_player.IsDamaged == false)
        {
            _player.ActionState(new ActionIdleState(_player));
        }
    }
}

