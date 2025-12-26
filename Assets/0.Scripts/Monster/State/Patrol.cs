using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Patrol : IState
{
    private MonsterPresenter _monster;
    private Transform _myTransform;
    private float _speed;
    private float _patrolRange;
    private List<Vector2> _patrolPath;
    private Tilemap _wall;
    private Tilemap _ground;
    private Astar _mobAster;

    private int _patrolIndex;
    private float _pathFindCool;    // 쿨타임 하드코딩 중

    Vector2 _patrolPoin;

    public Patrol(MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = _monster.View.MyTransform;
        _speed = monster.Model.MoveSpeed;
        _patrolRange = monster.Model.PatrolRange;
        _patrolPath = monster.Model.PatrolPath;
        _wall = monster.Model.WallTilemap;
        _ground = monster.Model.GroundTilemap;
        _mobAster = monster.Model.MobAstar;
        _patrolRange = monster.Model.PatrolRange;
    }


    public void Enter() 
    {
        _patrolIndex = 0;
    }

    public void Exit() { }

    public void Update()
    {
        OnPatrol();
    }


    private void OnPatrol()
    {
        if (_patrolPath == null || _patrolPath.Count <= 1)
        {
            _monster.View.OnIdleAni();
            if (Time.time < _pathFindCool)
                return;
            _pathFindCool = Time.time + 1.5f;

            _patrolPoin = SetPatrolPoint(_patrolRange);
            
            _patrolPath = _mobAster.Pathfinder(_myTransform.position, _patrolPoin);

            if (_patrolPath == null)
                return;

            _patrolIndex = 0;
        }
        _monster.View.OnDisIdleAni();

        Debug.DrawLine(_myTransform.position, _patrolPoin, Color.brown);

        if (_patrolIndex >= _patrolPath.Count)
        {
            _patrolPath = null;
            return;
        }


        Vector2 target = _patrolPath[_patrolIndex];
        _monster.View.OnMove(target, _speed);

        //target 도달시, 다음 포인트 인덱스로 변경
        if (Vector2.SqrMagnitude(target - (Vector2)_myTransform.position) <= 0.5f)
                _patrolIndex++;

        UpdateLOS();
    }

    // LOS 판정
    private void UpdateLOS()
    {
        if (_monster.OnSight())
            _monster.Model.SetState(MonsterState.Chase);
    }

    // 순찰 포인트를 찾는 메서드
    // 나중에 리펙토링 해보자
    private Vector3 SetPatrolPoint(float patrolRange)
    {
        for (int i = 0; i < 10; i++)
        {
            int range = (int)patrolRange;
            int x = (int)_myTransform.position.x + Random.Range(-range, range + 1);
            int y = (int)_myTransform.position.y + Random.Range(-range, range + 1);
            Vector3 pos = new Vector3(x + 0.5f, y + 0.5f);

            if (_ground.HasTile(_ground.WorldToCell(pos)) && !_wall.HasTile(_wall.WorldToCell(pos)))
                return (Vector3)pos;
        }
        return _myTransform.position;
    }


}