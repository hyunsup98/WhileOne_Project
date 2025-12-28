using UnityEngine;

/// <summary>
/// 상자방 이벤트
/// 상자 2개 중 1개만 선택 가능 (50% 확률, 진짜:가짜 1:1 확률)
/// </summary>
public class ChestRoom : BaseEventRoom
{
    [Header("Chest Room Settings")]
    [SerializeField] [Tooltip("상자 프리팹들 (2개)")]
    private GameObject[] chestPrefabs;
    
    private GameObject[] chests; // 인스턴스화된 상자들
    private bool hasOpenedChest = false;
    private int openedChestIndex = -1;
    private bool isProcessingInteraction = false; // 상호작용 처리 중 플래그 (경쟁 조건 방지)
    
    /// <summary>
    /// 상자를 이미 열었는지 확인 (외부 접근용)
    /// </summary>
    public bool HasOpenedChest => hasOpenedChest;
    
    protected override void InitializeEventRoom()
    {
        Debug.Log($"[ChestRoom] InitializeEventRoom 호출됨. chestPrefabs 배열: {(chestPrefabs == null ? "null" : $"Length={chestPrefabs.Length}")}");
        
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
        
        Debug.Log($"[ChestRoom] 상자 배치 시작. RoomSize: {RoomSize}, 방 중심: {transform.position}");
        
        // 상자 인스턴스화 및 배치
        PlaceChests();
        
        // 각 상자에 상호작용 컴포넌트 추가
        for (int i = 0; i < chests.Length; i++)
        {
            if (chests[i] == null) continue;
            
            ChestInteractable interactable = chests[i].GetComponent<ChestInteractable>();
            if (interactable == null)
            {
                interactable = chests[i].AddComponent<ChestInteractable>();
            }
            interactable.Initialize(this, i);
            
            Debug.Log($"[ChestRoom] 상자 {i}번 초기화 완료. 위치: {chests[i].transform.position}, 활성화: {chests[i].activeSelf}");
        }
    }
    
    /// <summary>
    /// 상자를 인스턴스화하여 배치합니다.
    /// </summary>
    private void PlaceChests()
    {
        Vector3 center = GetRoomCenter();
        float spacing = RoomSize * 0.3f;
        
        Debug.Log($"[ChestRoom] PlaceChests - center: {center}, spacing: {spacing}, RoomSize: {RoomSize}");
        
        // Interactive 부모 찾기 (현재는 항상 방의 자식으로 직접 배치됨)
        Transform interactiveParent = DungeonRoomHelper.FindInteractiveParent(transform);
        if (interactiveParent == null)
        {
            Debug.LogWarning($"[ChestRoom] Interactive 부모를 찾을 수 없어 방의 직접 자식으로 배치합니다.");
            interactiveParent = transform;
        }

        // 상자 인스턴스 배열 초기화
        chests = new GameObject[chestPrefabs.Length];
        
        // 상자 2개를 좌우로 배치
        for (int i = 0; i < chestPrefabs.Length; i++)
        {
            if (chestPrefabs[i] == null) continue;
            
            Vector3 spawnPos = i == 0 
                ? center + Vector3.left * spacing 
                : center + Vector3.right * spacing;
            
            // 프리팹 인스턴스화
            GameObject chestInstance = Instantiate(chestPrefabs[i], spawnPos, Quaternion.identity, interactiveParent);
            chests[i] = chestInstance;
            
            Debug.Log($"[ChestRoom] 상자 {i}번 인스턴스화 및 배치: {spawnPos}");
        }
    }
    
