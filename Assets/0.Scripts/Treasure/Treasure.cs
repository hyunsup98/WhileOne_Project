using UnityEngine;

public enum TreasureType
{
    Common = 1,         // 기획서 상의 1티어
    Rare,               // 기획서 상의 2티어
    Orb = 4,            // 오브 아이템 (보스를 잡고 나오는 보물 / 엔딩에 필요)
}

/// <summary>
/// 보물 클래스
/// </summary>
public class Treasure
{
    private TreasureDataSO _treasureDataSO;
    public TreasureDataSO TreasureData
    {
        get { return _treasureDataSO; }
        set
        {
            _treasureDataSO = value;
            _treasureType = (TreasureType)_treasureDataSO.treasureTier;
            Debug.Log(_treasureType);
        }
    }

    public TreasureType _treasureType { get; private set; }
}
