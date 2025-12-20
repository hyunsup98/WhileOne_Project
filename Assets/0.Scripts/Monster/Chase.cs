using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 어택 레인지 리펙토링 필요
public class Chase : IState
{
    private MonsterPresenterMVP _monster;
    private Transform _myTransform;
    private float _speed;
    private float _attRange;
    private Astar _astar;
    private Transform _target;
    private List<Vector2> _chasePoint;
    private int _chaseIndex;
    private float _sight;              // LOS판정을 위한 시야 거리
    private Coroutine _pathfinder;          // 경로재탐색 제어를 위한 코루틴

    public Chase(MonsterPresenterMVP monster)
    {
        _monster = monster;
        _myTransform = monster.View.transform;
        _speed = monster.Model.MoveSpeed;
        _sight = monster.Model.MoveSpeed;

        //리펙토링 진행해야 함
        _attRange = monster.AttRange;
        _astar = monster.Model.MobAstar;
    }

    public void Enter() 
    {
        _target = _monster.Model.Target;
    }

    public void Exit() 
    {
    }

    public void Update()
    {
        _monster.View.OnTurn(_target.position);

        OnChase();

        UpdateLOS(_target.position);
    }


    private void OnChase()
    {
        Vector3 dir = _target.position - _myTransform.position;
        if (Vector3.SqrMagnitude(dir) <= _attRange)
        {
            _monster.Model.SetState(MonsterState.Attack);
            Debug.LogWarning("<color=red>공격실행</color>");
            return;
        }

        _myTransform.Translate(dir.normalized * Time.deltaTime * _speed);
    }

    // LOS 판정
    private void UpdateLOS(Vector2 target)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        RaycastHit2D hit = _monster.OnLOS(target, _sight, playerLayer);

        if (hit.collider != null && hit.collider.gameObject.layer != playerLayer)
        {
            Debug.LogWarning("<color=blue>탐색모드 실행</color>" + _pathfinder);
            _monster.Model.SetState(MonsterState.Search);
        }


        // 레이 시각표현용 임시 코드
        Vector2 start = (Vector2)_myTransform.position;
        Vector2 dir = target - start;
        Debug.DrawRay(start, dir.normalized * _sight, Color.yellow);
    }
}
