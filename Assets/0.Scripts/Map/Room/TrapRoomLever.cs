using UnityEngine;

/// <summary>
/// 함정 방 레버 동작 스크립트.
/// - 플레이어가 근처에서 E키를 눌렀을 때 상호작용으로 한 번만 작동.
/// - 실제 방 로직은 TrapRoomController에 위임합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TrapRoomLever : Interactable
{
    private TrapRoomController controller;
    private bool activated = false;

    [Header("Treasure Settings")]
    [SerializeField] private GameObject treasurePrefab;
    [SerializeField] private float treasureChance = 0.1f; // 0~1 사이

    public void Initialize(TrapRoomController trapRoomController, GameObject treasurePrefab, float treasureChance)
    {
        controller = trapRoomController;
        this.treasurePrefab = treasurePrefab;
        this.treasureChance = treasureChance;

        // Collider2D를 Trigger로 설정 (원하면 충돌 사용 가능)
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// 플레이어가 E키를 눌러 상호작용할 때 호출되는 실제 동작.
    /// Player 쪽에서는 Interactable.Interact(player) 를 호출해 주면 됩니다.
    /// </summary>
    protected override void OnInteract(Player player)
    {
        Debug.Log($"[TrapRoomLever] OnInteract - activated:{activated}, canInteract:{canInteract}");

        if (!canInteract || activated)
            return;

        activated = true;
        canInteract = false;

        Debug.Log("[TrapRoomLever] Lever activated by player, notifying TrapRoomController.");
        controller?.OnLeverActivated();

        // 10% 확률로 레버 위치에 보물상자 생성
        if (treasurePrefab != null && Random.value < treasureChance)
        {
            Debug.Log("[TrapRoomLever] Spawning treasure chest at lever position via interaction.");
            Instantiate(treasurePrefab, transform.position, Quaternion.identity, transform.parent);
        }

        // 레버 비활성화 (상호작용 한 번만)
        gameObject.SetActive(false);
    }
}


