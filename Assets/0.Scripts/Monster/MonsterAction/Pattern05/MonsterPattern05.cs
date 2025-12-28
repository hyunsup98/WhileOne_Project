using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPattern05 : MonsterPattern
{
    private float _hitBoxRadius;
    private float _actionAngle;
    private float _createdEffectDistance;
    private float _createdEffectTime;

    private float _fallingStartTime;
    private float _fallingFrequency;
    private int _fallingCycle;
    private float _fallingHitTiming = 0.9f;
    private float _fallingDestroyTime;
    private GameObject _fallingObject;

    private Transform _myTransform;
    private Transform _target;
    private Queue<GameObject> _fallingObjQue;

    public string AniTrigger { get; private set; }

    public MonsterPattern05(Pattern05SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _damage = actionData.ActionDamage;
        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;
        _maxCoolTime = actionData.ActionCoolTime;
        _hitDecision = actionData.HitDecision;

        _hitBoxRadius = actionData.HitBoxRadius;
        _actionAngle = Mathf.Deg2Rad * actionData.ActionAngle;
        _createdEffectDistance = actionData.CreatedEffectDistance;
        _createdEffectTime = actionData.CreatedEffectTime;

        _fallingStartTime = actionData.FallingStartTime;
        _fallingFrequency = actionData.FallingFrequency;
        _fallingCycle = actionData.FallingCycle;
        _fallingDestroyTime = actionData.FallingDestroyTime;
        _fallingObject = actionData.FallingObject;

        
        _myTransform = monster.View.MyTransform;
        _fallingObjQue = new Queue<GameObject>();
    }

    public override void StartAction()
    {
        _target = _monster.Model.ChaseTarget;
        Vector2 dir =
            (_monster.Model.ChaseTarget.position - _myTransform.position).normalized;
        // 벡터 내적으로 플레이어와의 액션각보다 클 때는 스킬 시전X
        if (!IsCalculateDot(dir))
            return;

        IsAction = true;

        // 콜라이더 크기 조절
        if (_hitDecision.TryGetComponent<CircleCollider2D>(out var collider))
            collider.radius = _hitBoxRadius;
        else
            Debug.LogWarning("이펙트에 Collider없음");

        // 시전 준비 후 이펙트 생성
        Vector2 createdPos = (Vector2)_myTransform.position;
        createdPos.x += (_myTransform.localScale.x *_createdEffectDistance);
        OnCreateedEffect(createdPos);

        float createdTime = _beforeDelay + _createdEffectTime;
        _monster.StartCoroutine(OnDelay(() => GameObject.Destroy(_actionEffect.gameObject), createdTime + 0.85f));

        // 낙하물 생성
        _monster.StartCoroutine(CreateFallingObj());
    }

    public override void OnAction()
    {
        _timer += Time.deltaTime;

        if (_isDelay)
            return;


        if (_timer > 2.2f)
        {
            _isDelay = false;
            _monster.StartCoroutine(OnDelay(() => IsAction = false, _afterDelay));
            return;
        }
    }

    public override void EndAction()
    {
        Init();
    }

    // 내적을 통한 몬스터 공격 모션 실행 여부
    private bool IsCalculateDot(Vector2 dir)
    {
        Vector2 dirX = new Vector2(dir.normalized.x, 0);
        float dot = Vector2.Dot(dirX, dir.normalized);
        if (dot < Mathf.Cos(_actionAngle))
            return false;

        return true;
    }

    // 시전시간 이후 이펙트 생성
    private void OnCreateedEffect(Vector2 createdPos)
    {
        _isDelay = true;
        float createdTime = _beforeDelay + _createdEffectTime;

        _ani.OnPlayAni("Idle");
        _monster.StartCoroutine(OnDelay(() => _ani.OnPlayAni("Pattern04"), _beforeDelay));
        _monster.StartCoroutine(OnDelay(() => CreatedEffect(createdPos), createdTime));
        _monster.StartCoroutine(OnDelay(() => _isDelay = false, createdTime));
    }

    // 낙하물 떨어지는 타이밍을 조절하는 메서드
    private IEnumerator CreateFallingObj()
    {
        float createdTime = _beforeDelay + _createdEffectTime + _fallingStartTime;
        yield return CoroutineManager.waitForSecondsRealtime(createdTime);

        for (int i = 0; i < _fallingCycle; i++)
        {
            yield return CoroutineManager.waitForSeconds(_fallingFrequency);

            GameObject obj = Create();

            _fallingObjQue.Enqueue(obj);
            _monster.StartCoroutine(OnDelay(() => GameObject.Destroy(_fallingObjQue.Dequeue()), _fallingDestroyTime));
        }
    }

    private GameObject Create()
    {
        GameObject obj = GameObject.Instantiate
                (
                _fallingObject,
                _target.position,
                Quaternion.identity,
                _myTransform.parent
                );

        if (obj.TryGetComponent<Collider2D>(out var collider))
            _monster.StartCoroutine(OnDelay(() => collider.enabled = true, _fallingHitTiming));

        return obj;
    }
}
