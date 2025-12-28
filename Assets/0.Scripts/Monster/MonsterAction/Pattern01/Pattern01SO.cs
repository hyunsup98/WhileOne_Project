using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern01")]
public class Pattern01SO : MonsterActionSO
{
    [Header("패턴별 데이터")]

    [SerializeField] private float _rushSpeed;
    [SerializeField] private float _rushDistance;
    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;
    [SerializeField] private Vector2 _createPos;
    [SerializeField] private Vector2 _hitBoxSize;
    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;
    

    public float RushSpeed => _rushSpeed;
    public float RushDistance => _rushDistance;
    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public Vector2 CreatePos => _createPos;
    public Vector2 HitBoxSize => _hitBoxSize;
    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
