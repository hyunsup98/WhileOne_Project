using UnityEngine;
using System.Reflection;

/// <summary>
/// 상자방 이벤트
/// 상자 2개 중 1개만 선택 가능 (50% 확률, 진짜:가짜 1:1 확률)
/// </summary>
public class ChestRoom : BaseEventRoom
{
    [Header("Chest Room Settings")]
    [SerializeField] [Tooltip("상자 프리팹들 (2개)")]
    private GameObject[] chestPrefabs;
    
    private ChestForEventRoom[] chests; // 인스턴스화된 상자들
    private bool hasOpenedChest = false;
    
    /// <summary>
    /// 상자를 이미 열었는지 확인 (외부 접근용)
    /// </summary>
    public bool HasOpenedChest => hasOpenedChest;
    
    protected override void InitializeEventRoom()
    {
        if (chestPrefabs == null || chestPrefabs.Length != 2)
        {
            Debug.LogError($"[ChestRoom] 상자 프리팹은 정확히 2개여야 합니다. 현재: {(chestPrefabs == null ? "null" : chestPrefabs.Length.ToString())}");
            return;
        }
        
        // 프리팹이 null인지 확인
        for (int i = 0; i < chestPrefabs.Length; i++)
        {
            if (chestPrefabs[i] == null)
            {
                Debug.LogError($"[ChestRoom] 상자 프리팹 {i}번이 null입니다.");
                return;
            }
        }
        
        // 상자 인스턴스화 및 배치
        PlaceChests();
    }
    
    /// <summary>
    /// 상자를 인스턴스화하여 배치합니다.
    /// </summary>
    private void PlaceChests()
    {
        Vector3 center = GetRoomCenter();
        float roomSize = Mathf.Max(RoomWidth, RoomHeight);
        float spacing = roomSize * 0.3f;
        
        // Interactive 부모 찾기
        Transform interactiveParent = DungeonRoomHelper.FindInteractiveParent(transform);
        if (interactiveParent == null)
        {
            interactiveParent = transform;
        }

        // 상자 인스턴스 배열 초기화
        chests = new ChestForEventRoom[chestPrefabs.Length];
        
        // 상자 2개를 좌우로 배치
        for (int i = 0; i < chestPrefabs.Length; i++)
        {
            if (chestPrefabs[i] == null) continue;
            
            Vector3 spawnPos = i == 0 
                ? center + Vector3.left * spacing 
                : center + Vector3.right * spacing;
            
            // 프리팹 인스턴스화
            GameObject chestInstance = Instantiate(chestPrefabs[i], spawnPos, Quaternion.identity, interactiveParent);
            
            // ChestForEventRoom 컴포넌트 추가 또는 가져오기
            ChestForEventRoom chestForEventRoom = chestInstance.GetComponent<ChestForEventRoom>();
            if (chestForEventRoom == null)
            {
                // 기존 Chest 컴포넌트가 있으면 제거하고 ChestForEventRoom으로 교체
                Chest existingChest = chestInstance.GetComponent<Chest>();
                if (existingChest != null)
                {
                    DestroyImmediate(existingChest);
                }
                chestForEventRoom = chestInstance.AddComponent<ChestForEventRoom>();
            }
            
            chests[i] = chestForEventRoom;
            chestForEventRoom.InitializeForEventRoom(this, i);
        }
        
        // 진짜/가짜 상자 결정 (50% 확률, 1:1)
        int realChestIndex = Random.Range(0, 2);
        
        // 각 상자에 dropWeaponPer 설정
        for (int i = 0; i < chests.Length; i++)
        {
            if (chests[i] == null) continue;
            
            // 진짜 상자는 100, 가짜 상자는 0
            if (i == realChestIndex)
            {
                SetDropWeaponPer(chests[i], 100);
                Debug.Log($"[ChestRoom] 상자 {i}번은 진짜 상자입니다. (dropWeaponPer: 100)");
            }
            else
            {
                SetDropWeaponPer(chests[i], 0);
                Debug.Log($"[ChestRoom] 상자 {i}번은 가짜 상자입니다. (dropWeaponPer: 0)");
            }
        }
    }
    
    /// <summary>
    /// Chest 컴포넌트의 dropWeaponPer 값을 설정합니다 (리플렉션 사용)
    /// </summary>
    private void SetDropWeaponPer(ChestForEventRoom chest, int value)
    {
        if (chest == null) return;
        
        // 리플렉션을 사용하여 private 필드에 접근
        FieldInfo field = typeof(Chest).GetField("dropWeaponPer", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(chest, value);
        }
        else
        {
            Debug.LogWarning($"[ChestRoom] Chest 클래스에서 dropWeaponPer 필드를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 상자 상호작용 처리 - 다른 상자 비활성화
    /// </summary>
    public void OnChestInteracted(int chestIndex)
    {
        Debug.Log($"[ChestRoom] OnChestInteracted 호출됨 - chestIndex: {chestIndex}, hasOpenedChest: {hasOpenedChest}");
        
        // 이미 상자를 열었으면 반환
        if (hasOpenedChest)
        {
            Debug.Log($"[ChestRoom] 이미 상자를 열었음...");
            return;
        }
        
        if (chestIndex < 0 || chestIndex >= chests.Length || chests == null)
        {
            return;
        }
        
        // 즉시 hasOpenedChest를 true로 설정하여 다른 상자가 동시에 열리는 것을 방지
        hasOpenedChest = true;
        
        // 다른 상자들의 상호작용을 막기 위해 canInteract를 false로 설정
        for (int i = 0; i < chests.Length; i++)
        {
            if (i != chestIndex && chests[i] != null)
            {
                // 다른 상자의 InteractObj 해제
                if (GameManager.Instance.InteractObj == chests[i])
                {
                    GameManager.Instance.InteractObj = null;
                }

                // canInteract를 false로 설정하여 상호작용 불가능하게 만듦
                chests[i].SetCanInteract(false);
                Debug.Log($"[ChestRoom] 상자 {i}번의 상호작용을 비활성화했습니다.");
            }
        }
        
        Debug.Log($"[ChestRoom] 상자 {chestIndex}번이 열렸습니다. 다른 상자들의 상호작용이 비활성화되었습니다.");
    }

    /// <summary>
    /// 방의 중심 위치를 반환합니다.
    /// </summary>
    private Vector3 GetRoomCenter()
    {
        // RoomCenterMarker가 있으면 그 위치를 사용, 없으면 transform.position 사용
        Transform centerMarker = DungeonRoomHelper.FindRoomCenterMarker(gameObject);
        if (centerMarker != null)
        {
            return centerMarker.position;
        }
        return transform.position;
    }
}
