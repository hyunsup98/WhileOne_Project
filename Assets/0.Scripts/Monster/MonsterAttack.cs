using UnityEngine;

public class MonsterAttack : IState
{
    private Monster _monster;
    private IAttack _attack;

    public MonsterAttack(Monster monster)
    {
        _monster = monster;
        _attack = monster.Attack;
    }


    public void Enter()
    {
        _attack.StartAttack();
    }

    public void Exit()
    {
        _attack.EndAttack();
    }

    public void Update()
    {
        OnAttack();
    }

    private void OnAttack()
    {

        _monster.Attack.OnAttack();

        if (!_attack.IsAttack)
            _monster.SetState(MonsterState.Chase);
    }
}
