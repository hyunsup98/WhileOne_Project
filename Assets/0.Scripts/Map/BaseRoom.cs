using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 방의 기본 클래스
/// 문 연결 정보를 관리하고 방을 구성합니다.
/// </summary>
public class BaseRoom : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] [Tooltip("방의 크기 (단위: Unity 단위)")]
    protected float roomSize = 4f;
    [SerializeField] [Tooltip("타일 하나의 크기 (단위: Unity 단위)")]
    protected float tileSize = 1f;
    [SerializeField] [Tooltip("프리펩을 그대로 사용할지 여부. true면 방을 재생성하지 않고 프리펩 상태 그대로 사용합니다.")]
    protected bool usePrefabAsIs = false; // 프리펩을 그대로 사용 (재생성 안 함)
    
    /// <summary>
    /// 방의 크기를 반환합니다. (외부 접근용)
    /// </summary>
    public float RoomSize => roomSize;
    
    /// <summary>
    /// 타일 크기를 반환합니다. (외부 접근용)
    /// </summary>
    public float TileSize => tileSize;
    
    [Header("Sprites")]
    [SerializeField] [Tooltip("바닥 타일에 사용할 스프라이트")]
    protected Sprite floorSprite;
    [SerializeField] [Tooltip("벽 타일에 사용할 스프라이트")]
    protected Sprite wallSprite;
    [SerializeField] [Tooltip("문 타일에 사용할 스프라이트")]
    protected Sprite doorSprite;
    
    [Header("Containers")]
    [SerializeField] [Tooltip("바닥 타일들이 생성될 부모 Transform 컨테이너")]
    protected Transform floorContainer;
    [SerializeField] [Tooltip("벽 타일들이 생성될 부모 Transform 컨테이너")]
    protected Transform wallContainer;
    [SerializeField] [Tooltip("문 오브젝트들이 생성될 부모 Transform 컨테이너")]
    protected Transform doorContainer;
    
    [Header("Trap Room Settings")]
    [SerializeField] [Tooltip("함정방 미로를 생성할 때 사용할 TrapRoomMazeGenerator 프리팹")]
    private GameObject trapRoomMazeGeneratorPrefab; // 함정방 미로 생성기 프리펩
    
    protected Room roomData;
    protected Dictionary<Vector2Int, GameObject> doorObjects;
    
    // DoorSpace와 NoDoor 오브젝트 캐시
    private Dictionary<Vector2Int, GameObject> doorSpaces;
    private Dictionary<Vector2Int, GameObject> noDoors;
    
    // 함정방 미로 생성기
    private TrapRoomMazeGenerator mazeGenerator;
    
    /// <summary>
    /// 방을 초기화합니다.
    /// </summary>
    public virtual void InitializeRoom(Room room)
    {
        Debug.Log($"[{name}] InitializeRoom 호출, roomData gridPos={room.gridPosition}");
        roomData = room;
        doorObjects = new Dictionary<Vector2Int, GameObject>();
        doorSpaces = new Dictionary<Vector2Int, GameObject>();
        noDoors = new Dictionary<Vector2Int, GameObject>();
        
        // DoorSpace와 NoDoor 오브젝트 찾기
        FindDoorSpacesAndNoDoors();
        
        // 프리펩이 이미 완성되어 있는지 확인
        bool isPrefabComplete = CheckIfPrefabIsComplete();
        
        if (!isPrefabComplete && !usePrefabAsIs)
        {
            // 프리펩이 완성되지 않았으면 생성
            SetupContainers();
            GenerateRoom();
        }
        else
        {
            // 프리펩이 완성되어 있으면 크기만 계산
            CalculateRoomSizeFromPrefab();
            // SetupContainers();
        }
        
        // DoorSpace와 NoDoor 활성화/비활성화
        UpdateDoorSpaces();
        
        CreateDoors();
        
        // 함정방인 경우 미로 생성
        if (room.roomType == RoomType.Trap)
        {
            GenerateTrapRoomMaze();
        }
    }

    /// <summary>
    /// 외부에서 문 연결 상태 변화 후 DoorSpace/NoDoor를 갱신할 때 호출.
    /// </summary>
    public void RefreshDoorStates()
    {
        if (roomData == null)
        {
            Debug.LogWarning($"[{name}] roomData가 없어 DoorSpace/NoDoor 갱신 불가");
            return;
        }
        Debug.Log($"[{name}] DoorSpace/NoDoor 갱신 시작");
        UpdateDoorSpaces();
    }
    
    /// <summary>
    /// DoorSpace와 NoDoor 오브젝트를 찾습니다.
    /// </summary>
    private void FindDoorSpacesAndNoDoors()
    {
        // 모든 자식 오브젝트에서 DoorSpace와 NoDoor 찾기
        foreach (Transform child in transform)
        {
            string childName = child.name;
            
            // DoorSpace 찾기
            if (childName.Contains("DoorSpace"))
            {
                if (childName.Contains("Up") || childName.EndsWith("_Up"))
                {
                    doorSpaces[Direction.Up] = child.gameObject;
                }
                else if (childName.Contains("Down") || childName.EndsWith("_Down"))
                {
                    doorSpaces[Direction.Down] = child.gameObject;
                }
                else if (childName.Contains("Left") || childName.EndsWith("_Left"))
                {
                    doorSpaces[Direction.Left] = child.gameObject;
                }
                else if (childName.Contains("Right") || childName.EndsWith("_Right"))
                {
                    doorSpaces[Direction.Right] = child.gameObject;
                }
            }
            
            // NoDoor/NoneDoor 찾기 (둘 다 지원)
            if (childName.Contains("NoDoor") || childName.Contains("NoneDoor"))
            {
                if (childName.Contains("Up") || childName.EndsWith("_Up"))
                {
                    noDoors[Direction.Up] = child.gameObject;
                }
                else if (childName.Contains("Down") || childName.EndsWith("_Down"))
                {
                    noDoors[Direction.Down] = child.gameObject;
                }
                else if (childName.Contains("Left") || childName.EndsWith("_Left"))
                {
                    noDoors[Direction.Left] = child.gameObject;
                }
                else if (childName.Contains("Right") || childName.EndsWith("_Right"))
                {
                    noDoors[Direction.Right] = child.gameObject;
                }
            }
        }
        
        // 재귀적으로 모든 하위 오브젝트 검색
        SearchInChildren(transform);
    }
    
    /// <summary>
    /// 자식 오브젝트를 재귀적으로 검색합니다.
    /// </summary>
    private void SearchInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            string childName = child.name;
            
            // DoorSpace 찾기
            if (childName.Contains("DoorSpace"))
            {
                if (childName.Contains("Up") || childName.EndsWith("_Up"))
                {
                    if (!doorSpaces.ContainsKey(Direction.Up))
                        doorSpaces[Direction.Up] = child.gameObject;
                }
                else if (childName.Contains("Down") || childName.EndsWith("_Down"))
                {
                    if (!doorSpaces.ContainsKey(Direction.Down))
                        doorSpaces[Direction.Down] = child.gameObject;
                }
                else if (childName.Contains("Left") || childName.EndsWith("_Left"))
                {
                    if (!doorSpaces.ContainsKey(Direction.Left))
                        doorSpaces[Direction.Left] = child.gameObject;
                }
                else if (childName.Contains("Right") || childName.EndsWith("_Right"))
                {
                    if (!doorSpaces.ContainsKey(Direction.Right))
                        doorSpaces[Direction.Right] = child.gameObject;
                }
            }
            
            // NoDoor 찾기
            if (childName.Contains("NoDoor") || childName.Contains("NoneDoor"))
            {
                if (childName.Contains("Up") || childName.EndsWith("_Up"))
                {
                    if (!noDoors.ContainsKey(Direction.Up))
                        noDoors[Direction.Up] = child.gameObject;
                }
                else if (childName.Contains("Down") || childName.EndsWith("_Down"))
                {
                    if (!noDoors.ContainsKey(Direction.Down))
                        noDoors[Direction.Down] = child.gameObject;
                }
                else if (childName.Contains("Left") || childName.EndsWith("_Left"))
                {
                    if (!noDoors.ContainsKey(Direction.Left))
                        noDoors[Direction.Left] = child.gameObject;
                }
                else if (childName.Contains("Right") || childName.EndsWith("_Right"))
                {
                    if (!noDoors.ContainsKey(Direction.Right))
                        noDoors[Direction.Right] = child.gameObject;
                }
                else
                {
                    // 방향명이 없는 NoDoor 컨테이너라면 하위에서 방향별 자식을 찾아 매핑
                    foreach (Transform sub in child)
                    {
                        string subName = sub.name;
                        if ((subName.Contains("Up") || subName.EndsWith("_Up")) && !noDoors.ContainsKey(Direction.Up))
                            noDoors[Direction.Up] = sub.gameObject;
                        else if ((subName.Contains("Down") || subName.EndsWith("_Down")) && !noDoors.ContainsKey(Direction.Down))
                            noDoors[Direction.Down] = sub.gameObject;
                        else if ((subName.Contains("Left") || subName.EndsWith("_Left")) && !noDoors.ContainsKey(Direction.Left))
                            noDoors[Direction.Left] = sub.gameObject;
                        else if ((subName.Contains("Right") || subName.EndsWith("_Right")) && !noDoors.ContainsKey(Direction.Right))
                            noDoors[Direction.Right] = sub.gameObject;
                    }
                }
            }
            
            // 재귀 검색
            if (child.childCount > 0)
            {
                SearchInChildren(child);
            }
        }
    }
    
    /// <summary>
    /// DoorSpace와 NoDoor를 문 연결 상태에 따라 활성화/비활성화합니다.
    /// </summary>
    private void UpdateDoorSpaces()
    {
        if (roomData == null) return;
        
        // 4방향 모두 처리
        Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        foreach (Vector2Int direction in directions)
        {
            bool isConnected = roomData.IsDoorConnected(direction);
            
            // DoorSpace: 부모는 항상 활성, 연결되면 ON, 끊기면 OFF
            bool hasDoorSpace = doorSpaces.ContainsKey(direction);
            if (doorSpaces.ContainsKey(direction))
            {
                GameObject ds = doorSpaces[direction];
                if (!ds.activeSelf) ds.SetActive(true);
                SetComponentsActive(ds, isConnected);
                SetChildrenActive(ds, isConnected);
                
                if (isConnected)
                {
                    SetDoorSpacePassable(ds);
                }
            }
            
            // NoDoor: 부모는 항상 활성, DoorSpace와 반대로
            bool hasNoDoor = noDoors.ContainsKey(direction);
            if (noDoors.ContainsKey(direction))
            {
                GameObject nd = noDoors[direction];
                if (!nd.activeSelf) nd.SetActive(true);
                bool noDoorOn = !isConnected;
                SetComponentsActive(nd, noDoorOn);
                SetChildrenActive(nd, noDoorOn);
            }
            else
            {
                Debug.LogWarning($"[{name}] NoDoor가 없어 방향 {direction} 비활성 처리 불가");
            }

            // 로그: 방향별 DoorSpace/NoDoor 상태
            string dirName = DirectionToString(direction);
            string dsState = hasDoorSpace ? (isConnected ? "ON" : "OFF") : "없음";
            string ndState = hasNoDoor ? (!isConnected ? "ON" : "OFF") : "없음";
            Debug.Log($"[{name}] 방향:{dirName} DoorSpace:{dsState} / NoDoor:{ndState}");
        }
    }
    
    /// <summary>
    /// DoorSpace를 플레이어가 통과 가능하도록 설정합니다.
    /// </summary>
    private void SetDoorSpacePassable(GameObject doorSpace)
    {
        // DoorSpace와 그 자식들의 충돌체 확인
        Collider2D[] colliders = doorSpace.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            // 충돌체를 Trigger로 설정하여 통과 가능하게
            collider.isTrigger = true;
        }
    }
    
    /// <summary>
    /// 특정 방향의 DoorSpace를 활성화합니다. (외부에서 호출 가능)
    /// </summary>
    public void ActivateDoorSpace(Vector2Int direction)
    {
        if (doorSpaces.ContainsKey(direction))
        {
            GameObject ds = doorSpaces[direction];
            if (!ds.activeSelf) ds.SetActive(true);
            SetComponentsActive(ds, true);
            SetChildrenActive(ds, true);
            SetDoorSpacePassable(ds);
        }
        
        if (noDoors.ContainsKey(direction))
        {
            GameObject nd = noDoors[direction];
            if (!nd.activeSelf) nd.SetActive(true);
            SetComponentsActive(nd, false);
            SetChildrenActive(nd, false);
        }
    }

    /// <summary>
    /// 부모 활성 상태는 유지하고 자식들만 활성/비활성 토글합니다.
    /// </summary>
    private void SetChildrenActive(GameObject parentObj, bool active)
    {
        if (parentObj == null) return;
        foreach (Transform child in parentObj.transform)
        {
            child.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 부모 활성 상태는 유지하고, 부모/자식의 Renderer, Collider2D, TilemapRenderer, SpriteRenderer를 on/off합니다.
    /// </summary>
    private void SetComponentsActive(GameObject parentObj, bool active)
    {
        if (parentObj == null) return;
        
        // 부모 포함 Renderer/Collider on/off
        foreach (Renderer r in parentObj.GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = active;
        }
        foreach (Collider2D c in parentObj.GetComponentsInChildren<Collider2D>(true))
        {
            c.enabled = active;
        }
    }

    private string DirectionToString(Vector2Int dir)
    {
        if (dir == Direction.Up) return "Up";
        if (dir == Direction.Down) return "Down";
        if (dir == Direction.Left) return "Left";
        if (dir == Direction.Right) return "Right";
        return dir.ToString();
    }
    
    /// <summary>
    /// 프리펩이 이미 완성되어 있는지 확인합니다.
    /// </summary>
    protected virtual bool CheckIfPrefabIsComplete()
    {
        // floorContainer에 이미 자식이 있으면 완성된 것으로 간주
        if (floorContainer != null && floorContainer.childCount > 0)
        {
            return true;
        }
        
        // 또는 직접 자식 중에 Floor나 Wall이 있으면 완성된 것으로 간주
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Floor") || child.name.Contains("Wall") || 
                child.name.Contains("floor") || child.name.Contains("wall"))
            {
                return true;
            }
        }
        
        return usePrefabAsIs;
    }
    
    /// <summary>
    /// 프리펩의 실제 크기를 계산합니다.
    /// </summary>
    protected virtual void CalculateRoomSizeFromPrefab()
    {
        // 이미 Inspector에서 roomSize를 지정했다면 자동 계산을 건너뜁니다.
        if (roomSize > 0f)
        {
            Debug.Log($"[{gameObject.name}] roomSize를 수동 값({roomSize})으로 사용합니다.");
            return;
        }
        
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;
        
        // 모든 Renderer의 bounds를 합쳐서 방 크기 계산
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // 문은 제외 (문은 나중에 추가되므로)
            if (renderer.name.Contains("Door") || renderer.name.Contains("door"))
                continue;
                
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        
        if (hasBounds)
        {
            // 로컬 공간으로 변환 (부모의 스케일 고려)
            Vector3 localSize = bounds.size;
            if (transform.parent != null)
            {
                localSize.x /= transform.parent.lossyScale.x;
                localSize.y /= transform.parent.lossyScale.y;
            }
            roomSize = Mathf.Max(localSize.x, localSize.y);
            
            Debug.Log($"[{gameObject.name}] 프리펩 크기 감지: {roomSize} (bounds: {bounds.size})");
        }
        else
        {
            // bounds를 찾을 수 없으면 Collider로 시도
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            if (colliders.Length > 0)
            {
                Bounds colliderBounds = colliders[0].bounds;
                foreach (Collider2D col in colliders)
                {
                    if (col.name.Contains("Door") || col.name.Contains("door"))
                        continue;
                    colliderBounds.Encapsulate(col.bounds);
                }
                
                Vector3 localSize = colliderBounds.size;
                if (transform.parent != null)
                {
                    localSize.x /= transform.parent.lossyScale.x;
                    localSize.y /= transform.parent.lossyScale.y;
                }
                roomSize = Mathf.Max(localSize.x, localSize.y);
                
                Debug.Log($"[{gameObject.name}] 프리펩 크기 감지 (Collider): {roomSize}");
            }
        }
    }
    
    /// <summary>
    /// 컨테이너를 설정합니다.
    /// </summary>
    protected virtual void SetupContainers()
    {
        if (floorContainer == null)
        {
            GameObject obj = new GameObject("FloorContainer");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            floorContainer = obj.transform;
        }
        
        if (wallContainer == null)
        {
            GameObject obj = new GameObject("WallContainer");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            wallContainer = obj.transform;
        }
        
        if (doorContainer == null)
        {
            GameObject obj = new GameObject("DoorContainer");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            doorContainer = obj.transform;
        }
    }
    
    /// <summary>
    /// 방을 생성합니다.
    /// </summary>
    protected virtual void GenerateRoom()
    {
        int tilesX = Mathf.RoundToInt(roomSize / tileSize);
        int tilesY = Mathf.RoundToInt(roomSize / tileSize);
        int halfTilesX = tilesX / 2;
        int halfTilesY = tilesY / 2;
        
        // 바닥 생성
        if (floorSprite != null)
        {
            for (int x = -halfTilesX; x < halfTilesX; x++)
            {
                for (int y = -halfTilesY; y < halfTilesY; y++)
                {
                    Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
                    CreateSpriteObject("Floor", position, floorSprite, floorContainer, 0);
                }
            }
        }
        
        // 벽 생성 (문 위치는 나중에 처리)
        if (wallSprite != null)
        {
            CreateWalls(halfTilesX, halfTilesY);
        }
    }
    
    /// <summary>
    /// 벽을 생성합니다.
    /// </summary>
    protected virtual void CreateWalls(int halfTilesX, int halfTilesY)
    {
        // 위쪽 벽
        for (int x = -halfTilesX; x < halfTilesX; x++)
        {
            if (!IsDoorPosition(x * tileSize, (halfTilesY - 1) * tileSize))
            {
                Vector3 position = new Vector3(x * tileSize, (halfTilesY - 1) * tileSize, 0);
                CreateWallObject(position);
            }
        }
        
        // 아래쪽 벽
        for (int x = -halfTilesX; x < halfTilesX; x++)
        {
            if (!IsDoorPosition(x * tileSize, -halfTilesY * tileSize))
            {
                Vector3 position = new Vector3(x * tileSize, -halfTilesY * tileSize, 0);
                CreateWallObject(position);
            }
        }
        
        // 왼쪽 벽
        for (int y = -halfTilesY; y < halfTilesY; y++)
        {
            if (!IsDoorPosition(-halfTilesX * tileSize, y * tileSize))
            {
                Vector3 position = new Vector3(-halfTilesX * tileSize, y * tileSize, 0);
                CreateWallObject(position);
            }
        }
        
        // 오른쪽 벽
        for (int y = -halfTilesY; y < halfTilesY; y++)
        {
            if (!IsDoorPosition((halfTilesX - 1) * tileSize, y * tileSize))
            {
                Vector3 position = new Vector3((halfTilesX - 1) * tileSize, y * tileSize, 0);
                CreateWallObject(position);
            }
        }
    }
    
    /// <summary>
    /// 문을 생성합니다.
    /// </summary>
    protected virtual void CreateDoors()
    {
        if (roomData == null || doorSprite == null) return;
        
        float doorY = (roomSize / 2f) - (tileSize * 0.5f);
        float doorX = (roomSize / 2f) - (tileSize * 0.5f);
        
        // 위쪽 문
        if (roomData.IsDoorConnected(Direction.Up))
        {
            CreateDoorObject(new Vector3(0, doorY, 0), Direction.Up);
        }
        
        // 아래쪽 문
        if (roomData.IsDoorConnected(Direction.Down))
        {
            CreateDoorObject(new Vector3(0, -doorY, 0), Direction.Down);
        }
        
        // 왼쪽 문
        if (roomData.IsDoorConnected(Direction.Left))
        {
            CreateDoorObject(new Vector3(-doorX, 0, 0), Direction.Left);
        }
        
        // 오른쪽 문
        if (roomData.IsDoorConnected(Direction.Right))
        {
            CreateDoorObject(new Vector3(doorX, 0, 0), Direction.Right);
        }
    }
    
    /// <summary>
    /// 문 오브젝트를 생성합니다.
    /// </summary>
    protected virtual void CreateDoorObject(Vector3 position, Vector2Int direction)
    {
        GameObject doorObj = CreateSpriteObject("Door", position, doorSprite, doorContainer, 2);
        doorObjects[direction] = doorObj;
        
        // 문에 충돌체 추가 (선택사항 - 문을 통과 가능하게 하려면 제거)
        // BoxCollider2D collider = doorObj.AddComponent<BoxCollider2D>();
        // collider.isTrigger = true; // 통과 가능하게
    }
    
    /// <summary>
    /// 벽 오브젝트를 생성합니다.
    /// </summary>
    protected virtual void CreateWallObject(Vector3 position)
    {
        GameObject wallObj = CreateSpriteObject("Wall", position, wallSprite, wallContainer, 1);
        
        // 벽에 충돌체 추가
        BoxCollider2D collider = wallObj.AddComponent<BoxCollider2D>();
        collider.size = wallSprite != null ? wallSprite.bounds.size : Vector2.one;
    }
    
    /// <summary>
    /// 스프라이트 오브젝트를 생성합니다.
    /// </summary>
    protected GameObject CreateSpriteObject(string name, Vector3 position, Sprite sprite, Transform parent, int sortingOrder)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = position;
        
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        
        return obj;
    }
    
    /// <summary>
    /// 해당 위치가 문 위치인지 확인합니다.
    /// </summary>
    protected virtual bool IsDoorPosition(float x, float y)
    {
        if (roomData == null) return false;
        
        float threshold = tileSize * 0.5f;
        float doorY = (roomSize / 2f) - (tileSize * 0.5f);
        float doorX = (roomSize / 2f) - (tileSize * 0.5f);
        
        // 위/아래 문 위치
        if (roomData.IsDoorConnected(Direction.Up) && Mathf.Abs(y - doorY) < threshold && Mathf.Abs(x) < threshold)
            return true;
        if (roomData.IsDoorConnected(Direction.Down) && Mathf.Abs(y + doorY) < threshold && Mathf.Abs(x) < threshold)
            return true;
        
        // 왼/오른 문 위치
        if (roomData.IsDoorConnected(Direction.Left) && Mathf.Abs(x + doorX) < threshold && Mathf.Abs(y) < threshold)
            return true;
        if (roomData.IsDoorConnected(Direction.Right) && Mathf.Abs(x - doorX) < threshold && Mathf.Abs(y) < threshold)
            return true;
        
        return false;
    }
    
    /// <summary>
    /// 함정방 미로를 생성합니다.
    /// </summary>
    private void GenerateTrapRoomMaze()
    {
        GameObject prefabToUse = trapRoomMazeGeneratorPrefab;
        
        // 프리팹이 설정되지 않았으면 자동으로 찾기
        if (prefabToUse == null)
        {
            Debug.LogWarning($"[{name}] 함정방 미로 생성기 프리펩이 설정되지 않았습니다. 자동으로 찾는 중...");
            
            // Resources 폴더에서 찾기
            prefabToUse = Resources.Load<GameObject>("TrapRoomMazeGenerator");
            
            // Resources에서 못 찾으면 씬에서 찾기
            if (prefabToUse == null)
            {
                TrapRoomMazeGenerator foundGenerator = FindFirstObjectByType<TrapRoomMazeGenerator>();
                if (foundGenerator != null)
                {
                    prefabToUse = foundGenerator.gameObject;
                    Debug.Log($"[{name}] 씬에서 TrapRoomMazeGenerator를 찾았습니다: {foundGenerator.name}");
                }
            }
            
            if (prefabToUse == null)
            {
                Debug.LogError($"[{name}] 함정방 미로 생성기 프리펩을 찾을 수 없습니다. " +
                    "다음 중 하나를 수행하세요:\n" +
                    "1. BaseRoom 프리팹의 'Trap Room Maze Generator Prefab' 필드에 TrapRoomMazeGenerator 프리팹을 할당\n" +
                    "2. Resources 폴더에 'TrapRoomMazeGenerator' 이름의 프리팹 생성\n" +
                    "3. 씬에 TrapRoomMazeGenerator 컴포넌트가 있는 GameObject 배치");
                return;
            }
        }
        
        // 미로 생성기 인스턴스 생성
        GameObject generatorObj = Instantiate(prefabToUse, transform);
        mazeGenerator = generatorObj.GetComponent<TrapRoomMazeGenerator>();
        
        if (mazeGenerator == null)
        {
            Debug.LogError($"[{name}] TrapRoomMazeGenerator 컴포넌트가 없습니다. 프리팹에 TrapRoomMazeGenerator 컴포넌트를 추가하세요.");
            Destroy(generatorObj);
            return;
        }
        
        // 입구 방향 찾기 (연결된 모든 문 방향 수집)
        Vector2Int primaryEntryDirection = Direction.Up;
        Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        List<Vector2Int> connectedDirections = new List<Vector2Int>();
        
        foreach (var dir in directions)
        {
            if (roomData.IsDoorConnected(dir))
            {
                connectedDirections.Add(dir);
            }
        }
        
        // 연결된 문이 하나도 없으면 기본값 사용
        if (connectedDirections.Count == 0)
        {
            connectedDirections.Add(Direction.Up);
        }
        
        // 메인 입구 방향은 첫 번째 연결된 방향으로 설정
        primaryEntryDirection = connectedDirections[0];
        
        // 미로 생성 (여러 입출구 지원)
        mazeGenerator.GenerateMaze(roomSize, tileSize, primaryEntryDirection, connectedDirections, transform);
        
        Debug.Log($"[{name}] 함정방 미로 생성 완료");
    }
}

