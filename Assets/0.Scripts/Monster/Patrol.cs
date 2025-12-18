using System.Collections.Generic;
using UnityEngine;

public class Patrol : IState
{
    private Monster _monster;
    private float _speed;
    private float _sight;
    private List<Vector2> _patrolPoint;
    private int _patrolIndex;
    private bool _isRise = true;        // 순찰인덱스 방향 true: 0 -> 끝 , false: 끝 -> 0

    public Patrol(Monster monster)
    {
        _monster = monster;
        _speed = monster.Speed;
        _patrolPoint = monster.PatrolPoint;
        _sight = monster.Sight;
    }


    public void Enter() 
    {
        _patrolIndex = 0;
        _isRise = true;
    }

    public void Exit() { }

    public void Update()
    {
        OnPatrol();
    }


    private void OnPatrol()
    {
        Vector2 target = _patrolPoint[_patrolIndex];

        _monster.OnMove(target, _speed);

        // 최종 포인트 도달 시, 역순 <-> 정순으로 순회 방향 전환
        if (_patrolIndex == 0)
            _isRise = true;
        if(_patrolIndex == _patrolPoint.Count - 1)
            _isRise = false;


        //target 도달시, 다음 포인트 인덱스로 변경
        if ((Vector2)_monster.transform.position == target)
        {
            if (_isRise)
                _patrolIndex++;

            if (!_isRise)
                _patrolIndex--;
        }

        UpdateLOS(_monster.transform.position, target);
    }

    // LOS 판정
    private void UpdateLOS(Vector2 start, Vector2 target)
    {
        Vector2 dir = target - start;
        RaycastHit2D hit = Physics2D.Raycast(start, dir, _sight);

        Debug.DrawRay(start, dir.normalized * _sight);

        if (hit.transform != null && hit.transform.CompareTag("Player"))
        {
            _monster.SetTarget(hit.transform);
            _monster.SetState(MonsterState.Chase);
        }

        
    }

}