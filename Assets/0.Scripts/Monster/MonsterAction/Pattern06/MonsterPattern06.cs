using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MonsterPattern06 : MonsterPattern
{
    private Tilemap _wallTilemap;
    private float _startFallingTime;
    private float _fallingCount;
    private float _fallingFrequency;
    private float _fallingCycle;
    private float _fallingHitTiming;
    private float _fallingRange;

    private GameObject _fallingObjectPrefab;

    private Vector2 _mapCenterPos;
    private Transform _myTransform;
    private List<GameObject> _fallingObjects;


    public MonsterPattern06(Pattern06SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _damage = actionData.ActionDamage;
        _beforeDelay = actionData.BeforeDelay;
        _afterDelay = actionData.AfterDelay;
        _maxCoolTime = actionData.ActionCoolTime;

        _wallTilemap = monster.Model.WallTilemap;
        _myTransform = _monster.View.MyTransform;
        _sfxID = actionData.ActionSound;

        _startFallingTime = actionData.FallingStartTime;
        _fallingCount = actionData.FallingCount;
        _fallingFrequency = actionData.FallingFrequency;
        _fallingCycle = actionData.FallingCycle;
        _fallingHitTiming = actionData.FallingHitTiming;
        _fallingRange = actionData.FallingRange;
        _fallingObjectPrefab = actionData.FallingObjectPrefab;
        _fallingObjects = new List<GameObject>();
    }

    public override void StartAction()
    {
        IsAction = true;
        _monster.View.SetCollider(false);
        _ani.OnPlayAni("Idle");
        _mapCenterPos = GetMapCenter().position;
        _myTransform.GetComponentInChildren<Collider2D>().enabled = false;

        float timer = _beforeDelay;
        OnDelayAndStart(OnTeleport, timer);

        timer += (_startFallingTime + 6f);

        OnDelayAndStart(() => _monster.StartCoroutine(CreateFallingObject()), timer);
    }

    public override void OnAction()
    {

        
    }


    public override void EndAction()
    {
        _myTransform.GetComponentInChildren<Collider2D>().enabled = true;
        _timer = 0;
        Init();
    }


    private void OnDelayAndStart(Action action, float delay) =>
        _monster.StartCoroutine(OnDelay(action, delay));

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

    // 중앙으로 텔레포트 이동하는 메서드
    private void OnTeleport()
    {
        _monster.View.SetCollider(false);
        _myTransform.position = _mapCenterPos;
        _ani.OnPlayAni("Pattern06Start");
    }


    // 랜덤 위치에 낙하물 생성 메서드
    private IEnumerator CreateFallingObject()
    {
        _monster.View.SetCollider(false);
        for (int i = 0; i < _fallingCycle; i++)
        {
            int safeCount = 0;
            for (int j = 0; j < _fallingCount; j++)
            {
                if (safeCount == 100)
                {
                    Debug.LogWarning("생성체 생성 범위가 좁습니다.");
                    break;
                }

                Vector2 createPos = RandomVector(j);
                if (_wallTilemap.HasTile(_wallTilemap.WorldToCell(createPos)))
                {
                    safeCount++;
                    j--;
                    continue;
                }

                GameObject obj = Create(createPos);

                _fallingObjects.Add(obj);
            }

            yield return CoroutineManager.waitForSeconds(_fallingFrequency);
        }

        _ani.OnPlayAni("Pattern06End");
        yield return CoroutineManager.waitForSeconds(1f);

        yield return CoroutineManager.waitForSeconds(_afterDelay);
        IsAction = false;
    }

    private Vector2 RandomVector(int num)
    {
        float angle = UnityEngine.Random.Range(0, 45f) + (45 * num);
        float length = UnityEngine.Random.Range(1.5f, _fallingRange);

        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * length;
        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * length;
        Vector2 myPos = (Vector2)_myTransform.position;
        Vector2 newVector;
        if (y > 0.71f)
            newVector = new Vector2(x, -y) + myPos;
        else
            newVector = new Vector2(x, y) + myPos;

        return newVector;
    }


    private GameObject Create(Vector2 createPos)
    {
        GameObject obj = GameObject.Instantiate
                (
                _fallingObjectPrefab,
                createPos,
                Quaternion.identity,
                _myTransform.parent
                );

        return obj;
    }
}
