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
public class Treasure : MonoBehaviour
{
    [SerializeField] protected int _keyId;
    [SerializeField] protected SpriteRenderer _renderer;

    protected TreasureDataSO _treasureDataSO;
    public TreasureDataSO TreasureData
    {
        get { return _treasureDataSO; }
        set
        {
            _treasureDataSO = value;
            _keyId = value.treasureID;
            TreasureType = (TreasureType)_treasureDataSO.treasureTier;
        }
    }

    public TreasureType TreasureType { get; private set; }

    protected virtual void Awake()
    {
        if (_renderer == null)
            TryGetComponent(out _renderer);

        InitData(DataManager.Instance.TreasureData.TreasureDatabase[_keyId]);
    }

    public void InitData(TreasureDataSO data)
    {
        if (data == null) return;

        TreasureData = data;
        _keyId = TreasureData.treasureID;
        _renderer.sprite = TreasureData.treasureIconPath_Sprite;
    }
}
