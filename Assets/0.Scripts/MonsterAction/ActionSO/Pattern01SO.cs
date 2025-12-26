using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern01")]
public class Pattern01SO : MonsterActionSO
{
    [Header("패턴별 데이터")]

    [SerializeField] private float _rushSpeed;
    [SerializeField] private float _rushDistance;
    [SerializeField] private float _chargeDelay;
    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;
    

    public float RushSpeed => _rushSpeed;
    public float RushDistance => _rushDistance;
    public float ChargeDelay => _chargeDelay;
    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
