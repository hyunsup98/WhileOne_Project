using System.Collections;
using UnityEngine;

public class MonsterAction : IState
{
    private MonsterPresenter _monster;
    private MonsterView _view;
    private MonsterPattern _action;

    public MonsterAction(MonsterPresenter monster)
    {
        _monster = monster;
        _view = monster.View;

        foreach(var actionDict in monster.Model.ActionDict)
            _action = actionDict.Value;
    }


    public void Enter()
    {
        _action.OnAniTrigger += _view.OnPlayAni;
        _action.StartAction();
    }

    public void Exit()
    {
        //_isAttackStart = false;
        _action.EndAction();
        _action.OnAniTrigger -= _view.OnPlayAni;
    }

    public void Update()
    {
        _action.OnAction();

        if (!_action.IsAction)
        {
            _monster.Model.SetState(MonsterState.Chase);
            return;
        }
    }
}
