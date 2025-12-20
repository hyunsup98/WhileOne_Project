using UnityEngine;
using UnityEngine.InputSystem;

public class MoveState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    Animator _animator;


    //¸®Áþ¹Ùµð, ¹æÇâ °ü·Ã"
    Vector2 _dir;
    Vector3 _move;

    private bool _isMoving;


    public MoveState(Player player)
    {
        _player = player;

        _playerMovement = _player.PlayerMove;
        
        _animator = _player.Animator;
    }

    public void Enter()
    {
       
    }

    public void Exit()
    {
       _animator.SetBool("isMoving", false);
    }

    public void Update()
    {
        _player.transform.Translate(_playerMovement.Move * Time.deltaTime * _player.MoveSpeed);
        _animator.SetBool("isMoving", true);
        
        if (_playerMovement.Move == Vector3.zero)
        {
            _player.MoveState(new IdleState(_player));
            Debug.Log("¸ØÃã»óÅÂ ÀüÈ¯");
        }
    }
   
}
