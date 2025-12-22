using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public Transform target;

    public void Angle(Vector2 Target)
    {
        float Sight = 5;

        Vector2 dir = Target - (Vector2)transform.position;

        float dot = Vector2.Dot(dir.normalized, transform.right);
        Debug.DrawRay(transform.position, transform.right);

        float Angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        Debug.Log("<color=blue>각도</color>" + Angle);
    }

    public void Frontal()
    {
        Vector2 dir = (target.position - transform.position);
        Debug.LogWarning("dir:" + dir);
        Debug.LogWarning("dir의 x:" + dir.x);

        Vector2 frontal = new Vector2(dir.x, 0).normalized;
        Debug.Log("프론트 벡터" + frontal);
    }


    private void Update()
    {
        Frontal();
    }
}
