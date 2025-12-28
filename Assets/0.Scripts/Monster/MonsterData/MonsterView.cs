using UnityEngine;
using UnityEngine.Tilemaps;


public class MonsterView : MonoBehaviour, IStunable
{
    [SerializeField] private MonsterData _monsterData;    // 몬스터 데이터 SO
    private Animator _animator;
    
    public Transform MyTransform { get; private set; }
    public MonsterPresenter Presenter { get; private set; }
    public bool IsStun { get; private set; }


    [ContextMenu("세팅값 갱신")]
    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // 특정 오브젝트의 중심값 보정을 위한 코드
        if (transform.parent.CompareTag("Monster"))
            MyTransform = transform.parent;
        else
            MyTransform = transform;
        
        // 순찰 지점 갱신을 위한 타일맵 정보
        Tilemap groundTilemap = GetTilemap("Ground");
        Tilemap wallTilemap = GetTilemap("Wall");

        Presenter = new MonsterPresenter(_monsterData, this, groundTilemap, wallTilemap);
    }

    private void Start()
    {
        Presenter.OnStart();
    }

    private void Update()
    {
        Presenter.Tick();
    }

    #region 애니메이션 출력
    public void OnPlayAni(string aniName)
    {
        switch (aniName)
        {
            case "Idle":
                _animator.SetBool("Idle", true);
                break;
            case "Teleport":
                _animator.SetTrigger("Teleport");
                break;
            case "Stun":
                _animator.SetBool("Stun", true);
                break;

            case "Hurt":
                _animator.SetTrigger("Hurt");
                break;

            case "Death":
                _animator.SetBool("Death", true);
                break;

            case "Pattern01":
                _animator.SetTrigger("Pattern01");
                break;

            case "Pattern04":
                _animator.SetTrigger("Pattern04");
                break;
            case "Pattern05":
                _animator.SetTrigger("Pattern05");
                break;
            case "Pattern06Start":
                _animator.SetTrigger("Pattern06Start");
                break;
            case "Pattern06End":
                _animator.SetTrigger("Pattern06End");
                break;
            

            default:
                Debug.LogWarning("출력할 애니메이션 없음");
                break;
        }
    }

    public void OnStopAni(string aniName)
    {
        switch (aniName)
        {
            case "Idle":
                _animator.SetBool("Idle", false);
                break;

            default:
                Debug.LogWarning("출력할 애니메이션 없음");
                break;
        }
    }


    public void OnHurtAni() => _animator.SetTrigger("Hurt");
    public void OnDeathAni() => _animator.SetBool("Death", true);
    public void OnAttackAni() => _animator.SetTrigger("Pattern01");
    public void OnIdleAni() => _animator.SetBool("Idle", true);
    public void OnDisIdleAni() => _animator.SetBool("Idle", false);
    #endregion


    // 현재 실행중인 애니메이션의 정보 반환
    public AnimatorStateInfo GetPlayingAni() => _animator.GetCurrentAnimatorStateInfo(0);


    public void OnMove(Vector2 target, float speed)
    {
        OnTurn(target);

        MyTransform.position = Vector2.MoveTowards
            (
            MyTransform.position,
            target,
            speed * Time.deltaTime
            );
    }

    // 타겟의 방향으로 몸을 돌리는 로직
    public void OnTurn(Vector2 target)
    {
        Vector2 dir = target - (Vector2)MyTransform.position;
        Vector2 dirX = new Vector2(dir.x, 0f).normalized;
        if (dirX.x != 0f)
            MyTransform.localScale = new Vector3(dirX.x, 1f, 1f);
    }

    // target방향으로 LOS를 발사했을 때, 플레이어와 직선 거리에 존재시에 true 반환
    public RaycastHit2D OnLOS(Vector2 target, float sight)
    {
        Vector2 start = MyTransform.position;
        Vector2 dir = target - start;

        int layerMask = LayerMask.GetMask("Wall", "Player");
        RaycastHit2D hit = Physics2D.Raycast(start, dir, sight, layerMask);

        return hit;
    }

    public void RequestDestroy(float delay) => Destroy(gameObject, delay);

    // 형제 오브젝트 중에서 특정 타일맵을 반환하는 메서드
    public Tilemap GetTilemap(string tag)
    {
        Transform parent = transform.parent;

        // 몬스터 중심축이 다른 오브젝트인 경우 부모의 부모의 정보를 가져온다.
        if(parent.CompareTag("Monster"))
            parent = parent.parent;

        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child.GetComponent<Tilemap>();
        }

        Debug.LogWarning(tag + "타일맵을 찾을 수 없습니다.");
        return null;
    }

    public void OnHit(float damage) => Presenter.OnHit(damage);

    public void OnStun()
    {
        IsStun = true;
        transform.GetComponent<Collider2D>().enabled = false;
        Presenter.OnStun();
    }

    public void SetStun(bool isStun) => IsStun = isStun;
}
