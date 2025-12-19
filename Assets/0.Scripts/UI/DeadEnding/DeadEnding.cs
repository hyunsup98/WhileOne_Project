using TMPro;
using UnityEngine;

public class DeadEnding : MonoBehaviour
{
    [SerializeField] private TMP_Text text_title;
    [SerializeField] private TMP_Text text_desc;

    public void OnClick_GoToTitle()
    {
        GameManager.Instance._CurrentGameState = GameState.Title;
    }
}
