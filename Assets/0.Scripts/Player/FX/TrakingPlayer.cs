using UnityEngine;
using UnityEngine.InputSystem;

public class TrakingPlayer : MonoBehaviour
{
    [SerializeField] private Transform _standard;
    Vector3 _mousePosition;
    private SpriteRenderer _rend;
    private void Awake()
    {
        _rend = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 relativePosition = _mousePosition - _standard.position;
        Vector2 dir = relativePosition.normalized;
        Debug.Log(dir);
        if (_standard.localScale.x > 0)
        {
            gameObject.transform.position = _standard.position + new Vector3(-0.5f, Mathf.Clamp(dir.y, -0.1f,0.7f), 0);
            gameObject.transform.rotation = Quaternion.Euler(_mousePosition);
            _rend.flipX = true;
        }
        else
        {
            gameObject.transform.position = _standard.position + new Vector3(0.5f, Mathf.Clamp(dir.y, - 0.1f, 0.7f),0);
            gameObject.transform.rotation = Quaternion.Euler(_mousePosition);
            _rend.flipX = false;
        }

    }

    private void OnDisable()
    {
        gameObject.transform.position = Vector3.zero;
    }
}
