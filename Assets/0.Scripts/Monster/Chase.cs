using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : IState
{
    private Monster _monster;
    private float _speed;
    private float _attRange;
    private Astar _astar;
    private Transform _target;
    private List<Vector2> _chasePoint;
    private int _chaseIndex;
    private float _visibility;              // LOS판정을 위한 시야 거리
    private Coroutine _pathfinder;          // 경로재탐색 제어를 위한 코루틴

    public Chase(Monster monster)
    {
        _monster = monster;
        _speed = monster.Speed;
        _visibility = monster.Visibility;
        _attRange = monster.AttRange;
        _astar = monster.MobAstar;
    }

    public void Enter() 
    {
        _target = _monster.Target;
        _pathfinder = _monster.StartCoroutine(UpdatePathfinder());

    }

    public void Exit() 
    {
        _monster.StopCoroutine(_pathfinder);
    }

    public void Update()
    {
        OnChase();

        UpdateLOS(_monster.transform.position, _target.position);
    }


    private void OnChase()
    {
        Vector2 chasePos = _chasePoint[_chaseIndex];

        Vector3 dir = _target.position - _monster.transform.position;
        if (Vector3.SqrMagnitude(dir) <= _attRange)
        {
            Debug.Log("공격 실행");
            return;
        }

        Move(chasePos);
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
            _chaseIndex++;

    }

    // 경로 탐색 업데이트
    private IEnumerator UpdatePathfinder()
    {
        while (true)
        {
            Debug.Log("경로 탐색");
            Vector2 start = _monster.transform.position;
            Vector2 target = _target.position;

            // 1초 단위로 경로를 재탐색
            _chasePoint = _astar.Pathfinder(start, target);
            _chaseIndex = 1;

            yield return CoroutineManager.waitForSeconds(1f);
        }

    }

    // LOS 판정
    private void UpdateLOS(Vector2 start, Vector2 target)
    {
        Vector2 dir = target - start;
        RaycastHit2D hit = Physics2D.Raycast(start, dir, _visibility);

        Debug.Log("추적LOS: " + hit);

        if (hit.transform == null)
            _monster.SetState(MonsterState.Search);
    }
}
