using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 어택 레인지 리펙토링 필요
public class Chase : IState
{
    private Monster _monster;
    private float _speed;
    private float _attRange;
    private Astar _astar;
    private Transform _target;
    private List<Vector2> _chasePoint;
    private int _chaseIndex;
    private float _sight;              // LOS판정을 위한 시야 거리
    private Coroutine _pathfinder;          // 경로재탐색 제어를 위한 코루틴

    public Chase(Monster monster)
    {
        _monster = monster;
        _speed = monster.Model.MoveSpeed;
        _sight = monster.Model.MoveSpeed;

        //리펙토링 진행해야 함
        _attRange = monster.AttRange;
        _astar = monster.Model.MobAstar;
    }

    public void Enter() 
    {
        _target = _monster.Model.Target;
        //_pathfinder = _monster.StartCoroutine(UpdatePathfinder());
    }

    public void Exit() 
    {
        //_monster.StopCoroutine(_pathfinder);
        //_pathfinder = null;
    }

    public void Update()
    {
        _monster.OnTurn(_target.position);

        OnChase();

        UpdateLOS(_target.position);
    }


    private void OnChase()
    {
        Vector3 dir = _target.position - _monster.transform.position;
        if (Vector3.SqrMagnitude(dir) <= _attRange)
        {
            _monster.SetState(MonsterState.Attack);
            Debug.LogWarning("<color=red>공격실행</color>");
            return;
        }

        _monster.transform.Translate(dir.normalized * Time.deltaTime * _speed);

        //Vector2 target = _chasePoint[_chaseIndex];
        ////// 목표 지점 도달시, 바로 플레이어의 다음 위치를 경로탐색
        ////if(_chaseIndex == _chasePoint.Count - 1)
        ////{
        ////    Debug.LogWarning("<color=yellow>스탑 코루틴</color>");
        ////    _monster.StopCoroutine(_pathfinder);
        ////    Debug.LogWarning("<color=yellow>코루틴 재시작</color>");
        ////    _pathfinder = _monster.StartCoroutine(UpdatePathfinder());
        ////}

        //_monster.OnMove(target, _speed);

        ////target 도달시, 다음 포인트 인덱스로 변경
        //if ((Vector2)_monster.transform.position == target)
        //    _chaseIndex++;
    }

    // 경로 탐색 업데이트
    private IEnumerator UpdatePathfinder()
    {
        while (true)
        {
            Debug.Log("추적 경로 탐색");
            Vector2 start = _monster.transform.position;
            Vector2 target = _target.position;

            // 1초 단위로 경로를 재탐색
            _chasePoint = _astar.Pathfinder(start, target);
            _chaseIndex = 1;

            yield return CoroutineManager.waitForSeconds(5f);
        }
    }

    // LOS 판정
    private void UpdateLOS(Vector2 target)
    {
        RaycastHit2D hit = _monster.OnLOS(target);

        int playerLayer = LayerMask.NameToLayer("Player");
        if (hit.collider != null && hit.collider.gameObject.layer != playerLayer)
        {
            Debug.LogWarning("<color=blue>탐색모드 실행</color>" + _pathfinder);
            _monster.SetState(MonsterState.Search);
        }


        // 레이 시각표현용 임시 코드
        Vector2 start = (Vector2)_monster.transform.position;
        Vector2 dir = target - start;
        Debug.DrawRay(start, dir.normalized * _sight, Color.yellow);
    }
}
