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
        
        // 각 상자에 상호작용 컴포넌트 추가 (기존 IInteractable 시스템 사용)
        for (int i = 0; i < chests.Length; i++)
        {
            if (chests[i] == null) continue;
            
            ChestRoomInteractable interactable = chests[i].GetComponent<ChestRoomInteractable>();
            if (interactable == null)
            {
                interactable = chests[i].AddComponent<ChestRoomInteractable>();
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
    public void OnChestInteracted(int chestIndex)
    {
        // 이미 상자를 열었거나 처리 중이면 즉시 반환 (경쟁 조건 방지)
        if (hasOpenedChest || isProcessingInteraction)
        {
            Debug.Log($"[ChestRoom] 이미 상자를 열었거나 처리 중입니다. 상자 {chestIndex}번은 열 수 없습니다. (hasOpenedChest={hasOpenedChest}, isProcessing={isProcessingInteraction})");
            return;
        }
        
        if (chestIndex < 0 || chestIndex >= chests.Length || chests == null)
        {
            Debug.LogWarning($"[ChestRoom] 잘못된 상자 인덱스: chestIndex={chestIndex}, chests={chests}");
            return;
        }
        
        // 플레이어 찾기
        Player player = GetPlayer();
        if (player == null)
        {
            Debug.LogWarning($"[ChestRoom] 플레이어를 찾을 수 없습니다.");
            return;
        }
        
        // 상호작용 처리 시작 플래그 설정 (다른 스레드/호출 차단)
        isProcessingInteraction = true;
        
        // 즉시 hasOpenedChest를 true로 설정하여 다른 상자가 동시에 열리는 것을 방지
        hasOpenedChest = true;
        openedChestIndex = chestIndex;
        
        // 다른 상자들의 Box Collider 2D를 비활성화하여 물리적으로 상호작용 차단
        // 가장 먼저 수행하여 경쟁 조건 완전 차단
        for (int i = 0; i < chests.Length; i++)
        {
            if (i != chestIndex && chests[i] != null)
            {
                // 다른 상자의 InteractObj 해제
                ChestRoomInteractable otherInteractable = chests[i].GetComponent<ChestRoomInteractable>();
                if (otherInteractable != null && GameManager.Instance.InteractObj == otherInteractable)
                {
                    GameManager.Instance.InteractObj = null;
                }
                
                // Box Collider 2D 찾아서 비활성화 (상자 자체 또는 자식 오브젝트에 있을 수 있음)
                BoxCollider2D boxCollider = chests[i].GetComponent<BoxCollider2D>();
                if (boxCollider != null)
                {
                    boxCollider.enabled = false;
                    Debug.Log($"[ChestRoom] 상자 {i}번의 Box Collider 2D를 비활성화했습니다.");
                }
                else
                {
                    // 상자 자체에 없으면 자식 오브젝트에서 찾기
                    boxCollider = chests[i].GetComponentInChildren<BoxCollider2D>();
                    if (boxCollider != null)
                    {
                        boxCollider.enabled = false;
                        Debug.Log($"[ChestRoom] 상자 {i}번의 자식 오브젝트 Box Collider 2D를 비활성화했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning($"[ChestRoom] 상자 {i}번에서 Box Collider 2D를 찾을 수 없습니다.");
                    }
                }
            }
        }
        
        Debug.Log($"[ChestRoom] 상자 {chestIndex}번이 열렸습니다. 다른 상자들의 Box Collider 2D가 비활성화되었습니다.");
        
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

        // 기존 상자와 동일하게 fail UI 표시
        if (GameManager.Instance?.CurrentDungeon?.WeaponUI != null)
        {
            GameManager.Instance.CurrentDungeon.WeaponUI.EnableFailUI();
        }

        // TODO: 페널티 적용
        // 체력 감소 혹은 몹 생성

        // 플레이어 체력 10% 감소
        float damageAmount = player.MaxHp * 0.1f;
        player.ChangedHealth -= damageAmount;
        Debug.Log($"[ChestRoom] 플레이어에게 {damageAmount}의 피해를 입혔습니다.");
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
/// 상자방 상자 상호작용 컴포넌트 (기존 IInteractable 시스템 사용)
/// </summary>
public class ChestRoomInteractable : MonoBehaviour, IInteractable
{
    private ChestRoom chestRoom;
    private int chestIndex;
    private bool canInteract = true;
    
    [field: SerializeField] public float YOffset { get; set; } = 1.5f;
    public Vector3 Pos => transform.position;
    
    public void Initialize(ChestRoom room, int index)
    {
        chestRoom = room;
        chestIndex = index;
    }
    
    /// <summary>
    /// 기존 IInteractable 인터페이스 구현 - GameManager에서 호출됨
    /// </summary>
    public void OnInteract()
    {
        // GameManager.InteractObj를 null로 설정 (기존 Chest 클래스 패턴 따름)
        GameManager.Instance.InteractObj = null;
        
        // ChestRoom에서 이미 열렸는지 확인 (경쟁 조건 방지)
        if (chestRoom == null)
        {
            Debug.LogWarning($"[ChestRoomInteractable] 상자 {chestIndex}번: chestRoom이 null입니다.");
            return;
        }
        
        if (!canInteract || chestRoom.HasOpenedChest)
        {
            Debug.Log($"[ChestRoomInteractable] 상자 {chestIndex}번: 이미 다른 상자가 열려서 상호작용할 수 없습니다.");
            return;
        }
        
        // ChestRoom의 OnChestInteracted 호출
        chestRoom.OnChestInteracted(chestIndex);
    }
    
    /// <summary>
    /// 기존 Chest 클래스 패턴 - OnTriggerEnter2D에서 GameManager.InteractObj 설정
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canInteract && chestRoom != null && !chestRoom.HasOpenedChest)
        {
            GameManager.Instance.InteractObj = this;
        }
    }
    
    /// <summary>
    /// 기존 Chest 클래스 패턴 - OnTriggerExit2D에서 GameManager.InteractObj 해제
    /// </summary>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 현재 InteractObj가 이 오브젝트인 경우에만 null로 설정
            if (GameManager.Instance.InteractObj == this)
            {
                GameManager.Instance.InteractObj = null;
            }
        }
    }
}


