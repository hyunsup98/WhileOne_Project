using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDataSO", menuName = "Scriptable Objects/Data/MonsterDataSO")]
public class MonsterDataSO : TableBase<int>
{
    // 몬스터ID
    [field: SerializeField] public int monsterID { get; private set; }

    // 몬스터 이름
    [field: SerializeField] public string monsterName { get; private set; }

    // 몬스터 등급
    [field: SerializeField] public int tier { get; private set; }

    // 몬스터 HP
    [field: SerializeField] public float hp { get; private set; }

    // 몬스터 이동속도
    [field: SerializeField] public float monsterSpeed { get; private set; }

    // 몬스터 시야
    [field: SerializeField] public float sight { get; private set; }

    // 몬스터 행동01
    [field: SerializeField] public int action1 { get; private set; }

    // 몬스터 행동02
    [field: SerializeField] public int action2 { get; private set; }

    // 몬스터 행동03
    [field: SerializeField] public int action3 { get; private set; }

    // 몬스터 행동04
    [field: SerializeField] public int action4 { get; private set; }

    // 몬스터 행동05
    [field: SerializeField] public int action5 { get; private set; }

    // 몬스터 행동06
    [field: SerializeField] public int action6 { get; private set; }

    // 몬스터 행동07
    [field: SerializeField] public int action7 { get; private set; }

    // 몬스터 모델 리소스
    [field: SerializeField] public Sprite monsterModelPath_Sprite { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override int GetID() => monsterID;
}
