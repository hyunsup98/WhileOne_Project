using UnityEngine;
using UnityEngine.UI;

public class TreasureSlot : MonoBehaviour
{
    [SerializeField] private Image iconImg;     //보물 아이콘을 띄울 이미지

    public void SetIcon(Sprite icon)
    {
        if (iconImg == null || icon == null) return;

        iconImg.sprite = icon;
    }
}
