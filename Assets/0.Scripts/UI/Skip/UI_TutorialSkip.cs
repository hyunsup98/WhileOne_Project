using UnityEngine;

public class UI_TutorialSkip : MonoBehaviour
{
    private const string HasSeenTutorial = "HASSEENTUTORIAL";

    [SerializeField] private GameObject _skipPanel;

    private void Awake()
    {
        if (PlayerPrefs.HasKey(HasSeenTutorial))
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
            PlayerPrefs.SetString(HasSeenTutorial, "true");
        }
    }

    public void ShowSkipPanel()
    {
        SoundManager.Instance.PlaySoundEffect("Mouse_Click_Possible_FX_001");

        if (_skipPanel == null) return;

        _skipPanel.SetActive(true);
    }
}
