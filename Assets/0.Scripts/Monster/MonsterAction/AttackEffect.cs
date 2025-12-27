using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    protected float _damage;

    //public event Action<Collider2D> OnAttack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //OnAttack?.Invoke(collision);

        DealDamage(collision);
    }

    public virtual void DealDamage(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out var player))
            player.GetDamage.TakenDamage(_damage, transform.position);
    }

    public void SetDamage(float damage) => _damage = damage;

    //public void Init() => OnAttack = null;

}
