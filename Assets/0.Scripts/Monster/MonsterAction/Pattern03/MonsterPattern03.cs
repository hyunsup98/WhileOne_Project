using System.Collections;
using UnityEngine;

public class MonsterPattern03 : MonsterPattern
{
    private int _teleportCount;
    private int _currentCount = 1;
    private Vector2 _returnPos;

    private Transform _myTransform;
    
    public MonsterPattern03(Pattern03SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _myTransform = monster.View.MyTransform;
        _damage = actionData.ActionDamage;
        _maxCoolTime = actionData.ActionCoolTime;
        _sfxID = actionData.ActionSound;

        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;
        _teleportCount = actionData.TeleportCount;
        _returnPos = monster.View.MyTransform.position;
    }

    public override void StartAction()
    {
        IsAction = true;
        _myTransform.GetComponentInChildren<Collider2D>().enabled = false;
        _ani.OnPlayAni("Idle");

        Debug.Log("<color=red>피격카운트</color>" + _currentCount);
        if (_currentCount < _teleportCount)
        {
            IsAction = false;
            return;
        }

        _monster.StartCoroutine(OnTelepor());
    }

    public override void EndAction()
    {
        Init();
        if (_currentCount > _teleportCount)
            _currentCount = 1;

        _currentCount++;
    }

    public override void OnAction()
    {
    }


    private IEnumerator OnTelepor()
    {
        _ani.OnPlayAni("Pattern03");
        yield return CoroutineManager.waitForSeconds(_beforeDelay);

        _myTransform.position = _returnPos;
        
        yield return CoroutineManager.waitForSeconds(_afterDelay);

        _myTransform.GetComponentInChildren<Collider2D>().enabled = true;
        IsAction = false;
    }
}