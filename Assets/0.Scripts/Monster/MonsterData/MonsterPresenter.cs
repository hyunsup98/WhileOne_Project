using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;



public class MonsterPresenter : IAnimationable
{
    public MonsterModel Model { get; private set; }  // 현재 몬스터 데이터 보관한 Model
    public MonsterView View { get; private set; }

    public bool IsPattern03 { get; private set; }
    public bool IsUlt { get; private set; }        // 궁극기(행동06)를 실행하는 메서드

    private bool _isHit;
    private bool _isDeath;

    public event Action OnDeath;

    // 추후 지워야 할 목록
    public float ActionTrigger { get; private set; } = 5f;

    // 생성자
    public MonsterPresenter
        (
        MonsterData monsterData,
        MonsterView monsterView,
        Tilemap GroundTilmap,
        Tilemap wallTilmap
        )
    {
        View = monsterView;
        Model = new MonsterModel(monsterData, View.MyTransform, GroundTilmap, wallTilmap);


        // 경로 탐색으로 순찰 포인트 초기화
        Model.SetAstar(wallTilmap);
        Model.SetPatrolPath(View.MyTransform);

        Debug.LogWarning("행동매칭");
        // 몬스터 행동 매칭
        foreach (var action in monsterData.ActionList)
        {
            Model.ActionDict.Add(
                (ActionID)(action.MonsterActionID % 10),
                ActionFactory.Create(action, this)
                );
            Debug.LogWarning(Model.Name + $"<color=brown>{action.name}</color>");
        }


        // 상태 패턴 세팅
        Model.SetState(this);
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
        Vector2 dir = Model.ChaseTarget.position - View.MyTransform.position;
        Vector2 frontal = new Vector2(View.MyTransform.localScale.x, 0).normalized;

        //LOS 검사를 위한 테스트용 레이
        Vector2 taget = dir.normalized * Model.Sight;
        Debug.DrawRay(View.MyTransform.position, taget, Color.blue);
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
        if (Model.CurrentState != Model.StateList[MonsterState.Action])
            View.OnPlayAni("Hurt");

        StartCoroutine(View.OnHitBlink(_isDeath));
        if (Model.ActionDict.TryGetValue(ActionID.three, out var action))
        {
            IsPattern03 = true;
            Model.SetState(MonsterState.Action);
            return;
        }


        Model.TakeDamage(Damage);

        if (Model.Hp <= 0)
            View.StartCoroutine(OnDead());


        if (!_isHit)
            _isHit = true;
    }

    // 죽음 애니메이션 호출
    public IEnumerator OnDead()
    {
        _isDeath = true;
        View.OnDeathSound();
        View.SetCollider(false);
        View.OnPlayAni("Death");
        while (View.GetPlayingAni().normalizedTime < 0.5f)
            yield return null;

        yield return CoroutineManager.waitForSeconds(0.5f);
        float destroyTime = View.GetPlayingAni().length;
        Debug.Log("파괴합니다.");
        View.RequestDestroy(destroyTime + 1f);
        View.OnDead();
    }

    public void StartCoroutine(IEnumerator coroutine) => View.StartCoroutine(coroutine);

    public void OnStun() => Model.SetState(MonsterState.Stun);
    public void SetIsUlt(bool isUlt) => IsUlt = isUlt;
    public void setIsPattern03(bool isPattern03) => IsPattern03 = isPattern03;

    public void OnPlayAni(string animationName) => View.OnPlayAni(animationName);
    public void OnStopAni(string animationName) => View.OnStopAni(animationName);

    
}