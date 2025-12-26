using System;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public event Action<Collider2D> OnAttack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnAttack?.Invoke(collision);
    }

    public void Init() => OnAttack = null;

}
