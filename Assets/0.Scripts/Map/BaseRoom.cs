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

    protected Room roomData;
    
    // Door 오브젝트 캐시
    protected Dictionary<Vector2Int, GameObject> doors;
    
    /// <summary>
    /// 방을 초기화합니다.
    /// </summary>
    public virtual void InitializeRoom(Room room)
    {
        roomData = room;
        doors = new Dictionary<Vector2Int, GameObject>();
        
        // Door 오브젝트 찾기
        FindDoors();

        // 프리팹으로 방 크기 계산
        CalculateRoomSizeFromPrefab();

        // Door 활성화/비활성화
        UpdateDoors();
    }

    /// <summary>
    /// 외부에서 문 연결 상태 변화 후 Door를 갱신할 때 호출.
    /// </summary>
    public void RefreshDoorStates()
    {
        if (roomData == null)
        {
            Debug.LogWarning($"[{name}] roomData가 없어 Door 갱신 불가");
            return;
        }
        UpdateDoors();
    }

    /// <summary>
    /// Door 오브젝트를 찾습니다.
    /// Door 컨테이너의 자식으로 Door_North, Door_South, Door_West, Door_East를 찾습니다.
    /// </summary>
    protected void FindDoors()
    {
        // Door 컨테이너 찾기
        Transform doorContainer = null;
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Door") && !child.name.Contains("_"))
            {
                doorContainer = child;
                break;
            }
        }
        
        if (doorContainer == null)
        {
            Debug.LogWarning($"[{name}] Door 컨테이너를 찾을 수 없습니다.");
            return;
        }
        
        // Door 컨테이너의 자식에서 방향별 Door 찾기
        foreach (Transform child in doorContainer)
        {
            string childName = child.name;
            
            if (childName.Contains("North") || childName.EndsWith("_North"))
            {
                doors[Direction.Up] = child.gameObject;
            }
            else if (childName.Contains("South") || childName.EndsWith("_South"))
            {
                doors[Direction.Down] = child.gameObject;
            }
            else if (childName.Contains("West") || childName.EndsWith("_West"))
            {
                doors[Direction.Left] = child.gameObject;
            }
            else if (childName.Contains("East") || childName.EndsWith("_East"))
            {
                doors[Direction.Right] = child.gameObject;
            }
        }
    }
    
    /// <summary>
    /// Door를 문 연결 상태에 따라 활성화/비활성화합니다.
    /// 연결된 경우: active = false (문이 열려있음)
    /// 연결되지 않은 경우: active = true (벽처럼 막힘)
    /// </summary>
    protected void UpdateDoors()
    {
        if (roomData == null) return;
        
        // 4방향 모두 처리
        Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        foreach (Vector2Int direction in directions)
        {
            bool isConnected = roomData.IsDoorConnected(direction);
            
            if (doors.ContainsKey(direction))
            {
                GameObject door = doors[direction];
                if (door != null)
                {
                    // 연결된 경우: active = false (문이 열려있음)
                    // 연결되지 않은 경우: active = true (벽처럼 막힘)
                    door.SetActive(!isConnected);
                    
                    // 연결되지 않은 경우 벽처럼 막기 위해 Wall 태그 설정
                    if (!isConnected)
                    {
                        Collider2D[] colliders = door.GetComponentsInChildren<Collider2D>(true);
                        foreach (var col in colliders)
                        {
                            if (col != null)
                            {
                                col.enabled = true;
                                col.isTrigger = true; // 물리 충돌은 피하고 이동 로직으로만 막기
                                col.gameObject.tag = "Wall";
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 방향의 문을 "잠금" 상태로 전환합니다.
    /// 모든 Door를 active = true로 설정하여 벽처럼 막습니다.
    /// roomData의 연결 정보는 건드리지 않으므로, UnlockAllDoors로 원래 상태를 복원할 수 있습니다.
    /// </summary>
    public void LockAllDoors()
    {
        Vector2Int[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        foreach (var direction in directions)
        {
            if (doors != null && doors.ContainsKey(direction))
            {
                GameObject door = doors[direction];
                if (door != null)
                {
                    // 모든 문을 닫기 (벽처럼 막기)
                    door.SetActive(true);
                    
                    // PlayerMoveController.CanMoveTo에서 벽으로 인식되도록
                    // 콜라이더들이 "Wall" 태그를 가지게 설정
                    Collider2D[] colliders = door.GetComponentsInChildren<Collider2D>(true);
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
        UpdateDoors();
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
    
}

