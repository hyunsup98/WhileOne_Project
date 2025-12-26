using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "Scriptable Objects/Data/CharacterDataSO")]
public class CharacterDataSO : TableBase<string>
{
    // 캐릭터ID
    [field: SerializeField] public string characterID { get; private set; }

    // 캐릭터HP
    [field: SerializeField] public float characterHP { get; private set; }

    // 캐릭터ATK
    [field: SerializeField] public float characterATK { get; private set; }

    // 캐릭터 스테미나
    [field: SerializeField] public float stamina { get; private set; }

    // 스테미나 재생
    [field: SerializeField] public float staminaRecovery { get; private set; }

    // 캐릭터 이동속도
    [field: SerializeField] public float characterSpeed { get; private set; }

    // 캐릭터 공격속도
    [field: SerializeField] public float attackSpeed { get; private set; }

    // 캐릭터 모델링리소스
    [field: SerializeField] public Sprite characterModelPath_Sprite { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override string GetID() => characterID;
}
