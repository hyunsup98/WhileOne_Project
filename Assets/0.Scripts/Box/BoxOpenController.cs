using UnityEngine;

public class BoxOpenController : MonoBehaviour
{
    public GameObject boxUI;                         // 상자 UI
    public Weapon.WeaponList[] boxWeaponList;        // 상자에서 나올 수 있는 무기 종류들
    public float openChance = 0.2f;                  // 20% 확률로 무기 획득

    private bool isPlayerInRange = false;
    private WeaponManager weaponManager;

    void Start()
    {
        weaponManager = Object.FindAnyObjectByType<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OpenBox();
        }
    }

    void OpenBox()
    {
        if (Random.value <= openChance)
        {
            GetWeaponFromBox();
        }
        else
        {
            Debug.Log("빈 상자 입니다.");
        }

        boxUI.SetActive(false);
    }

    void GetWeaponFromBox()
    {
        if (boxWeaponList == null || boxWeaponList.Length == 0)
        {
            Debug.LogWarning("상자에 설정된 무기가 없습니다.");
            return;
        }

        int randomIndex = Random.Range(0, boxWeaponList.Length);
        Weapon.WeaponList selectedWeapon = boxWeaponList[randomIndex];

        weaponManager.EquipWeapon(selectedWeapon);
        Debug.Log("상자에서 무기 획득: " + selectedWeapon);
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
