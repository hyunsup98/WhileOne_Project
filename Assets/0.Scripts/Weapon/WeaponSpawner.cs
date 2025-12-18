using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    public Weapon[] weaponPrefabs;

    public void SpawnRandomWeapon()
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0)
        {
            Debug.LogWarning("WeaponSpawner에 무기 프리팹이 설정되지 않았습니다.");
            return;
        }

        int randomIndex = Random.Range(0, weaponPrefabs.Length);
        Weapon selectedPrefab = weaponPrefabs[randomIndex];

        Instantiate(
            selectedPrefab,
            transform.position,
            transform.rotation
        );

        Debug.Log("무기 생성됨: " + selectedPrefab.name);
    }
}
