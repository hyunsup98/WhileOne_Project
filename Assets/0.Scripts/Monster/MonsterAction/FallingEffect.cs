using System.Collections;
using UnityEngine;

public class FallingEffect : ActionEffect
{
    [SerializeField] private GameObject _fallingPathPreview;

    private void Start()
    {
        Destroy(_fallingPathPreview, 0.08f);
    }

    public override void DealDamage(Collider2D collision)
    {

        if (collision.TryGetComponent<IStunable>(out var monster) && !monster.IsStun)
            monster.OnStun();

        if (collision.TryGetComponent<Player>(out var player))
            player.GetDamage.TakenDamage(_damage, transform.position);
    }
}
