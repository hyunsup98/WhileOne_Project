using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Monster : MonoBehaviour
{
    [SerializeField] private string _name;
    [field: SerializeField] public int Hp {  get; private set; }
    [field: SerializeField] public float Att {  get; private set; }
    [field: SerializeField] public float AttRange {  get; private set; }
    [field: SerializeField] public float Speed {  get; private set; }
    [field: SerializeField] public float Sight { get; private set; } = 5;
    [field: SerializeField] public int Tier {  get; private set; }
    [field: SerializeField] public List<Transform> PatrolTarget {  get; private set; }
    public List<IAttack> Attack {  get; private set; }
    
    [SerializeField] private Tilemap _wallTilemap;


    public List<Vector2> PatrolPoint { get; private set; }
    public Astar MobAstar { get; private set; }
    public Transform Target { get; private set; }

    
    private IState _currentState;
    private Dictionary<MonsterState, IState> _monsterState;


    private void Awake()
    {
        // 경로 탐색으로 순찰 포인트 초기화
        MobAstar = new Astar(_wallTilemap);
        PatrolPoint = MobAstar.Pathfinder(PatrolTarget[0].position, PatrolTarget[1].position);

        // 공격 세팅
        Attack = new();
        Attack.Add(new ProtoAttack(this));

        // 상태 패턴 세팅
        _monsterState = new Dictionary<MonsterState, IState>();
        _monsterState.Add(MonsterState.Patrol, new Patrol(this));
        _monsterState.Add(MonsterState.Chase, new Chase(this));
        _monsterState.Add(MonsterState.Search, new Search(this));
        _monsterState.Add(MonsterState.BackReturn, new BackReturn(this));
        _monsterState.Add(MonsterState.Attack, new MonsterAttack(this));
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


    public void OnMove(Vector2 target, float speed)
    {
        transform.position = Vector2.MoveTowards
            (
            transform.position,
            target,
            speed * Time.deltaTime
            );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("플레이어 타격");
    }

}


public enum MonsterState
{
    Patrol, Chase, Search, BackReturn, Attack
}