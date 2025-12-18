using UnityEngine;

[CreateAssetMenu(fileName = "MonsterActionSO", menuName = "MonsterSO/MonsterActionSO")]
public class MonsterActionSO : ScriptableObject
{
    [SerializeField] private int _monsterActionID;
    [SerializeField] private string _name;
    [SerializeField] private int _monsterID;

    [Tooltip("행동 분류")]
    [Range(1, 7)]
    [SerializeField] private int _actionClass;

    [Tooltip("액션 조건(플레이어와 몬스터의 거리")]
    [SerializeField] private float _actionCondition;

    [Tooltip("액션 타입(1. 공격형 , 2. 버프형")]
    [Range(1, 2)]
    [SerializeField] private int _actionType;

    [Tooltip("행동 데미지")]
    [SerializeField] private float _actionDamage;

    [Tooltip("행동 주기")]
    [SerializeField] private float _actionCoolTime;

    [Tooltip("공격력 증가량")]
    [SerializeField] private float _attackBoost;

    [Tooltip("이동속도 증가량")]
    [SerializeField] private float _speedBoost;

    [Tooltip("행동 모션 리소스(경로)")]
    [SerializeField] private string _actionResource;

    [Tooltip("행동 사운드 리소스(경로)")]
    [SerializeField] private string _actionSound;


    // 외부 참조 프로퍼티
    public int MonsterActionID => _monsterActionID;
    public string Name => _name;
    public int MonsterID => _monsterID;
    public int ActionClass => _actionClass;
    public float ActionCondition => _actionCondition;
    public int ActionType => _actionType;
    public float ActionDamage => _actionDamage;
    public float ActionCoolTime => _actionCoolTime;
    public float AttackBoost => _attackBoost;
    public float SpeedBoost => _speedBoost;
    public string ActionResource => _actionResource;
    public string ActionSound => _actionSound;
}
