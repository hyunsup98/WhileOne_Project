using System;
using System.Collections;
using UnityEngine;

public abstract class MonsterPattern
{
    protected MonsterPresenter _monster;
    protected IAnimationable _ani;
    protected float _damage;
    protected float _chargeDelay;
    protected float _coolTime;
    protected GameObject _hitDecision;
    protected GameObject _pathPreview;

    protected GameObject _createdHitDecition;
    protected AttackEffect _attackEffect;
    protected bool _isDelay;
    protected float _timer;


    // 몬스터 행동을 수행여부 판단
    public bool IsAction { get; protected set; }
    public bool IsActionable { get; protected set; }     // 제거해도 될 것 같음

    public event Action<string> OnAniTrigger;

    // 몬스터 행동 시작시, 1번 호출
    public abstract void StartAction();

    // 몬스터 행동 진행 중
    public abstract void OnAction();

    // 몬스터 행동 종료시, 1번 호출
    public abstract void EndAction();


    protected void CreatedEffect(Vector2 createdPos)
    {
        // 스킬 이펙트 오브젝트 생성
        _createdHitDecition = GameObject.Instantiate
            (
            _hitDecision,
            createdPos,
            Quaternion.identity,
            _monster.View.MyTransform
            );

        _attackEffect = _createdHitDecition.GetComponent<AttackEffect>();
        _attackEffect.OnAttack += OnCrash;
    }

    // 플레이어 타격시 데미지 계산
    protected void OnCrash(Collider2D collision)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        if (collision.gameObject.layer == playerLayer)
        {
            Player player = collision.GetComponent<Player>();
            player.GetDamage.TakenDamage(_damage, _monster.View.MyTransform.position);
        }
    }

    // 쿨타임 시간
    protected IEnumerator OnCool()
    {
        IsActionable = false;
        yield return CoroutineManager.waitForSecondsRealtime(_coolTime);
        IsActionable = true;
    }

    protected void OnDisEffect()
    {
        if(_attackEffect != null)
            _attackEffect.Init();
        GameObject.Destroy(_createdHitDecition);
        //OnCool();
    }


    // 시전 시간(스킬과 애니메이션 사이의 aniDelayTime은 하드코딩)
    public virtual IEnumerator OnChargeDelay(Vector2 createdPos, string aniName, float aniDelayTime = 0f)
    {
        _isDelay = true;

        OnAniTrigger?.Invoke("Idle");
        yield return CoroutineManager.waitForSecondsRealtime(_chargeDelay);
        OnAniTrigger?.Invoke(aniName);

        yield return CoroutineManager.waitForSecondsRealtime(aniDelayTime);

        // 시전 준비 시간이 끝나고 액션 이펙트를 생성
        CreatedEffect(createdPos);

        _isDelay = false;
    }
}
