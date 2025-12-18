using UnityEngine;

public class MonsterAttack : IState
{
    private Monster _monster;
    private IAttack _attack;

    public MonsterAttack(Monster monster)
    {
        _monster = monster;
        _attack = monster.Attack[0];
    }


    public void Enter()
    {

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
        _monster.Attack[0].OnAttack();

        if ((Vector2)_monster.transform.position == null)
            return;
    }
}
