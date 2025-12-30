using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadEnding : MonoBehaviour
{
    [SerializeField] private string _titleSceneName;
    [SerializeField] private TMP_Text text_title;
    [SerializeField] private TMP_Text text_desc;

    public void OnClick_GoToTitle()
    {
        SoundManager.Instance.PlaySoundEffect("Mouse_Click_Possible_FX_001");
        SceneManager.LoadScene(_titleSceneName);
    }
}
