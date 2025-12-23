using System.Collections;

public class MonsterAction : IState
{
    private MonsterPresenter _monster;
    private MonsterView _view;
    private IAction _action;
    private float _attackReadyTime = 1.5f;
    private bool _isAttackStart;

    public MonsterAction(MonsterPresenter monster)
    {
        _monster = monster;
        _view = monster.View;
        _action = monster.Model.ActionDict[ActionID.one];
    }


    public void Enter()
    {
        _action.StartAction();
        _view.OnIdleAni();
        _monster.StartCoroutine(AttackTimer());
    }

    public void Exit()
    {
        _isAttackStart = false;
        _action.EndAction();
        _view.OnDisIdleAni();
    }

    public void Update()
    {
        if(_isAttackStart)
            OnAttack();
    }

    private void OnAttack()
    {
        //_monster.Attack.OnAction();

        _monster.Model.ActionDict[ActionID.one].OnAction();

        if (!_action.IsAction)
        {
            _monster.Model.SetState(MonsterState.Chase);
        }
    }

    private IEnumerator AttackTimer()
    {
        yield return CoroutineManager.waitForSecondsRealtime(_attackReadyTime);
        _isAttackStart = true;
        _view.OnAttackAni();
    }
}
