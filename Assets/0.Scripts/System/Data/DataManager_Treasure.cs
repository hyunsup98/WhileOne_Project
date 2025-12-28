using System.Collections.Generic;
using UnityEngine;

public class DataManager_Treasure : MonoBehaviour
{
    // 보물 데이터베이스 SO
    [field: SerializeField] public TreasureDatabaseSO TreasureDatabase { get; private set; }

    // 런타임 중에 사용할 보물이 든 자료구조
    // 보물은 한 번 획득하면 더 이상 획득할 수 없기 때문에 자료구조에 따로 담아두고 사용
    public List<TreasureDataSO> TreasureList { get; private set; }

    [SerializeField] private Treasure _defaultTreasure;

    private void Awake()
    {
        TreasureList = new List<TreasureDataSO>();
    }

    private void Start()
    {
        if(TreasureDatabase != null)
        {
            foreach(var data in TreasureDatabase.datas)
            {
                TreasureList.Add(data);
            }
        }
    }


    public Treasure PickTreasure()
    {
        if (TreasureList.Count == 0) return null;

        int rand = Random.Range(0, TreasureList.Count);
        Treasure treasure = TreasurePool.Instance.GetObject(_defaultTreasure, transform);
        treasure.TreasureData = TreasureList[rand];
        treasure.InitData(treasure.TreasureData);

        int lastIndex = TreasureList.Count - 1;
        TreasureList[rand] = TreasureList[lastIndex];
        TreasureList.RemoveAt(lastIndex);

        return treasure;
    }
}
