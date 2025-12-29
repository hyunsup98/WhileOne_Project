using UnityEngine;

/// <summary>
/// 체력 회복 방 이벤트
/// 조각상 오브젝트에 E키로 상호작용 시 체력 회복
/// </summary>
public class HealingRoom : BaseEventRoom
{
    [Header("Healing Room Settings")]
    [SerializeField] [Tooltip("조각상 오브젝트")]
    private GameObject statue;
    [SerializeField] [Tooltip("체력 회복량 (0~1 비율, 1.0 = 100% 회복)")]
    [Range(0f, 1f)]
    private float healRatio = 0.2f; // 최대체력의 20% 회복
    
    private bool hasHealed = false;
    
    protected override void InitializeEventRoom()
    {
        if (statue != null)
        {
            // 조각상에 상호작용 컴포넌트 추가
            StatueInteractable interactable = statue.GetComponent<StatueInteractable>();
            if (interactable == null)
            {
                interactable = statue.AddComponent<StatueInteractable>();
            }
            interactable.Initialize(this);
        }
    }
    
    /// <summary>
    /// 조각상 상호작용 처리
    /// </summary>
    public void OnStatueInteracted(Player player)
    {
        if (hasHealed || player == null) return;
        
        hasHealed = true;
        
        // 체력 회복 (헬퍼 메서드 사용)
        float healAmount = player.MaxHp * healRatio;
        player.ChangedHealth += healAmount;
        
        Debug.Log($"[HealingRoom] 체력을 {healAmount:F1} 회복했습니다. (현재 HP: {GetPlayerHealth(player):F1}/{GetPlayerMaxHealth(player):F1})");
        
        // 상호작용 비활성화
        StatueInteractable interactable = statue?.GetComponent<StatueInteractable>();
        if (interactable != null)
        {
            interactable.SetCanInteract(false);
        }
    }
    
    /// <summary>
    /// 방의 중심 위치를 반환합니다.
    /// </summary>
    private Vector3 GetRoomCenter()
    {
        return transform.position;
    }
}

/// <summary>
/// 조각상 상호작용 컴포넌트
/// </summary>
public class StatueInteractable : Interactable, IInteractable
{
    private HealingRoom healingRoom;
    private bool isLayerInitialized = false;
    
    [field: SerializeField] public float YOffset { get; set; } = 1.5f;
    
    public Vector3 Pos => transform.position;
    
    private void Awake()
    {
        InitializePlayerLayer();
    }
    
    private void Start()
    {
        // Start에서도 한 번 더 확인 (Awake가 호출되지 않았을 경우 대비)
        if (!isLayerInitialized)
        {
            InitializePlayerLayer();
        }
    }
    
    private void InitializePlayerLayer()
    {
        // Player 레이어로 설정
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        if (playerLayerIndex == -1)
        {
            Debug.LogError("[StatueInteractable] Player 레이어를 찾을 수 없습니다!");
            return;
        }
        
        playerLayer = 1 << playerLayerIndex; // LayerMask로 변환
        isLayerInitialized = true;
        
        Debug.Log($"[StatueInteractable] Player 레이어 설정 완료. LayerIndex: {playerLayerIndex}, LayerMask: {playerLayer.value}");
    }
    
    public void Initialize(HealingRoom room)
    {
        healingRoom = room;
        
        // Initialize에서도 레이어 설정 확인
        if (!isLayerInitialized)
        {
            InitializePlayerLayer();
        }
        
        // 디버그: 레이어 설정 확인
        if (playerLayer.value == 0)
        {
            Debug.LogError($"[StatueInteractable] playerLayer가 설정되지 않았습니다! 위치: {transform.position}");
        }
        else
        {
            Debug.Log($"[StatueInteractable] 초기화 완료. 위치: {transform.position}, 범위: {interactionRange}, LayerMask: {playerLayer.value}");
        }
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
        // 상호작용 불가능해지면 GameManager에서 제거
        if (!value && GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }
    }
    
    // IInteractable 인터페이스 구현 (GameManager에서 호출)
    public void OnInteract()
    {
        if (!canInteract || !isPlayerNearby) 
        {
            Debug.Log($"[StatueInteractable] OnInteract 호출되었지만 상호작용 불가. canInteract: {canInteract}, isPlayerNearby: {isPlayerNearby}");
            return;
        }
        
        Player player = GetNearbyPlayer();
        if (player == null)
        {
            player = GetPlayer();
        }
        
        if (player != null)
        {
            OnInteract(player);
        }
        else
        {
            Debug.LogWarning("[StatueInteractable] OnInteract 호출되었지만 플레이어를 찾을 수 없습니다.");
        }
    }
    
    protected override void OnInteract(Player player)
    {
        Debug.Log($"[StatueInteractable] 조각상과 상호작용함. healingRoom: {healingRoom}");
        if (healingRoom != null)
        {
            healingRoom.OnStatueInteracted(player);
        }
        
        // 상호작용 후 GameManager에서 제거
        if (GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }
    }
    
    // 플레이어가 범위에 들어왔을 때 GameManager에 등록
    protected override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        Debug.Log($"[StatueInteractable] 플레이어 감지됨! 위치: {transform.position}");
        
        if (GameManager.Instance != null && canInteract)
        {
            GameManager.Instance.InteractObj = this;
            Debug.Log($"[StatueInteractable] GameManager에 등록됨");
        }
    }
    
    // 플레이어가 범위를 벗어났을 때 GameManager에서 제거
    protected override void OnPlayerExit()
    {
        base.OnPlayerExit();
        Debug.Log($"[StatueInteractable] 플레이어가 범위를 벗어남");
        
        if (GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }
    }
}

