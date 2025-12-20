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
    public List<Transform> PatrolTarget {  get; set; }  // 순찰 할 지점 저장
    public List<Vector2> PatrolPoint { get; set; }      // 순찰 경로 저장
    public Astar MobAstar { get; set; }
    public Transform Target { get; set; }
    public IState CurrentState { get; set; }
    public Dictionary<MonsterState, IState> StateList { get; set; }

    private MonsterViewMVP view;


    public MonsterModelMVP(MonsterDataSO monsterData, List<Transform> patrolTarget, MonsterViewMVP view)
    {
        _hp = monsterData.Hp;
        MonsterID = monsterData.MonsterID;
        Name = monsterData.Name;
        Tier = monsterData.Tier;
        MoveSpeed = monsterData.MoveSpeed;
        Sight = monsterData.Sight;
        ActionList = monsterData.ActionList;
        PatrolTarget = patrolTarget;
        this.view = view;
    }


    public void SetState(MonsterState state)
    {
        CurrentState?.Exit();
        CurrentState = StateList[state];
        CurrentState.Enter();
    }

    public void SetTarget(Transform target) => Target = target;

    public void TakeDamage(float damage)
    {
        view.OnHurtAni();
        Hp -= damage;
        Debug.Log("몬스터HP" + Hp);
        
        if (Hp <= 0f)
        {
            view.OnDead();
            //view.OnDeathAni();        //죽는 애니메이션 재생
        }
    }
}

public enum MonsterState
{
    Patrol, Chase, Search, BackReturn, Attack
}