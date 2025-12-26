using System.Collections.Generic;
using UnityEngine;



public class MonsterModel
{
    private float _hp;

    public int MonsterID { get; set; }
    public string Name { get; set; }
    public int Tier { get; set; }
    public float Hp
    {
        get => _hp;
        set => _hp = Mathf.Max(0, value);
    }

    public float MoveSpeed { get; set; }
    public float Sight { get; set; }
    public float SightAngle { get; set; }
    public float SearchTime { get; set; }
    public Dictionary<ActionID, MonsterPattern> ActionDict { get; set; } // 몬스터 행동 목록
    public List<Transform> PatrolTarget {  get; set; }  // 순찰 할 지점 저장
    public List<Vector2> PatrolPoint { get; set; }      // 순찰 경로 저장
    public Astar MobAstar { get; set; }
    public Transform ChaseTarget { get; set; }
    public IState CurrentState { get; set; }
    public Dictionary<MonsterState, IState> StateList { get; set; }


    public MonsterModel(MonsterDataSO monsterData, List<Transform> patrolTarget)
    {
        _hp = monsterData.Hp;
        MonsterID = monsterData.MonsterID;
        Name = monsterData.Name;
        Tier = monsterData.Tier;
        MoveSpeed = monsterData.MoveSpeed;
        Sight = monsterData.Sight;
        SightAngle = Mathf.Cos(monsterData.SightAngle * Mathf.Deg2Rad);
        SearchTime = monsterData.SearchTime;
        PatrolTarget = patrolTarget;

        ActionDict = new Dictionary<ActionID, MonsterPattern>();
    }


    public void SetState(MonsterState state)
    {
        CurrentState?.Exit();
        CurrentState = StateList[state];
        CurrentState.Enter();
    }

    // 오브젝트 씬에 배치되는 타이밍에 타겟을 세팅해줘야 함(라이프사이클 참고)
    public void SetTarget(Transform target) => ChaseTarget = target;

    public void TakeDamage(float damage)
    {
        Hp -= damage;
        Debug.Log("몬스터HP" + Hp);
    }
}

public enum MonsterState
{
    Patrol, Chase, Search, BackReturn, Attack
}

public enum ActionID
{
    one = 01, two, three, four, five, six
}