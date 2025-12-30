using System.Collections;
using UnityEngine;

public class MonsterPattern02 : MonsterPattern
{
    private float _startTime;
    private float _duration;
    private float _attackBoost;
    private float _speedBoost;
    private Vector2 _createPos;

    private Transform _myTransform;



    public MonsterPattern02(Pattern02SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _myTransform = monster.View.MyTransform;
        _damage = actionData.ActionDamage;
        _maxCoolTime = actionData.ActionCoolTime;
        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;
        _hitDecision = actionData.ActionEffectPrefab;
        
        _startTime = actionData.StartTime;
        _duration = actionData.Duration;
        _attackBoost = actionData.AttackBoost;
        _speedBoost = actionData.SpeedBoost;
        _createPos = actionData.CreatePos;
        
    }
    public override void StartAction()
    {
        Vector2 createPos = new Vector2(_createPos.x * _myTransform.localScale.x, _createPos.y);
        createPos += (Vector2)_myTransform.position;

        _monster.StartCoroutine(OnBoosting(createPos));
    }

    public override void EndAction()
    {
        Init();
        _monster.StartCoroutine(StartCool());
    }

    public override void OnAction()
    {
        if (_isDelay)
            return;

        _timer += Time.deltaTime;
        if (_timer > _beforeDelay + _startTime)
        {
            _isDelay = true;
            _monster.StartCoroutine(OnDelay(() => IsAction = false, _afterDelay));
            return;
        }

    }

    private IEnumerator OnBoosting(Vector2 createdPos)
    {
        IsAction = true;
        _isDelay = true;
        yield return CoroutineManager.waitForSeconds(_beforeDelay);

        _isDelay = false;
        CreatedEffect(createdPos);
        IsAction = false;

        Debug.Log("버프 강화");
        _monster.Model.SetBoost(_attackBoost, _speedBoost);

        yield return CoroutineManager.waitForSeconds(_duration + _startTime);

        Debug.Log("버프 강화 종료");
        _monster.Model.SetBoost(-_attackBoost, -_speedBoost);

        if (_actionEffect != null)
            GameObject.Destroy(_actionEffect.gameObject);
    }

    protected override void Init()
    {
        _isDelay = false;
        IsAction = false;
        _timer = 0;
    }
}
