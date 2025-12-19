using System;
using System.Collections;
using UnityEngine;

public class ProtoAttack : IAttack
{
    private GameObject _attackPrefab;
    private Monster _monster;
    private Vector2 _target;
    private float _attackSpeed = 7f;
    private GameObject _attackObj;
    private AttackEffect _attackEffect;
    private float _attackTime;

    public bool IsAttack { get; private set; }

    public ProtoAttack(Monster monster)
    {
        _monster = monster;
        _attackPrefab = monster.AttackEffect;
    }


    public void StartAttack()
    {
        _target = _monster.Model.Target.position;
        IsAttack = true;

        _attackObj = GameObject.Instantiate
            (
            _attackPrefab,
            _monster.transform.position,
            Quaternion.identity,
            _monster.transform
            );

        _attackEffect = _attackObj.GetComponent<AttackEffect>();
        _attackEffect.OnAttack += OnCrash;

        _target = (_target - (Vector2)_monster.transform.position).normalized;
    }

    public void OnAttack()
    {
        _attackTime += Time.unscaledDeltaTime;

        Vector2 start = _monster.transform.position;
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
            _monster.transform.Translate(_target * Time.deltaTime * _attackSpeed);
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
        Debug.Log("접촉 대상" + collision.gameObject.name);

        if (collision.gameObject.layer == playerLayer)
        {
            Player player = collision.GetComponent<Player>();

            player.TakenDamage(_monster.Att);
            Debug.Log("<color=red>테이크 데미지</color>");
            Debug.Log(player.Hp);
        }
    }
}