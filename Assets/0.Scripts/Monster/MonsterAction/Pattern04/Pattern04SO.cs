using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern04")]
public class Pattern04SO : MonsterActionSO
{
    [Header("-------------------- 패턴별 데이터 --------------------")]

    [Header("액션 관련 데이터")]

    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;
    [Tooltip("액션 발동 트리거 시야각 범위")]
    [SerializeField] private float _actionAngle;

    [Header("액션 이펙트 관련 데이터")]

    [Tooltip("이펙트 생성 타이밍")]
    [SerializeField] private float _createdEffectTime = 0.75f;
    [Tooltip("이펙트 생성 거리")]
    [SerializeField] private float _createdEffectDistance = 4;
    [Tooltip("원형 히트박스 반지름")]
    [SerializeField] private float _hitBoxRadius;
    [Tooltip("액션 이펙트 프리펩")]
    [SerializeField] private GameObject _hitDecision;
    [Tooltip("이펙트 경로 프리펩")]
    [SerializeField] private GameObject _pathPreview;


    public float HitBoxRadius => _hitBoxRadius;
    public float ActionAngle => _actionAngle;
    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public float CreatedEffectDistance => _createdEffectDistance;
    public float CreatedEffectTime => _createdEffectTime;
    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
