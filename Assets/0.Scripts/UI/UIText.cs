using TMPro;
using UnityEngine;

public class UIText : MonoBehaviour
{
    private TMP_Text text;
    private int floor = 3;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        text.richText = true;
        text.text = string.Format(DataManager.Instance.UIData.UIDatabase["Floor_Text"].ko, floor);
    }
}
