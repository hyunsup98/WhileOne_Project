//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class MonsterPresenterMVP : Monster
//{

//    public MonsterModelMVP Model { get; private set; }  // 현재 몬스터 데이터 보관한 Model

//    public MonsterViewMVP View { get; private set; }

//    // 추후 지워야 할 목록
//    public GameObject AttackEffect;
//    public float Att { get; private set; } = 10f;
//    public float AttRange { get; private set; } = 5f;
//    public IAttack Attack { get; private set; }

//    public MonsterPresenterMVP(MonsterDataSO monsterData, MonsterViewMVP monsterView, Tilemap wallTilmap)
//    {
//        Model = new MonsterModelMVP(monsterData, this);
//        View = monsterView;

//        // 경로 탐색으로 순찰 포인트 초기화
//        Model.MobAstar = new Astar(wallTilmap);
//        Model.PatrolPoint = Model.MobAstar.Pathfinder
//            (
//            Model.PatrolTarget[0].position,
//            Model.PatrolTarget[1].position
//            );

//        // 공격 세팅
//        Attack = new ProtoAttack(this);
//    }

//    public void OnUpdate()
//    {
//        Model.CurrentState.Update();
//    }

//    public void OnMove(Vector2 target, float speed)
//    {
//        OnTurn(target);

//        transform.position = Vector2.MoveTowards
//            (
//            transform.position,
//            target,
//            speed * Time.deltaTime
//            );
//    }

//    // 타겟의 방향으로 몸을 돌리는 로직
//    public void OnTurn(Vector2 target)
//    {
//        Vector2 dir = target - (Vector2)transform.position;
//        Vector2 dirX = new Vector2(dir.x, 0f).normalized;
//        if (dirX.x != 0f)
//            transform.localScale = new Vector3(dirX.x, 1f, 1f);
//    }

//    // target방향으로 LOS를 발사했을 때, 플레이어와 직선 거리에 존재시에 true 반환
//    public RaycastHit2D OnLOS(Vector2 target)
//    {
//        Vector2 start = transform.position;
//        Vector2 dir = target - start;

//        int layerMask = LayerMask.GetMask("Wall", "Player");
//        RaycastHit2D hit = Physics2D.Raycast(start, dir, Model.Sight, layerMask);

//        return hit;
//    }

//}
