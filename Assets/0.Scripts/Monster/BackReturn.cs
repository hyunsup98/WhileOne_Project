using System.Collections.Generic;
using UnityEngine;

public class BackReturn : IMonsterState
{
    private Monster _monster;
    private float _speed;
    private Astar _astar;
    private int _backReturnIndex;
    private List<Transform> _patrolTarget;
    private List<Vector2> _backReturnPoint;

    public BackReturn(Monster monster)
    {
        _monster = monster;
        _speed = monster.Speed;
        _astar = monster.MobAstar;
        _patrolTarget = _monster.PatrolTarget;
    }



    public void Enter()
    {
        Vector2 start = _monster.transform.position;
        Vector2 target = _patrolTarget[Random.Range(0, 2)].position;
        _backReturnPoint = _astar.Pathfinder(start, target);
    }

    public void Exit()
    {
        _backReturnIndex = 0;
    }

    public void Update()
    {
        OnBackReturn();
    }

    private void OnBackReturn()
    {
        Vector2 target = _backReturnPoint[_backReturnIndex];

        Move(target);

        Vector2 monsterPos = _monster.transform.position;
        if (monsterPos == _backReturnPoint[_backReturnPoint.Count - 1])
            _monster.SetState(MonsterState.Patrol);

    }

    private void Move(Vector2 target)
    {
        _monster.transform.position = Vector2.MoveTowards
            (
            _monster.transform.position,
            target,
            _speed * Time.deltaTime
            );

        //target 도달시, 다음 포인트 인덱스로 변경
        if ((Vector2)_monster.transform.position == target)
            _backReturnIndex++;

    }
}
