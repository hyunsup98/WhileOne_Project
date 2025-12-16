using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 던전을 생성하는 메인 클래스
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [SerializeField] private int roomCount = 9; // 생성할 방의 개수 (1층 기본값: 9개)
    [SerializeField] private int gridSize = 10; // 그리드 크기 (시작 위치(0,0)를 중심으로 한 반경, -10 ~ +10 범위)
    [SerializeField] private int eventRoomCount = 2; // 이벤트 방 개수
    
    [Header("Room Prefabs")]
    [SerializeField] private GameObject normalRoomPrefab; // 기본 Room 프리펩 (1.Prefabs > Map > Room)
    [SerializeField] private GameObject startRoomPrefab; // 시작 방 프리펩 (없으면 normalRoomPrefab 사용)
    [SerializeField] private GameObject exitRoomPrefab; // 다음층 입구 방 프리펩 (없으면 normalRoomPrefab 사용)
    [SerializeField] private GameObject eventRoomPrefab; // 이벤트 방 프리펩 (없으면 normalRoomPrefab 사용)
    
    [Header("Corridor")]
    [SerializeField] private bool useTilemapForCorridors = false; // Tilemap 방식 사용 여부
    [SerializeField] private GameObject corridorPrefabHorizontal; // 가로 복도 프리펩 (좌우 연결, 너비 1칸) - Sprite 방식
    [SerializeField] private GameObject corridorPrefabVertical; // 세로 복도 프리펩 (상하 연결, 너비 1칸) - Sprite 방식
    [SerializeField] private float corridorLength = 2f; // 복도 길이 (칸 수)
    
    [Header("Corridor Tilemap (Tilemap 방식 사용 시)")]
    [SerializeField] private TileBase corridorFloorTile; // 복도 바닥 타일
    [SerializeField] private TileBase corridorWallTile; // 복도 벽 타일 (선택사항)
    
    [Header("Generation Settings")]
    [SerializeField] private float roomSpacing = 6f; // 방 간격 (roomSize + corridorLength 권장)
    [SerializeField] private bool autoCalculateSpacing = true; // roomSize와 corridorLength로 자동 계산
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private Transform gridParent; // Grid 오브젝트 (없으면 자동으로 찾거나 생성)
    [SerializeField] private float minTileSpacing = 5f; // 최소 타일 간격 (칸)
    [Header("Scene Objects")]
    [SerializeField] private GameObject exitPrefab; // 출구 프리펩 (방 중앙에 생성)
    [SerializeField] private GameObject playerObject; // 시작 방에 배치할 플레이어 오브젝트
    [SerializeField] private bool showRoomTypeLabels = true; // 방 타입 텍스트 표시 여부
    [SerializeField] private float roomLabelOffsetY = 0.5f; // 방 타입 텍스트 Y 오프셋 (방 위쪽)
    [SerializeField] private bool showCorridorLabels = false; // 복도 타일 텍스트 표시 여부 (디버그용)
    [SerializeField] private float corridorLabelOffsetY = 0.2f; // 복도 텍스트 Y 오프셋
    [SerializeField] private Font roomLabelFont; // 방 타입 라벨에 사용할 폰트 (null이면 기본 폰트 사용)
    private const float DefaultCellSize = 0.32f; // 타일 한 칸 기본 크기
    
    private DungeonGrid dungeonGrid;
    private Grid unityGrid; // Unity Grid 컴포넌트
    private Vector2Int startRoomPosition;
    private Vector2Int exitRoomPosition;
    private List<Vector2Int> eventRoomPositions;
    private Dictionary<Vector2Int, GameObject> corridors; // 복도 오브젝트
    private Tilemap corridorTilemap; // 복도용 Tilemap (Tilemap 방식 사용 시)
    private Canvas worldSpaceCanvas; // UI Text용 World Space Canvas
    
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
        // 기존 던전 제거
        ClearDungeon();
        
        // Grid 오브젝트 찾기 또는 생성
        SetupGridParent();
        
        // Grid 셀 크기를 방 타일 크기와 동일하게 맞춤 (World/Cell 변환 일관성 확보)
        float resolvedCellSize = ResolveCellSize();
        if (unityGrid != null)
        {
            unityGrid.cellSize = new Vector3(resolvedCellSize, resolvedCellSize, 1f);
        }
        
        // UI Text용 World Space Canvas 설정
        // if (showRoomTypeLabels)
        // {
        //     SetupWorldSpaceCanvas();
        // }
        
        // Tilemap 방식 사용 시 복도용 Tilemap 초기화
        if (useTilemapForCorridors)
        {
            SetupCorridorTilemap();
        }
        
        // 방 간격 자동 계산
        if (autoCalculateSpacing && normalRoomPrefab != null)
        {
            CalculateRoomSpacing();
        }
        
        // 1. 그리드 초기화
        dungeonGrid = new DungeonGrid(gridSize);
        corridors = new Dictionary<Vector2Int, GameObject>();
        eventRoomPositions = new List<Vector2Int>();
        
        // 2. 시작 방 생성
        startRoomPosition = Vector2Int.zero;
        Room startRoom = new Room(startRoomPosition, RoomType.Start);
        dungeonGrid.AddRoom(startRoomPosition, startRoom);
        
        Room currentRoom = startRoom;
        int count = 1;
        
        // 3. 방 생성 루프
        while (count < roomCount)
        {
            // 방향 선택
            Vector2Int direction = Direction.Random();
            Vector2Int nextPosition = currentRoom.gridPosition + direction;
            
            // next가 grid 안인가?
            if (!dungeonGrid.IsInGrid(nextPosition))
            {
                continue; // 다시 방향 선택
            }
            
            // next 위치가 비어있는가?
            if (dungeonGrid.IsEmpty(nextPosition))
            {
                // 새 방 생성
                Room newRoom = new Room(nextPosition, RoomType.Normal);
                dungeonGrid.AddRoom(nextPosition, newRoom);
                
                // 문 연결
                currentRoom.ConnectDoor(direction);
                newRoom.ConnectDoor(Direction.Opposite(direction));
                
                currentRoom = newRoom;
                count++;
            }
            else
            {
                // 통로만 업데이트 (이미 있는 방과 연결)
                Room existingRoom = dungeonGrid.GetRoom(nextPosition);
                if (existingRoom != null)
                {
                    currentRoom.ConnectDoor(direction);
                    existingRoom.ConnectDoor(Direction.Opposite(direction));
                    currentRoom = existingRoom;
                }
            }
        }
        
        // 4. 방 생성 완료 후 처리
        ProcessPostGeneration();
        // 4-1. 인접 방 사이의 문을 보정 (모든 인접 방은 연결되도록, 이거 나중에 뺄 수 있을 것 같은데 일단 2번 확인하도록함...)
        EnsureAdjacentDoorsConnected();
        
        // 5. 방 오브젝트 생성
        CreateRoomObjects();
        
        // 6. 복도 생성 (방과 방 사이를 연결... 일단 주석처리 해둠...)
        // CreateCorridors();
        
        // 7. DoorSpace/NoDoor 갱신
        RefreshAllDoorStates();
        
        // 8. 플레이어를 시작 방 중심으로 이동
        PlacePlayerObject();
        
        // 9. Exit 프리펩을 출구 방 중앙에 배치
        PlaceExitObject();
        
        Debug.Log($"던전 생성 완료: {dungeonGrid.GetRoomCount()}개 방");
    }
    
    /// <summary>
    /// 방 생성 완료 후 처리 (시작 방, 탈출 방, 이벤트 방 지정)
    /// </summary>
    private void ProcessPostGeneration()
    {
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
        Dictionary<Vector2Int, int> distances = CalculateDistancesFrom(startRoomPosition);
        
        // 출구 방 지정 (가장 먼 방)
        exitRoomPosition = SelectExitRoom(distances, startRoomPosition);
        var exitRoom = dungeonGrid.GetRoom(exitRoomPosition);
        if (exitRoom != null) exitRoom.roomType = RoomType.Exit;
        
        // 함정 방 지정 (남은 방 중 랜덤 1개)
        var remaining = allPositions.Where(p => p != startRoomPosition && p != exitRoomPosition).ToList();
        if (remaining.Count > 0)
        {
            var trapPos = remaining[Random.Range(0, remaining.Count)];
            var trapRoom = dungeonGrid.GetRoom(trapPos);
            if (trapRoom != null) trapRoom.roomType = RoomType.Trap;
            remaining.Remove(trapPos);
        }
        
        // 이벤트 방 지정 (남은 방 중 랜덤 2개)
        if (remaining.Count >= 2)
        {
            // 첫 번째 이벤트 방
            var eventPos1 = remaining[Random.Range(0, remaining.Count)];
            var eventRoom1 = dungeonGrid.GetRoom(eventPos1);
            if (eventRoom1 != null) eventRoom1.roomType = RoomType.Event;
            remaining.Remove(eventPos1);
            
            // 두 번째 이벤트 방
            if (remaining.Count > 0)
            {
                var eventPos2 = remaining[Random.Range(0, remaining.Count)];
                var eventRoom2 = dungeonGrid.GetRoom(eventPos2);
                if (eventRoom2 != null) eventRoom2.roomType = RoomType.Event;
            }
        }
        //else if (remaining.Count == 1)
        //{
        //    // 이벤트 방이 1개만 가능한 경우
        //    var eventPos = remaining[0];
        //    var eventRoom = dungeonGrid.GetRoom(eventPos);
        //    if (eventRoom != null) eventRoom.roomType = RoomType.Event;
        //}
    }
    
    /// <summary>
    /// 시작 방으로부터의 실제 경로 거리를 계산합니다. (BFS 사용)
    /// </summary>
    private Dictionary<Vector2Int, int> CalculateDistancesFrom(Vector2Int startPos)
    {
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        // 시작 방 초기화
        queue.Enqueue(startPos);
        visited.Add(startPos);
        distances[startPos] = 0;
        
        // BFS로 실제 경로 거리 계산
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            Room currentRoom = dungeonGrid.GetRoom(current);
            if (currentRoom == null) continue;
            
            // 연결된 모든 방향 확인
            foreach (var kvp in currentRoom.doors)
            {
                if (kvp.Value) // 문이 연결되어 있으면
                {
                    Vector2Int direction = kvp.Key;
                    Vector2Int nextPos = current + direction;
                    
                    if (!visited.Contains(nextPos) && dungeonGrid.GetRoom(nextPos) != null)
                    {
                        visited.Add(nextPos);
                        distances[nextPos] = distances[current] + 1;
                        queue.Enqueue(nextPos);
                    }
                }
            }
        }
        
        return distances;
    }
    
    /// <summary>
    /// 출구 방을 선택합니다. (시작 방에서 가장 먼 방)
    /// </summary>
    private Vector2Int SelectExitRoom(Dictionary<Vector2Int, int> distances, Vector2Int startPos)
    {
        // 시작 방 제외한 모든 방 중 가장 먼 방 선택
        Vector2Int farthest = startPos;
        int maxDistance = 0;
        
        foreach (var kvp in distances)
        {
            // 시작 방은 제외
            if (kvp.Key == startPos) continue;
            
            if (kvp.Value > maxDistance)
            {
                maxDistance = kvp.Value;
                farthest = kvp.Key;
            }
        }
        
        return farthest;
    }
    
    // 이벤트 방 로직은 현재 사용하지 않음 (1층 구성: 일반 7, 함정 1, 출구 1)

    // 이벤트 방 로직은 현재 사용하지 않음 (1층 구성: 일반 7, 함정 1, 출구 1)
    
    /// <summary>
    /// 방 오브젝트를 생성합니다.
    /// </summary>
    private void CreateRoomObjects()
    {
        Transform parent = gridParent != null ? gridParent : transform;
        float cellSize = ResolveCellSize();
        
        int spacingInCells = Mathf.RoundToInt(roomSpacing / cellSize);
        Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
        
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null) continue;
            
            GameObject prefab = GetRoomPrefab(room.roomType);
            if (prefab == null)
            {
                Debug.LogWarning($"{room.roomType} 프리펩이 설정되지 않았습니다.");
                continue;
            }
            
            // 공통 기준: Grid 셀 좌표를 사용하여 방 중심을 정렬
            Vector3Int roomCenterCell = new Vector3Int(position.x * spacingInCells, position.y * spacingInCells, 0);
            Vector3 worldPosition = unityGrid != null
                ? unityGrid.CellToWorld(roomCenterCell) + cellCenterOffset
                : new Vector3(position.x * roomSpacing, position.y * roomSpacing, 0f);
            
            // Grid 하위에 생성
            GameObject roomObj = Instantiate(prefab, worldPosition, Quaternion.identity, parent);
            room.roomObject = roomObj;
            Debug.Log($"[CreateRoomObjects] room:{roomObj.name} gridPos:{room.gridPosition} worldPos:{worldPosition}");
            
            // 방 스크립트에 문 정보 전달
            BaseRoom roomScript = roomObj.GetComponent<BaseRoom>();
            if (roomScript != null)
            {
                roomScript.InitializeRoom(room);
                roomScript.RefreshDoorStates(); // 생성 직후 문/NoDoor 상태 재정렬
            }
            else
            {
                Debug.LogWarning($"[CreateRoomObjects] BaseRoom 컴포넌트가 없습니다: {roomObj.name}");
            }
            
            // 방 타입 텍스트 표시
            if (showRoomTypeLabels)
            {
                CreateRoomTypeLabel(roomObj, room.roomType, worldPosition);
            }
        }
    }
    
    /// <summary>
    /// UI Text용 World Space Canvas를 설정합니다.
    /// </summary>
    private void SetupWorldSpaceCanvas()
    {
        if (worldSpaceCanvas != null) return;
        
        // Canvas 찾기 또는 생성
        worldSpaceCanvas = FindFirstObjectByType<Canvas>();
        if (worldSpaceCanvas != null && worldSpaceCanvas.renderMode == RenderMode.WorldSpace)
        {
            // 기존 World Space Canvas 사용
            return;
        }
        
        // 새 World Space Canvas 생성
        GameObject canvasObj = new GameObject("WorldSpaceCanvas");
        canvasObj.transform.SetParent(gridParent != null ? gridParent : transform);
        worldSpaceCanvas = canvasObj.AddComponent<Canvas>();
        
        // World Space 설정
        worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        worldSpaceCanvas.worldCamera = Camera.main;
        
        // Canvas Scaler 추가 (선택사항)
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        
        // GraphicRaycaster 추가 (선택사항)
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Canvas 크기 설정 (월드 단위)
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1000f, 1000f); // 충분히 큰 크기
        
        // World Space Canvas는 스케일을 조정하여 텍스트 크기 조절
        // UI Text는 픽셀 단위이므로, 월드 스케일을 작게 설정
        float cellSize = ResolveCellSize();
        float scaleFactor = cellSize / 100f; // 셀 크기에 맞춰 스케일 조정
        canvasRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        
        Debug.Log($"World Space Canvas 생성됨 (스케일: {scaleFactor:F4})");
    }
    
    /// <summary>
    /// 방 타입을 표시하는 텍스트 라벨을 생성합니다. (TextMesh 사용, 방 자식으로 붙임)
    /// </summary>
    private void CreateRoomTypeLabel(GameObject roomObj, RoomType roomType, Vector3 roomWorldPos)
    {
        // 방 중심 기준 크기 계산 (월드 단위)
        float roomSize = 0f;
        BaseRoom baseRoom = roomObj.GetComponent<BaseRoom>();
        if (baseRoom != null && baseRoom.RoomSize > 0)
        {
            float cellSize = ResolveCellSize();
            float tileSize = baseRoom.TileSize > 0f ? baseRoom.TileSize : cellSize;
            roomSize = baseRoom.RoomSize * tileSize; // RoomSize는 칸 수, tileSize를 곱해 월드 크기
        }
        else
        {
            // Renderer로부터 크기 추정
            Renderer renderer = roomObj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                roomSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y);
            }
        }

        float halfRoomSize = roomSize > 0 ? roomSize * 0.5f : 0f;

        // 라벨의 월드 위치 계산 (방 중심에서 위로 halfRoomSize + 오프셋)
        Vector3 worldLabelPos = roomWorldPos + new Vector3(0f, halfRoomSize + roomLabelOffsetY, 0f);

        // 방의 로컬 좌표계로 변환하여 자식으로 붙였을 때도 정확한 위치 유지
        Vector3 localLabelPos = roomObj.transform.InverseTransformPoint(worldLabelPos);

        // TextMesh 오브젝트 생성 (방 자식)
        GameObject labelObj = new GameObject($"RoomTypeLabel_{roomType}");
        labelObj.transform.SetParent(roomObj.transform, false);
        labelObj.transform.localPosition = localLabelPos;

        TextMesh textMesh = labelObj.AddComponent<TextMesh>();

        // 방 타입에 따라 텍스트와 색상 설정
        string roomTypeText = "";
        Color textColor = Color.white;

        switch (roomType)
        {
            case RoomType.Start:
                roomTypeText = "시작 방";
                textColor = Color.green;
                break;
            case RoomType.Exit:
                roomTypeText = "탈출 방";
                textColor = Color.cyan;
                break;
            case RoomType.Event:
                roomTypeText = "이벤트 방";
                textColor = Color.yellow;
                break;
            case RoomType.Trap:
                roomTypeText = "함정 방";
                textColor = Color.red;
                break;
            case RoomType.Normal:
            default:
                roomTypeText = "전투 방";
                textColor = Color.white;
                break;
        }

        // TextMesh 설정
        textMesh.text = roomTypeText;
        textMesh.color = textColor;
        textMesh.fontSize = 32;          // 글자 해상도
        textMesh.characterSize = 0.1f;   // 실제 월드 크기 (너무 크면 0.08, 작으면 0.12 정도로 조정)
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        // 폰트 설정: 인스펙터에서 지정된 폰트가 있으면 사용, 아니면 기본 폰트 유지
        if (roomLabelFont != null)
        {
            textMesh.font = roomLabelFont;
        }

        Debug.Log($"[방 타입 라벨] {roomTypeText}: 방 중심({roomWorldPos.x:F2}, {roomWorldPos.y:F2}), 방 크기: {roomSize:F2}, 로컬 라벨 위치: ({localLabelPos.x:F2}, {localLabelPos.y:F2})");
    }
    
    /// <summary>
    /// 복도를 생성합니다. (인접한 방이 있을 경우 방의 DoorSpace에서 다음 방의 DoorSpace까지 연결)
    /// </summary>
    private void CreateCorridors()
    {
        Transform parent = gridParent != null ? gridParent : transform;
        
        // 방 크기 가져오기
        float roomSize = GetRoomSize();
        float cellSize = ResolveCellSize();
        int spacingInCells = Mathf.RoundToInt(roomSpacing / cellSize);
        
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null || room.roomObject == null) continue;
            
            // 4방향 모두 확인
            Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            
            foreach (Vector2Int direction in directions)
            {
                // 문이 연결되어 있는지 확인
                if (!room.IsDoorConnected(direction)) continue;
                
                // 인접한 방 위치 계산
                Vector2Int nextPos = position + direction;
                
                // 인접한 방이 실제로 존재하는지 확인
                Room nextRoom = dungeonGrid.GetRoom(nextPos);
                if (nextRoom == null || nextRoom.roomObject == null) continue;
                
                // 복도 키 생성 (중복 방지 - 한 쌍의 방 사이에는 하나의 복도만 생성)
                Vector2Int minPos = new Vector2Int(Mathf.Min(position.x, nextPos.x), Mathf.Min(position.y, nextPos.y));
                Vector2Int maxPos = new Vector2Int(Mathf.Max(position.x, nextPos.x), Mathf.Max(position.y, nextPos.y));
                Vector2Int corridorKey = minPos * 1000 + maxPos; // 순서와 상관없이 동일 키
                
                if (corridors.ContainsKey(corridorKey)) continue;
                
                // DoorSpace의 Grid 셀 좌표 가져오기 (벽의 중앙 칸)
                Vector3Int doorStartCell = GetDoorSpaceCell(room, room.roomObject, direction, roomSize, spacingInCells);
                Vector3Int doorEndCell = GetDoorSpaceCell(nextRoom, nextRoom.roomObject, Direction.Opposite(direction), roomSize, spacingInCells);
                
                // 복도는 문 바로 밖 1칸부터 시작/끝나도록 오프셋
                Vector3Int corridorStartCell = doorStartCell + new Vector3Int(direction.x, direction.y, 0);
                Vector3Int corridorEndCell = doorEndCell - new Vector3Int(direction.x, direction.y, 0);
                
                // 디버그: 복도 생성 시작 (Grid 셀 좌표만)
                string dirName = direction == Direction.Up ? "위" : direction == Direction.Down ? "아래" : 
                                direction == Direction.Right ? "오른쪽" : "왼쪽";
                Debug.Log($"[복도 생성 시작] {room.roomObject.name} -> {nextRoom.roomObject.name} ({dirName} 방향)");
                Debug.Log($"[복도 문 위치] 시작 문 Grid 셀: ({doorStartCell.x}, {doorStartCell.y}), 끝 문 Grid 셀: ({doorEndCell.x}, {doorEndCell.y})");
                Debug.Log($"[복도 시작/끝 셀] 시작: ({corridorStartCell.x}, {corridorStartCell.y}), 끝: ({corridorEndCell.x}, {corridorEndCell.y})");
                
                // 복도 프리팹 선택 (가로/세로)
                GameObject corridorPrefabToUse = (direction == Direction.Up || direction == Direction.Down)
                    ? corridorPrefabVertical
                    : corridorPrefabHorizontal;
                
                // Tilemap 방식 또는 Sprite 프리팹 방식 선택
                if (useTilemapForCorridors && corridorFloorTile != null)
                {
                    // Tilemap 방식: 타일을 직접 Tilemap에 배치
                    CreateCorridorWithTilemap(corridorStartCell, corridorEndCell, corridorKey);
                }
                else if (corridorPrefabToUse != null)
                {
                    // Sprite 프리팹 방식: 1칸씩 프리팹 배치
                    CreateCorridorTiles(corridorStartCell, corridorEndCell, corridorPrefabToUse, cellSize, parent, corridorKey);
                }
                
                // DoorSpace 활성화 및 통과 가능하게 설정
                if (corridorPrefabToUse != null || (useTilemapForCorridors && corridorFloorTile != null))
                {
                    ActivateDoorSpacesForCorridor(room.roomObject, nextRoom.roomObject, direction);
                }
            }
        }
    }
    
    /// <summary>
    /// 복도를 1칸씩 타일로 배치합니다. (Grid 셀 좌표 사용, 문 셀부터 문 셀까지 직선)
    /// 2D Sprite 프리팹 사용: 피벗이 Center(0.5, 0.5)인 경우 셀 중심에 정확히 배치됨
    /// </summary>
    private void CreateCorridorTiles(Vector3Int startCell, Vector3Int endCell, 
        GameObject corridorPrefab, float cellSize, Transform parent, Vector2Int corridorKey)
    {
        if (unityGrid == null) return;
        
        // 셀 중심 위치 계산 (CellToWorld는 셀의 왼쪽 아래 모서리를 반환)
        // 2D Sprite 프리팹의 피벗이 Center(0.5, 0.5)이므로 셀 중심에 배치해야 함
        Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
        
        // 문 셀부터 문 셀까지 직선 경로
        Vector3Int corridorStartCell = startCell;
        Vector3Int corridorEndCell = endCell;
        
        Vector3Int delta = corridorEndCell - corridorStartCell;
        Vector3Int cellDirection = Vector3Int.zero;
        if (delta.x != 0) cellDirection = new Vector3Int(Mathf.Sign(delta.x) > 0 ? 1 : -1, 0, 0);
        else if (delta.y != 0) cellDirection = new Vector3Int(0, Mathf.Sign(delta.y) > 0 ? 1 : -1, 0);
        
        int cellDistance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        
        // 중복 방지를 위한 Set 사용
        HashSet<Vector3Int> placedCells = new HashSet<Vector3Int>();
        
        // 각 셀에 복도 타일 배치
        for (int i = 0; i <= cellDistance; i++)
        {
            Vector3Int currentCell = corridorStartCell + cellDirection * i;
            
            // 이미 배치된 셀은 건너뛰기 (중복 방지)
            if (placedCells.Contains(currentCell)) continue;
            
            // 셀 위치를 월드 좌표로 변환 (셀 중심)
            // CellToWorld는 셀의 왼쪽 아래 모서리를 반환하므로, 셀 중심으로 보정
            // 복도 셀 좌표는 이미 spacingInCells가 곱해진 값이므로, 그대로 CellToWorld 사용
            // 2D Sprite 프리팹의 피벗이 Center이므로 이 위치에 정확히 배치됨
            Vector3 tilePos = unityGrid.CellToWorld(currentCell) + cellCenterOffset;
            
            // 디버그: 복도 타일 위치 출력 (Grid 셀 좌표와 월드 좌표)
            Debug.Log($"[복도 타일] 인덱스: {i}/{cellDistance}, Grid 셀: ({currentCell.x}, {currentCell.y}), 월드: ({tilePos.x:F2}, {tilePos.y:F2})");
            
            // 복도 타일 생성 (2D Sprite 프리팹)
            GameObject corridorTile = Instantiate(corridorPrefab, tilePos, Quaternion.identity, parent);
            
            // 복도 타일을 플레이어가 통과 가능하도록 설정
            SetCorridorTilePassable(corridorTile);
            
            // 복도 타일 라벨 표시 (디버그용)
            if (showCorridorLabels)
            {
                CreateCorridorTileLabel(corridorTile, currentCell, tilePos, cellSize);
            }
            
            // 배치된 셀 기록
            placedCells.Add(currentCell);
            
            // 첫 번째 타일을 복도 키로 저장 (중복 체크용)
            if (i == 0)
            {
                corridors[corridorKey] = corridorTile;
            }
        }
    }
    
    /// <summary>
    /// 복도 타일에 라벨을 생성합니다. (디버그용)
    /// </summary>
    private void CreateCorridorTileLabel(GameObject corridorTile, Vector3Int cellPos, Vector3 tileWorldPos, float cellSize)
    {
        // 텍스트 오브젝트 생성
        GameObject labelObj = new GameObject($"CorridorLabel_{cellPos.x}_{cellPos.y}");
        labelObj.transform.SetParent(corridorTile.transform);
        
        // 복도 타일 중심 위에 배치
        // cellSize는 타일 한 칸의 크기이므로, 절반만큼 위로 올림
        float halfCellSize = cellSize * 0.5f;
        Vector3 labelPos = tileWorldPos + new Vector3(0f, halfCellSize + corridorLabelOffsetY, 0f);
        labelObj.transform.position = labelPos;
        
        // TextMesh 컴포넌트 추가
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        
        // 셀 좌표를 텍스트로 표시
        textMesh.text = $"({cellPos.x},{cellPos.y})";
        textMesh.color = Color.cyan;
        textMesh.fontSize = 10;
        textMesh.characterSize = 0.1f; // 작은 크기
        
        // 중요: TextMesh의 앵커를 중앙으로 설정
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    }
    
    /// <summary>
    /// 복도 연결 시 DoorSpace를 활성화하고 통과 가능하게 설정합니다.
    /// </summary>
    private void ActivateDoorSpacesForCorridor(GameObject roomObj, GameObject nextRoomObj, Vector2Int direction)
    {
        // 시작 방의 DoorSpace 활성화
        BaseRoom baseRoom = roomObj.GetComponent<BaseRoom>();
        if (baseRoom != null)
        {
            baseRoom.ActivateDoorSpace(direction);
        }
        
        // 다음 방의 DoorSpace 활성화
        BaseRoom nextBaseRoom = nextRoomObj.GetComponent<BaseRoom>();
        if (nextBaseRoom != null)
        {
            nextBaseRoom.ActivateDoorSpace(Direction.Opposite(direction));
        }
    }
    
    /// <summary>
    /// 복도 타일을 플레이어가 통과 가능하도록 설정합니다.
    /// </summary>
    private void SetCorridorTilePassable(GameObject corridorTile)
    {
        // 복도 타일의 모든 충돌체를 Trigger로 설정
        Collider2D[] colliders = corridorTile.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            // 바닥 타일은 통과 가능하게 (벽은 제외할 수도 있음)
            // 이름에 "Wall"이 포함되어 있지 않으면 통과 가능하게 설정
            if (!collider.name.Contains("Wall") && !collider.name.Contains("wall"))
            {
                collider.isTrigger = true;
            }
        }
    }
    
    /// <summary>
    /// 방의 크기를 가져옵니다.
    /// </summary>
    private float GetRoomSize()
    {
        if (normalRoomPrefab != null)
        {
            BaseRoom baseRoom = normalRoomPrefab.GetComponent<BaseRoom>();
            if (baseRoom != null)
            {
                float ts = baseRoom.TileSize > 0f ? baseRoom.TileSize : 1f;
                return baseRoom.RoomSize * ts; // RoomSize를 칸 수로 보고 tileSize를 곱해 월드 크기로 변환
            }
            
            // BaseRoom이 없으면 Renderer로 추정
            Renderer renderer = normalRoomPrefab.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                return Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y);
            }
        }
        
        return 4f; // 기본값
    }
    
    /// <summary>
    /// DoorSpace의 Grid 셀 좌표를 가져옵니다. (벽의 중앙 칸 기준)
    /// </summary>
    private Vector3Int GetDoorSpaceCell(Room roomData, GameObject roomObj, Vector2Int direction, float roomSize, int spacingInCells)
    {
        if (unityGrid == null) return Vector3Int.zero;
        
        float cellSize = 1f;
        BaseRoom baseRoom = roomObj.GetComponent<BaseRoom>();
        if (baseRoom != null && baseRoom.TileSize > 0f)
            cellSize = baseRoom.TileSize;
        else if (unityGrid != null && unityGrid.cellSize.x > 0f)
            cellSize = unityGrid.cellSize.x;
        
        // BaseRoom에서 실제 방 크기 가져오기 (우선순위: Inspector 설정 > 동적 계산)
        float actualRoomSize = 0f;
        if (baseRoom != null && baseRoom.RoomSize > 0)
        {
            float ts = baseRoom.TileSize > 0f ? baseRoom.TileSize : 1f;
            actualRoomSize = baseRoom.RoomSize * ts; // RoomSize는 칸 수, tileSize를 곱해 월드 크기
        }
        else if (roomSize > 0)
        {
            actualRoomSize = roomSize;
        }
        else
        {
            // Renderer/Collider로 추정 (도어 제외)
            Renderer[] renderers = roomObj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.name.Contains("Door")) continue;
                actualRoomSize = Mathf.Max(actualRoomSize, Mathf.Max(r.bounds.size.x, r.bounds.size.y));
            }
            if (Mathf.Approximately(actualRoomSize, 0f))
            {
                Collider2D[] colliders = roomObj.GetComponentsInChildren<Collider2D>();
                foreach (var c in colliders)
                {
                    if (c.name.Contains("Door")) continue;
                    actualRoomSize = Mathf.Max(actualRoomSize, Mathf.Max(c.bounds.size.x, c.bounds.size.y));
                }
            }
        }
        
        // 방의 Grid 좌표 기반으로 방 중심 셀 계산 (공통 기준)
        Vector3Int roomCenterCell = new Vector3Int(roomData.gridPosition.x * spacingInCells, roomData.gridPosition.y * spacingInCells, 0);
        Vector3 roomCenterWorld = unityGrid.CellToWorld(roomCenterCell) + new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
        
        // 방의 크기를 Grid 셀 단위로 계산 (반올림 후 최소 1)
        int roomSizeInCells = actualRoomSize > 0 ? Mathf.Max(1, Mathf.RoundToInt(actualRoomSize / cellSize)) : 0;
        
        // 벽의 중앙 칸 계산
        // 예: 15칸 벽이면 중앙은 7번째 칸 (0-based) = 8번째 칸 (1-based)
        // 예: 31칸 벽이면 중앙은 15번째 칸 (0-based) = 16번째 칸 (1-based)
        // halfRoomSize는 방 중심에서 벽까지의 거리
        int halfRoomSize = roomSizeInCells > 0 ? roomSizeInCells / 2 : 0;
        
        // 디버그: 방 중심 정보
        Debug.Log($"[문 위치 계산] 방: {roomObj.name}, 방 중심 Grid 셀: ({roomCenterCell.x}, {roomCenterCell.y}), 월드: ({roomCenterWorld.x:F2}, {roomCenterWorld.y:F2})");
        // 디버그: 방 크기 정보
        Debug.Log($"[문 위치 계산] 방 크기: {actualRoomSize} (월드), {roomSizeInCells}칸 (셀), halfRoomSize: {halfRoomSize}, BaseRoom.RoomSize: {(baseRoom != null ? baseRoom.RoomSize.ToString() : "null")}, cellSizeUsed: {cellSize}");
        
        // DoorSpace Transform 위치 대신, 방 중심과 halfRoomSize 기준으로 문 셀을 계산 (DoorSpace가 오프셋되어 있어도 일정)
        Vector3Int doorCell = roomCenterCell;
        if (direction == Direction.Up)
        {
            doorCell.x = roomCenterCell.x;
            doorCell.y = roomCenterCell.y + halfRoomSize;
        }
        else if (direction == Direction.Down)
        {
            doorCell.x = roomCenterCell.x;
            doorCell.y = roomCenterCell.y - halfRoomSize;
        }
        else if (direction == Direction.Right)
        {
            doorCell.x = roomCenterCell.x + halfRoomSize;
            doorCell.y = roomCenterCell.y;
        }
        else // Left
        {
            doorCell.x = roomCenterCell.x - halfRoomSize;
            doorCell.y = roomCenterCell.y;
        }
        
        // 디버그: 문 위치 출력 (Grid 셀 좌표만)
        string dirName = direction == Direction.Up ? "위" : direction == Direction.Down ? "아래" : 
                        direction == Direction.Right ? "오른쪽" : "왼쪽";
        Debug.Log($"[문 위치] 방: {roomObj.name}, 방향: {dirName}, Grid 셀: ({doorCell.x}, {doorCell.y}), 방 크기: {roomSizeInCells}칸");
        
        return doorCell;
    }
    
    /// <summary>
    /// 복도용 Tilemap을 설정합니다.
    /// </summary>
    private void SetupCorridorTilemap()
    {
        if (unityGrid == null || gridParent == null) return;
        
        // 기존 복도 Tilemap 찾기
        corridorTilemap = gridParent.GetComponentInChildren<Tilemap>();
        if (corridorTilemap != null && corridorTilemap.name.Contains("Corridor"))
        {
            // 기존 Tilemap 초기화
            corridorTilemap.ClearAllTiles();
            return;
        }
        
        // 새 복도 Tilemap 생성
        GameObject tilemapObj = new GameObject("CorridorTilemap");
        tilemapObj.transform.SetParent(gridParent);
        corridorTilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
        
        // Tilemap 설정
        corridorTilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f); // 셀 중심
        renderer.sortingOrder = 0; // 방과 동일한 레이어
        
        Debug.Log("복도용 Tilemap 생성됨");
    }
    
    /// <summary>
    /// Tilemap 방식으로 복도를 생성합니다.
    /// </summary>
    private void CreateCorridorWithTilemap(Vector3Int startCell, Vector3Int endCell, Vector2Int corridorKey)
    {
        if (corridorTilemap == null || corridorFloorTile == null) return;
        
        // 문 셀부터 문 셀까지 직선 경로
        Vector3Int corridorStartCell = startCell;
        Vector3Int corridorEndCell = endCell;
        
        Vector3Int delta = corridorEndCell - corridorStartCell;
        Vector3Int cellDirection = Vector3Int.zero;
        if (delta.x != 0) cellDirection = new Vector3Int(Mathf.Sign(delta.x) > 0 ? 1 : -1, 0, 0);
        else if (delta.y != 0) cellDirection = new Vector3Int(0, Mathf.Sign(delta.y) > 0 ? 1 : -1, 0);
        
        int cellDistance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        
        // 각 셀에 복도 바닥 타일 배치
        for (int i = 0; i <= cellDistance; i++)
        {
            Vector3Int currentCell = corridorStartCell + cellDirection * i;
            
            // 바닥 타일 배치
            corridorTilemap.SetTile(currentCell, corridorFloorTile);
            
            // 벽 타일이 있으면 양쪽에 배치 (가로 복도인 경우 위아래, 세로 복도인 경우 좌우)
            if (corridorWallTile != null)
            {
                if (delta.x != 0) // 가로 복도
                {
                    // 위쪽 벽
                    corridorTilemap.SetTile(new Vector3Int(currentCell.x, currentCell.y + 1, 0), corridorWallTile);
                    // 아래쪽 벽
                    corridorTilemap.SetTile(new Vector3Int(currentCell.x, currentCell.y - 1, 0), corridorWallTile);
                }
                else if (delta.y != 0) // 세로 복도
                {
                    // 오른쪽 벽
                    corridorTilemap.SetTile(new Vector3Int(currentCell.x + 1, currentCell.y, 0), corridorWallTile);
                    // 왼쪽 벽
                    corridorTilemap.SetTile(new Vector3Int(currentCell.x - 1, currentCell.y, 0), corridorWallTile);
                }
            }
            
            Debug.Log($"[Tilemap 복도] 셀: ({currentCell.x}, {currentCell.y})에 타일 배치");
        }
        
        // 복도 키 저장 (Tilemap GameObject 사용)
        if (!corridors.ContainsKey(corridorKey))
        {
            corridors[corridorKey] = corridorTilemap.gameObject;
        }
    }
    
    /// <summary>
    /// DoorSpace 오브젝트를 찾습니다.
    /// </summary>
    private Transform FindDoorSpace(Transform parent, Vector2Int direction)
    {
        string directionName = "";
        if (direction == Direction.Up) directionName = "Up";
        else if (direction == Direction.Down) directionName = "Down";
        else if (direction == Direction.Left) directionName = "Left";
        else if (direction == Direction.Right) directionName = "Right";
        
        // 직접 자식에서 찾기
        foreach (Transform child in parent)
        {
            if (child.name.Contains("DoorSpace") && 
                (child.name.Contains(directionName) || child.name.EndsWith("_" + directionName)))
            {
                return child;
            }
        }
        
        // 재귀적으로 찾기
        foreach (Transform child in parent)
        {
            Transform found = FindDoorSpaceRecursive(child, directionName);
            if (found != null)
                return found;
        }
        
        return null;
    }
    
    /// <summary>
    /// DoorSpace를 재귀적으로 찾습니다.
    /// </summary>
    private Transform FindDoorSpaceRecursive(Transform parent, string directionName)
    {
        if (parent.name.Contains("DoorSpace") && 
            (parent.name.Contains(directionName) || parent.name.EndsWith("_" + directionName)))
        {
            return parent;
        }
        
        foreach (Transform child in parent)
        {
            Transform found = FindDoorSpaceRecursive(child, directionName);
            if (found != null)
                return found;
        }
        
        return null;
    }
    
    /// <summary>
    /// 방 타입에 맞는 프리펩을 반환합니다.
    /// </summary>
    private GameObject GetRoomPrefab(RoomType type)
    {
        switch (type)
        {
            case RoomType.Start:
                return startRoomPrefab != null ? startRoomPrefab : normalRoomPrefab;
            case RoomType.Exit:
                return exitRoomPrefab != null ? exitRoomPrefab : normalRoomPrefab;
            case RoomType.Event:
                return eventRoomPrefab != null ? eventRoomPrefab : normalRoomPrefab;
            case RoomType.Trap:
                return eventRoomPrefab != null ? eventRoomPrefab : normalRoomPrefab;
            default:
                return normalRoomPrefab;
        }
    }
    
    /// <summary>
    /// Grid 오브젝트를 찾거나 생성합니다.
    /// </summary>
    private void SetupGridParent()
    {
        // 이미 설정되어 있으면 사용
        if (gridParent != null)
        {
            unityGrid = gridParent.GetComponent<Grid>();
            return;
        }
        
        // 씬에서 Grid 오브젝트 찾기
        Grid foundGrid = FindFirstObjectByType<Grid>();
        if (foundGrid != null)
        {
            gridParent = foundGrid.transform;
            unityGrid = foundGrid;
            Debug.Log($"Grid 오브젝트 발견: {gridParent.name}");
            return;
        }
        
        // Grid 오브젝트가 없으면 생성
        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(transform);
        gridParent = gridObj.transform;
        unityGrid = gridObj.AddComponent<Grid>();
        Debug.Log("Grid 오브젝트 생성됨");
    }
    
    /// <summary>
    /// 방 간격을 자동으로 계산합니다. (최소 5칸 이상 간격 보장)
    /// </summary>
    private void CalculateRoomSpacing()
    {
        if (normalRoomPrefab == null) return;
        
        // Grid의 셀 크기 가져오기
        float cellSize = ResolveCellSize();
        
        BaseRoom baseRoom = normalRoomPrefab.GetComponent<BaseRoom>();
        float roomSize = 0f;
        
        if (baseRoom != null)
        {
            float ts = baseRoom.TileSize > 0f ? baseRoom.TileSize : cellSize;
            roomSize = baseRoom.RoomSize * ts; // RoomSize는 칸 수
        }
        else
        {
            // BaseRoom이 없으면 프리펩의 크기로 추정
            Renderer renderer = normalRoomPrefab.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                roomSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y);
            }
        }
        
        if (roomSize > 0)
        {
            // corridorLength는 '칸 수'로 간주 → 월드 거리로 변환
            float corridorLengthWorld = corridorLength * cellSize;
            // 최소 간격 계산: roomSize + 복도(칸수*cell) + 최소 5칸 간격
            float minSpacing = roomSize + corridorLengthWorld + (minTileSpacing * cellSize);
            
            // roomSize + corridorLengthWorld와 비교해서 더 큰 값 사용
            float baseSpacing = roomSize + corridorLengthWorld;
            roomSpacing = Mathf.Max(minSpacing, baseSpacing);
            
            Debug.Log($"방 간격 자동 계산: roomSize({roomSize}) + corridorLength({corridorLength}칸→{corridorLengthWorld}) + 최소간격({minTileSpacing}칸 * {cellSize}) = {roomSpacing}");
        }
    }

    /// <summary>
    /// 셀 크기를 결정합니다. (기본값 0.32)
    /// </summary>
    private float ResolveCellSize()
    {
        // BaseRoom tileSize 우선
        BaseRoom prefabBaseRoom = normalRoomPrefab != null ? normalRoomPrefab.GetComponent<BaseRoom>() : null;
        if (prefabBaseRoom != null && prefabBaseRoom.TileSize > 0f)
            return prefabBaseRoom.TileSize;
        
        // Grid cellSize 사용
        if (unityGrid != null && unityGrid.cellSize.x > 0f)
            return unityGrid.cellSize.x;
        
        // 기본값
        return DefaultCellSize;
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
    /// Exit 프리펩을 출구 방 중심에 생성합니다.
    /// </summary>
    private void PlaceExitObject()
    {
        Room exitRoom = dungeonGrid.GetRoom(exitRoomPosition);
        if (exitRoom == null || exitRoom.roomObject == null)
        {
            Debug.LogWarning("출구 방 정보가 없어 Exit 프리펩을 생성하지 못했습니다.");
            return;
        }
        
        if (exitPrefab == null)
        {
            Debug.LogWarning("exitPrefab이 지정되지 않아 Exit를 생성할 수 없습니다.");
            return;
        }
        
        // 방의 실제 중심(렌더러/콜라이더 합산) 계산
        Vector3 center = GetRoomWorldCenter(exitRoom.roomObject);
        
        // 프리펩 생성
        Transform parent = gridParent != null ? gridParent : transform;
        Instantiate(exitPrefab, center, Quaternion.identity, parent);
        Debug.Log($"Exit 프리펩을 출구 방({exitRoomPosition}) 중심({center})에 생성");
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
            Debug.LogWarning("시작 방 정보가 없어 플레이어를 이동할 수 없습니다.");
            return;
        }
        
        Vector3 center = GetRoomWorldCenter(startRoom.roomObject);
        playerObject.transform.position = center;
        Debug.Log($"플레이어를 시작 방({startRoomPosition}) 중심({center})으로 이동");
    }
    
    /// <summary>
    /// 방 오브젝트의 렌더러/콜라이더 bounds 중심을 반환합니다. (없으면 transform.position)
    /// </summary>
    private Vector3 GetRoomWorldCenter(GameObject roomObj)
    {
        Bounds? bounds = null;
        foreach (var r in roomObj.GetComponentsInChildren<Renderer>())
        {
            if (r.name.Contains("Door")) continue;
            bounds = bounds.HasValue ? Encapsulate(bounds.Value, r.bounds) : r.bounds;
        }
        foreach (var c in roomObj.GetComponentsInChildren<Collider2D>())
        {
            if (c.name.Contains("Door")) continue;
            bounds = bounds.HasValue ? Encapsulate(bounds.Value, c.bounds) : c.bounds;
        }
        if (bounds.HasValue) return bounds.Value.center;
        return roomObj.transform.position;
    }

    private Bounds Encapsulate(Bounds a, Bounds b)
    {
        a.Encapsulate(b);
        return a;
    }

    /// <summary>
    /// 인접한 방들이 모두 서로 문이 연결되도록 보정합니다.
    /// </summary>
    private void EnsureAdjacentDoorsConnected()
    {
        Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        foreach (var position in dungeonGrid.GetAllPositions())
        {
            Room room = dungeonGrid.GetRoom(position);
            if (room == null) continue;
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = position + dir;
                Room neighbor = dungeonGrid.GetRoom(neighborPos);
                if (neighbor == null) continue;
                
                // 양쪽 모두 문 연결 상태 보정
                if (!room.IsDoorConnected(dir) || !neighbor.IsDoorConnected(Direction.Opposite(dir)))
                {
                    room.ConnectDoor(dir);
                    neighbor.ConnectDoor(Direction.Opposite(dir));
                }
            }
        }
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
                DestroyImmediate(gridParent.GetChild(i).gameObject);
            }
        }
        
        // transform 하위의 오브젝트도 제거 (기존 호환성)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            // Grid 오브젝트는 제거하지 않음
            if (child.GetComponent<Grid>() == null)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        dungeonGrid = null;
        corridors?.Clear();
        eventRoomPositions?.Clear();
    }
}

