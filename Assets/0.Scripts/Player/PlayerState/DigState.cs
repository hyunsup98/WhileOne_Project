using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class DigState : IState
{
    Player _player;
    PlayerMovement _playerMovement;
    PlayerDig _dig;
    public DigState(Player player)
    {
         _player = player;
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
        if(_dig._isDigging == false) // 피격 받을 때도 해제해야 하니까 꼭 넣으셈
        {
            if(_playerMovement.Move != Vector3.zero)
            {

            }
            else if(_playerMovement.Move == Vector3.zero)
            {

            }
            
        }

    }

    
}
