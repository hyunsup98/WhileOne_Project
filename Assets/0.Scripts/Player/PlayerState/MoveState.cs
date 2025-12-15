using UnityEngine;

public class MoveState : IPlayerState
{
    PlayerMovement _playerMovement;



    public MoveState(Player player)
    {

    }

    public void OnEnter()
    {
       
    }

    public void OnExit()
    {
       
    }

    public void OnUpdate()
    {
        if (_playerMovement.Move == Vector3.zero)
        {

        }
    }
}
