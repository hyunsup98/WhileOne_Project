using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponTooltip : MonoBehaviour
{
    [SerializeField] private GameObject _weaponTooltip;

    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TMP_Text _weaponName;
    [SerializeField] private Slider _durabilityBar;
    [SerializeField] private TMP_Text _durabilityText;
    [SerializeField] private TMP_Text _weaponStat;
    [SerializeField] private TMP_Text _weaponDesc;

    public void HideTooltip() => _weaponTooltip.SetActive(false);

    public void ShowTooltip(Weapon weapon)
    {
        if (weapon == null || _weaponTooltip == null) return;

        _weaponTooltip.SetActive(true);

        _weaponIcon.sprite = weapon.WeaponData.weaponResourcePath_Sprite;
        _weaponName.text = weapon.WeaponData.weapon_Name;

        if(weapon.WeaponData.weaponID == 4001)
        {
            _durabilityBar.gameObject.SetActive(false);
            _durabilityText.text = string.Empty;
        }
        else
        {
            _durabilityBar.gameObject.SetActive(true);
            _durabilityBar.value = (float)weapon.Durability / weapon.WeaponData.weaponDurability;
            _durabilityText.text = $"{weapon.Durability} / {weapon.WeaponData.weaponDurability}";
        }

        _weaponStat.text = $"공격력 + {weapon.WeaponData.weaponAttack1Damage}" +
            $"\n공격속도 + {weapon.WeaponData.weaponAttack1Speed}";
        _weaponDesc.text = $"{weapon.WeaponData.weapon_Desc}";
    }
}
