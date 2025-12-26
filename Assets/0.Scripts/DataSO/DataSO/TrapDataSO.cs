using UnityEngine;

[CreateAssetMenu(fileName = "TrapDataSO", menuName = "Scriptable Objects/Data/TrapDataSO")]
public class TrapDataSO : TableBase<int>
{
    // 함정 ID
    [field: SerializeField] public int trapID { get; private set; }

    // 함정 이름
    [field: SerializeField] public string trapName { get; private set; }

    // 함정 설명
    [field: SerializeField] public string trapDesc { get; private set; }

    // 함정 공격 대미지
    [field: SerializeField] public float trapAttackDamage { get; private set; }

    // "함정 공격 주기(공격 쿨다운 개념, 해당 시간마다 1대씩 때린다)"
    [field: SerializeField] public float trapAttackInterval { get; private set; }

    // 함정 모델 리소스경로
    [field: SerializeField] public Sprite trapResourcePath_Sprite { get; private set; }

    // 함정 이동 여부
    [field: SerializeField] public bool isTrapMove { get; private set; }

    // 함정 이동 속도
    [field: SerializeField] public int trapSpeed { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override int GetID() => trapID;
}
