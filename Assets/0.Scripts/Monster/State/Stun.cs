using System.Collections.Generic;
using UnityEngine;

public class Stun : IState
{
    private MonsterPresenter _monster;
    //private Transform _myTransform;

    private float _stunTime = 5;

    private float _timer;

    public Stun(MonsterPresenter monster)
    {
        _monster = monster;
        //_myTransform = monster.View.MyTransform;
    }



    public void Enter()
    {
        _monster.OnPlayAni("Stun");
        Debug.Log("<color=yellow>스턴상태 돌입</color>");
    }

    public void Exit()
    {
        _monster.OnPlayAni("Teleport");

        Debug.Log("<color=blue>스턴상태 해제</color>");
        _timer = 0;
        _monster.SetIsUlt(true);
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > _stunTime)
            _monster.Model.SetState(MonsterState.Action);
    }
}
