using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDataSO", menuName = "MonsterSO/MonsterDataSO")]
public class MonsterData : ScriptableObject
{
    [Header("몬스터 기본 정보")]

    [SerializeField] private int _monsterID;
    [SerializeField] private string _name;
    [SerializeField] private int _tier;
    [SerializeField] private float _hp = 100f;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _sight = 5f;

    [Header("프로그래밍 중 추가한 필드값")]
    [SerializeField] private float _sightAngle = 90f;
    [SerializeField] private float _actionDistance = 5f;
    [SerializeField] private float _searchTime = 3f;
    [SerializeField] private float _patrolRange = 5f;


    [Tooltip("몬스터의 행동 리스트")]
    [SerializeField] private List<MonsterActionSO> _actionList;


    // 외부 참조 프로퍼티
    public int MonsterID => _monsterID;
    public string Name => _name;
    public int Tier => _tier;
    public float Hp => _hp;
    public float MoveSpeed => _moveSpeed;
    public float Sight => _sight;
    public float SightAngle => _sightAngle;
    public float ActionDistance => _actionDistance;
    public float SearchTime => _searchTime;
    public float PatrolRange => _patrolRange;

    public List<MonsterActionSO> ActionList => _actionList;
}
