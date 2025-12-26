using System.Collections.Generic;
using System.Diagnostics;

public class MonsterAction : IState
{
    private MonsterPresenter _monster;
    private MonsterView _view;
    private MonsterPattern _action;

    private int _actionCount;

    public MonsterAction(MonsterPresenter monster)
    {
        _monster = monster;
        _view = monster.View;
    }


    public void Enter()
    {
        _action = SetAction();

        UnityEngine.Debug.Log("액션" + _action);
        if(_action == null)
        {
            _monster.Model.SetState(MonsterState.Chase);
            return;
        }

        // 행동01과 행동04 일 때(사용할 때), 행동 카운트가 증가
        if ((_action is MonsterPattern01) || (_action is MonsterPattern04))
            _actionCount++;

        if (_action is MonsterPattern05)
            _actionCount = 0;

        _action.StartAction();
    }

    public void Exit()
    {
        _action?.EndAction();
    }

    public void Update()
    {
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
        if (_actionCount == 5)
            return actionDict[ActionID.five];

        else if (actionDict[ActionID.four].IsActionable)
            return actionDict[ActionID.four];

        else if (actionDict[ActionID.one].IsActionable)
            return actionDict[ActionID.one];

        return null;
    }
}
