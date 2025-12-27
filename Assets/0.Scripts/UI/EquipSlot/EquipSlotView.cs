using UnityEngine;

public class EquipSlotView : MonoBehaviour
{
    [SerializeField] private EquipSlot _mainWeaponSlot;     // 메인 무기 슬롯 (삽)
    [SerializeField] private EquipSlot _subWeaponSlot;      // 서브 무기 슬롯 (플레이 중 획득한 무기)

    // 서브 무기 슬롯에 연결된 무기 아이콘 변경
    public void ChangeSubWeapon(Weapon weapon)
    {
        _subWeaponSlot.ChangeIcon(weapon);
    }

    // 서브 무기 슬롯에 연결된 내구도 바 변경
    public void ChangeSubWeaponDurability(int currentDurability, int maxDurability)
    {
        _subWeaponSlot.ChangeDurability(currentDurability, maxDurability);
    }

    // 1, 2 키보드 키를 통해 선택된 무기 장착 슬롯의 색상 변경
    public void EquipWeapon(int inputKey)
    {
        switch(inputKey)
        {
            case 1:
                _mainWeaponSlot.ToggleOn();
                break;

            case 2:
                _subWeaponSlot.ToggleOn();
                break;

            default:
                break;
        }
    }
}
