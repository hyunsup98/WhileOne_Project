using UnityEngine;

public class KISTest : MonoBehaviour
{
    public Transform Transform;

    private void Update()
    {
        float angle = Vector2.SignedAngle(Vector2.right, Transform.position -  transform.position);
        Debug.Log(angle);
    }
}
