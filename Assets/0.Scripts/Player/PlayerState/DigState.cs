using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class DigState : IPlayerState
{
    Player _player;
    PlayerDig _dig;
    public DigState(Player player)
    {
         _player = player;
    }
    public void OnEnter()
    {
        _player.MoveSpeed /= 2;
    }

    public void OnExit()
    {
        _player.MoveSpeed *= 2;
    }

    public void OnUpdate()
    {
       
    }

    
}
