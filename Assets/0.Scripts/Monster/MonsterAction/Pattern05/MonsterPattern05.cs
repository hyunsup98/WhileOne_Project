using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterPattern05 : MonsterPattern
{
    private float _attackStartRange;
    private float _actionAngle;
    private float _actionTime;
    private int _fallingCount;
    private float _fallingCycle;
    private float _fallingSpeed;
    private float _fallingHeight;
    private GameObject _fallingObject;

    private Transform _myTransform;
    private Transform _target;

    public string AniTrigger { get; private set; }

    public MonsterPattern05(Pattern05SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _damage = actionData.ActionDamage;
        _chargeDelay = actionData.ChargeDelay;
        _coolTime = actionData.ActionCoolTime;
        _hitDecision = actionData.HitDecision;

        _attackStartRange = actionData.ActionRange;
        _actionAngle = Mathf.Deg2Rad * actionData.ActionAngle;
        _actionTime = actionData.ActionTime;
        _fallingCount = actionData.FallingCount;
        _fallingCycle = actionData.FallingCycle;
        _fallingSpeed = actionData.FallingSpeed;
        _fallingHeight = actionData.FallingHeight;
        _fallingObject = actionData.FallingObject;

        
        _myTransform = monster.View.MyTransform;
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
        // 시전 준비 후 이펙트 생성
        Vector2 createdPos = (Vector2)_myTransform.position;
        createdPos.x += (dir.x * 3f);
        _hitDecision.GetComponent<CircleCollider2D>().radius = _attackStartRange;

        // 몬스터 내려찍는 모습과 이펙트 생성 사이의 0.75초의 딜레이 보정해줌
        _monster.StartCoroutine(OnChargeDelay(createdPos, "Pattern05", 0.75f));
        _monster.StartCoroutine(CreateFallingObj());
    }

    public override void OnAction()
    {
        if (_isDelay)
            return;

        _timer += Time.deltaTime;

        if (_timer > _actionTime)
        {
            IsAction = false;
            return;
        }
    }

    public override void EndAction()
    {
        _timer = 0;
        OnDisEffect();
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

    // 낙하물 떨어지는 타이밍을 조절하는 메서드
    private IEnumerator CreateFallingObj()
    {
        yield return CoroutineManager.waitForSecondsRealtime(_chargeDelay);

        for (int i = 0; i < _fallingCount; i++)
        {
            yield return CoroutineManager.waitForSeconds(_fallingCycle);
            Vector2 targetPos = new Vector2(_target.position.x, _target.position.y);
            GameObject fallingObj = GameObject.Instantiate
                (
                _fallingObject, 
                new Vector2(targetPos.x, targetPos.y + _fallingHeight),
                Quaternion.identity, 
                _myTransform.parent
                );
            Debug.Log(fallingObj);
            Debug.Log("낙하물 위치" + fallingObj.transform.position);
            
            while (((Vector2)fallingObj.transform.position - targetPos).SqrMagnitude() >= 0.01f)
            {
                fallingObj.transform.Translate(Vector2.down * _fallingSpeed * Time.deltaTime);
                yield return null;
            }

            GameObject.Destroy(fallingObj);
        }
    }

}
