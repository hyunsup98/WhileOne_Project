using UnityEngine;
using UnityEngine.InputSystem;

public class UI_Settings : MonoBehaviour
{
    [SerializeField] private GameObject _settings;          //설정창 오브젝트
    [SerializeField] private GameObject _warningWindow;     //경고 화면 창(게임 종료 버튼 클릭 시 확인창)
    [SerializeField] private GameObject _manualPage;        //조작설명 창

    private void Awake()
    {
        #region Input System Mapping
        OnToggle();
        #endregion
    }

    #region Input System
    //설정창을 키고 끄는 인풋 액션
    private void OnToggle()
    {
        InputSystem.actions["Settings"].started += ctx =>
        {
            if(_settings != null && !_warningWindow.activeSelf && !_manualPage.activeSelf)
            {
                //현재 설정창의 on/off 여부 반대로 실행
                _settings.SetActive(!_settings.activeSelf);

                //설정창의 on/off 여부에 따라서 게임 상태 변경
                GameManager.Instance.SetGameState(_settings.activeSelf ? GameState.Pause : GameState.InGame);
            }
        };
    }
    #endregion

    //조작설명 버튼 클릭
    public void OnClick_Manual()
    {
        //조작 설명 창 띄우기
        if (_manualPage != null && !_warningWindow.activeSelf && !_manualPage.activeSelf)
        {
            _manualPage.SetActive(true);
        }
    }

    //설정창 닫기 버튼 클릭
    public void OnClick_Exit()
    {
        if(_settings != null)
        {
            _settings.SetActive(false);

            //설정창의 on/off 여부에 따라서 게임 상태 변경
            GameManager.Instance.SetGameState(_settings.activeSelf ? GameState.Pause : GameState.InGame);
        }
    }

    //게임종료 버튼 클릭
    public void OnClick_QuitGame()
    {
        //게임 종료 확인 패널 띄우기
        if(_warningWindow != null && !_warningWindow.activeSelf && !_manualPage.activeSelf)
        {
            _warningWindow.SetActive(true);
        }
    }
}
