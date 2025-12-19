using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = LayerMask.NameToLayer("Wall");
        Debug.Log(collision.gameObject.name);
        if(collision.gameObject.layer == layer)
        {
            Debug.Log("Ãæµ¹");
        }
    }

}
