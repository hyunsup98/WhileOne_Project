using UnityEngine;

public class ProtoAttack : IAttack
{
    private Monster _monster;
    private Vector2 _target;
    private float _attackSpeed = 10f;
    private BoxCollider2D _attackRange;


    public ProtoAttack(Monster monster)
    {
        _monster = monster;
        _attackRange = _monster.GetComponent<BoxCollider2D>();
    }


    public void StartAttack()
    {
        _target = _monster.Target.position;
    }

    public void OnAttack()
    {
        _monster.OnMove(_target, _attackSpeed);
    }

    public void EndAttack()
    {
    }

}