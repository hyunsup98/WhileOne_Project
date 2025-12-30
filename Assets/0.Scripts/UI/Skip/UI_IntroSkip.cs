using UnityEngine;

public class UI_IntroSkip : MonoBehaviour
{
    private const string HasSeenTutorial = "HASSEENTUTORIAL";

    [SerializeField] private GameObject _skipPanel;
    [SerializeField] private IntroManager _introManager;

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

    public void SkipIntro()
    {
        if (_introManager == null) return;

        _introManager.EndSecen();
    }

    public void OnClick_ShowPanel()
    {
        SoundManager.Instance.PlaySoundEffect("Mouse_Click_Possible_FX_001");

        if (_skipPanel == null) return;

        _skipPanel.SetActive(true);
    }

    public void OnClick_HidePanel()
    {
        SoundManager.Instance.PlaySoundEffect("Mouse_Click_Possible_FX_001");

        if (_skipPanel == null) return;

        _skipPanel.SetActive(false);
    }
}