    /// <summary>
    /// 상자 상호작용 처리
    /// </summary>
    public void OnChestInteracted(int chestIndex, Player player)
    {
        // 이미 상자를 열었거나 처리 중이면 즉시 반환 (경쟁 조건 방지)
        if (hasOpenedChest || isProcessingInteraction)
        {
            Debug.Log($"[ChestRoom] 이미 상자를 열었거나 처리 중입니다. 상자 {chestIndex}번은 열 수 없습니다. (hasOpenedChest={hasOpenedChest}, isProcessing={isProcessingInteraction})");
            return;
        }
        
        if (chestIndex < 0 || chestIndex >= chests.Length || player == null || chests == null)
        {
            Debug.LogWarning($"[ChestRoom] 잘못된 상자 인덱스 또는 플레이어: chestIndex={chestIndex}, player={player}, chests={chests}");
            return;
        }
        
        // 상호작용 처리 시작 플래그 설정 (다른 스레드/호출 차단)
        isProcessingInteraction = true;
        
        // 즉시 hasOpenedChest를 true로 설정하여 다른 상자가 동시에 열리는 것을 방지
        hasOpenedChest = true;
        openedChestIndex = chestIndex;
        
        // 모든 상자 비활성화 (현재 상자 포함 - 한 번만 열 수 있도록)
        // 가장 먼저 수행하여 경쟁 조건 완전 차단
        for (int i = 0; i < chests.Length; i++)
        {
            if (chests[i] != null)
            {
                ChestInteractable interactable = chests[i].GetComponent<ChestInteractable>();
                if (interactable != null)
                {
                    interactable.SetCanInteract(false);
                }
            }
        }
        
        Debug.Log($"[ChestRoom] 상자 {chestIndex}번이 열렸습니다. 모든 상자들은 비활성화되었습니다.");
        
        // 50% 확률로 진짜/가짜 결정 (1:1 확률)
        bool isReal = Random.Range(0f, 1f) < 0.5f;
        
        if (isReal)
        {
            OnRealChestOpened(player);
        }
        else
        {
            OnFakeChestOpened(player);
        }
        
        // 상호작용 처리 완료
        isProcessingInteraction = false;
    }
    
    /// <summary>
    /// 진짜 상자 열기
    /// </summary>
    private void OnRealChestOpened(Player player)
    {
        Debug.Log("[ChestRoom] 진짜 상자를 열었습니다! 보물을 받았습니다!");
        
        // 보물 지급 (DataManager의 PickTreasure 함수 사용)
        if (GameManager.Instance?.CurrentDungeon?.TreasureBarUI != null && DataManager.Instance?.TreasureData != null)
        {
            var treasure = DataManager.Instance.TreasureData.PickTreasure();
            if (treasure != null)
            {
                GameManager.Instance.CurrentDungeon.TreasureBarUI.AddTreasure(treasure);
            }
        }
    }
    
    /// <summary>
    /// 가짜 상자 열기
    /// </summary>
    private void OnFakeChestOpened(Player player)
    {
        Debug.Log("[ChestRoom] 가짜 상자입니다. 아무것도 없습니다.");
        
        // TODO: 페널티 적용 (필요시)
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

/// <summary>
/// 상자 상호작용 컴포넌트
/// </summary>
public class ChestInteractable : Interactable
{
    private ChestRoom chestRoom;
    private int chestIndex;
    
    public void Initialize(ChestRoom room, int index)
    {
        chestRoom = room;
        chestIndex = index;
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
    
    protected override void OnInteract(Player player)
    {
        // 첫 번째 안전장치: canInteract 체크 (Interactable.Interact()에서도 체크하지만 중복 체크)
        if (!canInteract)
        {
            Debug.Log($"[ChestInteractable] 상자 {chestIndex}번: canInteract가 false여서 상호작용할 수 없습니다.");
            return;
        }
        
        if (chestRoom == null)
        {
            Debug.LogWarning($"[ChestInteractable] 상자 {chestIndex}번: chestRoom이 null입니다.");
            return;
        }
        
        // 두 번째 안전장치: ChestRoom에서 이미 열렸는지 확인 (가장 중요!)
        // 이 체크가 OnChestInteracted() 호출 전에 있어야 경쟁 조건을 방지할 수 있음
        if (chestRoom.HasOpenedChest)
        {
            Debug.Log($"[ChestInteractable] 상자 {chestIndex}번: 이미 다른 상자가 열려서 상호작용할 수 없습니다.");
            // 추가로 이 상자의 canInteract도 false로 설정
            canInteract = false;
            return;
        }
        
        // 세 번째 안전장치: OnChestInteracted() 내부에서도 hasOpenedChest 및 isProcessingInteraction 체크
        chestRoom.OnChestInteracted(chestIndex, player);
        
        // OnChestInteracted() 호출 후 다시 한 번 체크하여 상호작용 완료 시 이 상자도 비활성화
        if (chestRoom.HasOpenedChest)
        {
            canInteract = false;
        }
    }
}


