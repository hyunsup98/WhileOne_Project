using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private GameObject panel_GainWeaponUI;     // 무기 획득 UI 게임 오브젝트
    [SerializeField] private GameObject panel_FailWeaponUI;     // 무기 비획득 UI 게임 오브젝트

    [SerializeField] private TMP_Text text_WeaponName;      // 무기 이름 텍스트
    [SerializeField] private Image img_Weapon;              // 무기 아이콘 이미지
    [SerializeField] private TMP_Text text_WeaponDesc1;     // 무기 설명 텍스트1
    [SerializeField] private TMP_Text text_WeaponDesc2;     // 무기 설명 텍스트2

    private Weapon weapon;      // UI에 표시할 무기
    private Chest chest;        // 가장 최근에 열었던 상자

    // UI로 표시할 무기가 어떤 것인지 받아옴
    public void SetWeaponInit(Weapon weapon)
    {
        this.weapon = weapon;

        text_WeaponName.text = weapon.WeaponData.weapon_Name;
        img_Weapon.sprite = weapon.WeaponData.weaponResourcePath_Sprite;
        text_WeaponDesc1.text = $"공격력: {weapon.WeaponData.weaponAttack1Damage}" +
            $"\n공격속도: {weapon.WeaponData.weaponAttack1Speed}";
    }

    /// <summary>
    /// 획득하기 버튼 클릭
    /// 상자에서 무기를 챙기는 메서드
    /// </summary>
    public void OnClick_GetWeapon()
    {
        SoundManager.Instance.PlaySoundEffect("Weapon_Get_FX_001");
        chest.ChestClose(ChestState.OpenedTaken);
        GameManager.Instance.player.Player_WeaponChange.ChangeWeapon(weapon);
        DisableUI();
    }

    /// <summary>
    /// 돌려놓기 버튼 클릭
    /// 상자에 무기를 두고 UI를 끄는 메서드
    /// </summary>
    public void OnClick_LeaveWeapon()
    {
        // 상자에 무기를 둔 상태로 창 닫기
        SoundManager.Instance.PlaySoundEffect("WeaponBox_Off_FX_001");
        WeaponPool.Instance.TakeObject(weapon);
        chest.ChestClose(ChestState.OpenedLeft);
        DisableUI();
    }

    /// <summary>
    /// 무기 획득 UI창을 켜는 메서드
    /// </summary>
    /// <param name="weapon"></param>
    public void EnableGainUI(Weapon weapon, Chest chest)
    {
        GameManager.Instance.player.ActionMap.Disable();
        SetWeaponInit(weapon);
        this.chest = chest;

        panel_GainWeaponUI.SetActive(true);
        GameManager.Instance.SetGameState(GameState.Pause);
    }

    public void EnableFailUI()
    {
        GameManager.Instance.player.ActionMap.Disable();
        panel_FailWeaponUI.SetActive(true);
        GameManager.Instance.SetGameState(GameState.Pause);
    }

    /// <summary>
    /// 무기 획득 UI창을 끄는 메서드
    /// UI가 꺼질 때 알아서 필드를 초기화하는 등의 작업도 같이 함
    /// 현재 오브젝트가 꺼지는 것이 아닌 하위 자식 오브젝트를 껐다 켜기 때문에 따로 메서드를 정의함
    /// </summary>
    private void DisableUI()
    {
        panel_GainWeaponUI.SetActive(false);

        chest = null;
        weapon = null;

        text_WeaponName.text = string.Empty;
        img_Weapon.sprite = null;
        text_WeaponDesc1.text = string.Empty;
        text_WeaponDesc2.text = string.Empty;

        GameManager.Instance.player.ActionMap.Enable();
        GameManager.Instance.SetGameState(GameState.Playing);
    }

    public void DisableUI(GameObject obj)
    {
        SoundManager.Instance.PlaySoundEffect("WeaponBox_Off_FX_001");
        obj.SetActive(false);

        GameManager.Instance.player.ActionMap.Enable();
        GameManager.Instance.SetGameState(GameState.Playing);
    }
}
