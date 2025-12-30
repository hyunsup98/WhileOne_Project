using UnityEngine;
using UnityEngine.InputSystem;

public class DigState : IState
{
    Player _player;
    PlayerDig _dig;
    Animator _animator;

    float _move;
    float _running;
    float _waitTime = 1.4f;

    Vector3 _mousePosition;
    Vector3 dir;
    public DigState(Player player)
    {
        _player = player;

        _dig = _player.PlayerDig;

        _animator = _player.Animator;

    }

    public void Enter() //이 상태면 이속 절반
    {
        _running = 0;
        _player.MoveSpeed /= 2f;
        _move = _player.MoveSpeed;
    }

    public void Exit() //상태 나가면 이속 원래대로
    {
        _player.MoveSpeed *= 2f;
    }

    public void Update()
    {
        _running += Time.time * Time.deltaTime;
        _dig.Test.transform.position = new Vector3(_dig.Dir.x + _dig.OffSet, _dig.Dir.y + _dig.OffSet, 0);

        if (_dig.IsDigging == false)
        {
            _animator.SetTrigger("isDig");
            _player.MoveSpeed = _move;
            _player.ActionState(new ActionIdleState(_player));
        }
    }
}
