using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;



public class MonsterPresenterMVP
{

    public MonsterModelMVP Model { get; private set; }  // 현재 몬스터 데이터 보관한 Model

    public MonsterViewMVP View { get; private set; }

    // 추후 지워야 할 목록
    public GameObject AttackEffect;
    public float Att { get; private set; } = 10f;
    public float AttRange { get; private set; } = 5f;
    public IAttack Attack { get; private set; }

    public MonsterPresenterMVP
        (
        MonsterDataSO monsterData,
        MonsterViewMVP monsterView,
        Tilemap wallTilmap,
        List<Transform> patrolTarget
        )
    {
        View = monsterView;
        Model = new MonsterModelMVP(monsterData, patrolTarget, View);
        

        // 경로 탐색으로 순찰 포인트 초기화
        Model.MobAstar = new Astar(wallTilmap);
        Model.PatrolPoint = Model.MobAstar.Pathfinder
            (
            patrolTarget[0].position,
            patrolTarget[1].position
            );


        AttackEffect = View.AttackEffect;
        // 공격 세팅
        Attack = new ProtoAttack(this);



        // 상태 패턴 세팅
        Model.StateList = new Dictionary<MonsterState, IState>();
        Model.StateList.Add(MonsterState.Patrol, new Patrol(this));
        Model.StateList.Add(MonsterState.Chase, new Chase(this));
        Model.StateList.Add(MonsterState.Search, new Search(this));
        Model.StateList.Add(MonsterState.Attack, new MonsterAttack(this));
        Model.StateList.Add(MonsterState.BackReturn, new BackReturn(this));
        Model.CurrentState = Model.StateList[MonsterState.Patrol];
    }


    public void Tick()
    {
        Model.CurrentState.Update();
    }


    public RaycastHit2D OnLOS(Vector2 target, float sight, int layerMask)
    {
        return View.OnLOS(target, sight, layerMask);
    }

    public void StartCoroutine(IEnumerator coroutine) => View.StartCoroutine(coroutine);


}
