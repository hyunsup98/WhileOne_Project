using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    [SerializeField] Blink _player;

    int _layer;
   
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //_player.TakenDamage(10,gameObject.transform.position);
        }
    }
}
