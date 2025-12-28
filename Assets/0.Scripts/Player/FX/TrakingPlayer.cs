using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrakingPlayer : MonoBehaviour
{
    [SerializeField] private Transform parentTrans;
    private Transform atkParent;
    [SerializeField] private float atkDistance = 1f;
    [SerializeField] private Animator attackAnim;
    [SerializeField] AttackDamage _attackFX;
    public Transform _test;
    public AttackDamage AttackFxInstance { get; private set; }
    Vector3 _mousePosition;
    private SpriteRenderer _rend;

    public AttackDamage AttackFX => _attackFX;
    private void Awake()
    {
        atkParent = Instantiate(parentTrans);

        _attackFX = atkParent.GetComponentInChildren<AttackDamage>();
        attackAnim = _attackFX.GetComponent<Animator>();

        AttackFxInstance = AttackFX;
        //if (_attackFX != null)
        //{
        //   AttackFxInstance = Instantiate(_attackFX);
        //   AttackFxInstance.gameObject.SetActive(false);
        //}
        //_rend = AttackFxInstance.GetComponentInChildren<SpriteRenderer>(true);
    }

    public void PlayEffect()
    {
        if (AttackFxInstance == null) return;

        attackAnim.SetTrigger("attack");

        Vector3 _mouse = Mouse.current.position.ReadValue();
        float distanceToCamera = Mathf.Abs(Camera.main.transform.position.z);
        _mousePosition = Camera.main.ScreenToWorldPoint(new (_mouse.x, _mouse.y, distanceToCamera));
        _mousePosition.z = 0f;

        Vector3 dir = (_mousePosition - _test.position).normalized;
        float aaa = Mathf.Abs(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        atkParent.position = _test.position + dir * atkDistance;
        atkParent.transform.right = dir;

        //if (dir.x > 0)
        //{
        //    _attackFxInstance.transform.position = _test.position + dir * distance;
        //    _attackFxInstance.transform.right = -dir;
        //    _attackFxInstance.transform.localPosition -= new Vector3(0f, +0.45f, 0f);
        //    _rend.flipX = true;
        //}
        //else
        //{
        //    _attackFxInstance.transform.position = _test.position + dir * distance;
        //    _attackFxInstance.transform.right = dir;
        //    _attackFxInstance.transform.localPosition -= new Vector3(0f, -0.45f, 0f);
        //    _rend.flipX = false;
        //}


    }
   
}
