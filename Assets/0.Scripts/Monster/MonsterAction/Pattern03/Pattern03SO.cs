
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern03")]
public class Pattern03SO : MonsterActionSO
{
    [Header("-------------------- 패턴별 데이터 --------------------")]

    [Header("액션 관련 데이터")]

    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;

    [SerializeField] private int _teleportCount;
    [SerializeField] private Transform _teleportPoint;

    //[Header("액션 이펙트 관련 데이터")]

    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public int TeleportCount => _teleportCount;
    public Transform TeleportPoint => _teleportPoint;
}