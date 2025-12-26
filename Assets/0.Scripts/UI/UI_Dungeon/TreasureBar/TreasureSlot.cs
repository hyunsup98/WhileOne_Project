using UnityEngine;
using UnityEngine.UI;

public class TreasureSlot : MonoBehaviour
{
    [SerializeField] private Image iconImg;     // 보물 아이콘을 띄울 이미지

    private TreasureDataSO _treasureData;       // 현재 슬롯에 존재하는 보물 데이터

    public void SetIcon(TreasureDataSO data)
    {
        if (iconImg == null || data == null) return;

        _treasureData = data;
        iconImg.sprite = _treasureData.treasureIconPath_Sprite;
        iconImg.SetAlpha(1f);
    }
}
