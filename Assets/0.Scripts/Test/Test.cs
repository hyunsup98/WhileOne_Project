using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public float speed = 5;

    public Vector2 target;

    private void Start()
    {
        
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, target);

        Debug.Log("레이케스트: " + hit.collider);

        if (hit.collider != null)
            Debug.Log("이름: " + hit.transform);
    }


    

}
