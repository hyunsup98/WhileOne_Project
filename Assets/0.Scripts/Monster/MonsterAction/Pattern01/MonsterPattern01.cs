using System.Collections;
using UnityEngine;

public class MonsterPattern01 : MonsterPattern
{
    private float _rushSpeed;
    private float _rushDistance;

    private Vector2 _target;
    private Transform _myTransform;

    public MonsterPattern01(Pattern01SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _myTransform = monster.View.MyTransform;
        _damage = actionData.ActionDamage;
        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;
        _maxCoolTime = actionData.ActionCoolTime;

        _pathPreview = actionData.PathPreview;
        _hitDecision = actionData.HitDecision;
        IsActionable = true;

        _rushDistance = actionData.RushDistance;
        _rushSpeed = actionData.RushSpeed;
    }


    public override void StartAction()
    {
        IsAction = true;

        // 스킬 이펙트 오브젝트 생성
        Vector3 target = _monster.Model.ChaseTarget.position;
        _target = (target - _myTransform.position).normalized;

        OnCreatedEffect(_myTransform.position);
    }

    public override void OnAction()
    {
        _timer += Time.deltaTime;

        if (_isDelay)
            return;

        // 타이머로 돌진 종료 판정
        if (_timer >= (_rushDistance / _rushSpeed) + _beforeDelay)
        {
            _isDelay = true;
            _monster.StartCoroutine(OnDelay(() => IsAction = false, _afterDelay));
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
        Init();
        _monster.StartCoroutine(StartCool());
    }

    // 시전시간 이후 이펙트 생성
    private void OnCreatedEffect(Vector2 createdPos)
    {
        _isDelay = true;
        _ani.OnPlayAni("Idle");
        _monster.StartCoroutine(OnDelay(() => _ani.OnPlayAni("Pattern01"), _beforeDelay));
        _monster.StartCoroutine(OnDelay(() => CreatedEffect(createdPos), _beforeDelay));
        _monster.StartCoroutine(OnDelay(() => _isDelay = false, _beforeDelay));
    }
}