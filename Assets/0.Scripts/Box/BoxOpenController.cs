using UnityEngine;

public class BoxOpenController : MonoBehaviour
{
    public GameObject boxUI;  // 상자 UI (열기 버튼 등)
    public GameObject[] weaponPrefabs;  // 획득할 무기 프리팹들
    public Transform weaponSpawnPoint;  // 무기 생성 위치
    public float openChance = 0.2f;  // 20% 확률로 무기 획득 (0.2)
    public float weaponChance = 0.01f; // 1% 확률로 각 무기 획득

    private bool isPlayerInRange = false;
    private WeaponManager weaponManager;  // WeaponManager를 통해 무기 교환 관리

    void Start()
    {
        // WeaponManager 참조
        weaponManager = FindObjectOfType<WeaponManager>();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)) // 'E' 키로 상자 열기
        {
            OpenBox();
        }
    }

    void OpenBox()
    {
        // 20% 확률로 무기 획득 여부 판단
        float randomChance = Random.value;  // 0~1 사이의 난수 생성
        if (randomChance <= openChance)
        {
            // 무기 획득 (그 안에서 1% 확률로 무기 선택)
            SpawnWeapon();
        }
        else
        {
            Debug.Log("상자를 열었지만 무기가 없습니다.");
        }

        boxUI.SetActive(false); // UI 숨기기
    }

    void SpawnWeapon()
    {
        // 1% 확률로 무기 선택
        float weaponSelectionChance = Random.value; // 0~1 사이의 난수 생성
        if (weaponSelectionChance <= weaponChance)
        {
            // 1% 확률로 무기 획득, 무기 목록에서 랜덤하게 선택
            int randomIndex = Random.Range(0, weaponPrefabs.Length);
            GameObject selectedWeapon = weaponPrefabs[randomIndex];

            // 무기 획득 (기존 장착된 무기와 교환)
            weaponManager.EquipWeapon(selectedWeapon);
            Debug.Log("무기를 획득했습니다: " + selectedWeapon.name);
        }
        else
        {
            Debug.Log("무기를 획득하지 못했습니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            boxUI.SetActive(true);  // 상자 UI 활성화
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            boxUI.SetActive(false);  // 상자 UI 비활성화
        }
    }
}
