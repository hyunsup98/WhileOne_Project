using UnityEngine;

public class UI_WarningWindow : MonoBehaviour
{
    //게임 종료 확인 버튼
    public void OnClick_Ok()
    {
        Application.Quit();
    }

    //창 닫기 버튼 클릭
    public void OnClick_Exit()
    {
        gameObject.SetActive(false);
    }
}
