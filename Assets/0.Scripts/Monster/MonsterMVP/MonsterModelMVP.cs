using System.Collections.Generic;
using UnityEngine;

public class MonsterModelMVP
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
    public List<int> ActionList { get; set; }
    public List<Transform> PatrolTarget { get; set; }
    public List<Vector2> PatrolPoint { get; set; }      // 순찰 경로 저장
    public Astar MobAstar { get; set; }
    public Transform Target { get; set; }
    public IState CurrentState { get; set; }
    public Dictionary<MonsterState, IState> MonsterState { get; set; }


    public MonsterModelMVP(MonsterDataSO monsterData, Monster presenter)
    {
        _hp = monsterData.Hp;
        MonsterID = monsterData.MonsterID;
        Name = monsterData.Name;
        Tier = monsterData.Tier;
        MoveSpeed = monsterData.MoveSpeed;
        Sight = monsterData.Sight;
        ActionList = monsterData.ActionList;
        PatrolTarget = monsterData.PatrolTarget;

        // 상태 패턴 세팅
        MonsterState = new Dictionary<MonsterState, IState>();
        //MonsterState.Add(MonsterState.Patrol, new Patrol(presenter));
        //MonsterState.Add(MonsterState.Chase, new Chase(presenter));
        //MonsterState.Add(MonsterState.Search, new Search(presenter));
        //MonsterState.Add(MonsterState.BackReturn, new BackReturn(presenter));
        //MonsterState.Add(MonsterState.Attack, new MonsterAttack(presenter));
        //CurrentState = MonsterState[MonsterState.Patrol];
    }


    public void SetState(MonsterState state)
    {
        Debug.Log("이전 상태: " + CurrentState);

        CurrentState?.Exit();
        CurrentState = MonsterState[state];
        CurrentState.Enter();

        Debug.Log("현재 상태: " + CurrentState);
    }

    public void SetTarget(Transform target) => Target = target;

    public void TakeDamage(float damage)
    {
        //View.OnHurtAni();
        Hp -= damage;
        Debug.Log("몬스터HP" + Hp);

        //if (Hp <= 0f)
        //    View.OnDeathAni();
    }
}
public enum MonsterState
{
    Patrol, Chase, Search, BackReturn, Attack
}