using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Monster : MonoBehaviour
{
    [SerializeField] private MonsterDataSO _monsterData;    // 몬스터 데이터 SO
    [SerializeField] private Tilemap _wallTilemap;          // 경로 탐색을 위한 타일맵

    [field: SerializeField] public MonsterView View { get; private set; }
    
    public MonsterModel Model { get; private set; }  // 현재 몬스터 데이터 보관한 Model
    

    // 추후 지워야 할 목록
    public GameObject AttackEffect;
    [field: SerializeField] public float Att {  get; private set; }
    [field: SerializeField] public float AttRange {  get; private set; }
    public IAttack Attack {  get; private set; }




    private void Awake()
    {
        Model = new MonsterModel(_monsterData);

        // 경로 탐색으로 순찰 포인트 초기화
        Model.MobAstar = new Astar(_wallTilemap);
        Model.PatrolPoint = Model.MobAstar.Pathfinder
            (
            Model.PatrolTarget[0].position,
            Model.PatrolTarget[1].position
            );

        // 공격 세팅
        Attack = new ProtoAttack(this);

        // 상태 패턴 세팅
        Model.MonsterState = new Dictionary<MonsterState, IState>();
        Model.MonsterState.Add(MonsterState.Patrol, new Patrol(this));
        Model.MonsterState.Add(MonsterState.Chase, new Chase(this));
        Model.MonsterState.Add(MonsterState.Search, new Search(this));
        Model.MonsterState.Add(MonsterState.BackReturn, new BackReturn(this));
        Model.MonsterState.Add(MonsterState.Attack, new MonsterAttack(this));
        Model.CurrentState = Model.MonsterState[MonsterState.Patrol];
    }

    private void Update()
    {
        Model.CurrentState.Update();
    }

    public void SetState(MonsterState state)
    {
        Debug.Log("이전 상태: " + Model.CurrentState);

        Model.CurrentState?.Exit();
        Model.CurrentState = Model.MonsterState[state];
        Model.CurrentState.Enter();

        Debug.Log("현재 상태: " + Model.CurrentState);
    }

    public void SetTarget(Transform target) => Model.Target = target;


    public void OnMove(Vector2 target, float speed)
    {
        OnTurn(target);

        transform.position = Vector2.MoveTowards
            (
            transform.position,
            target,
            speed * Time.deltaTime
            );
    }

    // 타겟의 방향으로 몸을 돌리는 로직
    public void OnTurn(Vector2 target)
    {
        Vector2 dir = target - (Vector2)transform.position;
        Vector2 dirX = new Vector2(dir.x, 0f).normalized;
        if (dirX.x != 0f)
            transform.localScale = new Vector3(dirX.x, 1f, 1f);
    }

    // target방향으로 LOS를 발사했을 때, 플레이어와 직선 거리에 존재시에 true 반환
    public RaycastHit2D OnLOS(Vector2 target)
    {
        Vector2 start = transform.position;
        Vector2 dir = target - start;

        int layerMask = LayerMask.GetMask("Wall", "Player");
        RaycastHit2D hit = Physics2D.Raycast(start, dir, Model.Sight, layerMask);

        return hit;
    }

    public void TakeDamage(float damage)
    {
        View.OnHurtAni();
        Model.Hp -= damage;
        Debug.Log("몬스터HP" + Model.Hp);

        if(Model.Hp <= 0f)
            View.OnDeathAni();
    }

}


public enum MonsterState
{
    Patrol, Chase, Search, BackReturn, Attack
}