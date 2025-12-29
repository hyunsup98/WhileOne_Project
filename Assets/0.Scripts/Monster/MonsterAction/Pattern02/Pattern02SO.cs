using UnityEngine;

public class Pattern02SO : MonsterActionSO
{
    [Header("-------------------- 패턴별 데이터 --------------------")]

    [Header("액션 관련 데이터")]

    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;
    [SerializeField] private float _startTime = 1;
    [SerializeField] private float _attackBoost;
    [SerializeField] private float _speedBoost;

    [Header("액션 이펙트 관련 데이터")]

    [SerializeField] private Vector2 _createPos;
    [Tooltip("액션 이펙트 프리펩")]
    [SerializeField] private GameObject _actionEffectPrefab;

    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public float StartTime => _startTime;
    public float AttackBoost => _attackBoost;
    public float SpeedBoost => _speedBoost;
    public Vector2 CreatePos => _createPos;
    public GameObject ActionEffectPrefab => _actionEffectPrefab;
}