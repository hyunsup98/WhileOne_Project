using UnityEngine;

[CreateAssetMenu(fileName = "Monster_Action1DataSO", menuName = "Scriptable Objects/Data/Monster_Action1DataSO")]
public class Monster_Action1DataSO : TableBase<int>
{
    // 몬스터 행동 ID (몬스터ID + 행동분류)
    [field: SerializeField] public int actionID { get; private set; }

    // 몬스터 이름
    [field: SerializeField] public string monsterName { get; private set; }

    // 해당 행동 몬스터 ID
    [field: SerializeField] public int actionMonster { get; private set; }

    // "행동 분류(1.일반공격, 2. 돌진공격… 등)"
    [field: SerializeField] public int actionClass { get; private set; }

    // 액션 조건(플레이어와 몬스터의 거리)
    [field: SerializeField] public float actionCondition { get; private set; }

    // "액션 타입 (1.공격형,2.버프형)"
    [field: SerializeField] public int actionType { get; private set; }

    // 행동 시전 시간
    [field: SerializeField] public float actionCastTime { get; private set; }

    // 행동 대미지
    [field: SerializeField] public float actionDamage { get; private set; }

    // 행동 주기
    [field: SerializeField] public float actionCooltime { get; private set; }

    // 시전 스프라이트
    [field: SerializeField] public Sprite castResourcePath_Sprite { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override int GetID() => actionID;
}
