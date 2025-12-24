using System.Collections;
using UnityEngine;

public class MonsterPattern01 : MonsterPattern
{
    private Transform _myTransform;
    private Vector2 _target;
    private float _rushSpeed;
    private float _rushDistance;
    private float _timer;


    public MonsterPattern01(Pattern01SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = monster.View.transform;
        _damage = actionData.ActionDamage;
        _rushDistance = actionData.RushDistance;
        _rushSpeed = actionData.RushSpeed;
        _chargeDelay = actionData.ChargeDelay;
        _pathPreview = actionData.PathPreview;
        _hitDecision = actionData.HitDecision;
        IsActionable = true;
    }


    public override void StartAction()
    {
        IsAction = true;

        // 스킬 이펙트 오브젝트 생성
        Vector3 target = _monster.Model.ChaseTarget.position;
        _target = ( target - _myTransform.position ).normalized;
        _monster.StartCoroutine(OnChargeDelay(_myTransform.position, "Pattern01"));
    }
    
    public override void OnAction()
    {
        if (_isDelay)
            return;

        // 타이머로 돌진 종료 판정
        _timer += Time.unscaledDeltaTime;
        if (_timer >= ( _rushDistance / _rushSpeed ))
        {
            IsAction = false;
            return;
        }

        // 벽에 부딪히면 몬스터 위치 이동은 없음(애니메이션은 출력)
        Vector2 start = _myTransform.position;
        int layerMask = LayerMask.GetMask("Wall");
        RaycastHit2D hit = Physics2D.Raycast(start, _target, 0.5f, layerMask);
        if (hit.collider != null)
            return;

        // 몬스터 돌진 이동
        _myTransform.Translate(_target * Time.deltaTime * _rushSpeed);
    }

    public override void EndAction()
    {
        _timer = 0;
        OnDisEffect();
    }
}
