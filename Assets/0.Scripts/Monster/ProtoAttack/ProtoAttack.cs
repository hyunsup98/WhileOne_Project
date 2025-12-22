using System;
using System.Collections;
using UnityEngine;

public class ProtoAttack : IAttack
{
    private GameObject _attackPrefab;
    private MonsterPresenter _monster;
    private Transform _myTransform;
    private Vector2 _target;
    private float _attackSpeed = 7f;
    private GameObject _attackObj;
    private AttackEffect _attackEffect;
    private float _attackTime;

    public bool IsAttack { get; private set; }

    public ProtoAttack(MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = monster.View.transform;
        _attackPrefab = monster.AttackEffect;
    }


    public void StartAttack()
    {
        _target = _monster.Model.Target.position;
        IsAttack = true;

        _attackObj = GameObject.Instantiate
            (
            _attackPrefab,
            _myTransform.position,
            Quaternion.identity,
            _myTransform
            );

        _attackEffect = _attackObj.GetComponent<AttackEffect>();
        _attackEffect.OnAttack += OnCrash;

        _target = (_target - (Vector2)_myTransform.position).normalized;
    }

    public void OnAttack()
    {
        _attackTime += Time.deltaTime;

        Vector2 start = _myTransform.position;
        int layerMask = LayerMask.GetMask("Wall");
        RaycastHit2D hit = Physics2D.Raycast(start, _target, 0.5f, layerMask);

        if(_attackTime >= 0.5f)
        {
            _attackTime = 0;
            IsAttack = false;
        }

        if (hit.collider != null)
            return;
        else
        {
            _myTransform.Translate(_target * Time.deltaTime * _attackSpeed);
        }

    }

    public void EndAttack()
    {
        GameObject.Destroy(_attackObj);
        _attackEffect.Init();
    }

    private void OnCrash(Collider2D collision)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int wallLayer = LayerMask.NameToLayer("Wall");

        if (collision.gameObject.layer == playerLayer)
        {
            Player player = collision.GetComponent<Player>();
            player.TakenDamage(_monster.Att, _myTransform.position);
        }
    }
}