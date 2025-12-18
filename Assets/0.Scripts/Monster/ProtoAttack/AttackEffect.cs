using System;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public event Action OnAttack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            OnAttack?.Invoke();
    }

    public void Init() => OnAttack = null;

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position, collider.size);
    }
}
