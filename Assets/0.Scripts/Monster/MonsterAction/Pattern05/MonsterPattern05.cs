using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterPattern05 : MonsterPattern
{
    private float _attackStartRange;
    private float _actionAngle;
    private float _actionStopTime;
    private float _createdEffectDistance;
    private float _createdEffectTime;

    private float _fallingStartTime;
    private int _fallingFrequency;
    private float _fallingCycle;
    private float _fallingDestroyTime;
    private GameObject _fallingObject;

    private Transform _myTransform;
    private Transform _target;

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

        _attackStartRange = actionData.ActionRange;
        _actionAngle = Mathf.Deg2Rad * actionData.ActionAngle;
        _actionStopTime = actionData.ActionStopTime;
        _createdEffectDistance = actionData.CreatedEffectDistance;
        _createdEffectTime = actionData.CreatedEffectTime;

        _fallingStartTime = actionData.FallingStartTime;
        _fallingFrequency = actionData.FallingFrequency;
        _fallingCycle = actionData.FallingCycle;
        _fallingDestroyTime = actionData.FallingDestroyTime;
        _fallingObject = actionData.FallingObject;

        
        _myTransform = monster.View.MyTransform;
    }

    public override void StartAction()
    {
        Vector2 dir =
            (_monster.Model.ChaseTarget.position - _myTransform.position).normalized;
        // 벡터 내적으로 플레이어와의 액션각보다 클 때는 스킬 시전X
        if (!IsCalculateDot(dir))
            return;

        IsAction = true;
        _target = _monster.Model.ChaseTarget;

        // 시전 준비 후 이펙트 생성
        Vector2 createdPos = (Vector2)_myTransform.position;
        createdPos.x += (dir.x * _createdEffectDistance);
        _hitDecision.GetComponent<CircleCollider2D>().radius = _attackStartRange;

        OnCreateedEffect(createdPos);
        _monster.StartCoroutine(CreateFallingObj());
    }

    public override void OnAction()
    {
        _timer += Time.deltaTime;

        if (_isDelay)
            return;


        if (_timer > _actionStopTime)
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
        yield return CoroutineManager.waitForSecondsRealtime(_fallingStartTime);

        for (int i = 0; i < _fallingCycle; i++)
        {
            yield return CoroutineManager.waitForSeconds(_fallingFrequency);
            Vector2 targetPos = new Vector2(_target.position.x, _target.position.y);
            GameObject fallingObj = GameObject.Instantiate
                (
                _fallingObject, 
                new Vector2(targetPos.x, targetPos.y),
                Quaternion.identity, 
                _myTransform.parent
                );

            yield return CoroutineManager.waitForSeconds(_fallingDestroyTime);

            GameObject.Destroy(fallingObj);
        }
    }

}
