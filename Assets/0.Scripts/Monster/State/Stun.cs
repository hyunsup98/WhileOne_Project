using System.Collections.Generic;
using UnityEngine;

public class Stun : IState
{
    private MonsterPresenter _monster;
    private IStunable _stun;
    private Transform _myTransform;

    private float _stunTime = 5;

    private Collider2D _collider;
    private float _timer;

    public Stun(MonsterPresenter monster)
    {
        _monster = monster;
        _stun = monster.View;
        _myTransform = monster.View.MyTransform;
        _collider = _myTransform.GetComponentInChildren<Collider2D>();
    }



    public void Enter()
    {
        _monster.OnPlayAni("Stun");
        _collider.enabled = false;
        Debug.Log("<color=yellow>스턴상태 돌입</color>");
    }

    public void Exit()
    {
        _monster.OnPlayAni("Teleport");

        Debug.Log("<color=blue>스턴상태 해제</color>");
        _timer = 0;
        _monster.SetIsUlt(true);
        _stun.SetStun(false);
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        if (!_collider.enabled && _timer > 1f)
            _collider.enabled = true;

        if (_timer > _stunTime + 0.5f)
        {
            Debug.Log("타이머" + _timer);
            _monster.Model.SetState(MonsterState.Action);
        }
    }
}
