using UnityEngine;
using UnityEngine.InputSystem;

public class MoveState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    

    //리짓바디, 방향 관련"
    Vector2 _dir;
    Vector3 _move;

    private bool _isMoving;


    public MoveState(Player player)
    {
        //_playerMoveMent = state._playerMovement;

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
        _player.transform.Translate(_playerMovement.Move * Time.deltaTime * _player.MoveSpeed);

        if (_playerMovement.Move != Vector3.zero)
        {
            _player.MoveState(new IdleState(_player));
            Debug.Log("멈춤상태 전환");
        }
    }
   
}
