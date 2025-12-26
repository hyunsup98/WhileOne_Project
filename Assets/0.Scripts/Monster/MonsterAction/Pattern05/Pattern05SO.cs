using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO/Pattern05")]
public class Pattern05SO : MonsterActionSO
{
    [Header("패턴별 데이터")]
    [SerializeField] private float _actionRange;
    [SerializeField] private float _actionAngle;
    [SerializeField] private float _beforeDelay;
    [SerializeField] private float _afterDelay;
    [SerializeField] private float _actionStopTime;
    [SerializeField] private float _createdEffectDistance = 4;
    [SerializeField] private float _createdEffectTime = 0.75f;

    [Header("낙하물 관련 데이터")]
    [SerializeField] private int _fallingStartTime;
    [SerializeField] private int _fallingFrequency;
    [SerializeField] private float _fallingCycle;
    [SerializeField] private float _fallingDestroyTime;
    [SerializeField] private GameObject _fallingObject;


    [SerializeField] private GameObject _pathPreview;
    [SerializeField] private GameObject _hitDecision;


    public float ActionRange => _actionRange;
    public float ActionAngle => _actionAngle;
    public float BeforeDelay => _beforeDelay;
    public float AfterDelay => _afterDelay;
    public float ActionStopTime => _actionStopTime;
    public float CreatedEffectDistance => _createdEffectDistance;
    public float CreatedEffectTime => _createdEffectTime;
    public float FallingStartTime => _fallingStartTime;
    public int FallingFrequency => _fallingFrequency;
    public float FallingCycle => _fallingCycle;
    public float FallingDestroyTime => _fallingDestroyTime;
    public GameObject FallingObject => _fallingObject;


    public GameObject PathPreview => _pathPreview;
    public GameObject HitDecision => _hitDecision;
}
