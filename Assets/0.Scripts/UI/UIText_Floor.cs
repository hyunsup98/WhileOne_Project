using TMPro;
using UnityEngine;

public class UIText_Floor : UIText
{
    [SerializeField] private int floor;

    protected override void Start()
    {
        text.text = string.Format(DataManager.Instance.UIData.UIDatabase[_textId].ko, floor);
    }
}
