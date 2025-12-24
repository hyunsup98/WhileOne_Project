using UnityEngine;

public class MonsterPattern04 : MonsterPattern
{
    private Transform _myTransform;
    private float _acttackRange;
    private float _actionAngle;
    private float _actionTime;
    private float _timer;  // 맥스값 하드코딩


    public string AniTrigger { get; private set; }

    public MonsterPattern04(Pattern04SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = monster.View.transform;
        _damage = actionData.ActionDamage;
        _acttackRange = actionData.ActionRange;
        _actionAngle = Mathf.Deg2Rad * actionData.ActionAngle;
        _coolTime = actionData.ActionCoolTime;
        _actionTime = actionData.ActionTime;
        _chargeDelay = actionData.ChargeDelay;
        _hitDecision = actionData.HitDecision;
    }

    public override void StartAction()
    {
        Vector2 dir = 
            (_monster.Model.ChaseTarget.position - _myTransform.position).normalized;
        // 벡터 내적으로 플레이어와의 액션각보다 클 때는 스킬 시전X
        if (!IsCalculteDot(dir))
            return;

        IsAction = true;
        // 시전 준비 후 이펙트 생성
        Vector2 createdPos = (Vector2)_myTransform.position + (dir * 2f);
        _hitDecision.GetComponent<CircleCollider2D>().radius = _acttackRange;
        _monster.StartCoroutine(OnChargeDelay( createdPos, "Pattern04" ));
    }

    public override void OnAction()
    {
        if (_isDelay)
            return;

        _timer += Time.unscaledDeltaTime;
        if( _timer > _actionTime )
        { 
            IsAction = false;
            return;
        }
    }

    private bool IsCalculteDot(Vector2 dir)
    {
        Vector2 dirX = new Vector2(dir.normalized.x, 0);
        float dot = Vector2.Dot(dirX, dir.normalized);
        if (dot < Mathf.Cos(_actionAngle))
            return false;

        return true;
    }

    public override void EndAction()
    {
        _timer = 0;
        OnDisEffect();
    }
}
