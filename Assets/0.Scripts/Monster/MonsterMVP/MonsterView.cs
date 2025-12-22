using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MonsterView : MonoBehaviour
{
    [SerializeField] private MonsterDataSO _monsterData;    // 몬스터 데이터 SO
    [SerializeField] private Tilemap _wallTilemap;          // 경로 탐색을 위한 타일맵
    [SerializeField] private List<Transform> _patrolTarget;

    public MonsterPresenter Presenter { get; private set; }

    private Animator _animator;

    // 추후 지워야 할 목록
    public GameObject AttackEffect;

    private void Awake()
    {
        Presenter = new MonsterPresenter
            (_monsterData, this, _wallTilemap, _patrolTarget);
        _animator = GetComponent<Animator>();
    }

    // 테스트용 코드
    public void OnTest()
    {
        Presenter.OnHit(15);
    }

    private void Start()
    {
        Presenter.OnStart();
    }

    private void Update()
    {
        Presenter.Tick();
    }

    public void OnHurtAni() => _animator.SetTrigger("Hurt");
    public void OnDeathAni() => _animator.SetBool("Death", true);
    public void OnAttackAni() => _animator.SetTrigger("Attack");
    public void OnIdleAni() => _animator.SetBool("Idle", true);
    public void OnDisIdleAni() => _animator.SetBool("Idle", false);

    // 현재 실행중인 애니메이션의 정보 반환
    public AnimatorStateInfo GetPlayingAni() => _animator.GetCurrentAnimatorStateInfo(0);


    public void OnMove(Vector2 target, float speed)
    {
        OnTurn(target);

        transform.position = Vector2.MoveTowards
            (
            transform.position,
            target,
            speed * Time.deltaTime
            );
    }

    // 타겟의 방향으로 몸을 돌리는 로직
    public void OnTurn(Vector2 target)
    {
        Vector2 dir = target - (Vector2)transform.position;
        Vector2 dirX = new Vector2(dir.x, 0f).normalized;
        if (dirX.x != 0f)
            transform.localScale = new Vector3(dirX.x, 1f, 1f);
    }

    // target방향으로 LOS를 발사했을 때, 플레이어와 직선 거리에 존재시에 true 반환
    public RaycastHit2D OnLOS(Vector2 target, float sight)
    {
        Vector2 start = transform.position;
        Vector2 dir = target - start;

        int layerMask = LayerMask.GetMask("Wall", "Player");
        RaycastHit2D hit = Physics2D.Raycast(start, dir, sight, layerMask);

        return hit;
    }

    public void RequestDestroy(float delay) => Destroy(gameObject, delay);


}
