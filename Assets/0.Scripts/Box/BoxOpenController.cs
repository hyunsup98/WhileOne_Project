using UnityEngine;

public enum BoxLootState
{
    Closed,         // 상자를 한 번도 열지 않은 상태
    OpendTaken,     // 상자를 열고 무기를 가져간 상태
    OpendLeft       // 상자를 열었지만 무기는 가져가지 않은 상태
}

public class BoxOpenController : MonoBehaviour
{
    public GameObject boxUI;
    public WeaponSpawner weaponSpawner;

    private bool isPlayerInRange;
    private BoxLootState boxState;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && boxState == BoxLootState.Closed || boxState == BoxLootState.OpendLeft)
        {
            OpenBox();
        }
    }

    void OpenBox()
    {
        boxState = BoxLootState.OpendLeft;

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
