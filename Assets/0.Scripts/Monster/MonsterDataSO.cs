using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MonsterDataSO", menuName = "MonsterSO/MonsterDataSO")]
public class MonsterDataSO : ScriptableObject
{
    [Header("몬스터 기본 정보")]

    [SerializeField] private int _monsterID;
    [SerializeField] private string _name;
    [SerializeField] private int _tier;
    [SerializeField] private float _hp;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _sight = 5;

    [Tooltip("몬스터의 행동 리스트")]
    [SerializeField] private List<int> _actionList;


    [Header("순찰용 데이터")]

    [Tooltip("순찰을 위한 2개의 포인트 추가(반드시 왕복할 2개의 포인트 지정!")]
    [SerializeField] private List<Transform> _patrolTarget;


    // 외부 참조 프로퍼티
    public int MonsterID => _monsterID;
    public string Name => _name;
    public int Tier => _tier;
    public float Hp => _hp;
    public float MoveSpeed => _moveSpeed;
    public float Sight => _sight;
    public List<int> ActionList => _actionList;
    public List<Transform> PatrolTarget => _patrolTarget;

}
