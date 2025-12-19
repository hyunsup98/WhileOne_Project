using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private GameObject panel_WeaponUI;     // 무기 획득 UI 게임 오브젝트

    [SerializeField] private TMP_Text text_WeaponName;      // 무기 이름 텍스트
    [SerializeField] private Image img_Weapon;              // 무기 아이콘 이미지
    [SerializeField] private TMP_Text text_WeaponDesc1;     // 무기 설명 텍스트1
    [SerializeField] private TMP_Text text_WeaponDesc2;     // 무기 설명 텍스트2

    private WeaponData weapon;      //UI에 표시할 무기

    // UI로 표시할 무기가 어떤 것인지 받아옴
    public void SetWeaponInit(WeaponData weapon)
    {
        this.weapon = weapon;

        // todo: 무기에 관련된 정보를 이용해 UI 오브젝트에 적용하기
    }

    /// <summary>
    /// 획득하기 버튼 클릭
    /// 상자에서 무기를 챙기는 메서드
    /// </summary>
    public void OnClick_GetWeapon()
    {
        // todo: 플레이어에 현재 weapon 장착 후 창 닫기

        DisableUI();
    }

    /// <summary>
    /// 돌려놓기 버튼 클릭
    /// 상자에 무기를 두고 UI를 끄는 메서드
    /// </summary>
    public void OnClick_LeaveWeapon()
    {
        // todo: 상자에 무기를 둔 상태로 창 닫기

        DisableUI();
    }

    /// <summary>
    /// 무기 획득 UI창을 켜는 메서드
    /// </summary>
    /// <param name="weapon"></param>
    public void EnableUI(WeaponData weapon)
    {
        SetWeaponInit(weapon);

        panel_WeaponUI.SetActive(true);
    }

    /// <summary>
    /// 무기 획득 UI창을 끄는 메서드
    /// UI가 꺼질 때 알아서 필드를 초기화하는 등의 작업도 같이 함
    /// 현재 오브젝트가 꺼지는 것이 아닌 하위 자식 오브젝트를 껐다 켜기 때문에 따로 메서드를 정의함
    /// </summary>
    private void DisableUI()
    {
        panel_WeaponUI.SetActive(false);

        weapon = null;

        text_WeaponName.text = string.Empty;
        img_Weapon.sprite = null;
        text_WeaponDesc1.text = string.Empty;
        text_WeaponDesc2.text = string.Empty;
    }
}
