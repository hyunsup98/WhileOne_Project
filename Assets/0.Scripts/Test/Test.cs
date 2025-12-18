using UnityEngine;

public class Test : MonoBehaviour
{
    public float speed = 5;
    private float _maxAngle = 60;

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
