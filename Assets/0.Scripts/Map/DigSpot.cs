using UnityEngine;

/// <summary>
/// 도굴 방의 DigSpot 하나를 표현하는 상호작용 오브젝트.
/// - 진짜/가짜 여부는 DigRoomController에서 설정합니다.
/// - 가짜인 경우: 몬스터 소환 또는 플레이어 HP를 일정 퍼센트만큼 감소시킵니다.
/// </summary>
public class DigSpot : Interactable
{
    private DigRoomController roomController;
    private bool isReal = false;
    private bool isDug = false;

    [Header("Visual Settings")]
    [SerializeField] [Tooltip("한 번 파고 난 뒤 DigSpot 오브젝트를 비활성화할지 여부")]
    private bool disableAfterDig = true;

    /// <summary>
    /// 방 컨트롤러에서 초기화 시 호출.
    /// </summary>
    public void Initialize(DigRoomController controller)
    {
        roomController = controller;
    }

    /// <summary>
    /// 진짜/가짜 설정 (DigRoomController에서만 호출).
    /// </summary>
    public void SetIsReal(bool value)
    {
        isReal = value;
    }

    /// <summary>
    /// 플레이어가 E키로 상호작용했을 때의 실제 동작.
    /// </summary>
    protected override void OnInteract(GameObject player)
    {
        if (!canInteract || isDug)
            return;

        isDug = true;
        canInteract = false;

        PlayerController playerController = player.GetComponent<PlayerController>();

        if (isReal)
        {
            Debug.Log("[DigSpot] 진짜 도굴 지점 발견! (보상 로직은 이후에 추가 가능)");
            // TODO: 진짜 DigSpot 보상(아이템, 보물상자 등) 로직 추가
        }
        else
        {
            Debug.Log("[DigSpot] 가짜 도굴 지점입니다. 페널티 적용.");

            bool spawnedMonster = false;

            // 몬스터 프리팹이 있으면 50% 확률로 몬스터 소환
            if (roomController != null)
            {
                GameObject monsterPrefab = roomController.GetMonsterPrefab();
                if (monsterPrefab != null && Random.value < 0.5f)
                {
                    Object.Instantiate(monsterPrefab, transform.position, Quaternion.identity, transform.parent);
                    spawnedMonster = true;
                }
            }

            // 몬스터를 소환하지 않았으면 HP 패널티 적용
            if (!spawnedMonster && playerController != null && roomController != null)
            {
                float damagePercent = roomController.GetFakeDamagePercent(); // 예: 10 = 10%
                Debug.Log($"[DigSpot] 플레이어 HP {damagePercent}% 감소.");
                playerController.AddHpPercent(-damagePercent);
            }
        }

        if (disableAfterDig)
        {
            gameObject.SetActive(false);
        }
    }
}


