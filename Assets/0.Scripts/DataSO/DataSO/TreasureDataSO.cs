using UnityEngine;

[CreateAssetMenu(fileName = "TreasureDataSO", menuName = "Scriptable Objects/Data/TreasureDataSO")]
public class TreasureDataSO : TableBase<int>
{
    // 보물 ID
    [field: SerializeField] public int treasureID { get; private set; }

    // 보물 이름
    [field: SerializeField] public string treasure_Name { get; private set; }

    // 보물 설명
    [field: SerializeField] public string treasure_Desc { get; private set; }

    // "보물 티어 일반 1, 고급2"
    [field: SerializeField] public int treasureTier { get; private set; }

    // 캐릭터 파워업 여부
    [field: SerializeField] public bool isPowerUp { get; private set; }

    // 공격력 증가율 (단위 : +2)
    [field: SerializeField] public int damageBoost { get; private set; }

    // 보물 모델 리소스
    [field: SerializeField] public Sprite treasureResourcePath_Sprite { get; private set; }

    // 해당 Treasure_Icon 파일경로
    [field: SerializeField] public Sprite treasureIconPath_Sprite { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override int GetID() => treasureID;
}
