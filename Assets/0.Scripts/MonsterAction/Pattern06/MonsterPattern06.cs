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
    private float _fallingCycle;
    private float _fallingRange;
    private float _fallingSpeed;
    private float _fallingHeight;

    private GameObject _fallingObjectPrefab;

    private Vector2 _mapCenterPos;
    private Transform _myTransform;
    private List<Vector2> _fallingPos;
    private List<GameObject> _fallingObjects;

    public string AniTrigger { get; private set; }

    public MonsterPattern06(Pattern06SO actionData, MonsterPresenter monster)
    {
        _monster = monster;
        _ani = monster;
        _damage = actionData.ActionDamage;
        _chargeDelay = actionData.ChargeDelay;
        _coolTime = actionData.ActionCoolTime;
        _hitDecision = actionData.HitDecision;

        _wallTilemap = monster.Model.WallTilemap;

        _myTransform = _monster.View.MyTransform;
        _fallingPos = new List<Vector2>();
        _fallingObjects = new List<GameObject>();
    }

    public override void StartAction()
    {
        IsAction = true;
        _isDelay = true;
        _ani.OnPlayAni("Idle");
        _mapCenterPos = GetMapCenter().position;

        float timer = _chargeDelay;
        OnDelayAndStart(OnTeleport, timer);

        timer += _startFallingTime;

        OnDelayAndStart(() => _monster.StartCoroutine(CreateFallingObject()), timer);
    }

    public override void OnAction()
    {
        for (int i = _fallingObjects.Count; i > 0; i--)
        {
            Vector3 objPos = _fallingPos[i];
            GameObject obj = _fallingObjects[i];
            obj.transform.position += Vector3.down * Time.deltaTime * _fallingSpeed;
            Vector2 dir = objPos - obj.transform.position;
            if (dir.sqrMagnitude <= 0.01f)
            {
                GameObject.Destroy(obj);
                _fallingPos.RemoveAt(i);
                _fallingObjects.RemoveAt(i);
            }
            Debug.LogWarning("낙하물 위치" + _fallingObjects.Count);
        }

        if (_fallingObjects.Count == 0)
            IsAction = false;
    }


    public override void EndAction()
    {
        _timer = 0;
        OnDisEffect();
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
        _myTransform.GetComponent<Collider2D>().enabled = false;
        _myTransform.position = _mapCenterPos;
        _ani.OnPlayAni("Patter06)");
    }


    // 랜덤 위치에 낙하물 생성 메서드
    private IEnumerator CreateFallingObject()
    {
        Vector2 createPos = Vector2.zero;
        for (int i = 0; i < _fallingCount; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                float x = _mapCenterPos.x + UnityEngine.Random.Range(-_fallingRange, _fallingRange + 1);
                float y = _mapCenterPos.y + UnityEngine.Random.Range(-_fallingRange, _fallingRange + 1);

                createPos = new Vector2(x, y);
                if (_wallTilemap.HasTile(_wallTilemap.WorldToCell(createPos)))
                    continue;

                GameObject obj = GameObject.Instantiate(
                _fallingObjectPrefab,
                new Vector2(createPos.x, createPos.y + _fallingHeight),
                Quaternion.identity,
                _myTransform
                );

                _fallingPos.Add(createPos);
                _fallingObjects.Add(obj);
            }
            _isDelay = false;

            yield return CoroutineManager.waitForSeconds(_fallingCycle);
        }
    }
}
