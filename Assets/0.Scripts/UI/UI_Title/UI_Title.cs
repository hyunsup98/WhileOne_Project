using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬의 UI 동작을 담당하는 클래스
/// 게임시작 버튼과 게임종료 버튼 기능 담당
/// </summary>
public class UI_Title : MonoBehaviour
{
    private const string HasSeenTutorial = "HASSEENTUTORIAL";

    [Header("게임시작 버튼 클릭 시 이동할 씬 이름")]
    [SerializeField] private string _gameSceneName = "Dungeon_Floor1";      // 던전 1층 씬 이름
    [SerializeField] private string _tutorialSceneName = "Tutorial";        // 튜토리얼 씬 이름

    [Header("버튼 클릭시 재생할 오디오 클립")]
    [SerializeField] private AudioClip _buttonClickClip;    // 버튼을 클릭했을 때 재생할 효과음

    private void Awake()
    {
        if (_gameSceneName == null)
            _gameSceneName = "SHS_InGame_FirstFloor";
    }

    //게임 시작 버튼
    public void OnClick_GameStart()
    {
        if(_buttonClickClip != null)
            SoundManager.Instance.PlaySoundEffect(_buttonClickClip);

        if (!PlayerPrefs.HasKey(HasSeenTutorial))
        {
            PlayerPrefs.SetString(HasSeenTutorial, "seen");
            SceneManager.LoadScene(_tutorialSceneName);
        }
        else
        {
            SceneManager.LoadScene(_gameSceneName);
        }
    }

    //게임 종료 버튼
    public void OnClick_GameExit()
    {
        Application.Quit();
    }
}
