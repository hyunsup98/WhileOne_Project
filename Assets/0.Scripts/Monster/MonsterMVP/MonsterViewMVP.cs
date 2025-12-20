using UnityEngine;
using UnityEngine.Tilemaps;

public class MonsterViewMVP : MonoBehaviour
{
    [SerializeField] private MonsterDataSO _monsterData;    // 몬스터 데이터 SO
    [SerializeField] private Tilemap _wallTilemap;          // 경로 탐색을 위한 타일맵

    public Monster Presenter {  get; private set; }

    private Animator _animator;

    private void Awake()
    {
        Presenter = new Monster(_monsterData, this, _wallTilemap);
        _animator = GetComponent<Animator>();
    }

    public void OnHurtAni() => _animator.SetTrigger("Hurt");

    public void OnDeathAni() => _animator.SetBool("Death", true);
    public void OnAttackAni() => _animator.SetTrigger("Attack");

    public void OnIdleAni() => _animator.SetBool("Idle", true);

    public void OnDisIdleAni() => _animator.SetBool("Idle", false);
}
