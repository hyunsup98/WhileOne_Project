using UnityEngine;
using UnityEngine.UI;

public class Chest_Tutorial : Chest
{
    [SerializeField] private Image _icon;

    public override void OnInteract()
    {
        base.OnInteract();
        _icon.sprite = Weapon.WeaponData.weaponResourcePath_Sprite;
    }
}
