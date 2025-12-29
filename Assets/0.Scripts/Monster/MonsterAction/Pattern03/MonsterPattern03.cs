using System.Collections;
using UnityEngine;

public class MonsterPattern03 : MonsterPattern
{

    private int _teleportCount;
    private int _currentCount;
    private Vector2 _returnPos;

    private Transform _myTransform;
    
    public MonsterPattern03(Pattern03SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _myTransform = monster.View.MyTransform;
        _damage = actionData.ActionDamage;
        _maxCoolTime = actionData.ActionCoolTime;

        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;
        _returnPos = actionData.TeleportPoint.position;
    }


    public override void EndAction()
    {
        IsAction = true;
        _ani.OnPlayAni("Idle");
        if (_currentCount < _teleportCount)
        {
            Debug.Log("<color=red>피격카운트</color>" + _currentCount);
            IsAction = false;
            return;
        }

        _monster.StartCoroutine(OnTelepor());

    }

    public override void OnAction()
    {
    }

    public override void StartAction()
    {
        if (_currentCount >= _teleportCount)
            _currentCount = 0;
        Init();
    }

    private IEnumerator OnTelepor()
    {
        _ani.OnPlayAni("Pattern03");
        yield return CoroutineManager.waitForSeconds(_beforeDelay);

        _myTransform.position = _returnPos;
        
        yield return CoroutineManager.waitForSeconds(_afterDelay);
        IsAction = false;
    }
}