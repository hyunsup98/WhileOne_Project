using System.Collections;
using System.Threading;
using UnityEngine;


public class Search : IState
{
    private MonsterPresenter _monster;
    private Transform _myTransform;
    private float _sight;
    private Vector2 _targetPos;
    private float _maxAngle = 20;
    private float _searchTime = 3f;
    private float _timer;
    private MonsterView _view;



    public Search(MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = monster.View.transform;
        _sight = monster.Model.Sight;
        _view = monster.View;
    }


    public void Enter()
    {
        _targetPos = _monster.Model.ChaseTarget.position;
        //_monster.StartCoroutine(UpdateLOS(_myTransform.position, _targetPos));
        _view.OnIdleAni();
    }

    public void Exit()
    {
        _timer = 0f;
        _view.OnDisIdleAni();
    }

    public void Update() 
    {
        OnSearch();
    }


    private void OnSearch()
    {

        _timer += Time.unscaledDeltaTime;

        Vector2 dir = _monster.Model.ChaseTarget.position - _myTransform.position;
        Debug.DrawRay(_myTransform.position, dir.normalized * _sight, Color.brown);
        if(_timer < _searchTime)
        {
            if(_monster.OnSight())
                _monster.Model.SetState(MonsterState.Chase);

            Debug.Log("<color=brown>탐색 중</color>" + _timer);
            return;
        }

        _monster.Model.SetState(MonsterState.BackReturn);
    }

    //private IEnumerator UpdateLOS(Vector2 start, Vector2 target)
    //{
    //    Vector2 dirNomlized = (target - start).normalized;

    //    int layerMask = LayerMask.GetMask("Player", "Wall");
    //    int playerLayer = LayerMask.NameToLayer("Player");

    //    while (_searchTime > 0)
    //    {
    //        // 전방에 _maxAngle * 2의 범위 LOS 발사
    //        for (float angle = -_maxAngle; angle <= _maxAngle; angle++)
    //        {
    //            Vector2 dir = Quaternion.Euler(0, 0, angle) * dirNomlized;

    //            RaycastHit2D hit = Physics2D.Raycast(start, dir, _sight, layerMask);

    //            Debug.DrawRay(start, dir * _sight, Color.red);

    //            if (hit.collider == null)
    //                continue;

    //            if (hit.collider.gameObject.layer == playerLayer)
    //            {
    //                _monster.Model.SetTarget(hit.transform);
    //                _monster.Model.SetState(MonsterState.Chase);
    //                yield break;
    //            }
    //        }

    //        yield return CoroutineManager.waitForSeconds(0.1f);
    //        _searchTime--;
    //    }

    //    if (_searchTime <= 0)
    //        _monster.Model.SetState(MonsterState.BackReturn);
    //}
}
