using System.Collections.Generic;
using UnityEngine;

public class Patrol : IState
{
    private MonsterPresenterMVP _monster;
    private Transform _myTransform;
    private float _speed;
    private float _sight;
    private List<Vector2> _patrolPoint;
    private int _patrolIndex;
    private bool _isRise = true;        // 순찰인덱스 방향 true: 0 -> 끝 , false: 끝 -> 0

    public Patrol(MonsterPresenterMVP monster)
    {
        _monster = monster;
        _myTransform = _monster.View.transform;
        _speed = monster.Model.MoveSpeed;
        _patrolPoint = monster.Model.PatrolPoint;
        _sight = monster.Model.Sight;
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
        Debug.Log("타겟" + target);

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

        UpdateLOS(target);
    }

    // LOS 판정
    private void UpdateLOS(Vector2 target)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        RaycastHit2D hit = _monster.View.OnLOS(target, _sight, playerLayer);


        if (hit.collider != null && hit.collider.gameObject.layer == playerLayer)
        {
            _monster.Model.SetTarget(hit.transform);
            _monster.Model.SetState(MonsterState.Chase);
        }

        // 레이 시각표현용 임시 코드
        Vector2 start = _myTransform.position;
        Vector2 dir = target - start;
        Debug.DrawRay(start, dir.normalized * _sight, Color.aliceBlue);

    }

}