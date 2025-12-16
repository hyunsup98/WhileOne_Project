using UnityEngine;

public class UI_ManualPage : MonoBehaviour
{
    //조작설명창 끄기 버튼 클릭
    public void OnClick_Exit()
    {
        gameObject.SetActive(false);
    }
}
