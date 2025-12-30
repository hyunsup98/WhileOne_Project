using UnityEngine;

public class UI_ManualPage : MonoBehaviour
{
    //조작설명창 끄기 버튼 클릭
    public void OnClick_Exit()
    {
        SoundManager.Instance.PlaySoundEffect("Mouse_Click_Possible_FX_001");
        gameObject.SetActive(false);
    }
}
