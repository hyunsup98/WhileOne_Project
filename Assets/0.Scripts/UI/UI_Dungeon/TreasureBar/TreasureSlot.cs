using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreasureSlot : Slot
{
    [SerializeField] private Image iconImg;     // 보물 아이콘을 띄울 이미지

    private Treasure _treasure;       // 현재 슬롯에 존재하는 보물 데이터
    private Color _mouseEnterColor = new Color(255f, 255f, 255f);
    private Color _mouseExitColor = new Color(200f, 200f, 200f);

    public override void OnPointerEnter(PointerEventData eventData)
    {
        iconImg.color = _mouseEnterColor;
        GameManager.Instance.CurrentDungeon.TreasureTooltip.ShowTooltip(_treasure);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        iconImg.color = _mouseExitColor;
        GameManager.Instance.CurrentDungeon.TreasureTooltip.HideTooltip();
    }

    public void SetIcon(Treasure data)
    {
        if (iconImg == null || data == null) return;

        _treasure = data;
        iconImg.sprite = _treasure.TreasureData.treasureIconPath_Sprite;
        iconImg.SetAlpha(1f);
    }
}
