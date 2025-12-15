using UnityEngine;

public class IdleState : IPlayerState
{
    Player _player;
    PlayerMovement _playerMovement;
    public IdleState(Player player)
    {
        _player = player;
    }



    public void OnEnter()
    {
       
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        //만약 움직이면 이동상태로 
        if(_playerMovement.Move != Vector3.zero)
        {
            _player.SetState(new MoveState(_player));
        }
    }

    
}
