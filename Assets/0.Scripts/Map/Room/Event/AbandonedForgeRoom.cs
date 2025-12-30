using UnityEngine;

/// <summary>
/// 버려진 대장간 이벤트
/// 15x15 크기의 방, 중앙에 모루가 있고 상호작용 시 장비 수리
/// </summary>
public class AbandonedForgeRoom : BaseEventRoom
{
    [Header("Abandoned Forge Settings")]
    [SerializeField] [Tooltip("모루 오브젝트 (중앙에 배치)")]
    private GameObject anvil;
    [SerializeField] [Tooltip("망가진 모루 오브젝트 (수리 후 교체)")]
    private GameObject brokenAnvil;
    [SerializeField] [Tooltip("잔해 오브젝트들")]
    private GameObject[] debris;
    
    private bool isAnvilUsed = false;
    
    protected override void InitializeEventRoom()
    {
        if (anvil != null)
        {
            // 모루에 상호작용 컴포넌트 추가
            AnvilInteractable interactable = anvil.GetComponent<AnvilInteractable>();
            if (interactable == null)
            {
                interactable = anvil.AddComponent<AnvilInteractable>();
            }
            interactable.Initialize(this);
        }
        
        // 잔해 배치
        PlaceDebris();
    }
    
    /// <summary>
    /// 잔해를 배치합니다.
    /// </summary>
    private void PlaceDebris()
    {
        if (debris == null || debris.Length == 0) return;
        
        Vector3 center = GetRoomCenter();
        float roomSize = Mathf.Min(RoomWidth, RoomHeight);
        float radius = roomSize * 0.3f;
        
        foreach (GameObject debrisObj in debris)
        {
            if (debrisObj == null) continue;
            
            Vector3 randomPos = center + (Vector3)(Random.insideUnitCircle * radius);
            debrisObj.transform.position = randomPos;
            debrisObj.SetActive(true);
        }
    }
    
    /// <summary>
    /// 모루 상호작용 처리
    /// </summary>
    public void OnAnvilInteracted(Player player)
    {
        if (isAnvilUsed || player == null) return;
        
        // 규칙 안내
        ShowRules();
        
        isAnvilUsed = true;
        
        // 50% 확률로 성공
        bool success = Random.Range(0f, 1f) < 0.5f;
        
        if (success)
        {
            OnRepairSuccess(player);
        }
        else
        {
            OnRepairFailure(player);
        }
        
        // 모루를 망가진 모루로 교체
        ReplaceAnvilWithBroken();
        
        // 상호작용 비활성화
        DisableAnvilInteraction();
    }
    
    /// <summary>
    /// 규칙을 안내합니다.
    /// </summary>
    private void ShowRules()
    {
        string rules = "버려진 대장간 규칙:\n" +
                      "50% 확률로 장비가 수리됩니다.\n" +
                      "실패 시 30% 확률로 장비가 파괴되거나 70% 확률로 내구도가 절반으로 줄어듭니다.";
        
        Debug.Log($"[AbandonedForgeRoom] {rules}");
        // TODO: UI에 규칙 표시
    }
    
    /// <summary>
    /// 수리 성공
    /// </summary>
    private void OnRepairSuccess(Player player)
    {
        Debug.Log("[AbandonedForgeRoom] 장비 수리 성공!");
        
        // 현재 장비 수리 (내구도를 최대치로 회복)
        if (player?.Player_WeaponChange?._slotWeapon2 != null)
        {
            Weapon weapon = player.Player_WeaponChange._slotWeapon2;
            int maxDurability = weapon.WeaponData.weaponDurability;
            int currentDurability = weapon.Durability;
            int repairAmount = maxDurability - currentDurability;
            
            if (repairAmount > 0)
            {
                // ReduceDurability에 음수 값을 전달하여 내구도 회복
                weapon.ReduceDurability(-repairAmount);
                
                // UI 업데이트
                if (GameManager.Instance?.CurrentDungeon?.EquipSlotController != null)
                {
                    GameManager.Instance.CurrentDungeon.EquipSlotController.ChangeSubWeaponDurability(
                        weapon.Durability, maxDurability);
                }
                
                Debug.Log($"[AbandonedForgeRoom] 장비 내구도가 {currentDurability}에서 {maxDurability}로 회복되었습니다.");
            }
            else
            {
                Debug.Log("[AbandonedForgeRoom] 장비가 이미 최대 내구도입니다.");
            }
        }
        else
        {
            Debug.LogWarning("[AbandonedForgeRoom] 수리할 장비가 없습니다.");
        }
    }
    
