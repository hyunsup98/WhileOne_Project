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
        _attackEffect.OnAttack += OnDamage;

        _target = (_target - (Vector2)_monster.transform.position).normalized;
    }

    public void OnAttack()
    {
        _attackTime += Time.unscaledDeltaTime;
        _monster.transform.Translate(_target * Time.deltaTime * _attackSpeed);

        if(_attackTime >= 0.5f)
        {
            _attackTime = 0;
            IsAttack = false;
        }
    }

    public void EndAttack()
    {
        GameObject.Destroy(_attackObj);
        _attackEffect.Init();
    }


    private void OnDamage(Player player)
    {
        Debug.Log("<color=red>테이크 데미지</color>");
        player.TakenDamage(_monster.Att);
        Debug.Log(player.Hp);
    }
}