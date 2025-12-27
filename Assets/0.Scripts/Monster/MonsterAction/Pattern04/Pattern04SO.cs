using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern04")]
public class Pattern04SO : MonsterActionSO
{
    [Header("패턴별 데이터")]

    [SerializeField] private float _hitBoxRadius;
    [SerializeField] private float _actionAngle;
    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;
    [SerializeField] private float _createdEffectDistance = 4;
    [SerializeField] private float _createdEffectTime = 0.75f;

    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;


    public float HitBoxRadius => _hitBoxRadius;
    public float ActionAngle => _actionAngle;
    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public float CreatedEffectDistance => _createdEffectDistance;
    public float CreatedEffectTime => _createdEffectTime;
    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
