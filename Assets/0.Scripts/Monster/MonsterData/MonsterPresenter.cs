using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class MonsterPresenter
{

    public MonsterModel Model { get; private set; }  // 현재 몬스터 데이터 보관한 Model

    public MonsterView View { get; private set; }

    private bool _isHit;
    private bool _isDeath;

    // 추후 지워야 할 목록
    public float ActionTrigger { get; private set; } = 5f;

    // 생성자
    public MonsterPresenter
        (
        MonsterDataSO monsterData,
        MonsterView monsterView,
        Tilemap wallTilmap,
        List<Transform> patrolTarget
        )
    {
        View = monsterView;
        Model = new MonsterModel(monsterData, patrolTarget);
        

        // 경로 탐색으로 순찰 포인트 초기화
        Model.MobAstar = new Astar(wallTilmap);
        Model.PatrolPoint = Model.MobAstar.Pathfinder
            (
            patrolTarget[0].position,
            patrolTarget[1].position
            );


        // 몬스터 행동 매칭
        foreach (var action in monsterData.ActionList)
        {
            Model.ActionDict.Add(
                (ActionID)(action.MonsterActionID % 10),
                ActionFactory.Create(action, this)
                );

            Debug.Log("액션ID" + (ActionID)(action.MonsterActionID % 10));
            Debug.Log("어떤 액션인지" + Model.ActionDict[(ActionID)(action.MonsterActionID % 10)]);
        }


        // 상태 패턴 세팅
        Model.StateList = new Dictionary<MonsterState, IState>();
        Model.StateList.Add(MonsterState.Patrol, new Patrol(this));
        Model.StateList.Add(MonsterState.Chase, new Chase(this));
        Model.StateList.Add(MonsterState.Search, new Search(this));
        Model.StateList.Add(MonsterState.BackReturn, new BackReturn(this));
        Model.StateList.Add(MonsterState.Attack, new MonsterAction(this));
        Model.CurrentState = Model.StateList[MonsterState.Patrol];
    }


    public void OnStart()
    {
        Model.SetTarget(GameObject.FindWithTag("Player").transform);
    }


    // 업데이트로 전달하는 내용
    public void Tick()
    {
        _isHit = View.GetPlayingAni().IsName("Hurt");


        if (!_isHit && !_isDeath)
            Model.CurrentState.Update();
    }


    // 몬스터 시야각에 플레이어가 들어왔는지 판단 후 LOS 발사
    public bool OnSight()
    {
        Vector2 dir = Model.ChaseTarget.position - View.transform.position;
        Vector2 frontal = new Vector2(View.transform.localScale.x, 0).normalized;

        //LOS 검사를 위한 테스트용 레이
        Vector2 taget = dir.normalized * Model.Sight;
        Debug.DrawRay(View.transform.position, taget, Color.blue);
        //


        // 몬스터와의 타겟의 거리가 가시 거리에 들어오는지 판단
        if (Vector2.SqrMagnitude(dir) > Model.Sight * Model.Sight)
            return false;

        // 내적으로 시야각에 포착됐는지 판단
        float dot = Vector2.Dot(dir.normalized, frontal);
        if (dot < Model.SightAngle)
            return false;

        // LOS를 발사해 적중한 게 Player라면 true 반환
        int playerLayer = LayerMask.NameToLayer("Player");
        RaycastHit2D hit = View.OnLOS(Model.ChaseTarget.position, Model.Sight);

        if (hit.collider == null || hit.collider.gameObject.layer != playerLayer)
            return false;

        return true;
    }

    public void OnHit(float Damage)
    {
        if(!_isHit)
            _isHit = true;

        View.OnHurtAni();
        Model.TakeDamage(Damage);

        if(Model.Hp <= 0)
            View.StartCoroutine(OnDead());
    }

    // 죽음 애니메이션 호출
    public IEnumerator OnDead()
    {
        _isDeath = true;
        View.OnDeathAni();
        while (View.GetPlayingAni().normalizedTime < 0.5f)
            yield return null;

        yield return CoroutineManager.waitForSeconds(0.5f);
        float destroyTime = View.GetPlayingAni().length;

        View.RequestDestroy(destroyTime + 1f);
    }

    public void StartCoroutine(IEnumerator coroutine) => View.StartCoroutine(coroutine);
}
