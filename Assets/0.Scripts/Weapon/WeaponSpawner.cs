using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    public GameObject[] weaponPrefabs;

    public WeaponPickup SpawnRandomWeapon()
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0)
        {
            Debug.LogWarning("WeaponSpawner에 무기 프리팹이 없습니다.");
            return null;
        }

        int index = Random.Range(0, weaponPrefabs.Length);
        GameObject obj = Instantiate(
            weaponPrefabs[index],
            transform.position,
            Quaternion.identity
        );

        WeaponPickup pickup = obj.GetComponent<WeaponPickup>();
        if (pickup == null)
        {
            Debug.LogError("스폰된 무기에 WeaponPickup이 없습니다!");
            return null;
        }

        Debug.Log("무기 스폰됨: " + pickup.weaponType);
        return pickup;
    }
}
