using UnityEngine;

public class IdleState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    
    public IdleState(Player player)
    {
        _player = player;
        _playerMovement = _player.PlayerMove;
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
        if (_playerMovement.Move != Vector3.zero)
        {
            _player.MoveState(new MoveState(_player));
            Debug.Log("이동상태 전환");
        }

    }
}
