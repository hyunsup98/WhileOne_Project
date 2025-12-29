using UnityEngine;

public class ActionEffect : MonoBehaviour
{
    protected float _damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {

        DealDamage(collision);
    }

    public virtual void DealDamage(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out var player))
            player.GetDamage.TakenDamage(_damage, transform.position);
    }

    public void SetDamage(float damage) => _damage = damage;


}
