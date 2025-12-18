using System.Collections;
using UnityEngine;


public class Search : IState
{
    private Monster _monster;
    private float _sight;
    private Vector2 _targetPos;
    private float _maxAngle = 20;
    private int _searchTime = 30;
    private Coroutine _updateLOS;

    public Search(Monster monster)
    {
        _monster = monster;
        _sight = monster.MonsterModel.MoveSpeed;
    }


    public void Enter()
    {
        _targetPos = _monster.MonsterModel.Target.position;
        _updateLOS = _monster.StartCoroutine(UpdateLOS(_monster.transform.position, _targetPos));
    }

    public void Exit()
    {
        _monster.StopCoroutine(_updateLOS);
        _searchTime = 30;
    }

    public void Update() { }



    private IEnumerator UpdateLOS(Vector2 start, Vector2 target)
    {
        while (_searchTime > 0)
        {
            Vector2 dirNomlized = (target - start).normalized;

            // 전방에 _maxAngle * 2의 범위 LOS 발사
            for (float angle = -_maxAngle; angle <= _maxAngle; angle++)
            {

                Vector2 dir = Quaternion.Euler(0, 0, angle) * dirNomlized;

                RaycastHit2D hit = Physics2D.Raycast(start, dir, _sight);

                Debug.DrawRay(start, dir * _sight, Color.red);

                if (hit.transform != null && hit.transform.CompareTag("Player"))
                {
                    Debug.Log("플레이어 탐색");
                    _monster.SetTarget(hit.transform);
                    _monster.SetState(MonsterState.Chase);
                    break;
                }
            }

            yield return CoroutineManager.waitForSeconds(0.1f);
            _searchTime--;
        }

        if (_searchTime <= 0)
            _monster.SetState(MonsterState.BackReturn);
    }

    
}
