using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DataBase SO가 상속받을 부모 클래스
/// </summary>
/// <typeparam name="TKey"> 딕셔너리의 키(아이디 컬럼 타입) </typeparam>
/// <typeparam name="TRow"> 해당 테이블 데이터 </typeparam>
public abstract class TableDatabase<TKey, TRow> : ScriptableObject where TRow : TableBase<TKey>
{
    public List<TRow> datas = new List<TRow>();
    private Dictionary<TKey, TRow> cache;

    public TRow this[TKey id]
    {
        get
        {
            if (cache == null && datas.Count > 0)
                CacheData();

            if(cache.TryGetValue(id, out TRow row))
            {
                return row;
            }

            return null;
        }
    }

    public void CacheData()
    {
        cache = new Dictionary<TKey, TRow>();
        foreach(var data in datas)
        {
            if(!cache.TryAdd(data.GetID(), data))
            {
                Debug.LogWarning($"중복된 ID 발견: {data.GetID()}, 이 데이터는 무시합니다.");
            }
        }
    }
}
