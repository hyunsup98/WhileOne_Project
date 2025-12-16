using System.Collections.Generic;
using UnityEngine;

public class Patrol : IMonsterState
{
    private Monster _monster;
    private float _speed;
    private List<Vector2> _patrolPoint;
    private Astar _astar;
    private int _patrolIndex;
    private bool _isRise = true;

    public Patrol(Monster monster)
    {
        _monster = monster;
        _speed = monster.Speed;
        _patrolPoint = monster.PatrolPoint;
        _astar = monster.MobAstar;
        
    }


    public void Enter() { }

    public void Exit() { }

    public void Update()
    {
        OnPatrol();
    }

    public void OnPatrol()
    {
        Vector2 target = _patrolPoint[_patrolIndex];

        Move(target);


        if ((Vector2)_monster.transform.position == target)
        {
            if (_isRise)
                _patrolIndex++;
            if (!_isRise)
                _patrolIndex--;
        }

        if (_patrolIndex == 0)
            _isRise = true;
        if(_patrolIndex == _patrolPoint.Count - 1)
            _isRise = false;
    }

    private void Move(Vector2 target)
    {
        _monster.transform.position = Vector2.MoveTowards
            (
            _monster.transform.position,
            target,
            _speed * Time.deltaTime
            );
    }
}