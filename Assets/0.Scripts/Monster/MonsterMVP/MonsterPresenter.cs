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
    public GameObject AttackEffect;
    public float Att { get; private set; } = 10f;
    public float AttRange { get; private set; } = 5f;
    public IAttack Attack { get; private set; }

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


    // 업데이트로 전달하는 내용
    public void Tick()
    {
        _isHit = View.GetPlayingAni().IsName("Hurt");


        if (!_isHit && !_isDeath)
            Model.CurrentState.Update();
    }


    public RaycastHit2D OnLOS(Vector2 target, float sight, int layerMask)
    {
        return View.OnLOS(target, sight, layerMask);
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
