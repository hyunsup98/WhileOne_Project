using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터를 오브젝트 풀로 관리하는 클래스
/// 오브젝트 풀로 관리할 데이터가 여러 종류일 경우 사용
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPools<T> : Singleton<ObjectPools<T>> where T : MonoBehaviour
{
    protected Dictionary<string, Queue<T>> pool = new Dictionary<string, Queue<T>>();

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 큐에서 데이터 꺼내오기
    /// </summary>
    /// <param name="type"> 꺼내올 데이터 타입 </param>
    /// <param name="trans"> 데이터를 생성한다면 생성해줄 위치 트랜스폼 </param>
    /// <returns> pool에서 꺼내온 T 타입의 데이터 </returns>
    public T GetObject(T type, Transform trans)
    {
        string name = type.name;
        T data;

        if(!pool.ContainsKey(name))
        {
            pool.Add(name, new Queue<T>());
        }

        if (pool[name].Count == 0)
        {
            data = Instantiate(type, trans);
            data.name = name;
        }
        else
        {
            data = pool[name].Dequeue();
            data.gameObject.SetActive(true);
        }

        return data;
    }

    /// <summary>
    /// 큐에 데이터 집어넣기
    /// </summary>
    /// <param name="data"> 집어넣을 데이터 </param>
    public void TakeObject(T data)
    {
        if (data == null) return;

        string name = data.name;

        if(!pool.ContainsKey(name))
        {
            pool.Add(name, new Queue<T>());
        }

        data.gameObject.SetActive(false);
        pool[name].Enqueue(data);
    }
}
