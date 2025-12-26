using UnityEngine;
using UnityEngine.UI;

public class TreasureSlot : MonoBehaviour
{
    [SerializeField] private Image iconImg;     // 보물 아이콘을 띄울 이미지

    private Treasure _treasure;                 // 현재 슬롯에 존재하는 보물 데이터

    public void SetIcon(Treasure treasure)
    {
        if (iconImg == null || treasure == null) return;

        _treasure = treasure;
        iconImg.sprite = _treasure.TreasureData.treasureIconPath_Sprite;
        iconImg.SetAlpha(1f);
    }
}
