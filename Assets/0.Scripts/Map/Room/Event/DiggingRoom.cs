using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 도굴 방 이벤트
/// 15x15 크기의 방, 중앙에 경고 표지판이 있고 5개 중 진짜 2개 가짜 3개
/// </summary>
public class DiggingRoom : BaseEventRoom
{
    [Header("Digging Room Settings")]
    [SerializeField] [Tooltip("경고 표지판 오브젝트 (중앙에 배치)")]
    private GameObject warningSign;
    [SerializeField] [Tooltip("진짜 Dig Spot 개수")]
    private int realDigSpotCount = 2;
    [SerializeField] [Tooltip("가짜 Dig Spot 개수")]
    private int fakeDigSpotCount = 3;
    [SerializeField] [Tooltip("가짜를 파면 받는 HP 피해 비율 (0.1 = 10%)")]
    private float fakeDigDamageRatio = 0.1f;
    [SerializeField] [Tooltip("Dig Spot 타일맵의 Sorting Order (Order in Layer)")]
    private int digSpotTilemapSortingOrder = 3;
    
    private Tilemap digSpotTileMap;
    private Tilemap floorTileMap;
    private Dictionary<Vector3Int, bool> digSpotTypes; // true = 진짜, false = 가짜
    private Dictionary<Vector3Int, bool> lastKnownAfterDigTiles; // AfterDigTile이 설정된 위치 추적
    private int realDigSpotsFound = 0;
    private int fakeDigSpotsFound = 0;
    private Tile digSpotTile; // DungeonManager에서 가져온 Dig Spot 타일
    private Tile afterDigTile; // DungeonManager에서 가져온 AfterDigTile

    protected override void InitializeEventRoom()
    {
        digSpotTile = GameManager.Instance?.CurrentDungeon.DigSpotTile;
        afterDigTile = GameManager.Instance?.CurrentDungeon.AfterDigTile;

        if (digSpotTile == null)
        {
            Debug.LogWarning("[DiggingRoom] CurrentDungeon에서 Dig Spot 타일을 찾을 수 없습니다.");
        }
        
        // RoomController 찾기 및 타일맵 참조
        RoomController roomController = GetComponent<RoomController>();
        if (roomController != null)
        {
            digSpotTileMap = roomController.DigSpotTileMap;
            floorTileMap = roomController.FloorTileMap;
        }
        else
        {
            Debug.LogWarning("[DiggingRoom] RoomController를 찾을 수 없습니다.");
            return;
        }
        
        if (digSpotTileMap == null || floorTileMap == null)
        {
            Debug.LogError("[DiggingRoom] 타일맵을 찾을 수 없습니다.");
            return;
        }
        
        // 타일맵의 Sorting Order 설정
        TilemapRenderer tilemapRenderer = digSpotTileMap.GetComponent<TilemapRenderer>();
        if (tilemapRenderer != null)
        {
            tilemapRenderer.sortingOrder = digSpotTilemapSortingOrder;
        }
        
        // AfterDigTile 추적용 딕셔너리 초기화
        lastKnownAfterDigTiles = new Dictionary<Vector3Int, bool>();
        
        if (warningSign != null)
        {
            // 경고 표지판에 상호작용 컴포넌트 추가
            WarningSignInteractable signInteractable = warningSign.GetComponent<WarningSignInteractable>();
            if (signInteractable == null)
            {
                signInteractable = warningSign.AddComponent<WarningSignInteractable>();
            }
            signInteractable.Initialize(this);
        }
        
        // Dig Spot 배치
        PlaceDigSpots();
        
        // 디버그: 타일맵과 타일 정보 확인
        Debug.Log($"[DiggingRoom] 초기화 완료 - digSpotTileMap:{digSpotTileMap?.name}, digSpotTile:{digSpotTile?.name}");
    }
    
    //private void Update()
    //{
    //    // TileManager.Dig()가 호출되어 AfterDigTile이 설정되었는지 감지
    //    if (floorTileMap == null || afterDigTile == null) return;
        
    //    CheckForDiggedTiles();
    //}
    
