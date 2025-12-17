using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public GameObject[] weaponPrefabs;  // 가능한 무기 프리팹들
    public Transform weaponHoldPoint;  // 플레이어가 무기를 장착할 위치

    private GameObject currentWeapon;  // 현재 장착 중인 무기

    // 새로운 무기를 장착하는 메서드
    public void EquipWeapon(GameObject weaponPrefab)
    {
        // 기존 무기가 있을 경우, 해당 무기 비활성화
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(false);  // 기존 무기 비활성화 (교환)
        }

        // 새로운 무기 생성
        currentWeapon = Instantiate(weaponPrefab, weaponHoldPoint.position, weaponHoldPoint.rotation);
        currentWeapon.transform.SetParent(weaponHoldPoint);  // 무기 장착
        currentWeapon.SetActive(true);  // 새로운 무기 활성화

        Debug.Log("새로운 무기를 장착했습니다: " + currentWeapon.name);
    }

    // 1% 확률로 무기 반환
    public GameObject GetRandomWeapon()
    {
        int index = Random.Range(0, weaponPrefabs.Length);
        return weaponPrefabs[index];
    }
}
