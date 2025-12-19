using UnityEngine;
using UnityEngine.InputSystem;

public class DigState : IState
{
    Player _player;
    PlayerDig _dig;
   

    public bool _isDigging;

    Vector3 _mousePosition;
    Vector3 dir;
    public DigState(Player player)
    {
        _player = player;

        _dig = _player.PlayerDig;

        
    }
   
    public void Enter() //이 상태면 이속 절반
    {
        _player.MoveSpeed /= 2;
    }

    public void Exit() //상태 나가면 이속 원래대로
    {
        _player.MoveSpeed *= 2;
    }

    public void Update()
    {
        _dig.Test.transform.position = new Vector3(_dig.Dir.x + _dig.OffSet, _dig.Dir.y + _dig.OffSet, 0);

        if(_dig.IsDigging == false)
        {
            Debug.Log("일반 모드");
            _player.ActionState(new ActionIdleState(_player));
        }

    }
    


}
