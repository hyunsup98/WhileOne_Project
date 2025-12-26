using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{
    [Header("무기 장착 슬롯 변수")]
    [SerializeField] private Toggle _weaponToggle;      // 현재 무기가 장착되어 있는지 판단할 토글
    [SerializeField] private Image _frameImg;           // 무기 아이콘 겉의 프레임 이미지
    [SerializeField] private Image _iconImg;            // 무기 아이콘이 표시될 이미지

    [Header("내구도 바 변수")]
    [SerializeField] private GameObject _durability;    // 내구도 바 게임오브젝트
    [SerializeField] private Image _durabilityBar;      // 내구도 바 이미지

    [Header("선택된 슬롯에 적용해줄 색상")]
    [SerializeField] private Color _selectColor;        // 현재 무기가 선택되었을 때 변경해줄 색상

    private Weapon weapon;

    private void Awake()
    {
        ChangeIcon(weapon);
    }

    /// <summary>
    /// 플레이어가 무기를 획득하거나 깨지는 등의 변화가 생길 때 아이콘을 갱신
    /// </summary>
    /// <param name="weapon"> 슬롯에 보여줄 무기 </param>
    public void ChangeIcon(Weapon weapon)
    {
        if (_iconImg == null || _durabilityBar == null || _durability == null) return;

        this.weapon = weapon;

        if (weapon == null)
        {
            // 아이콘 숨기기
            _iconImg.SetAlpha(0f);

            // 내구도 바가 켜져있다면 꺼주기
            if (_durability.activeSelf)
                _durability.SetActive(false);
        }
        else
        {
            // 아이콘 변경
            _iconImg.sprite = weapon.WeaponData.weaponResourcePath_Sprite;
            _iconImg.SetAlpha(1f);

            // 내구도 바가 꺼져있다면 켜주고 수치 적용
            if (!_durability.activeSelf)
                _durability.SetActive(true);
            ChangeDurability(weapon.Durability, weapon.WeaponData.weaponDurability);

        }
    }

    /// <summary>
    /// 토글 켜기, 단 슬롯에 무기가 없을 경우 선택 무시
    /// </summary>
    public void ToggleOn()
    {
        if (weapon == null) return;

        _weaponToggle.isOn = true;
    }

    public void ChangeDurability(int currentDurability, int maxDurability)
    {
        if (_durabilityBar == null || weapon == null || _durability == null) return;

        _durabilityBar.fillAmount = (float)currentDurability / maxDurability;
    }

    /// <summary>
    /// 현재 무기가 장착되어 있는지에 따라 프레임 색상을 변경
    /// </summary>
    public void OnSelectChanged()
    {
        if (_weaponToggle == null) return;

        if (_weaponToggle.isOn)
            _frameImg.color = _selectColor;
        else
            _frameImg.color = Color.white;
    }
}
