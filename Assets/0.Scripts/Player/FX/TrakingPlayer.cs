using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrakingPlayer : MonoBehaviour
{
    [SerializeField] AttackDamage _attackFX;
    public Transform _test;
    public AttackDamage _attackFxInstance { get; private set; }
    Vector3 _mousePosition;
    private SpriteRenderer _rend;

    public AttackDamage AttackFX => _attackFX;
    private void Awake()
    {
        if(_attackFX != null)
        {
           _attackFxInstance = Instantiate(_attackFX);
           _attackFxInstance.gameObject.SetActive(false);
        }
        _rend = _attackFxInstance.GetComponentInChildren<SpriteRenderer>(true);
    }

    public void PlayEffect()
    {
        if (_attackFxInstance == null) return;

        Vector3 _mouse = Mouse.current.position.ReadValue();
        float distanceToCamera = Mathf.Abs(Camera.main.transform.position.z);
        _mousePosition = Camera.main.ScreenToWorldPoint(new (_mouse.x, _mouse.y, distanceToCamera));
        _mousePosition.z = 0f;

        Vector2 dir = (_mousePosition - transform.position).normalized;
        float aaa = Mathf.Abs(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        Debug.Log(aaa);

        if (transform.localScale.x > 0)
        {
            _attackFxInstance.transform.position = transform.position + (Vector3)(dir);
            _attackFxInstance.transform.right = -dir;
            _rend.flipX = true;
        }
        else
        {
            _attackFxInstance.transform.position = transform.position + (Vector3)(dir);
            _attackFxInstance.transform.right = dir;
            _rend.flipX = false;
        }
        

        _attackFxInstance.gameObject.SetActive(false);
        _attackFxInstance.gameObject.SetActive(true);

    }
   
}
