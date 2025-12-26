using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern05")]
public class Pattern05SO : MonsterActionSO
{
    [Header("패턴별 데이터")]

    [SerializeField] private float _actionRange;
    [SerializeField] private float _actionAngle;
    [SerializeField] private float _chargeDelay;
    [SerializeField] private float _actionTime;
    [SerializeField] private int _fallingCount;
    [SerializeField] private float _fallingCycle;
    [SerializeField] private float _fallingSpeed;
    [SerializeField] private float _fallingHeight;
    [SerializeField] private GameObject _fallingObject;


    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;


    public float ActionRange => _actionRange;
    public float ActionAngle => _actionAngle;
    public float ChargeDelay => _chargeDelay;
    public float ActionTime => _actionTime;
    public int FallingCount => _fallingCount;
    public float FallingCycle => _fallingCycle;
    public float FallingSpeed => _fallingSpeed;
    public float FallingHeight => _fallingHeight;
    public GameObject FallingObject => _fallingObject;


    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
