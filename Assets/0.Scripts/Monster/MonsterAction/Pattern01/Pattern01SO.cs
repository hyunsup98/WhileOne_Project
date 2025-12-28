using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern01")]
public class Pattern01SO : MonsterActionSO
{
    [Header("-------------------- 패턴별 데이터 --------------------")]

    [Header("액션 관련 데이터")]

    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;
    [SerializeField] private float _rushSpeed;
    [SerializeField] private float _rushDistance;

    
    [Header("액션 이펙트 관련 데이터")]

    [Tooltip("이펙트 생성 타이밍")]
    [SerializeField] private float _createdEffectTime = 0.3f;
    [Tooltip("이펙트 생성 위치")]
    [SerializeField] private Vector2 _createPos;
    [Tooltip("히트박스 크기(가로 × 세로")]
    [SerializeField] private Vector2 _hitBoxSize;
    [Tooltip("액션 이펙트 프리펩")]
    [SerializeField] private GameObject _hitDecision;
    [Tooltip("이펙트 경로 프리펩")]
    [SerializeField] private GameObject _pathPreview;


    public float CreatedEffectTime => _createdEffectTime;
    public float RushSpeed => _rushSpeed;
    public float RushDistance => _rushDistance;
    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public Vector2 CreatePos => _createPos;
    public Vector2 HitBoxSize => _hitBoxSize;
    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
