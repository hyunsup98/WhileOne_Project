using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public List<int> ActionList { get; set; }
    public List<Transform> PatrolTarget { get; set; }

    public List<Vector2> PatrolPoint { get; set; }      // 순찰 경로 저장
    public Astar MobAstar { get; set; }
    public Transform Target { get; set; }
    public IState CurrentState { get; set; }
    public Dictionary<MonsterState, IState> MonsterState {  get; set; }


    public MonsterModel(MonsterDataSO monsterData)
    {
        _hp = monsterData.Hp;
        MonsterID = monsterData.MonsterID;
        Name = monsterData.Name;
        Tier = monsterData.Tier;
        MoveSpeed = monsterData.MoveSpeed;
        Sight = monsterData.Sight;
        ActionList = monsterData.ActionList;
        PatrolTarget = monsterData.PatrolTarget;
    }
}
