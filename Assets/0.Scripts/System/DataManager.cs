using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    [SerializeField] private List<Treasure> treasures = new List<Treasure>();

    /// <summary>
    /// 보물 뽑기
    /// 뽑은 보물은 리스트에서 제외
    /// </summary>
    /// <returns> 뽑은 보물 </returns>
    public Treasure PickTreasure()
    {
        int index = Random.Range(0, treasures.Count);

        Treasure treasure = treasures[index];
        treasures.RemoveAt(index);

        return treasure;
    }

    /// <summary>
    /// 데이터 초기화
    /// 타이틀 화면으로 이동했을 때 데이터를 다시 초기화해줌
    /// </summary>
    public void DataInit()
    {

    }


    protected override void Awake()
    {
        base.Awake();
    }
}
