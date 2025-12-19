using UnityEngine;
using System;

public class WeaponManager : MonoBehaviour
{
    public WeaponData[] weaponDatabase;
    private WeaponInstance _equippedWeapon;

    // Box에서 무기 타입으로 장착
    public void EquipWeapon(WeaponType type)
    {
        WeaponData data = Array.Find(weaponDatabase, w => w.weaponType == type);
        if (data == null)
        {
            Debug.LogWarning($"WeaponData 없음: {type}");
            return;
        }

        _equippedWeapon = new WeaponInstance(data);
        Debug.Log($"무기 장착: {type}");
    }

    // Player 기본 공격력 + 무기 공격력
    public int GetFinalAttack(Player player)
    {
        if (_equippedWeapon == null || _equippedWeapon.IsBroken)
            return player.Attack;

        return player.Attack + _equippedWeapon.data.attackPower;
    }

    // 공격 시 호출
    public void OnAttack()
    {
        if (_equippedWeapon == null) return;

        _equippedWeapon.ConsumeDurability();

        if (_equippedWeapon.IsBroken)
        {
            Debug.Log("무기 파손");
            _equippedWeapon = null;
        }
    }

    public string GetWeaponInfo()
    {
        if (_equippedWeapon == null) return "무기 없음";
        return $"{_equippedWeapon.data.weaponType} " +
               $"({_equippedWeapon.currentDurability}/{_equippedWeapon.data.maxDurability})";
    }
}
