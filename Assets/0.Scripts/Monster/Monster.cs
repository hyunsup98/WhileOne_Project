using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Monster : MonoBehaviour
{
    [SerializeField] private MonsterDataSO _monsterData;
    [SerializeField] private Tilemap _wallTilemap;
    public MonsterModel MonsterModel { get; private set; }


    // 추후 지워야 할 목록
    public GameObject AttackEffect;
    [field: SerializeField] public float Att {  get; private set; }
    [field: SerializeField] public float AttRange {  get; private set; }
    public IAttack Attack {  get; private set; }




    private void Awake()
    {
        MonsterModel = new MonsterModel(_monsterData);

        // 경로 탐색으로 순찰 포인트 초기화
        MonsterModel.MobAstar = new Astar(_wallTilemap);
        MonsterModel.PatrolPoint = MonsterModel.MobAstar.Pathfinder
            (
            MonsterModel.PatrolTarget[0].position,
            MonsterModel.PatrolTarget[1].position
            );

        // 공격 세팅
        Attack = new ProtoAttack(this);

        // 상태 패턴 세팅
        MonsterModel.MonsterState = new Dictionary<MonsterState, IState>();
        MonsterModel.MonsterState.Add(MonsterState.Patrol, new Patrol(this));
        MonsterModel.MonsterState.Add(MonsterState.Chase, new Chase(this));
        MonsterModel.MonsterState.Add(MonsterState.Search, new Search(this));
        MonsterModel.MonsterState.Add(MonsterState.BackReturn, new BackReturn(this));
        MonsterModel.MonsterState.Add(MonsterState.Attack, new MonsterAttack(this));
        MonsterModel.CurrentState = MonsterModel.MonsterState[MonsterState.Patrol];
    }

    private void Update()
    {
        MonsterModel.CurrentState.Update();
    }

    public void SetState(MonsterState state)
    {
        Debug.Log("이전 상태: " + MonsterModel.CurrentState);

        MonsterModel.CurrentState?.Exit();
        MonsterModel.CurrentState = MonsterModel.MonsterState[state];
        MonsterModel.CurrentState.Enter();

        Debug.Log("현재 상태: " + MonsterModel.CurrentState);
    }

    public void SetTarget(Transform target) => MonsterModel.Target = target;


    public void OnMove(Vector2 target, float speed)
    {
        transform.position = Vector2.MoveTowards
            (
            transform.position,
            target,
            speed * Time.deltaTime
            );
    }

    public void TakeDamage(float damage)
    {
        MonsterModel.Hp -= damage;
    }

}


public enum MonsterState
{
    Patrol, Chase, Search, BackReturn, Attack
}