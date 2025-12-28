using UnityEngine;

public class FallingEffect : AttackEffect
{
    public override void DealDamage(Collider2D collision)
    {

        if (collision.TryGetComponent<IStunable>(out var monster) && !monster.IsStun)
            monster.OnStun();

        if (collision.TryGetComponent<Player>(out var player))
            player.GetDamage.TakenDamage(_damage, transform.position);
    }
}
