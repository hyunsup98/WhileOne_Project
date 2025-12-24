using UnityEngine;

public class AttackDamage : MonoBehaviour
{
    BoxCollider2D _collider;

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
                monster.Presenter.OnHit(25f);
            }
        }
    }
}
