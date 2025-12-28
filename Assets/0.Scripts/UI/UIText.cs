using TMPro;
using UnityEngine;

public class UIText : MonoBehaviour
{
    [SerializeField] protected string _textId;
    protected TMP_Text text;

    protected void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.richText = true;
    }

    protected virtual void Start()
    {
        text.text = $"{DataManager.Instance.UIData.UIDatabase[_textId].ko}";
    }
}
