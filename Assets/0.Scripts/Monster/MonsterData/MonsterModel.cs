using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MonsterModel
{
    private float _hp;
    private float _moveSpeed;
    private float _attackBoost;
    private float _speedBoost;

    // SO데이터에서 값을 받아오는 필드
    public int MonsterID { get; private set; }
    public string Name { get; private set; }
    public int Tier { get; private set; }
    public float Hp
    {
        get => _hp;
        private set => _hp = Mathf.Max(0, value);
    }
    public float MoveSpeed
    {
        get => _moveSpeed + _speedBoost;
        private set => _moveSpeed = Mathf.Max(0.1f , value + _speedBoost); 
    }

    public float AttackBoost { get => _attackBoost; }

    public float Sight { get; private set; }
    public float SightAngle { get; private set; }
    public float ActionDistance;
    public float SearchTime { get; private set; }
    public float PatrolRange { get; private set; } // 현재 미할당
    public Transform PatrolPoint { get; private set; }  // 순찰 할 지점 저장
    public Dictionary<ActionID, MonsterPattern> ActionDict { get; private set; } // 몬스터 행동 목록


    // 내부 로직에서 사용되는 필드
    public List<Vector2> PatrolPath { get; private set; }      // 순찰 경로 저장
    public Astar MobAstar { get; private set; }
    public Transform ChaseTarget { get; private set; }
    public IState CurrentState { get; private set; }
    public Dictionary<MonsterState, IState> StateList { get; private set; }
    public Tilemap GroundTilemap { get; private set; }
    public Tilemap WallTilemap { get; private set; }


    public MonsterModel(MonsterData monsterData, Transform transform, Tilemap ground, Tilemap wall)
    {
        _hp = monsterData.Hp;
        MonsterID = monsterData.MonsterID;
        Name = monsterData.Name;
        Tier = monsterData.Tier;
        MoveSpeed = monsterData.MoveSpeed;
        Sight = monsterData.Sight;
        SightAngle = Mathf.Cos(monsterData.SightAngle * Mathf.Deg2Rad);
        ActionDistance = monsterData.ActionDistance;
        SearchTime = monsterData.SearchTime;
        PatrolRange = monsterData.PatrolRange;
        PatrolPoint = transform;

        GroundTilemap = ground;
        WallTilemap = wall;

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

    public void SetAstar(Tilemap tilemap) => MobAstar = new Astar(tilemap);

    public void SetTilemap(Tilemap tilemap)
    {
        if(tilemap.transform.CompareTag("Wall"))
            WallTilemap = tilemap;

        else if(tilemap.transform.CompareTag("Ground"))
            GroundTilemap = tilemap;
    }

    public void SetPatrolPath(Transform myTransform) => 
        PatrolPath = MobAstar.Pathfinder(
            myTransform.position,
            PatrolPoint.position
            );

    public void SetBoost(float attackBoost = 0, float speedBoost = 0)
    {
        _attackBoost += attackBoost;
        _speedBoost += speedBoost;
    }

    public void SetState(MonsterPresenter presenter)
    {
        // 상태 패턴 세팅
        StateList = new Dictionary<MonsterState, IState>();
        StateList.Add(MonsterState.Patrol, new Patrol(presenter));
        StateList.Add(MonsterState.Chase, new Chase(presenter));
        StateList.Add(MonsterState.Search, new Search(presenter));
        StateList.Add(MonsterState.Action, new MonsterAction(presenter));
        StateList.Add(MonsterState.Stun, new Stun(presenter));
        CurrentState = StateList[MonsterState.Patrol];
    }
}

public enum MonsterState
{
    Patrol, Chase, Search, Action, Stun
}

public enum ActionID
{
    one = 01, two, three, four, five, six
}