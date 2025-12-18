using UnityEngine;

public class BoxOpenController : MonoBehaviour
{
    public GameObject boxUI;              // 상호작용 UI
    public float openChance = 0.2f;       // 무기 나올 확률
    public WeaponSpawner weaponSpawner;   // 무기 생성 담당

    private bool isPlayerInRange = false;
    private bool isOpened = false;        // 상자 1회용 처리

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

        if (Random.value <= openChance)
        {
            weaponSpawner.SpawnRandomWeapon();
        }
        else
        {
            Debug.Log("빈 상자 입니다.");
        }

        boxUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            boxUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            boxUI.SetActive(false);
        }
    }
}
