using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerDamage : MonoBehaviour
{
    private Player _player;

    private Rigidbody2D _rg2d;

    //레이어 관련 변수
    private SortingGroup _group;

    private float _finsihTime = 0.7f;
    private float _checkTime = 0;
    private WaitForSeconds _blinkTime;
    [SerializeField] float time = 0.12f;
    private bool _isDamage;

    public bool IsDamaged { get { return _isDamage; } set { _isDamage = value; } }

    private void Awake()
    {
        _rg2d = GetComponent<Rigidbody2D>();
        _player = transform.root.GetComponent<Player>();
    }
    private void Start()
    {
        _blinkTime = new WaitForSeconds(time);
        _group = transform.GetChild(0).GetComponent<SortingGroup>();
    }
    public void TakenDamage(float damage, Vector2 target)
    {
        IsDamaged = true;
        _player.ChangedHealth -= damage;
        StartCoroutine(KnockBack(target));
        StartCoroutine(Blink());
    }
    IEnumerator Blink()
    {
        _checkTime = 0;
        gameObject.layer = LayerMask.NameToLayer("PlayerDamage");

        while (_finsihTime > _checkTime)
        {
            _group.sortingOrder = 0;

            yield return _blinkTime;

            _group.sortingOrder = 3;

            _checkTime += time;

            yield return _blinkTime;
        }
        gameObject.layer = LayerMask.NameToLayer("Player");
        _isDamage = false;
        yield return null;

    }
    IEnumerator KnockBack(Vector2 target)
    {
        int dirx = transform.position.x - target.x > 0 ? 1 : -1;
        int diry = transform.position.y - target.y > 0 ? 1 : -1;
        _rg2d.linearVelocityX = dirx * 1f;
        _rg2d.linearVelocityY = diry * 1f;
        yield return new WaitForSeconds(0.5f);
        _rg2d.linearVelocity = Vector2.zero;
    }
}
