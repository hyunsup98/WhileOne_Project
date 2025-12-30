using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern06")]
public class Pattern06SO : MonsterActionSO
{
    [Header("-------------------- 패턴별 데이터 --------------------")]

    [Header("액션 관련 데이터")]

    [Header("액션 이펙트 관련 데이터")]
    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;

    [Header("낙하물 관련 데이터")]

    [Tooltip("낙하물 생성 시작 타이밍")]
    [SerializeField] private float _fallingStartTime;
    [Tooltip("낙하물 생성주기(빈도)")]
    [SerializeField] private float _fallingFrequency;
    [Tooltip("낙하물 생성 횟수")]
    [SerializeField] private int _fallingCycle;
    [Tooltip("낙하물 데미지 타이밍")]
    [SerializeField] private int _fallingHitTiming;
    [Tooltip("낙하물 생성 갯수")]
    [SerializeField] private float _fallingCount;
    [Tooltip("낙하물 생성 범위")]
    [SerializeField] private float _fallingRange;
    [Tooltip("낙하물 프리펩")]
    [SerializeField] private GameObject _fallingObjectPrefab;


    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public float FallingStartTime => _fallingStartTime;
    public float FallingCount => _fallingCount;
    public float FallingFrequency => _fallingFrequency;
    public float FallingCycle => _fallingCycle;
    public float FallingHitTiming => _fallingHitTiming;
    public float FallingRange => _fallingRange;
    public GameObject FallingObjectPrefab => _fallingObjectPrefab;
}