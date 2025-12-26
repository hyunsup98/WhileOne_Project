using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern06")]
public class Pattern06SO : MonsterActionSO
{
    [Header("패턴별 데이터")]

    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;
    [SerializeField] private float _startFallingTime;
    [SerializeField] private float _fallingCount;
    [SerializeField] private float _fallingFrequency;
    [SerializeField] private float _fallingCycle;
    [SerializeField] private float _fallingRange;
    [SerializeField] private float _actionStopTime;
    [SerializeField] private GameObject _fallingObjectPrefab;


    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;


    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public float StartFallingTime => _startFallingTime;
    public float FallingCount => _fallingCount;
    public float FallingFrequency => _fallingFrequency;
    public float FallingCycle => _fallingCycle;
    public float FallingRange => _fallingRange;
    public float ActionStopTime => _actionStopTime;
    public GameObject FallingObjectPrefab => _fallingObjectPrefab;

    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}