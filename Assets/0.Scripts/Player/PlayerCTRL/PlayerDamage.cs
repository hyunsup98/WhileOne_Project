using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private bool _isDamage;
    private bool _isCoroutine;
    private bool _isknock;
    [SerializeField] float time = 0.12f;

    SpriteRenderer[] allRender;
    float alpha = 1f;

    public bool IsDamaged { get { return _isDamage; } set { _isDamage = value; } }
    public bool IsKnock { get { return _isknock; } set { _isknock = value; } }



    private void Awake()
    {
        _rg2d = GetComponentInParent<Rigidbody2D>();
        _player = transform.root.GetComponent<Player>();
    }
    private void Start()
    {
        allRender = GetComponentsInChildren<SpriteRenderer>();
        _blinkTime = new WaitForSeconds(time);
        _group = transform.GetChild(0).GetComponent<SortingGroup>();
    }

    private void Update()
    {
        if(Keyboard.current.qKey.wasPressedThisFrame)
        {
            TakenDamage(10f, Vector2.zero);
        }
    }

    public void TakenDamage(float damage, Vector2 target)
    {

        if (IsDamaged || _player.PlayerDash.IsDash)
        {
            return;
        }
        _player.ChangedHealth -= damage;
        if (!_isknock)
        {
            _isknock = true;
            StartCoroutine(KnockBack(target));
        }
        if(_isCoroutine == false)
        {
            StartCoroutine(Blink());
        }
        
    }
    IEnumerator Blink()
    {

        gameObject.tag = "Untagged";
        _checkTime = 0;
        _isDamage = true;
        _isCoroutine = true;
        while (_finsihTime > _checkTime)
        {
            alpha = (allRender[0].color.a == 1f) ? 0.5f : 1f;
            foreach (var sr in allRender)
            {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
            }
            yield return new WaitForSeconds(0.2f);
            _checkTime += time;
        }
        foreach (var sr in allRender)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
        gameObject.tag = "Player";
        _isCoroutine = false;
        _isDamage = false;

    }
    IEnumerator KnockBack(Vector2 target)
    {
        Vector3 dir = (transform.position - (Vector3)target).normalized;
        _rg2d.linearVelocity = _player.MoveSpeed * 1.2f * dir;
        yield return new WaitForSeconds(0.2f);

        _rg2d.linearVelocity = Vector2.zero;
        _isknock = false;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
