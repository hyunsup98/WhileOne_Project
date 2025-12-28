using System;
using System.Collections;
using UnityEngine;

public abstract class MonsterPattern
{
    protected MonsterPresenter _monster;
    protected IAnimationable _ani;
    protected float _damage;
    protected float _beforeDelay;
    protected float _afterDelay;
    protected float _maxCoolTime;
    protected GameObject _hitDecision;
    protected GameObject _pathPreview;

    //protected GameObject _createdHitDecition;
    protected AttackEffect _actionEffect;
    protected float _coolTime;
    protected bool _isDelay;
    protected float _timer;


    // 몬스터 행동을 수행여부 판단
    public bool IsAction { get; protected set; }
    public bool IsActionable { get; protected set; } = true;   // 제거해도 될 것 같음


    // 몬스터 행동 시작시, 1번 호출
    public abstract void StartAction();

    // 몬스터 행동 진행 중
    public abstract void OnAction();

    // 몬스터 행동 종료시, 1번 호출
    public abstract void EndAction();


    protected void CreatedEffect(Vector2 createdPos)
    {
        if (_hitDecision == null)
        {
            Debug.LogWarning("이펙트 프리팹을 SO에 세팅하세요");
            return;
        }

        if (!IsAction)
            return;

        // 스킬 이펙트 오브젝트 생성
        _actionEffect = GameObject.Instantiate
            (
            _hitDecision,
            createdPos,
            Quaternion.identity,
            _monster.View.MyTransform
            ).GetComponent<AttackEffect>();

        if (_actionEffect == null)
        {
            Debug.LogWarning(_hitDecision.name + "의 액션 이펙트 스크립트 없음");
            return;
        }

        _actionEffect.SetDamage(_damage);
    }

    // 쿨타임 시간
    protected IEnumerator StartCool()
    {
        IsActionable = false;
        _coolTime = _maxCoolTime;
        while (_coolTime > 0)
        {
            //Debug.Log($"<color=green>{_monster.Model.Name} 쿨타임</color>" + _coolTime);
            _coolTime -= 0.1f;
            yield return CoroutineManager.waitForSeconds(0.1f);
        }
        _coolTime = 0;
        IsActionable = true;
    }

    // 메서드 호출을 지연
    protected IEnumerator OnDelay(Action action, float delayTime)
    {
        yield return CoroutineManager.waitForSeconds(delayTime);
        action?.Invoke();
    }

    protected void Init()
    {
        _isDelay = false;
        IsAction = false;
        _timer = 0;

        if(_actionEffect != null)
            GameObject.Destroy(_actionEffect.gameObject);
    }
}
