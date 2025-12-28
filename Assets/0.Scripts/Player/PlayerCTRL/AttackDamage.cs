using System;
using UnityEngine;

public class AttackDamage : MonoBehaviour
{
    BoxCollider2D _collider;

    public event Action<GameObject> OnHit;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {

            if (collision.TryGetComponent<MonsterView>(out var monster))
            {
                if(GameManager.Instance.player != null)
                {
                    WeaponChange weapon = GameManager.Instance.player.Player_WeaponChange;

                    // 무기 공격력 + 보물로 인한 추가 공격력만큼 대미지 입힘
                    monster.OnHit(weapon.currentweapon.WeaponData.weaponAttack1Damage + DataManager.Instance.CharacterData._bonusAtk);

                    if (weapon.currentweapon == weapon._slotWeapon2)
                    {
                        weapon._slotWeapon2.ReduceDurability(1);
                        GameManager.Instance.CurrentDungeon.EquipSlotController.ChangeSubWeaponDurability(weapon._slotWeapon2.Durability, weapon._slotWeapon2.WeaponData.weaponDurability);

                        if (weapon._slotWeapon2.Durability <= 0)
                            weapon._slotWeapon2 = weapon.currentweapon = null;
                    }
                }
            }


            //OnHit?.Invoke(collision.gameObject);
           

        }
    }
}
