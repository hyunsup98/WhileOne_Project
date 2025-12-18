using System.Collections;
using UnityEngine;


public class Search : IMonsterState
{
    private Monster _monster;
    private float _visibility;
    private Vector2 _targetPos;
    private float _maxAngle = 20;
    private int _searchTime = 3;

    public Search(Monster monster)
    {
        _monster = monster;
        _visibility = monster.Visibility;
    }


    public void Enter()
    {
        _targetPos = _monster.Target.position;
        _monster.StartCoroutine(UpdateLOS(_monster.transform.position, _targetPos));
    }

    public void Exit()
    {
        _searchTime = 3;
    }

    public void Update()
    {
    }



    private IEnumerator UpdateLOS(Vector2 start, Vector2 target)
    {
        while (_searchTime > 0)
        {
            Vector2 dirNomlized = (target - start).normalized;

            for (float angle = -_maxAngle; angle <= _maxAngle; angle++)
            {

                Vector2 dir = Quaternion.Euler(0, 0, angle) * dirNomlized;

                RaycastHit2D hit = Physics2D.Raycast(start, dir, _visibility);

                Debug.Log("Å½»öLOS: " + hit);

                if (hit.transform != null && hit.transform.CompareTag("Player"))
                {
                    Debug.Log("ÇÃ·¹ÀÌ¾î Å½»ö");
                    _monster.SetTarget(hit.transform);
                    _monster.SetState(MonsterState.Chase);
                    break;
                }
            }

            yield return CoroutineManager.waitForSeconds(1f);
            _searchTime--;

        }

        if (_searchTime <= 0)
            _monster.SetState(MonsterState.BackReturn);
    }

    
}
