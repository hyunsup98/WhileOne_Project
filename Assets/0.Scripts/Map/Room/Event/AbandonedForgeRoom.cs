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
        // 모루 배치 (방 중앙)
        if (anvil != null)
        {
            Vector3 center = GetRoomCenter();
            anvil.transform.position = center;
            anvil.SetActive(true);
            
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
        float radius = RoomSize * 0.3f;
        
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
        
        isAnvilUsed = true;
        
        // 규칙 안내
        ShowRules();
        
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
        
        // TODO: 무기 완전 회복 기능 구현 필요
        // Weapon 클래스에 내구도를 최대치로 회복하는 메서드가 없음
        // 필요 시 Weapon 클래스에 RepairToFull() 또는 SetDurability(int) 메서드 추가 필요
        // if (player?.Player_WeaponChange?._slotWeapon2 != null)
        // {
        //     Weapon weapon = player.Player_WeaponChange._slotWeapon2;
        //     weapon.RepairToFull(); // 또는 weapon.SetDurability(weapon.WeaponData.weaponDurability);
        // }
    }
    
    /// <summary>
    /// 수리 실패
    /// </summary>
    private void OnRepairFailure(Player player)
    {
        Debug.Log("[AbandonedForgeRoom] 장비 수리 실패!");
        
        // 30% 확률로 장비 파괴, 70% 확률로 내구도 절반
        if (Random.Range(0f, 1f) < 0.3f)
        {
            // 장비 파괴
            Debug.Log("[AbandonedForgeRoom] 장비가 파괴되었습니다!");
            
            // 무기 시스템 확인
            if (player?.Player_WeaponChange?._slotWeapon2 != null)
            {
                Weapon weapon = player.Player_WeaponChange._slotWeapon2;
                // 내구도를 0으로 만들어서 파괴 효과 (Weapon의 ReduceDurability는 내구도가 0 이하가 되면 WeaponPool로 반환됨)
                weapon.ReduceDurability(weapon.Durability);
                player.Player_WeaponChange._slotWeapon2 = null;
                player.Player_WeaponChange.currentweapon = null;
            }
        }
        else
        {
            // 내구도 절반
            Debug.Log("[AbandonedForgeRoom] 장비 내구도가 절반으로 줄어들었습니다!");
            
            // 무기 시스템 확인
            if (player?.Player_WeaponChange?._slotWeapon2 != null)
            {
                Weapon weapon = player.Player_WeaponChange._slotWeapon2;
                int currentDurability = weapon.Durability;
                int reduceAmount = currentDurability / 2; // 현재 내구도의 절반 감소
                weapon.ReduceDurability(reduceAmount);
            }
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
public class AnvilInteractable : Interactable
{
    private AbandonedForgeRoom forgeRoom;
    
    public void Initialize(AbandonedForgeRoom room)
    {
        forgeRoom = room;
    }
    
    protected override void OnInteract(Player player)
    {
        if (forgeRoom != null)
        {
            forgeRoom.OnAnvilInteracted(player);
        }
    }
}

