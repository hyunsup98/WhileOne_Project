using UnityEngine;
public class BoxOpenController : MonoBehaviour
{
    public GameObject boxUI;
    public WeaponSpawner weaponSpawner;

    private bool isPlayerInRange;
    private bool isOpened;

    void Update()
    {
        if (isPlayerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenBox();
        }
    }

    void OpenBox()
    {
        isOpened = true;

        WeaponPickup weapon = weaponSpawner.SpawnRandomWeapon();
        if (weapon == null)
        {
            Debug.Log("상자는 비어있었습니다.");
        }

        boxUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;
        boxUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false;
        boxUI.SetActive(false);
    }
}
