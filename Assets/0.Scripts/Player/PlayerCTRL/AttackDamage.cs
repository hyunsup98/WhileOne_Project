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
                monster.OnHit(25f);

                if(GameManager.Instance.player != null)
                {
                    WeaponChange weapon = GameManager.Instance.player.Player_WeaponChange;

                    if(weapon.currentweapon == weapon._slotWeapon2)
                    {
                        weapon._slotWeapon2.ReduceDurability(1);

                        if (weapon._slotWeapon2.Durability <= 0)
                            weapon._slotWeapon2 = weapon.currentweapon = null;
                    }
                }
            }


            //OnHit?.Invoke(collision.gameObject);
           

        }
    }
}
