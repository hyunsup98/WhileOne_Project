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

            OnHit?.Invoke(collision.gameObject);
            //if (collision.TryGetComponent<MonsterView>(out var monster))
            //{
            //    monster.Presenter.OnHit(25f);
            //}
        }
    }
}
