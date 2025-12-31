using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MonsterPattern03 : MonsterPattern
{
    private Tilemap _wallTilemap;
    private Tilemap _groundTilemap;
    private int _teleportCount;
    private int _currentCount = 1;
    private Vector2 _returnPos;

    private Transform _myTransform;
    private Vector3 _centerPos;
    
    public MonsterPattern03(Pattern03SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _wallTilemap = monster.Model.WallTilemap;
        _groundTilemap = monster.Model.GroundTilemap;
        _centerPos = GetMapCenter().position;
        _myTransform = monster.View.MyTransform;
        _damage = actionData.ActionDamage;
        _maxCoolTime = actionData.ActionCoolTime;
        _sfxID = actionData.ActionSound;

        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;
        _teleportCount = actionData.TeleportCount;
        _returnPos = GetVector();
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

    // 맵 중앙을 찾는 메서드
    private Transform GetMapCenter()
    {
        Transform parent = _myTransform.parent;
        if (parent.CompareTag("Monster"))
            parent = parent.parent;

        foreach (Transform child in parent)
        {
            if (child.CompareTag("RoomCenterMarker"))
                return child;
        }

        Debug.LogError("맵 중앙을 찾지 못했습니다.");
        return null;
    }

    private Vector2 GetVector()
    {
        int count = 0;
        Vector3 pos = new Vector3(Random.Range(-15, 16), Random.Range(-15, 16), 0) + _centerPos;
        Vector3Int tilePos = _wallTilemap.WorldToCell(pos);
        while (count > 100 || _wallTilemap.HasTile(tilePos) || !_groundTilemap.HasTile(tilePos))
        {
            pos = new Vector3(Random.Range(-15, 16), Random.Range(-15, 16), 0) + _centerPos;
            count++;
        }

        return pos;

    }
}