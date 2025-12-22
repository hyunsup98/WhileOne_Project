using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 함정 방 프리팹
/// BaseRoom을 상속받아 함정방 미로 생성 기능을 추가합니다.
/// </summary>
public class TrapRoom : BaseRoom
{
    [Header("Trap Room Settings")]
    [SerializeField] [Tooltip("함정방 미로를 생성할 때 사용할 TrapRoomMazeGenerator 프리팹")]
    private GameObject trapRoomMazeGeneratorPrefab; // 함정방 미로 생성기 프리펩
    
    // 함정방 미로 생성기
    private TrapRoomMazeGenerator mazeGenerator;
    
    /// <summary>
    /// 방을 초기화합니다.
    /// </summary>
    public override void InitializeRoom(Room room)
    {
        base.InitializeRoom(room);
        
        // 함정방 미로 생성
        GenerateTrapRoomMaze();
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
                    "1. TrapRoom 프리팹의 'Trap Room Maze Generator Prefab' 필드에 TrapRoomMazeGenerator 프리팹을 할당\n" +
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

