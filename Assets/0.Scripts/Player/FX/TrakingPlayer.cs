using UnityEngine;
using UnityEngine.InputSystem;

public class TrakingPlayer : MonoBehaviour
{
    [SerializeField] private Transform _standard;
    [SerializeField] private Transform _trans;
    Mouse _mouse;
    Vector3 _mousePosition;
    private SpriteRenderer _rend;
    private void Awake()
    {
        _rend = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (_standard.localScale.x > 0)
        {
            gameObject.transform.position = _trans.position + new Vector3 (-0.3f,0,0);
            gameObject.transform.rotation = Quaternion.Euler(_mousePosition);
            _rend.flipX = true;
        }
        else
        {
            gameObject.transform.position = _trans.position + new Vector3(0.3f, 0, 0);
            gameObject.transform.rotation = Quaternion.Euler(_mousePosition);
            _rend.flipX = false;
        }

    }

    private void OnDisable()
    {
        gameObject.transform.position = Vector3.zero;
    }
}
