using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern06")]
public class Pattern06SO : MonsterActionSO
{
    [Header("패턴별 데이터")]

    [SerializeField] private float _chargeDelay;
    [SerializeField] private float _startFallingTime;
    [SerializeField] private float _fallingCount;
    [SerializeField] private float _fallingCycle;
    [SerializeField] private float _fallingRange;
    [SerializeField] private float _fallingSpeed;
    [SerializeField] private float _fallingHeight;
    [SerializeField] private GameObject _fallingObjectPrefab;


    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;


    public float ChargeDelay => _chargeDelay;
    public float StartFallingTime => _startFallingTime;
    public float FallingCount => _fallingCount;
    public float FallingCycle => _fallingCycle;
    public float FallingRange => _fallingRange;
    public float FallingSpeed => _fallingSpeed;
    public float FallingHeight => _fallingHeight;
    public GameObject FallingObjectPrefab => _fallingObjectPrefab;


    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}