using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public Tilemap tilemap;

    private void Update()
    {
        var myPos = tilemap.WorldToCell(transform.position);

        Debug.Log(myPos);
    }
}
