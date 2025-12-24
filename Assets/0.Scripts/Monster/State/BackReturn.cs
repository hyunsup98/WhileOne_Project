using System.Collections.Generic;
using UnityEngine;

public class BackReturn : IState
{
    private MonsterPresenter _monster;
    private Transform _myTransform;
    private float _speed;
    private Astar _astar;
    private int _backReturnIndex;
    private List<Transform> _patrolTarget;          // 되돌아갈 순찰 포인트
    private List<Vector2> _backReturnPoint;         // 탐색한 경로를 저장

    public BackReturn(MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = monster.View.MyTransform;
        _speed = monster.Model.MoveSpeed;
        _astar = monster.Model.MobAstar;
        _patrolTarget = _monster.Model.PatrolTarget;
    }



    public void Enter()
    {
        Vector2 start = _myTransform.position;
        Vector2 target = _patrolTarget[0].position;
        _backReturnPoint = _astar.Pathfinder(start, target);
    }

    public void Exit()
    {
        _backReturnIndex = 0;
    }

    public void Update()
    {
        OnBackReturn();

        UpdateLOS();
    }

    // 탐색 후에 플레이어를 찾지 못하면 순찰 포인트로 되돌아가는 메서드
    private void OnBackReturn()
    {
        Vector2 target = _backReturnPoint[_backReturnIndex];

        _monster.View.OnMove(target, _speed);

        Vector2 monsterPos = _myTransform.position;
        if (monsterPos == _backReturnPoint[_backReturnPoint.Count - 1])
            _monster.Model.SetState(MonsterState.Patrol);

        //target 도달시, 다음 포인트 인덱스로 변경
        if ((Vector2)_myTransform.position == target)
            _backReturnIndex++;
    }

    private void UpdateLOS()
    {
        if (_monster.OnSight())
            _monster.Model.SetState(MonsterState.Chase);
    }

}
