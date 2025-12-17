using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Monster : MonoBehaviour
{
    [field: SerializeField] public int Hp {  get; private set; }
    [field: SerializeField] public float Att {  get; private set; }
    [field: SerializeField] public float AttRange {  get; private set; }
    [field: SerializeField] public float Vision { get; private set; }
    [field: SerializeField] public float Speed {  get; private set; }
    [field: SerializeField] public float Visibility { get; private set; } = 5;
    [field: SerializeField] public int Tier {  get; private set; }
    [field: SerializeField] public List<Transform> TargetPoint {  get; private set; }
    [SerializeField] private Tilemap _wallTilemap;


    public List<Vector2> PatrolPoint { get; private set; }
    public Astar MobAstar { get; private set; }
    public Transform Target { get; private set; }

    
    private IMonsterState _currentState;
    private Dictionary<MonsterState, IMonsterState> _monsterState;


    private void Awake()
    {
        MobAstar = new Astar(_wallTilemap);
        // 경로 탐색으로 순찰 포인트 초기화
        PatrolPoint = MobAstar.Pathfinder(TargetPoint[0].position, TargetPoint[1].position);

        // 상태 패턴 세팅
        _monsterState = new Dictionary<MonsterState, IMonsterState>();
        _monsterState.Add(MonsterState.Patrol, new Patrol(this));
        _monsterState.Add(MonsterState.Chase, new Chase(this));
        _monsterState.Add(MonsterState.Search, new Search(this));
        _currentState = _monsterState[MonsterState.Patrol];
    }

    private void Update()
    {
        _currentState.Update();
    }

    public void SetState(MonsterState state)
    {
        Debug.Log("이전 상태: " + _currentState);

        _currentState?.Exit();
        _currentState = _monsterState[state];
        _currentState.Enter();

        Debug.Log("현재 상태: " + _currentState);
    }

    public void SetTarget(Transform target) => Target = target;

}


public enum MonsterState
{
    Patrol, Chase, Search
}