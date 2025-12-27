using UnityEngine;

public class MonsterPattern04 : MonsterPattern
{
    private float _hitBoxRadius;
    private float _actionAngle;
    private float _createdEffectDistance;
    private float _createdEffectTime;

    private Transform _myTransform;

    public MonsterPattern04(Pattern04SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _damage = actionData.ActionDamage;
        _maxCoolTime = actionData.ActionCoolTime;
        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;

        _hitDecision = actionData.HitDecision;

        _hitBoxRadius = actionData.HitBoxRadius;
        _actionAngle = Mathf.Deg2Rad * actionData.ActionAngle;
        _createdEffectDistance = actionData.CreatedEffectDistance;
        _createdEffectTime = actionData.CreatedEffectTime;
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


        // 콜라이더 크기 조절
        if (_hitDecision.TryGetComponent<CircleCollider2D>(out var collider))
            collider.radius = _hitBoxRadius;
        else
            Debug.LogWarning("이펙트에 Collider없음");

        // 시전 준비 후 이펙트 생성
        Vector2 createdPos = (Vector2)_myTransform.position;
        createdPos.x += (dir.x * _createdEffectDistance);
        OnCreateedEffect(createdPos);

        float createdTime = _beforeDelay + _createdEffectTime;
        _monster.StartCoroutine(OnDelay(() => GameObject.Destroy(_actionEffect.gameObject), createdTime + 1f));
    }

    public override void OnAction()
    {
        _timer += Time.deltaTime;

        if (_isDelay)
            return;
        
        if( _timer > 2.2f)
        {
            Debug.Log("타이밍 확인");
            _isDelay = false;
            _monster.StartCoroutine(OnDelay(() => IsAction = false, _afterDelay));
            return;
        }
    }

    public override void EndAction()
    {
        Init();
        _monster.StartCoroutine(StartCool());
    }


    // 시전시간 이후 이펙트 생성
    private void OnCreateedEffect(Vector2 createdPos)
    {
        _isDelay = true;
        float createdTime = _beforeDelay + _createdEffectTime;

        _ani.OnPlayAni("Idle");
        _monster.StartCoroutine(OnDelay(() => _ani.OnPlayAni("Pattern04"), _beforeDelay));
        _monster.StartCoroutine(OnDelay(() => _isDelay = false, createdTime));
        _monster.StartCoroutine(OnDelay(() => CreatedEffect(createdPos), createdTime));
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

}