    /// <summary>
    /// FloorTileMap에 새로 설정된 AfterDigTile을 감지하여 Dig Spot 처리
    /// </summary>
    private void CheckForDiggedTiles()
    {
        if (floorTileMap == null || afterDigTile == null) return;
        
        // 타일맵의 bounds 확인
        floorTileMap.CompressBounds();
        BoundsInt bounds = floorTileMap.cellBounds;
        
        // 새로 AfterDigTile이 설정된 위치 찾기
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase currentTile = floorTileMap.GetTile(cellPos);
                
                // AfterDigTile이 설정되어 있고, 이전에 추적하지 않았던 위치인지 확인
                if (currentTile == afterDigTile && !lastKnownAfterDigTiles.ContainsKey(cellPos))
                {
                    // DigSpotTileMap에서 해당 위치의 타일이 제거되었는지 확인
                    if (digSpotTileMap != null && digSpotTileMap.GetTile(cellPos) == null)
                    {
                        // 이전에 DigSpot이었던 위치인지 확인
                        if (digSpotTypes != null && digSpotTypes.ContainsKey(cellPos))
                        {
                            lastKnownAfterDigTiles[cellPos] = true;
                            
                            // 플레이어 찾기
                            Player player = GetPlayer();
                            if (player != null)
                            {
                                Debug.Log($"[DiggingRoom] Dig 감지 - cell:{cellPos}");
                                OnDigSpotDug(cellPos);
                            }
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Dig Spot을 배치합니다.
    /// </summary>
    private void PlaceDigSpots()
    {
        if (digSpotTileMap == null || digSpotTile == null) return;
        
        // Dig Spot 타입 딕셔너리 초기화
        digSpotTypes = new Dictionary<Vector3Int, bool>();
        
        // RoomCenterMarker를 사용하여 방의 중앙 좌표 계산
        Vector3 roomCenterWorldPos = DungeonRoomHelper.GetRoomWorldCenter(gameObject);
        Vector3Int roomCenterCellPos = digSpotTileMap.WorldToCell(roomCenterWorldPos);
        
        // 방 크기 계산 (15x15 타일)
        int roomSize = 15;
        int centerX = roomSize / 2;
        int centerY = roomSize / 2;
        
        // Dig Spot 배치 가능한 위치 리스트 생성 (중앙 제외)
        List<Vector3Int> availablePositions = new List<Vector3Int>();
        for (int x = 1; x < roomSize - 1; x++)
        {
            for (int y = 1; y < roomSize - 1; y++)
            {
                // 중앙 영역 제외 (경고 표지판 위치)
                if (Mathf.Abs(x - centerX) > 2 || Mathf.Abs(y - centerY) > 2)
                {
                    // RoomCenterMarker 기준으로 셀 좌표 계산
                    Vector3Int cellPos = new Vector3Int(
                        roomCenterCellPos.x + (x - centerX),
                        roomCenterCellPos.y + (y - centerY),
                        0
                    );
                    availablePositions.Add(cellPos);
                }
            }
        }
        
        // 랜덤하게 섞기
        for (int i = 0; i < availablePositions.Count; i++)
        {
            Vector3Int temp = availablePositions[i];
            int randomIndex = Random.Range(i, availablePositions.Count);
            availablePositions[i] = availablePositions[randomIndex];
            availablePositions[randomIndex] = temp;
        }
        
        // 진짜 Dig Spot 배치
        for (int i = 0; i < realDigSpotCount && i < availablePositions.Count; i++)
        {
            Vector3Int pos = availablePositions[i];
            digSpotTileMap.SetTile(pos, digSpotTile);
            digSpotTypes[pos] = true; // 진짜로 표시
        }
        
        // 가짜 Dig Spot 배치
        int startIndex = realDigSpotCount;
        for (int i = 0; i < fakeDigSpotCount && startIndex + i < availablePositions.Count; i++)
        {
            Vector3Int pos = availablePositions[startIndex + i];
            digSpotTileMap.SetTile(pos, digSpotTile);
            digSpotTypes[pos] = false; // 가짜로 표시
        }
        
        Debug.Log($"[DiggingRoom] Dig Spot 배치 완료: 진짜 {realDigSpotCount}개, 가짜 {fakeDigSpotCount}개 (중앙 기준: {roomCenterWorldPos})");
        
        // 디버그: 배치된 위치 확인
        foreach (var kvp in digSpotTypes)
        {
            TileBase placedTile = digSpotTileMap.GetTile(kvp.Key);
            Debug.Log($"[DiggingRoom] Dig Spot 배치 위치: {kvp.Key}, 타입:{(kvp.Value ? "진짜" : "가짜")}, 타일:{placedTile?.name}");
        }
    }
    
    /// <summary>
    /// 규칙을 안내합니다.
    /// </summary>
    public void ShowRules()
    {
        string rules = "도굴 방 규칙:\n" +
                      "5개 중 진짜는 2개, 가짜는 3개\n" +
                      "가짜를 파면 플레이어의 HP가 10% 감소합니다.";
        
        Debug.Log($"[DiggingRoom] {rules}");
        // TODO: UI에 규칙 표시
    }
    
    /// <summary>
    /// Dig Spot을 파는 이벤트 처리
    /// (타일은 이미 RoomController에서 제거되었으므로 여기서는 로직만 처리)
    /// </summary>
    public void OnDigSpotDug(Vector3Int cellPosition)
    {
        //if (player == null) return;
        
        // 해당 위치의 Dig Spot 타입 확인
        if (digSpotTypes == null || !digSpotTypes.ContainsKey(cellPosition))
        {
            Debug.LogWarning($"[DiggingRoom] Dig Spot 위치 {cellPosition}를 찾을 수 없습니다.");
            return;
        }
        
        bool isReal = digSpotTypes[cellPosition];
        
        // 딕셔너리에서 제거 (이미 파낸 것으로 처리)
        digSpotTypes.Remove(cellPosition);
        
        if (isReal)
        {
            realDigSpotsFound++;
            OnRealDigSpotFound();
        }
        else
        {
            fakeDigSpotsFound++;
            OnFakeDigSpotFound(cellPosition);
        }
    }
    
    /// <summary>
    /// 진짜 Dig Spot을 찾았을 때
    /// </summary>
    private void OnRealDigSpotFound()
    {
        Debug.Log("[DiggingRoom] 진짜 Dig Spot을 찾았습니다!");
        
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
    /// 가짜 Dig Spot을 찾았을 때
    /// </summary>
    private void OnFakeDigSpotFound(Vector3Int cellPosition)
    {
        Debug.Log("[DiggingRoom] 가짜 Dig Spot을 찾았습니다!");

        Player player = GameManager.Instance?.player;

        // HP 10% 감소
        float damage = player.MaxHp * fakeDigDamageRatio;
        //player.ChangedHealth += -damage;

        // TakenDamage로 변경 (cellPosition을 Vector2 월드 좌표로 변환)
        Vector2 worldPosition = floorTileMap != null 
            ? (Vector2)floorTileMap.CellToWorld(cellPosition) 
            : Vector2.zero;
        DamagePlayer(damage, worldPosition, player);

        Debug.Log($"[DiggingRoom] HP가 {damage:F1} 감소했습니다. (현재 HP: {player.Hp:F1}/{player.MaxHp:F1})");
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
/// 경고 표지판 상호작용 컴포넌트
/// </summary>
public class WarningSignInteractable : Interactable, IInteractable
{
    private DiggingRoom diggingRoom;
    private bool isLayerInitialized = false;
    
    [field: SerializeField] public float YOffset { get; set; } = 1.5f;
    
    public Vector3 Pos => transform.position;
    
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
            Debug.LogError("[WarningSignInteractable] Player 레이어를 찾을 수 없습니다!");
            return;
        }
        
        playerLayer = 1 << playerLayerIndex;
        isLayerInitialized = true;
    }
    
    public void Initialize(DiggingRoom room)
    {
        diggingRoom = room;
        
        if (!isLayerInitialized)
        {
            InitializePlayerLayer();
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
        if (diggingRoom != null)
        {
            diggingRoom.ShowRules();
        }
        
        // 규칙 표시 후 GameManager에서 제거
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

