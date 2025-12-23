using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MonsterPattern01 : IAction
{
    private MonsterPresenter _monster;
    private Transform _myTransform;
    private Vector2 _target;
    private float _damage;
    private float _rushSpeed;
    private float _rushDistance;
    private float _chargeDelay;

    private float _attackTime;

    private AttackEffect _attackEffect;
    private GameObject _createdHitDecition;
    private GameObject _hitDecision;
    private GameObject _pathPreview;

    public bool IsAction {  get; private set; }

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
    }


    public void StartAction()
    {
        _target = _monster.Model.ChaseTarget.position;
        IsAction = true;

        _createdHitDecition = GameObject.Instantiate
            (
            _hitDecision,
            _myTransform.position,
            Quaternion.identity,
            _myTransform
            );

        _attackEffect = _createdHitDecition.GetComponent<AttackEffect>();
        _attackEffect.OnAttack += OnCrash;

        _target = (_target - (Vector2)_myTransform.position).normalized;
    }
    
    public void OnAction()
    {
        _attackTime += Time.deltaTime;

        Vector2 start = _myTransform.position;
        int layerMask = LayerMask.GetMask("Wall");
        RaycastHit2D hit = Physics2D.Raycast(start, _target, 0.5f, layerMask);

        // 타이머로 돌진 종료 판정
        if (_attackTime >= 0.5f)
        {
            _attackTime = 0;
            IsAction = false;
        }

        // 벽에 부딪히면 몬스터 위치 이동은 없음(애니메이션은 출력)
        if (hit.collider != null)
            return;

        // 몬스터 돌진 이동
        _myTransform.Translate(_target * Time.deltaTime * _rushSpeed);
    }

    public void EndAction()
    {
        GameObject.Destroy(_createdHitDecition);
        _attackEffect.Init();
    }

    // 플레이어 타격시 데미지 계산
    private void OnCrash(Collider2D collision)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        if (collision.gameObject.layer == playerLayer)
        {
            Player player = collision.GetComponent<Player>();
            player.TakenDamage(_damage, _myTransform.position);
        }
    }
}
