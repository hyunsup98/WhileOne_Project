using System.Collections.Generic;
using UnityEngine;

public class MonsterAction : IState
{
    private MonsterPresenter _monster;
    private MonsterPattern _action;

    private int _actionCount;

    public MonsterAction(MonsterPresenter monster)
    {
        _monster = monster;
    }


    public void Enter()
    {
        if (_monster.IsDeath)
            return;

        _action = SetAction();
        if(_action == null)
        {
            _monster.Model.SetState(MonsterState.Chase);
            return;
        }

        Debug.Log("액션 카운트" + _actionCount);
        //행동01과 행동04 일 때(사용할 때), 행동 카운트가 증가
        if ((_action is MonsterPattern01) || (_action is MonsterPattern04))
            _actionCount++;

        if (_action is MonsterPattern05 || _action is MonsterPattern06)
            _actionCount = 0;

        _action.StartAction();
    }

    public void Exit()
    {
        if (_monster.IsDeath)
            return;

        _action?.EndAction();
    }

    public void Update()
    {
        if (_monster.IsDeath)
            return;

        _action?.OnAction();

        if (_action != null && !_action.IsAction)
        {
            _monster.Model.SetState(MonsterState.Chase);
            return;
        }
    }

    // 사용 조건에 맞는 우선되는 행동 반환 메서드
    private MonsterPattern SetAction()
    {
        Dictionary<ActionID, MonsterPattern> actionDict = _monster.Model.ActionDict;

        if (actionDict.TryGetValue(ActionID.three, out var value) && _monster.IsPattern03)
        {
            _monster.SetIsPattern03(false);
            return value;
        }

        else if (actionDict.TryGetValue(ActionID.six, out value) && _monster.IsUlt)
        {
            _monster.SetIsUlt(false);
            return value;
        }


        else if (actionDict.TryGetValue(ActionID.five, out value) && _actionCount >= 5)
            return value;

        else if (actionDict.TryGetValue(ActionID.two, out value) && value.IsActionable)
            return value;

        else if (
            actionDict.TryGetValue(ActionID.four, out value) && value.IsActionable)
            return value;

        else if (
            actionDict.TryGetValue(ActionID.one, out value) && value.IsActionable)
            return value;

        UnityEngine.Debug.LogWarning("쿨타임 또는 null로 인해 행동사용 불가");
        return null;
    }
}
