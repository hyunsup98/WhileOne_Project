using UnityEngine;

/// <summary>
/// 함정 방 레버 동작 스크립트.
/// - 플레이어가 근처에서 E키를 눌렀을 때 상호작용으로 한 번만 작동.
/// - 실제 방 로직은 TrapRoomController에 위임합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TrapRoomLever : Interactable, IInteractable
{
    private TrapRoomController controller;
    private bool activated = false;
    private bool isLayerInitialized = false;

    [Header("Treasure Settings")]
    [SerializeField] private GameObject treasurePrefab;
    [SerializeField] private float treasureChance = 0.1f; // 0~1 사이

    [field: SerializeField] public float YOffset { get; set; } = 1.5f;
    public Vector3 Pos => transform.position;
    [field: SerializeField] public string InteractText { get; set; } = "함정 해제";

    private void Awake()
    {
        InitializePlayerLayer();
        // 상호작용 범위를 1f로 설정 (옆 통로에서 상호작용 안되도록)
        this.interactionRange = 1f;
    }

    private void Start()
    {
        if (!isLayerInitialized)
        {
            InitializePlayerLayer();
        }
    }

    private void InitializePlayerLayer()
    {
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        if (playerLayerIndex == -1)
        {
            Debug.LogError("[TrapRoomLever] Player 레이어를 찾을 수 없습니다!");
            return;
        }
        
        playerLayer = 1 << playerLayerIndex;
        isLayerInitialized = true;
    }

    public void Initialize(TrapRoomController trapRoomController, GameObject treasurePrefab, float treasureChance)
    {
        controller = trapRoomController;
        this.treasurePrefab = treasurePrefab;
        this.treasureChance = treasureChance;

        if (!isLayerInitialized)
        {
            InitializePlayerLayer();
        }

        // Collider2D를 Trigger로 설정 (원하면 충돌 사용 가능)
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // IInteractable 인터페이스 구현 (GameManager에서 호출)
    public void OnInteract()
    {
        if (!canInteract || !isPlayerNearby) return;
        
        Player player = GetNearbyPlayer();
        if (player == null)
        {
            player = GetPlayer();
        }
        
        if (player != null)
        {
            OnInteract(player);
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

        // 상호작용 후 GameManager에서 제거
        if (GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }

        // 레버 비활성화 (상호작용 한 번만)
        gameObject.SetActive(false);
    }

    // 플레이어가 범위에 들어왔을 때 GameManager에 등록
    protected override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (GameManager.Instance != null && canInteract)
        {
            GameManager.Instance.InteractObj = this;
        }
    }

    // 플레이어가 범위를 벗어났을 때 GameManager에서 제거
    protected override void OnPlayerExit()
    {
        base.OnPlayerExit();
        if (GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }
    }
}


