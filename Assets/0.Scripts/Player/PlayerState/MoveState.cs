using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class MoveState : IPlayerState
{
    Player _player;
    PlayerMovement _playerMovement;



    public MoveState(Player player)
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
        //만약 움직임이 멈췄다면 정지 상태로
        if (_playerMovement.Move == Vector3.zero)
        {
            _player.SetState(new IdleState(_player));
        }
    }
}
