using System.Collections;
using UnityEngine;

public class MonsterAttack : IState
{
    private Monster _monster;
    private MonsterView _view;
    private IAttack _attack;
    private float _attackReadyTime = 1.5f;
    private bool _isAttackStart;

    public MonsterAttack(Monster monster)
    {
        _monster = monster;
        _view = monster.View;
        _attack = monster.Attack;
    }


    public void Enter()
    {
        _attack.StartAttack();
        _view.OnIdleAni();
        _monster.StartCoroutine(AttackTimer());
    }

    public void Exit()
    {
        _isAttackStart = false;
        _attack.EndAttack();
        _view.OnDisIdleAni();
    }

    public void Update()
    {
        if(_isAttackStart)
            OnAttack();
    }

    private void OnAttack()
    {
        _monster.Attack.OnAttack();

        if (!_attack.IsAttack)
        {
            _monster.SetState(MonsterState.Chase);
        }
    }

    private IEnumerator AttackTimer()
    {
        yield return CoroutineManager.waitForSecondsRealtime(_attackReadyTime);
        _isAttackStart = true;
        _view.OnAttackAni();
    }
}
