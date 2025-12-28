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
    private float healRatio = 1f;
    
    private bool hasHealed = false;
    
    protected override void InitializeEventRoom()
    {
        // 조각상 배치 (방 중앙)
        if (statue != null)
        {
            Vector3 center = GetRoomCenter();
            statue.transform.position = center;
            statue.SetActive(true);
            
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
        HealPlayer(healRatio, player);
        
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
public class StatueInteractable : Interactable
{
    private HealingRoom healingRoom;
    
    public void Initialize(HealingRoom room)
    {
        healingRoom = room;
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
    
    protected override void OnInteract(Player player)
    {
        if (healingRoom != null)
        {
            healingRoom.OnStatueInteracted(player);
        }
    }
}

