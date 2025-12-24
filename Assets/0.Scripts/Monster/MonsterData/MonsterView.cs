using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MonsterView : MonoBehaviour
{
    [SerializeField] private MonsterData _monsterData;    // 몬스터 데이터 SO
    
    public Transform MyTransform { get; private set; }
    public MonsterPresenter Presenter { get; private set; }

    private Animator _animator;

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
    public void OnActionAni(string action)
    {
        switch (action)
        {
            case "Idle":
                _animator.SetBool("Idle", true);
                break;
            case "Pattern01":
                _animator.SetTrigger("Pattern01");
                break;
            case "Pattern04":
                _animator.SetTrigger("Pattern04");
                break;
            default:
                Debug.Log("출력할 애니메이션 없음");
                break;
        }
    }

    public void OnDisActionAni(string action)
    {
        switch (action)
        {
            case "Idle":
                _animator.SetBool("Idle", false);
                break;
            default:
                Debug.Log("출력할 애니메이션 없음");
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

        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child.GetComponent<Tilemap>();
        }

        return null;
    }
}
