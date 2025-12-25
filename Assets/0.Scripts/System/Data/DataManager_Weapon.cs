using UnityEngine;

public class DataManager_Weapon : MonoBehaviour
{
    [field: SerializeField] public WeaponDatabaseSO WeaponDatabase { get; private set; }

    [SerializeField] private Weapon defaultWeapon;      // 기본 무기 프리팹

    // 랜덤으로 무기를 뽑는 메서드
    public Weapon GetRandomWeapon()
    {
        if(defaultWeapon == null) return null;

        Weapon weapon = WeaponPool.Instance.GetObject(defaultWeapon, transform);

        int totalPer = 0;   // 무기 데이터들의 드랍퍼 총합

        // 자료구조에 들어있는 무기 드랍퍼 총합 구하기
        // 0번에 들어있는 무기는 기본 무기(삽)이기 때문에 제외
        for(int i = 1; i < WeaponDatabase.datas.Count; i++)
        {
            totalPer += WeaponDatabase.datas[i].weaponDropRate;
        }
        int rand = Random.Range(0, totalPer);
        int index = 0;

        for(int i = 1; i < WeaponDatabase.datas.Count; i++)
        {
            index += WeaponDatabase.datas[i].weaponDropRate;

            if(index > rand)
            {
                weapon.WeaponData = WeaponDatabase.datas[i];
                break;
            }
        }

        return weapon;
    }
}
