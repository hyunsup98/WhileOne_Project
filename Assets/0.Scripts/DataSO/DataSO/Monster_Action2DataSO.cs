using UnityEngine;

[CreateAssetMenu(fileName = "Monster_Action2DataSO", menuName = "Scriptable Objects/Data/Monster_Action2DataSO")]
public class Monster_Action2DataSO : TableBase<int>
{
    // 몬스터 행동 ID (몬스터ID + 행동분류)
    [field: SerializeField] public int actionID { get; private set; }

    // 해당 행동 텀
    [field: SerializeField] public float repeatTerm { get; private set; }

    // 시전 횟수
    [field: SerializeField] public int repeatTime { get; private set; }

    // 액션 타임
    [field: SerializeField] public float actionTime { get; private set; }

    // 행동 모형(1-사각형2-타원)
    [field: SerializeField] public int actionShape { get; private set; }

    // 행동 가로 넓이
    [field: SerializeField] public float actionWidth { get; private set; }

    // 행동 세로 길이
    [field: SerializeField] public float actionLength { get; private set; }

    // 공격력 증가량
    [field: SerializeField] public float attackBoost { get; private set; }

    // 이동속도 증가량
    [field: SerializeField] public float speedBoost { get; private set; }

    // 행동 프리뷰 시간
    [field: SerializeField] public float pathPreviewTime { get; private set; }

    // 행동 프리뷰 이미지 경로
    [field: SerializeField] public Sprite pathPreviewImagePath_Sprite { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override int GetID() => actionID;
}
