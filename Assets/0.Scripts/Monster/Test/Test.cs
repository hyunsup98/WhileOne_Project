using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public List<Transform> list = new List<Transform>();
    public Tilemap tilemap;

    public float speed = 5;
    private float _maxAngle = 60;

    public Vector2 target;

    private void Start()
    {
        Astar astar = new Astar(tilemap);

        List<Vector2> vector2s = astar.Pathfinder(list[0].position, list[1].position);

        foreach (var pos in vector2s)
            Debug.Log(pos);
    }

    private void Update()
    {
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Vector2 start = transform.position;

    //    Vector2 dirNomlized = (target - start).normalized;


    //    for (float f = -_maxAngle; f <= _maxAngle; f++)
    //    {
    //        float rad = f * Mathf.Deg2Rad;
    //        Vector2 dir = Quaternion.Euler(0, 0, f) * dirNomlized;
    //        dir += start;

    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(start, dir);
    //    }

    //}

}
