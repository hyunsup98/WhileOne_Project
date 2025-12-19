using UnityEngine;

//무기 데이터 관리
[CreateAssetMenu(menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("무기 타입")]
    public WeaponType weaponType;

    [Header("공격력")]
    [Tooltip("무기 공격력 (Player 공격력에 더해짐)")]
    public int attackPower = 5;

    [Header("내구도")]
    [Tooltip("최대 내구도")]
    public int maxDurability = 100;
}
