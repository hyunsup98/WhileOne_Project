using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrakingPlayer : MonoBehaviour
{
    [SerializeField] GameObject _attackFX;
    private GameObject _attackFxInstance;
    Vector3 _mousePosition;
    private SpriteRenderer _rend;

    public GameObject AttackFX => _attackFX;
    private void Awake()
    {
        if(_attackFX != null)
        {
           _attackFxInstance = Instantiate(_attackFX);
           _attackFxInstance.SetActive(false);
        }
        _rend = _attackFxInstance.GetComponent<SpriteRenderer>();
    }

    public void PlayEffect()
    {
        if (_attackFxInstance == null) return;

        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _mousePosition.z = 0f;
        Vector2 dir = (_mousePosition - transform.position).normalized;

        if (gameObject.transform.root.localScale.x > 0)
        {
            _attackFxInstance.transform.position = gameObject.transform.root.position + new Vector3(-0.5f, Mathf.Clamp(dir.y, -0.1f, 0.7f), 0);
            _attackFxInstance.transform.rotation = Quaternion.Euler(_mousePosition);
            _rend.flipX = true;
        }
        else
        {
            _attackFxInstance.transform.position = gameObject.transform.root.position + new Vector3(0.5f, Mathf.Clamp(dir.y, -0.1f, 0.7f), 0);
            _attackFxInstance.transform.rotation = Quaternion.Euler(_mousePosition);
            _rend.flipX = false;
        }

        _attackFxInstance.SetActive(false);
        _attackFxInstance.SetActive(true);

    }
   
}
