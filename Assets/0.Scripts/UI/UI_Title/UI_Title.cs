using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬의 UI 동작을 담당하는 클래스
/// 게임시작 버튼과 게임종료 버튼 기능 담당
/// </summary>
public class UI_Title : MonoBehaviour
{
    [SerializeField] private string _gameSceneName;         //게임 시작 버튼을 눌렀을 때 이동할 씬 이름
    [SerializeField] private AudioClip _buttonClickClip;    //버튼을 클릭했을 때 재생할 효과음

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

        SceneManager.LoadScene(_gameSceneName);
    }

    //게임 종료 버튼
    public void OnClick_GameExit()
    {
        Application.Quit();
    }
}
