using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터를 오브젝트 풀로 관리하는 클래스
/// 오브젝트 풀로 관리할 데이터가 하나의 종류만 있을 경우 사용
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    //T타입 데이터를 담을 큐
    protected Queue<T> pool = new Queue<T>();

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
        T data;

        if(pool.Count == 0)
        {
            //큐에 아무 데이터가 없을 때 직접 생성
            data = Instantiate(type, trans);
            data.name = type.name;
        }
        else
        {
            //큐에 데이터가 있으면 꺼내오기
            data = pool.Dequeue();
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

        data.transform.SetParent(transform, false);
        data.gameObject.SetActive(false);
        pool.Enqueue(data);
    }
}
