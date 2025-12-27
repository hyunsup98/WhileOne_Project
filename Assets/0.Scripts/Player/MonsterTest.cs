using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    [SerializeField]Player _player;

    int _layer;
   
    //private void OnTriggerStay2D(Collision2D collision)
    //{
    //    GameObject hitTag = collision.otherCollider.gameObject;
    //    if (hitTag.gameObject.CompareTag("Player"))
    //    {
    //        _player.GetDamage.TakenDamage(10,gameObject.transform.position);
    //    }
    //}
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var damaged = collision.GetComponent<PlayerDamage>();
            if (damaged != null)
            {
                _player.GetDamage.TakenDamage(10, transform.position);
            }
        }
    }
}
