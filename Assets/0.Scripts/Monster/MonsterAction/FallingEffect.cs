using System.Collections;
using UnityEngine;

public class FallingEffect : ActionEffect
{
    [SerializeField] private GameObject _fallingPathPreview;
    private float _timer;

    private void Start()
    {
        Destroy(_fallingPathPreview, 0.7f);
        Destroy(gameObject, 1.4f);
        
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= 0.9f)
            GetComponent<Collider2D>().enabled = true;
    }

    public override void DealDamage(Collider2D collision)
    {

        if (collision.TryGetComponent<IStunable>(out var monster) && !monster.IsStun)
            monster.OnStun();

        if (collision.TryGetComponent<Player>(out var player))
            player.GetDamage.TakenDamage(_damage, transform.position);
    }
}
