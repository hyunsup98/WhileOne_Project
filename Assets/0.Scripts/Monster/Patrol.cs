using UnityEngine;

public class Patrol : IMonsterState
{
    private Monster _monster;
    private Transform _target;
    private int _targetIndex;

    public Patrol(Monster monster)
    {
        _monster = monster;
        _target = monster.PatrolPoint[0];
        _targetIndex = 0;
    }


    public void Enter()
    {

    }

    public void Exit()
    {

    }

    public void Update()
    {
        Vector2 dir = (_target.position - _monster.transform.position).normalized;
        _monster.transform.Translate(dir * Time.deltaTime * _monster.Speed);

        if(_target.position == _monster.transform.position)
        {
            _targetIndex++;
            int index =  _targetIndex % _monster.PatrolPoint.Count;
            _target = _monster.PatrolPoint[index];
        }

    }
}