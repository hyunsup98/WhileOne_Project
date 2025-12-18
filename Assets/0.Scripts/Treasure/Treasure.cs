using UnityEngine;

public enum TreasureType
{
    Common,         // 기획서 상의 1티어
    Rare            // 기획서 상의 2티어
}

/// <summary>
/// 보물 클래스
/// </summary>
public class Treasure : MonoBehaviour
{
    [field: SerializeField] public TreasureType type { get; private set; }
    [field: SerializeField] public Sprite icon { get; private set; }
}
