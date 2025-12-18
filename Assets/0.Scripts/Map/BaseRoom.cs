using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 방의 기본 클래스
/// 문 연결 정보를 관리하고 방을 구성합니다.
/// </summary>
public class BaseRoom : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] [Tooltip("방의 크기 (단위: Unity unit)")]
    protected float roomSize = 4f;
    [SerializeField] [Tooltip("타일 하나의 크기 (단위: Unity unit)")]
    protected float tileSize = 1f;
    [SerializeField] [Tooltip("프리펩을 그대로 사용할지 여부. true면 방을 재생성하지 않고 프리펩 상태 그대로 사용합니다.")]
    protected bool usePrefabAsIs = false; // 프리펩을 그대로 사용 (재생성 안 함)
    
    // 외부 접근용
    public float RoomSize => roomSize;
    public float TileSize => tileSize;

    [Header("Trap Room Settings")]
    [SerializeField] [Tooltip("함정방 미로를 생성할 때 사용할 TrapRoomMazeGenerator 프리팹")]
    private GameObject trapRoomMazeGeneratorPrefab; // 함정방 미로 생성기 프리펩

    // TODO: 프리팹보단 싱글톤으로 쓰는 게 낫지 않을까 싶음
    
    protected Room roomData;
    
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
        roomData = room;
        doorSpaces = new Dictionary<Vector2Int, GameObject>();
        noDoors = new Dictionary<Vector2Int, GameObject>();
        
        // DoorSpace와 NoDoor 오브젝트 찾기
        FindDoorSpacesAndNoDoors();

        // 프리팹으로 방 크기 계산
        CalculateRoomSizeFromPrefab();

        // DoorSpace와 NoDoor 활성화/비활성화
        UpdateDoorSpaces();
        
        //CreateDoors();
        
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
    /// 모든 방향의 문을 "잠금" 상태로 전환합니다.
    /// - DoorSpace 시각/충돌을 비활성화하고
    /// - NoDoor 시각/충돌을 활성화합니다.
    /// roomData의 연결 정보는 건드리지 않으므로, UnlockAllDoors로 원래 상태를 복원할 수 있습니다.
    /// </summary>
    public void LockAllDoors()
    {
        Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        foreach (var direction in directions)
        {
            if (doorSpaces != null && doorSpaces.ContainsKey(direction))
            {
                GameObject ds = doorSpaces[direction];
                if (ds != null)
                {
                    SetComponentsActive(ds, false);
                    SetChildrenActive(ds, false);
                }
            }

            if (noDoors != null && noDoors.ContainsKey(direction))
            {
                GameObject nd = noDoors[direction];
                if (nd != null)
                {
                    // 닫힌 문(벽)을 활성화
                    SetComponentsActive(nd, true);
                    SetChildrenActive(nd, true);

                    // PlayerMoveController.CanMoveTo에서 벽으로 인식되도록
                    // NoDoor 쪽 콜라이더들이 "Wall" 태그를 가지게 설정
                    // (태그 기반 체크는 레이어와 무관하게 동작)
                    Collider2D[] colliders = nd.GetComponentsInChildren<Collider2D>(true);
                    foreach (var col in colliders)
                    {
                        if (col != null)
                        {
                            col.enabled = true;
                            // 물리적으로는 Trigger로 두고, 이동 로직(CanMoveTo)으로만 막아서
                            // 문이 닫힐 때 플레이어가 벽에 끼지 않도록 한다.
                            col.isTrigger = true;
                            col.gameObject.tag = "Wall";
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 문을 roomData의 연결 상태에 맞게 다시 갱신합니다.
    /// (LockAllDoors로 잠갔던 문을 다시 여는 용도로 사용)
    /// </summary>
    public void UnlockAllDoors()
    {
        UpdateDoorSpaces();
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

    /// <summary>
    /// 프리펩의 실제 크기를 계산합니다.
    /// </summary>
    protected virtual void CalculateRoomSizeFromPrefab()
    {
        // 이미 Inspector에서 roomSize를 지정했다면 자동 계산을 건너뜁니다.
        if (roomSize > 0f)
        {
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
            }
        }
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

        // 함정 방 컨트롤러 설정 (플레이어 진입/레버 연동)
        // 주의: TrapRoomController는 Room 프리팹에 미리 붙여 두는 것을 전제로 합니다.
        TrapRoomController controller = GetComponent<TrapRoomController>();
        if (controller == null)
        {
            Debug.LogWarning($"[{name}] TrapRoomController 컴포넌트가 없습니다. 함정 방 입장/레버 연동 기능이 비활성화됩니다.");
            return;
        }

        if (mazeGenerator != null)
        {
            controller.Initialize(this, mazeGenerator);
        }
    }
}

