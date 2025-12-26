using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern04")]
public class Pattern04SO : MonsterActionSO
{
    [Header("패턴별 데이터")]

    [SerializeField] private float _actionRange;
    [SerializeField] private float _actionAngle;
    [SerializeField] private float _chargeDelay;
    [SerializeField] private float _actionTime;
    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;


    public float ActionRange => _actionRange;
    public float ActionAngle => _actionAngle;
    public float ChargeDelay => _chargeDelay;
    public float ActionTime => _actionTime;
    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
