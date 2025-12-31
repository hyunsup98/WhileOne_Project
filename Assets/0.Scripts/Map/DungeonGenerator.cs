using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using static FloorRoomSetManager;

/// <summary>
/// 던전을 생성하는 메인 클래스
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("던전 설정")]
    [SerializeField] [Tooltip("던전 그리드 크기 (시작 위치(0,0)를 중심으로 한 반경, -gridSize ~ +gridSize 범위)")]
    private int gridSize = 10; // 그리드 크기 (시작 위치(0,0)를 중심으로 한 반경, -10 ~ +10 범위)

    [Header("현재 층 정보 (나중에 다른 매니저에서 처리)")]
    [SerializeField] [Tooltip("현재 층 (기본값 1층, UIText_Floor에서 자동으로 가져옴)")]
    private static int _currentFloor = 1; // 내부 저장용
    private static UIText_Floor _cachedFloorText; // UIText_Floor 캐시 (static)
    
    /// <summary>
    /// 현재 층 정보를 반환합니다. UIText_Floor에서 값을 가져옵니다.
    /// </summary>
    public static int currentFloor
    {
        get
        {
            // 캐시된 UIText_Floor가 유효한지 확인
            if (_cachedFloorText == null)
            {
                // 씬에서 UIText_Floor 인스턴스 찾기
                _cachedFloorText = Object.FindFirstObjectByType<UIText_Floor>();
                if (_cachedFloorText != null)
                {
                    _currentFloor = _cachedFloorText.Floor;
                }
                else
                {
                    Debug.LogWarning("[DungeonGenerator] UIText_Floor를 찾을 수 없습니다. 기본값(1)을 사용합니다.");
                }
            }
            else
            {
                // 캐시가 있으면 캐시된 값 사용
                _currentFloor = _cachedFloorText.Floor;
            }
            
            return _currentFloor;
        }
    }

    [Header("복도 프리팹")]
    [SerializeField] [Tooltip("가로 복도 프리팹 (좌우 연결용, Corridor_H)")]
    private GameObject corridorPrefabHorizontal; // 가로 복도 프리펩 (좌우 연결)
    [SerializeField] [Tooltip("세로 복도 프리팹 (상하 연결용, Corridor_V)")]
    private GameObject corridorPrefabVertical; // 세로 복도 프리펩 (상하 연결)
    
    [Header("던전 생성 설정")]
    [SerializeField] [Tooltip("방 생성 시 여러 방향으로 분기할 확률 (0~100%, 높을수록 더 많은 분기)")]
    [Range(0f, 100f)]
    private float branchProbability = 40f; // 여러 방향으로 분기할 확률 (%)
    [SerializeField] [Tooltip("최대 분기 개수 (한 방에서 생성할 수 있는 최대 연결 수)")]
    [Range(1, 4)]
    private int maxBranchCount = 3; // 최대 분기 개수
    [SerializeField] [Tooltip("방과 방 사이의 간격 (칸 수, 4의 배수로 지정, 기본값: 12칸)")]
    [Range(4, 100)]
    private int roomSpacingInCells = 12; // 방 간격 (칸 수, 4의 배수)
    [SerializeField] [Tooltip("복도 최소 길이 (칸 수, 기본값: 4칸)")]
    [Range(1, 20)]
    private int minCorridorLengthInCells = 4; // 복도 최소 길이 (칸 수)
    [SerializeField] [Tooltip("복도 최소 길이를 보장하기 위해 방 간격을 자동 조정할지 여부")]
    private bool autoAdjustSpacingForCorridors = true; // 복도 최소 길이 보장을 위한 자동 조정
    [SerializeField] [Tooltip("시작 시 자동으로 던전을 생성할지 여부")]
    private bool generateOnStart = true;
    [SerializeField] [Tooltip("던전 오브젝트들의 부모가 될 Grid Transform (없으면 자동으로 찾거나 생성)")]
    private Transform gridParent; // Grid 오브젝트 (없으면 자동으로 찾거나 생성)
    [SerializeField] [Tooltip("최소 타일 간격 (칸 단위)")]
    private float minTileSpacing = 5f; // 최소 타일 간격 (칸)
    
    [Header("오브젝트")]
    [SerializeField] [Tooltip("시작 방에 배치할 플레이어 오브젝트")]
    private GameObject playerObject; // 시작 방에 배치할 플레이어 오브젝트

    [SerializeField] [Range(0f, 100f)] [Tooltip("Dig Spot 생성 확률 (0 ~ 100%)")]
    private float digSpotSpawnChance = 10f; // Dig Spot 생성 확률 (%)

    [SerializeField] [Tooltip("보물 상자 프리팹 (타일맵에 배치됨)")]
    private GameObject treasureChestPrefab; // 보물 상자 프리팹

    [SerializeField] [Tooltip("방 타입 텍스트를 표시할지 여부 (디버그용)")]
    private bool showRoomTypeLabels = true; // 방 타입 텍스트 표시 여부
    [SerializeField] [Tooltip("방 타입 텍스트의 X 오프셋 (방 중심 기준, 음수 = 왼쪽, 양수 = 오른쪽)")]
    private float roomLabelOffsetX = -0.5f;
    [SerializeField] [Tooltip("방 타입 텍스트의 Y 오프셋 (방 위쪽 기준, Unity unit)")]
    private float roomLabelOffsetY = 0.5f; // 방 타입 텍스트 Y 오프셋 (방 위쪽)
    
    private DungeonGrid dungeonGrid;
    private Grid unityGrid; // Unity Grid 컴포넌트
    private Vector2Int startRoomPosition;
    private Vector2Int exitRoomPosition;

    [SerializeField] private MonsterPresenter monster;
    
    private bool isGenerating = false; // 던전 생성 중 플래그 (재귀 호출 방지)
    
    private void Awake()
    {
        // UIText_Floor 캐시 초기화
        RefreshFloorInfo();
    }
    
    /// <summary>
    /// UIText_Floor에서 현재 층 정보를 새로고침합니다.
    /// 씬이 변경되거나 UIText_Floor가 업데이트되었을 때 호출할 수 있습니다.
    /// </summary>
    public static void RefreshFloorInfo()
    {
        _cachedFloorText = Object.FindFirstObjectByType<UIText_Floor>();
        if (_cachedFloorText != null)
        {
            _currentFloor = _cachedFloorText.Floor;
            Debug.Log($"[DungeonGenerator] UIText_Floor에서 현재 층 정보를 가져왔습니다: {_currentFloor}층");
        }
        else
        {
            Debug.LogWarning("[DungeonGenerator] UIText_Floor를 찾을 수 없습니다. 기본값(1)을 사용합니다.");
        }
    }
    
    private void Start()
    {
        if (generateOnStart)
        {
            GenerateDungeon();
        }
    }
    
    /// <summary>
    /// 던전을 생성합니다.
    /// </summary>
    [ContextMenu("던전 생성")]
    public void GenerateDungeon()
    {
        // 이미 생성 중이면 중복 호출 방지
        if (isGenerating)
        {
            Debug.LogWarning("[DungeonGenerator] 던전 생성이 이미 진행 중입니다. 중복 호출을 무시합니다.");
            return;
        }
        
        isGenerating = true;
        
        try
        {
            FloorInfo floorInfo = FloorRoomSetManager.GetFloorInfo(currentFloor);

        int roomCount = floorInfo != null ? floorInfo.Rooms.Length : 9;

        // 기존 던전 제거
        ClearDungeon();
        
        // Grid 오브젝트 찾기 또는 생성
        Transform finalGridParent;
        unityGrid = DungeonGridHelper.SetupGridParent(gridParent, transform, out finalGridParent);
        gridParent = finalGridParent;
        
        // 4의 배수 검증 및 조정
        if (roomSpacingInCells % 4 != 0)
        {
            roomSpacingInCells = ((roomSpacingInCells + 3) / 4) * 4; // 올림하여 4의 배수로 조정
            Debug.LogWarning($"[DungeonGenerator] roomSpacingInCells가 4의 배수가 아니어서 {roomSpacingInCells}로 조정되었습니다.");
        }
        
        // 1. 그리드 초기화
        dungeonGrid = new DungeonGrid(gridSize);
        
        // 2. 방 생성
        DungeonRoomGenerator.GenerateRooms(dungeonGrid, roomCount, branchProbability, maxBranchCount);
        
        // 3. 인접 방 사이의 문을 보정
        DungeonRoomGenerator.EnsureAdjacentDoorsConnected(dungeonGrid);
        
        // 4. 층별 설정에 따른 방 타입 지정 (방 오브젝트 생성 전에 설정해야 함)
        SetRooms(floorInfo);
        
        // 5. 방 오브젝트 생성
        // 층별 프리팹 리스트를 우선 사용하고, 없으면 기본 프리팹을 fallback으로 사용
        Dictionary<RoomType, GameObject[]> roomPrefabs = new Dictionary<RoomType, GameObject[]>();
        Dictionary<EventRoomType, GameObject[]> eventRoomTypePrefabs = new Dictionary<EventRoomType, GameObject[]>();
        
        // 층별 프리팹 리스트를 가져와서 사용 (없으면 기본 프리팹 사용)
        if (floorInfo != null && floorInfo.FloorRoomPrefabs != null)
        {
            foreach (RoomType roomType in System.Enum.GetValues(typeof(RoomType)))
            {
                GameObject[] prefabs = FloorRoomSetManager.GetRoomPrefabs(currentFloor, roomType);
                // 층별 프리팹 리스트가 있으면 사용, 없으면 기본 프리팹 사용
                //roomPrefabs[roomType] = (prefabs != null && prefabs.Length > 0) ? prefabs : defaultPrefabs[roomType];
                roomPrefabs[roomType] = (prefabs != null && prefabs.Length > 0) ? prefabs : null;

            }
            // 이벤트 방 컨셉별 프리팹 가져오기
            if (floorInfo.EventRoomTypePrefabs != null)
            {
                eventRoomTypePrefabs = floorInfo.EventRoomTypePrefabs;
                Debug.Log($"[DungeonGenerator] 층별 이벤트 방 프리팹이 {eventRoomTypePrefabs.Count}개 로드되었습니다.");
                
                // 각 EventRoomType별 프리팹 개수 로그 출력
                foreach (var kvp in eventRoomTypePrefabs)
                {
                    int validCount = kvp.Value != null ? System.Array.FindAll(kvp.Value, p => p != null).Length : 0;
                    Debug.Log($"[DungeonGenerator] - {kvp.Key}: {validCount}개 프리팹 등록됨");
                }
            }
            else
            {
                Debug.LogWarning($"[DungeonGenerator] floorInfo.EventRoomTypePrefabs가 null입니다.");
            }
        }
        else
        {
            // FloorInfo가 없으면 기본 프리팹만 사용
            //roomPrefabs = defaultPrefabs;
        }
        
        Transform parent = gridParent != null ? gridParent : transform;
        
        // 5-0. 모든 방 프리팹의 최대 크기를 계산하여 방 간격 자동 설정
        CalculateAndSetRoomSpacing(roomPrefabs, eventRoomTypePrefabs);
        
        // 5-1. 방 배치 및 복도 최소 길이 검증 (반복 조정)
        int maxAdjustmentAttempts = 10; // 최대 조정 시도 횟수
        int adjustmentAttempt = 0;
        bool allCorridorsValid = false;
        
        // 안전장치: 최대 반복 횟수 제한
        int safetyCounter = 0;
        const int MAX_SAFETY_COUNTER = 1000;
        
        while (!allCorridorsValid && adjustmentAttempt < maxAdjustmentAttempts)
        {
            // 안전장치: 무한 루프 방지
            safetyCounter++;
            if (safetyCounter > MAX_SAFETY_COUNTER)
            {
                Debug.LogError($"[DungeonGenerator] 안전장치: 최대 반복 횟수({MAX_SAFETY_COUNTER}) 초과. 루프를 강제 종료합니다.");
                break;
            }
            
            // 재시도 시에도 프리팹 딕셔너리를 다시 로드 (floorInfo가 변경될 수 있으므로)
            if (adjustmentAttempt > 0)
            {
                // roomPrefabs 다시 로드
                roomPrefabs.Clear();
                foreach (RoomType roomType in System.Enum.GetValues(typeof(RoomType)))
                {
                    GameObject[] prefabs = FloorRoomSetManager.GetRoomPrefabs(currentFloor, roomType);
                    roomPrefabs[roomType] = (prefabs != null && prefabs.Length > 0) ? prefabs : null;
                }
                
                // eventRoomTypePrefabs 다시 로드 - FloorRoomSetManager에서 직접 가져오기
                // 재시도 시에는 floorInfos를 강제로 재초기화
                FloorInfo retryFloorInfo = FloorRoomSetManager.GetFloorInfo(currentFloor, forceReinitialize: true);
                if (retryFloorInfo != null && retryFloorInfo.EventRoomTypePrefabs != null)
                {
                    eventRoomTypePrefabs.Clear();
                    foreach (var kvp in retryFloorInfo.EventRoomTypePrefabs)
                    {
                        eventRoomTypePrefabs[kvp.Key] = kvp.Value;
                    }
                    Debug.Log($"[DungeonGenerator] 재시도 시 층별 이벤트 방 프리팹이 {eventRoomTypePrefabs.Count}개 로드되었습니다.");
                    
                    // 각 EventRoomType별 프리팹 개수 로그 출력
                    foreach (var kvp in eventRoomTypePrefabs)
                    {
                        int validCount = kvp.Value != null ? System.Array.FindAll(kvp.Value, p => p != null).Length : 0;
                        Debug.Log($"[DungeonGenerator] - {kvp.Key}: {validCount}개 프리팹 등록됨");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DungeonGenerator] 재시도 시 floorInfo를 가져올 수 없습니다. (retryFloorInfo: {(retryFloorInfo != null ? "not null" : "null")})");
                    eventRoomTypePrefabs.Clear();
                }
                
                Debug.Log($"[DungeonGenerator] 재시도 시 층별 이벤트 방 프리팹이 {eventRoomTypePrefabs.Count}개 로드되었습니다.");
                
                // 각 EventRoomType별 프리팹 개수 로그 출력
                foreach (var kvp in eventRoomTypePrefabs)
                {
                    int validCount = kvp.Value != null ? System.Array.FindAll(kvp.Value, p => p != null).Length : 0;
                    Debug.Log($"[DungeonGenerator] - {kvp.Key}: {validCount}개 프리팹 등록됨");
                }
            }
            
            // 방 오브젝트 생성
            DungeonRoomPlacer.CreateRoomObjects(
                dungeonGrid, parent, unityGrid, roomSpacingInCells, roomPrefabs, eventRoomTypePrefabs,
                showRoomTypeLabels, roomLabelOffsetX, roomLabelOffsetY);
            
            // 방 겹침 검증
            bool hasRoomOverlap = DungeonCorridorValidator.ValidateRoomOverlaps(
                dungeonGrid, out List<string> overlappingRooms);
            
            if (hasRoomOverlap && adjustmentAttempt < maxAdjustmentAttempts - 1)
            {
                Debug.LogWarning($"[DungeonGenerator] 방 겹침 감지. 방 배치를 재시도합니다. (시도 {adjustmentAttempt + 1}/{maxAdjustmentAttempts})");
                foreach (string overlap in overlappingRooms)
                {
                    Debug.LogWarning($"[DungeonGenerator] - {overlap}");
                }
                
                // 방 간격 증가 (4칸 단위로 증가)
                int oldSpacing = roomSpacingInCells;
                roomSpacingInCells += 4;
                
                Debug.LogWarning($"[DungeonGenerator] 방 간격을 {oldSpacing}칸에서 {roomSpacingInCells}칸으로 증가시킵니다.");
                
                // 기존 방 오브젝트 제거
                ClearRoomObjects(parent);
                
                adjustmentAttempt++;
                continue;
            }
            else if (hasRoomOverlap)
            {
                Debug.LogWarning($"[DungeonGenerator] 방 겹침이 남아있지만 최대 조정 시도 횟수({maxAdjustmentAttempts})에 도달했습니다.");
            }
            
            // 복도 최소 길이 검증
            if (autoAdjustSpacingForCorridors)
            {
                float minCorridorLength = minCorridorLengthInCells;
                bool needsAdjustment = DungeonCorridorValidator.ValidateCorridorLengths(
                    dungeonGrid, unityGrid, minCorridorLength, out float minActualLength);
                
                if (needsAdjustment && adjustmentAttempt < maxAdjustmentAttempts - 1)
                {
                    // 방 간격 증가 (4칸 단위로 증가)
                    int oldSpacing = roomSpacingInCells;
                    roomSpacingInCells += 4;
                    
                    Debug.LogWarning($"[DungeonGenerator] 복도 최소 길이({minCorridorLengthInCells}칸) 미만 감지. " +
                        $"현재 최소 복도 길이: {minActualLength / 1:F2}칸. " +
                        $"방 간격을 {oldSpacing}칸에서 {roomSpacingInCells}칸으로 증가시킵니다. (시도 {adjustmentAttempt + 1}/{maxAdjustmentAttempts})");
                    
                    // 기존 방 오브젝트 제거
                    ClearRoomObjects(parent);
                    
                    adjustmentAttempt++;
                    continue;
                }
                else if (needsAdjustment)
                {
                    Debug.LogWarning($"[DungeonGenerator] 복도 최소 길이 보장 실패. 최대 조정 시도 횟수({maxAdjustmentAttempts})에 도달했습니다.");
                }
            }
            
            allCorridorsValid = true;
        }
        
        // 6. 복도 생성 및 검증 (반복 조정)
        int maxCorridorAttempts = 10;
        int corridorAttempt = 0;
        bool corridorsPlacedCorrectly = false;
        
        // 안전장치: 최대 반복 횟수 제한
        int corridorSafetyCounter = 0;
        const int MAX_CORRIDOR_SAFETY_COUNTER = 1000;
        
        while (!corridorsPlacedCorrectly && corridorAttempt < maxCorridorAttempts)
        {
            // 안전장치: 무한 루프 방지
            corridorSafetyCounter++;
            if (corridorSafetyCounter > MAX_CORRIDOR_SAFETY_COUNTER)
            {
                Debug.LogError($"[DungeonGenerator] 안전장치: 복도 생성 최대 반복 횟수({MAX_CORRIDOR_SAFETY_COUNTER}) 초과. 루프를 강제 종료합니다.");
                break;
            }
            
            // 기존 복도 제거 (재시도 시)
            if (corridorAttempt > 0)
            {
                ClearCorridors(parent);
            }
            
            // 복도 생성
            DungeonCorridorGenerator.CreateCorridors(
                dungeonGrid, parent, unityGrid,
                corridorPrefabHorizontal, corridorPrefabVertical);
            
            // 복도 배치 검증
            bool hasCorridorProblems = DungeonCorridorValidator.ValidateCorridorPlacement(
                dungeonGrid, unityGrid, roomSpacingInCells, out List<string> problematicCorridors, 2.0f);
            
            if (hasCorridorProblems && corridorAttempt < maxCorridorAttempts - 1)
            {
                Debug.LogWarning($"[DungeonGenerator] 복도 배치 문제 감지. 던전 구조를 재생성합니다. (시도 {corridorAttempt + 1}/{maxCorridorAttempts})");
                foreach (string problem in problematicCorridors)
                {
                    Debug.LogWarning($"[DungeonGenerator] - {problem}");
                }
                
                // 모든 오브젝트 제거
                ClearRoomObjects(parent);
                ClearCorridors(parent);
                
                // 방 간격 증가 (재시도 시)
                if (corridorAttempt > 0)
                {
                    int oldSpacing = roomSpacingInCells;
                    roomSpacingInCells += 4;
                    Debug.Log($"[DungeonGenerator] 방 간격을 {oldSpacing}칸에서 {roomSpacingInCells}칸으로 증가시킵니다.");
                }
                
                // 그리드 구조 재생성
                dungeonGrid = new DungeonGrid(gridSize);
                DungeonRoomGenerator.GenerateRooms(dungeonGrid, roomCount, branchProbability, maxBranchCount);
                DungeonRoomGenerator.EnsureAdjacentDoorsConnected(dungeonGrid);
                SetRooms(floorInfo);
                
                // 프리팹 딕셔너리 다시 로드
                roomPrefabs.Clear();
                foreach (RoomType roomType in System.Enum.GetValues(typeof(RoomType)))
                {
                    GameObject[] prefabs = FloorRoomSetManager.GetRoomPrefabs(currentFloor, roomType);
                    roomPrefabs[roomType] = (prefabs != null && prefabs.Length > 0) ? prefabs : null;
                }
                
                eventRoomTypePrefabs.Clear();
                foreach (EventRoomType eventType in System.Enum.GetValues(typeof(EventRoomType)))
                {
                    GameObject[] prefabs = FloorRoomSetManager.GetEventRoomPrefabs(currentFloor, eventType);
                    if (prefabs != null && prefabs.Length > 0)
                    {
                        eventRoomTypePrefabs[eventType] = prefabs;
                    }
                }
                
                // 방 배치 재시도
                DungeonRoomPlacer.CreateRoomObjects(
                    dungeonGrid, parent, unityGrid, roomSpacingInCells, roomPrefabs, eventRoomTypePrefabs,
                    showRoomTypeLabels, roomLabelOffsetX, roomLabelOffsetY);
                
                corridorAttempt++;
                continue;
            }
            else if (hasCorridorProblems)
            {
                Debug.LogWarning($"[DungeonGenerator] 복도 배치 문제가 남아있지만 최대 시도 횟수({maxCorridorAttempts})에 도달했습니다.");
            }
            
            corridorsPlacedCorrectly = true;
        }

        // 7. DoorSpace/NoDoor 갱신
        RefreshAllDoorStates();
        
        // 9. 플레이어를 시작 방 중심으로 이동
        PlacePlayerObject();
        
        // 10. 일반 전투 방에 Dig Spot 배치
        DungeonItemPlacer.PlaceDigSpots(dungeonGrid, RoomType.Normal, digSpotSpawnChance, unityGrid);

        // 11. 보물 방에 보물 상자 배치
        DungeonItemPlacer.PlaceTreasureChests(dungeonGrid, treasureChestPrefab, unityGrid);
        
        // 12. 보물 방에 Dig Spot 배치
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            // 보물 방만 처리
            if (room.roomType != RoomType.Treasure) continue;
            
            // TreasureRoom 컴포넌트 찾아서 Dig Spot 배치
            TreasureRoom treasureRoom = room.roomObject.GetComponent<TreasureRoom>();
            if (treasureRoom != null)
            {
                treasureRoom.PlaceDigSpots(unityGrid);
            }
        }
        }
        finally
        {
            isGenerating = false;
        }
    }

    /// <summary>
    /// 방 생성 완료 후 처리 (시작 방, 탈출 방, 이벤트 방 지정)
    /// 층별 설정에 따른 방 타입을 지정합니다.
    /// </summary>
    private void SetRooms(FloorInfo floorInfo)
    {
        if (floorInfo == null)
        {
            Debug.LogWarning("[DungeonGenerator] floorInfo가 null이어서 방 타입을 지정할 수 없습니다.");
            return;
        }
        
        // 모든 방을 Normal로 초기화
        var allPositions = dungeonGrid.GetAllPositions().ToList();
        foreach (var pos in allPositions)
        {
            var r = dungeonGrid.GetRoom(pos);
            if (r != null) r.roomType = RoomType.Normal;
        }
        
        // 시작 방 지정 (랜덤)
        if (allPositions.Count == 0) return;
        int startIndex = Random.Range(0, allPositions.Count);
        startRoomPosition = allPositions[startIndex];
        var startRoom = dungeonGrid.GetRoom(startRoomPosition);
        if (startRoom != null) startRoom.roomType = RoomType.Start;
        
        // 방 거리 계산
        Dictionary<Vector2Int, int> distances = DungeonRoomGenerator.CalculateDistancesFrom(dungeonGrid, startRoomPosition);

        // 출구 방 지정 (가장 먼 방)
        // 3층일 경우 출구방 대신 보스방으로 지정
        // 수정: 보스방 대신 포탈 방으로 지정
        exitRoomPosition = DungeonRoomGenerator.SelectExitRoom(distances, startRoomPosition);
        var exitRoom = dungeonGrid.GetRoom(exitRoomPosition);
        if (exitRoom != null)
        {
            if (floorInfo.FloorNumber == 3) exitRoom.roomType = RoomType.Portal;
            else exitRoom.roomType = RoomType.Exit;
        }

        // 이벤트 방, 함정 방, 보물 방, 등 기타 특수 방 지정
        Vector2Int startPos = startRoomPosition;
        Vector2Int exitPos = exitRoomPosition;
        var remaining = allPositions.Where(p => p != startPos && p != exitPos).ToList();

        // 함정 방 지정
        int trapRoomCount = floorInfo.GetRoomCountWithType(RoomType.Trap);
        if (trapRoomCount > 0 && remaining.Count >= trapRoomCount)
        {
            for (int i = 0; i < trapRoomCount; i++)
            {
                var trapPos = remaining[Random.Range(0, remaining.Count)];
                var trapRoom = dungeonGrid.GetRoom(trapPos);
                if (trapRoom != null) trapRoom.roomType = RoomType.Trap;
                remaining.Remove(trapPos);
            }
        }

        // 이벤트 방 지정
        int eventRoomCount = floorInfo.GetRoomCountWithType(RoomType.Event);
        if (eventRoomCount > 0 && remaining.Count >= eventRoomCount)
        {
            // 이벤트 방 컨셉 관리자 초기화
            EventRoomManager.InitializeForFloor();
            
            for (int i = 0; i < eventRoomCount; i++)
            {
                // 사용 가능한 이벤트 방 컨셉이 없으면 중단
                if (EventRoomManager.GetAvailableCount() == 0)
                {
                    Debug.LogWarning($"[DungeonGenerator] 사용 가능한 이벤트 방 컨셉이 없어서 {i}개만 생성했습니다.");
                    break;
                }
                
                // 이벤트 방 컨셉 선택
                EventRoomType? eventType = EventRoomManager.GetRandomEventType();
                if (!eventType.HasValue)
                {
                    Debug.LogWarning($"[DungeonGenerator] 이벤트 방 컨셉을 선택할 수 없어서 {i}개만 생성했습니다.");
                    break;
                }

                Debug.Log($"[DungeonGenerator] 이벤트 방 컨셉으로 {eventType.Value}을(를) 선택했습니다.");

                // 이벤트 방 위치 선택
                var eventPos = remaining[Random.Range(0, remaining.Count)];
                var eventRoom = dungeonGrid.GetRoom(eventPos);
                if (eventRoom != null)
                {
                    eventRoom.roomType = RoomType.Event;
                    eventRoom.eventRoomType = eventType.Value;
                }
                remaining.Remove(eventPos);
            }
        }
        

        // 보물 방 지정
        int treasureRoomCount = floorInfo.GetRoomCountWithType(RoomType.Treasure);
        if (treasureRoomCount > 0 && remaining.Count >= treasureRoomCount)
        {
            for (int i = 0; i < treasureRoomCount; i++)
            {
                var treasurePos = remaining[Random.Range(0, remaining.Count)];
                var treasureRoom = dungeonGrid.GetRoom(treasurePos);
                if (treasureRoom != null) treasureRoom.roomType = RoomType.Treasure;
                remaining.Remove(treasurePos);
            }
        }
    }

    /// <summary>
    /// 생성된 모든 방의 DoorSpace/NoDoor 상태를 갱신합니다.
    /// </summary>
    private void RefreshAllDoorStates()
    {
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            BaseRoom br = room.roomObject.GetComponent<BaseRoom>();
            if (br != null)
            {
                br.RefreshDoorStates();
            }
        }
    }

    /// <summary>
    /// 플레이어 오브젝트를 시작 방 중심으로 이동합니다.
    /// </summary>
    private void PlacePlayerObject()
    {
        if (playerObject == null)
        {
            Debug.LogWarning("playerObject가 지정되지 않아 시작 방으로 이동할 수 없습니다.");
            return;
        }
        
        Room startRoom = dungeonGrid.GetRoom(startRoomPosition);
        if (startRoom == null || startRoom.roomObject == null)
        {
            Debug.LogError("[DungeonGenerator] 시작 방 정보가 없어 플레이어를 이동할 수 없습니다. 던전 생성에 실패한 것 같습니다.");
            // GenerateDungeon()을 다시 호출하면 무한 루프와 메모리 누수가 발생할 수 있음,,,,,,,,,,,,,,
            isGenerating = false;
            GenerateDungeon();
            //return;
        }
        
        Vector3 center = DungeonRoomHelper.GetRoomWorldCenter(startRoom.roomObject);
        playerObject.transform.position = center;
    }
    
    /// <summary>
    /// 기존 던전을 제거합니다.
    /// </summary>
    private void ClearDungeon()
    {
        // Grid 하위의 오브젝트 제거 (room과 복도)
        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                GameObject child = gridParent.GetChild(i).gameObject;
                if (child != null)
                {
                    DestroyImmediate(child);
                }
            }
        }
        
        // transform 하위의 오브젝트도 제거 (기존 호환성)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            // Grid 오브젝트는 제거하지 않음
            if (child != null && child.GetComponent<Grid>() == null)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        // 메모리 정리
        dungeonGrid = null;
        
        // 강제 가비지 컬렉션 (메모리 누수 방지)
        System.GC.Collect();
    }
    
    /// <summary>
    /// 방 오브젝트만 제거합니다 (복도는 유지).
    /// </summary>
    private void ClearRoomObjects(Transform parent)
    {
        if (parent == null) return;
        
        // 모든 방 오브젝트 찾아서 제거
        List<GameObject> roomsToDestroy = new List<GameObject>();
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room != null && room.roomObject != null)
            {
                roomsToDestroy.Add(room.roomObject);
                room.roomObject = null; // 참조 제거
            }
        }
        
        // 방 오브젝트 제거
        foreach (GameObject roomObj in roomsToDestroy)
        {
            if (roomObj != null)
            {
                DestroyImmediate(roomObj);
            }
        }
    }
    
    /// <summary>
    /// 복도 오브젝트만 제거합니다 (방은 유지).
    /// </summary>
    private void ClearCorridors(Transform parent)
    {
        if (parent == null) return;
        
        // 모든 복도 오브젝트 찾아서 제거 (Corridor 태그 또는 이름으로 찾기)
        List<GameObject> corridorsToDestroy = new List<GameObject>();
        
        // 부모의 모든 자식 오브젝트 확인
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child == null) continue;
            
            // 복도 관련 오브젝트인지 확인 (이름에 "Corridor" 포함)
            string childName = child.name.ToLower();
            if (childName.Contains("corridor") || 
                (childName.Contains("clone") && (childName.Contains("h") || childName.Contains("v"))))
            {
                corridorsToDestroy.Add(child.gameObject);
            }
        }
        
        // 복도 오브젝트 제거
        foreach (GameObject corridorObj in corridorsToDestroy)
        {
            if (corridorObj != null)
            {
                DestroyImmediate(corridorObj);
            }
        }
        
        Debug.Log($"[DungeonGenerator] 복도 {corridorsToDestroy.Count}개 제거 완료");
    }
    
    /// <summary>
    /// 모든 방 프리팹의 최대 크기를 계산하여 방 간격을 자동으로 설정합니다.
    /// </summary>
    private void CalculateAndSetRoomSpacing(
        Dictionary<RoomType, GameObject[]> roomPrefabs,
        Dictionary<EventRoomType, GameObject[]> eventRoomTypePrefabs)
    {
        float maxRoomWidth = 0f;
        float maxRoomHeight = 0f;
        
        // 모든 RoomType 프리팹 확인
        foreach (var kvp in roomPrefabs)
        {
            if (kvp.Value == null) continue;
            
            foreach (GameObject prefab in kvp.Value)
            {
                if (prefab == null) continue;
                
                BaseRoom baseRoom = prefab.GetComponent<BaseRoom>();
                if (baseRoom != null)
                {
                    float width = baseRoom.RoomWidth;
                    float height = baseRoom.RoomHeight;
                    
                    if (width > maxRoomWidth) maxRoomWidth = width;
                    if (height > maxRoomHeight) maxRoomHeight = height;
                }
            }
        }
        
        // 모든 EventRoomType 프리팹 확인
        if (eventRoomTypePrefabs != null)
        {
            foreach (var kvp in eventRoomTypePrefabs)
            {
                if (kvp.Value == null) continue;
                
                foreach (GameObject prefab in kvp.Value)
                {
                    if (prefab == null) continue;
                    
                    BaseRoom baseRoom = prefab.GetComponent<BaseRoom>();
                    if (baseRoom != null)
                    {
                        float width = baseRoom.RoomWidth;
                        float height = baseRoom.RoomHeight;
                        
                        if (width > maxRoomWidth) maxRoomWidth = width;
                        if (height > maxRoomHeight) maxRoomHeight = height;
                    }
                }
            }
        }
        
        // 최대 크기를 기반으로 방 간격 계산
        // 최대 width와 height 중 큰 값을 사용하고, 여유 공간을 추가
        float maxRoomSize = Mathf.Max(maxRoomWidth, maxRoomHeight);
        
        if (maxRoomSize > 0f)
        {
            // 최대 방 크기 + 복도 최소 길이를 고려하여 간격 계산
            // 4의 배수로 올림
            float calculatedSpacing = maxRoomSize + minCorridorLengthInCells;
            int newSpacing = Mathf.CeilToInt(calculatedSpacing / 4f) * 4; // 4의 배수로 올림
            
            // 최소값 보장 (기본값 이상)
            if (newSpacing < 12) newSpacing = 12;
            
            int oldSpacing = roomSpacingInCells;
            roomSpacingInCells = newSpacing;
            
            Debug.Log($"[DungeonGenerator] 방 간격 자동 계산 완료 - 최대 방 크기: {maxRoomSize:F2} (Width: {maxRoomWidth:F2}, Height: {maxRoomHeight:F2}), " +
                     $"계산된 간격: {calculatedSpacing:F2}, 설정된 간격: {roomSpacingInCells}칸 (이전: {oldSpacing}칸)");
        }
        else
        {
            Debug.LogWarning($"[DungeonGenerator] 방 프리팹에서 크기 정보를 찾을 수 없어 기본 간격({roomSpacingInCells}칸)을 사용합니다.");
        }
    }
}

