using TMPro;
using UnityEngine;

public class UIText_Floor : UIText
{
    [SerializeField] private int floor;

    public int Floor { get => floor; } // 프로퍼티 추가

    protected override void Start()
    {
        text.text = string.Format(DataManager.Instance.UIData.UIDatabase[_textId].ko, Floor);
    }
}
