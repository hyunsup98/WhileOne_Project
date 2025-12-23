using System.Collections.Generic;
using UnityEngine;

public class Patrol : IState
{
    private MonsterPresenter _monster;
    private Transform _myTransform;
    private float _speed;
    private List<Vector2> _patrolPoint;
    private int _patrolIndex;
    private bool _isRise = true;        // 순찰인덱스 방향 true: 0 -> 끝 , false: 끝 -> 0

    public Patrol(MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = _monster.View.transform;
        _speed = monster.Model.MoveSpeed;
        _patrolPoint = monster.Model.PatrolPoint;
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

        _monster.View.OnMove(target, _speed);

        // 최종 포인트 도달 시, 역순 <-> 정순으로 순회 방향 전환
        if (_patrolIndex == 0)
            _isRise = true;
        if(_patrolIndex == _patrolPoint.Count - 1)
            _isRise = false;


        //target 도달시, 다음 포인트 인덱스로 변경
        if ((Vector2)_myTransform.position == target)
        {
            if (_isRise)
                _patrolIndex++;

            if (!_isRise)
                _patrolIndex--;
        }

        UpdateLOS();
    }

    // LOS 판정
    private void UpdateLOS()
    {
        if (_monster.OnSight())
            _monster.Model.SetState(MonsterState.Chase);
    }

}