using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_WarningWindow : MonoBehaviour
{
    [SerializeField] private string _sceneName = "Title";

    //게임 종료 확인 버튼
    public void OnClick_Ok()
    {
        SoundManager.Instance.PlaySoundEffect("Mouse_Click_Possible_FX_001");
        LoadingManager.nextSceneName = _sceneName;
        SceneManager.LoadScene("Loading");
    }

    //창 닫기 버튼 클릭
    public void OnClick_Exit()
    {
        SoundManager.Instance.PlaySoundEffect("Mouse_Click_Possible_FX_001");
        gameObject.SetActive(false);
    }
}