    /// <summary>
    /// 수리 실패
    /// </summary>
    private void OnRepairFailure(Player player)
    {
        Debug.Log("[AbandonedForgeRoom] 장비 수리 실패!");
        
        if (player?.Player_WeaponChange?._slotWeapon2 == null)
        {
            Debug.LogWarning("[AbandonedForgeRoom] 수리할 장비가 없습니다.");
            return;
        }
        
        Weapon weapon = player.Player_WeaponChange._slotWeapon2;
        int currentDurability = weapon.Durability;
        
        // 30% 확률로 내구도 1, 70% 확률로 내구도 절반
        if (Random.Range(0f, 1f) < 0.3f)
        {
            // 30% 확률: 내구도를 1로 설정
            weapon.ReduceDurability(weapon.Durability - 1);
            
            Debug.Log($"[AbandonedForgeRoom] 장비 내구도가 {currentDurability}에서 1로 감소했습니다.");
        }
        else
        {
            // 70% 확률: 내구도를 절반으로 감소
            int reduceAmount = currentDurability / 2;
            weapon.ReduceDurability(reduceAmount);
            
            Debug.Log($"[AbandonedForgeRoom] 장비 내구도가 {currentDurability}에서 {weapon.Durability}로 감소했습니다. (절반)");
        }
        
        // UI 업데이트
        if (GameManager.Instance?.CurrentDungeon?.EquipSlotController != null)
        {
            GameManager.Instance.CurrentDungeon.EquipSlotController.ChangeSubWeaponDurability(
                weapon.Durability, weapon.WeaponData.weaponDurability);
        }
    }
    
    /// <summary>
    /// 모루를 망가진 모루로 교체합니다.
    /// </summary>
    private void ReplaceAnvilWithBroken()
    {
        if (anvil == null) return;
        
        Vector3 anvilPos = anvil.transform.position;
        anvil.SetActive(false);
        
        if (brokenAnvil != null)
        {
            brokenAnvil.transform.position = anvilPos;
            brokenAnvil.SetActive(true);
        }
    }
    
    /// <summary>
    /// 모루 상호작용 비활성화
    /// </summary>
    private void DisableAnvilInteraction()
    {
        AnvilInteractable anvilInteractable = anvil?.GetComponent<AnvilInteractable>();
        if (anvilInteractable != null)
        {
            anvilInteractable.SetCanInteract(false);
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
/// 모루 상호작용 컴포넌트
/// </summary>
public class AnvilInteractable : Interactable, IInteractable
{
    private AbandonedForgeRoom forgeRoom;
    private bool isLayerInitialized = false;
    
    [field: SerializeField] public float YOffset { get; set; } = 1.5f;
    
    public Vector3 Pos => transform.position;
    [field: SerializeField] public string InteractText { get; set; } = "열기";

    private void Awake()
    {
        InitializePlayerLayer();
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
            Debug.LogError("[AnvilInteractable] Player 레이어를 찾을 수 없습니다!");
            return;
        }
        
        playerLayer = 1 << playerLayerIndex;
        isLayerInitialized = true;
    }
    
    public void Initialize(AbandonedForgeRoom room)
    {
        forgeRoom = room;
        
        if (!isLayerInitialized)
        {
            InitializePlayerLayer();
        }
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
        if (!value && GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }
    }
    
    // IInteractable 인터페이스 구현
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
    
    protected override void OnInteract(Player player)
    {
        if (forgeRoom != null)
        {
            forgeRoom.OnAnvilInteracted(player);
        }
        
        // 상호작용 후 GameManager에서 제거 (한 번만 상호작용 가능)
        if (GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }
    }
    
    protected override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (GameManager.Instance != null && canInteract)
        {
            GameManager.Instance.InteractObj = this;
        }
    }
    
    protected override void OnPlayerExit()
    {
        base.OnPlayerExit();
        if (GameManager.Instance != null && GameManager.Instance.InteractObj == this)
        {
            GameManager.Instance.InteractObj = null;
        }
    }
}

