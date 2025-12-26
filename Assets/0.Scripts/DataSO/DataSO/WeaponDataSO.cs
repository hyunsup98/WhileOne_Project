using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDataSO", menuName = "Scriptable Objects/Data/WeaponDataSO")]
public class WeaponDataSO : TableBase<int>
{
    // 무기 ID
    [field: SerializeField] public int weaponID { get; private set; }

    // 무기 이름
    [field: SerializeField] public string weapon_Name { get; private set; }

    // 무기 설명
    [field: SerializeField] public string weapon_Desc { get; private set; }

    // "무기 등급(1-일반, 2-고급)"
    [field: SerializeField] public string weaponTier { get; private set; }

    // 무기 공격1 평균 대미지
    [field: SerializeField] public float weaponAttack1Damage { get; private set; }

    // 무기 공격1 최소 대미지
    [field: SerializeField] public float weaponAttack1MinDamage { get; private set; }

    // 무기 공격1 최대 대미지
    [field: SerializeField] public float weaponAttack1MaxDamage { get; private set; }

    // 무기 공격1 공격속도(ex. 2 = 2초당 1대)
    [field: SerializeField] public float weaponAttack1Speed { get; private set; }

    // 무기 이동속도 증가량
    [field: SerializeField] public float weaponMoveSpeed { get; private set; }

    // 무기 스태미너 회복증가량
    [field: SerializeField] public float weaponStaminaRecovery { get; private set; }

    // 무기 공격1 모델 리소스 경로
    [field: SerializeField] public Sprite weaponAttack1ModelPath_Sprite { get; private set; }

    // 무기 특수기능 평균 대미지
    [field: SerializeField] public float weaponAttack2Damage { get; private set; }

    // 무기 특수스킬 최소 대미지
    [field: SerializeField] public float weaponAttack2MinDamage { get; private set; }

    // 무기 특수스킬 최대 대미지
    [field: SerializeField] public float weaponAttack2MaxDamage { get; private set; }

    // 무기 특수스킬 공격속도
    [field: SerializeField] public float weaponAttack2Speed { get; private set; }

    // 무기 특수스킬 쿨다운
    [field: SerializeField] public float weaponAttack2Cooldown { get; private set; }

    // 무기 특수스킬 스태미나 소모량
    [field: SerializeField] public float weaponAttack2Stamina { get; private set; }

    // 무기 특수스킬 모델 리소스 경로
    [field: SerializeField] public Sprite weaponAttack2ModelPath_Sprite { get; private set; }

    // 무기 모델 리소스 경로
    [field: SerializeField] public Sprite weaponResourcePath_Sprite { get; private set; }

    // 무기 내구도
    [field: SerializeField] public int weaponDurability { get; private set; }

    // 해당 무기 드랍률
    [field: SerializeField] public int weaponDropRate { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override int GetID() => weaponID;
}
