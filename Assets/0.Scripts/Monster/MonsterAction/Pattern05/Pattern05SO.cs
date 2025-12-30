using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern05")]
public class Pattern05SO : MonsterActionSO
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

    [Header("낙하물 관련 데이터")]

    [Tooltip("낙하물 생성 시작 타이밍")]
    [SerializeField] private float _fallingStartTime;
    [Tooltip("낙하물 생성주기(빈도)")]
    [SerializeField] private float _fallingFrequency;
    [Tooltip("낙하물 생성 횟수")]
    [SerializeField] private int _fallingCycle;
    [Tooltip("낙하물 프리펩")]
    [SerializeField] private GameObject _fallingObject;




    public float HitBoxRadius => _hitBoxRadius;
    public float ActionAngle => _actionAngle;
    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public float CreatedEffectDistance => _createdEffectDistance;
    public float CreatedEffectTime => _createdEffectTime;
    public float FallingStartTime => _fallingStartTime;
    public float FallingFrequency => _fallingFrequency;
    public int FallingCycle => _fallingCycle;
    public GameObject FallingObject => _fallingObject;


    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
