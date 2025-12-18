using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Weapon.WeaponList currentWeapon;

    public void EquipWeapon(Weapon.WeaponList weapon)
    {
        currentWeapon = weapon;
        Debug.Log("¹«±â ÀåÂø: " + weapon);
    }
}
