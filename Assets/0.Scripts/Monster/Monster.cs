using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Monster : MonoBehaviour
{
    [field: SerializeField] public int Hp {  get; private set; }
    [field: SerializeField] public float Att {  get; private set; }
    [field: SerializeField] public float Vision { get; private set; }
    [field: SerializeField] public float Speed {  get; private set; }
    [field: SerializeField] public int Tier {  get; private set; }
    [field: SerializeField] public List<Transform> TargetPoint {  get; private set; }
    [SerializeField] private Tilemap _wallTilemap;


    public List<Vector2> PatrolPoint { get; private set; }
    public Astar MobAstar { get; private set; }

    
    private Dictionary<MonsterState, IMonsterState> _monsterState;
    private IMonsterState _currentState;


    private void Awake()
    {
        _monsterState = new Dictionary<MonsterState, IMonsterState>();
        _monsterState.Add(MonsterState.Patrol, new Patrol(this));
        _monsterState.Add(MonsterState.Chase, new Chase(this));
        _monsterState.Add(MonsterState.Search, new Search(this));
        _currentState = _monsterState[MonsterState.Patrol];

        MobAstar = new Astar(_wallTilemap);
        // 경로 탐색으로 순찰 포인트 초기화
        PatrolPoint = MobAstar.Pathfinder(TargetPoint[0].position, TargetPoint[1].position);
    }
    private void Update()
    {
        _currentState.Update();
    }

    public void SetState(MonsterState state)
    {
        _currentState?.Exit();
        _currentState = _monsterState[state];
        _currentState.Enter();
    }

}


public enum MonsterState
{
    Patrol, Chase, Search
}