using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public float speed = 5;
    public Tilemap _wallTilemap;
    public Transform target;

    private List<Vector2Int> list;
    private Astar astar;
    private int index;

    private void Awake()
    {
        astar = new Astar(_wallTilemap);
    }

    private void Start()
    {
        list = astar.Pathfinder(transform.position, target.transform.position);
    }

    private void Update()
    {
        Vector2 tempTarget = astar.GetCellPos(list[index]);

        Move(tempTarget);

        Debug.Log("목표:" + tempTarget);
        Debug.Log("거리"+ Vector2.Distance(transform.position, tempTarget));
        if (Vector2.Distance(transform.position, tempTarget) <= 0.3f)
            index++;
    }

    public void Move(Vector2 target)
    {
        target = astar.GetCellPos(target);

        Vector2 dir = ( target - (Vector2)transform.position ).normalized;

        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    

}
