using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 함정방 미로 생성기
/// 기획서 요구사항에 따라 미로, 함정, 레버를 배치합니다.
/// </summary>
public class TrapRoomMazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] [Tooltip("미로의 가로 크기 (타일 수). 홀수여야 합니다.")]
    private int mazeWidth = 15; // 미로 가로 크기 (타일 수)
    [SerializeField] [Tooltip("미로의 세로 크기 (타일 수). 홀수여야 합니다.")]
    private int mazeHeight = 15; // 미로 세로 크기 (타일 수)
    
    [Header("Trap Settings")]
    [SerializeField] [Tooltip("함정 ON 상태에 사용할 프리팹 (1x1 크기)")]
    private GameObject trapPrefab; // 함정 ON 프리펩 (1x1)
    [SerializeField] [Tooltip("함정 OFF 상태에 사용할 프리펩 (선택, 비어 있으면 ON 프리펩의 활성/비활성으로 대체)")]
    private GameObject trapOffPrefab; // 함정 OFF 프리펩
    //[SerializeField] [Tooltip("함정이 공격하는 간격 (초 단위)")]
    //private float trapAttackInterval = 2f; // 함정 공격 간격 (초)
    [SerializeField] [Tooltip("미로 내 함정의 최대 비율 (0.0 ~ 1.0). 예: 0.3 = 30%")]
    private float maxTrapRatio = 0.3f; // 함정 최대 비율 (30%)
    [SerializeField] [Tooltip("입구/출구 주변에 함정을 배치하지 않는 안전 구역의 반경 (칸 단위)")]
    private int safeZoneRadius = 2; // 입구/출구 주변 안전 구역 (칸)
    
    [Header("Lever Settings")]
    [SerializeField] [Tooltip("레버로 사용할 프리팹")]
    private GameObject leverPrefab; // 레버 프리펩
    [SerializeField] [Tooltip("보물상자 프리팹 (레버 대신 일정 확률로 생성됨)")]
    private GameObject treasureChestPrefab; // 보물상자 프리펩 (10% 확률)
    [SerializeField] [Tooltip("보물상자가 레버 대신 등장할 확률 (0.0 ~ 1.0). 예: 0.1 = 10%")]
    private float treasureChance = 0.1f; // 보물상자 등장 확률 (10%)
    
    [Header("Maze Prefabs")]
    [SerializeField] [Tooltip("미로의 벽 타일로 사용할 프리팹 (통로를 만들기 위해 사용)")]
    private GameObject wallTilePrefab; // 벽 타일 프리펩 (통로를 만들기 위해 사용)
    
    // 미로 그리드 (true = 통로, false = 벽)
    private bool[,] mazeGrid;
    
    // 함정 위치
    private List<Vector2Int> trapPositions;
    // 생성된 함정 오브젝트 (ON/OFF 전환용)
    private List<GameObject> trapObjects = new List<GameObject>();
    
    // 레버 위치
    private Vector2Int leverPosition;
    
    // 메인 입구 위치
    private Vector2Int entryPosition;
    
    // 여러 입출구가 있을 때, 모든 입구 셀 위치를 저장
    private List<Vector2Int> entryPositions = new List<Vector2Int>();
    
    // 방 크기 (월드 단위)
    private float roomSize;
    private float cellSize;
    
    /// <summary>
    /// 함정방 미로를 생성합니다. (기존 방식 - 단일 입구 방향)
    /// </summary>
    public void GenerateMaze(float roomSize, float cellSize, Vector2Int entryDirection, Transform parent)
    {
        // 단일 입구만 사용하는 기존 호출은, 내부적으로 동일한 방향 하나만 가진 리스트를 만들어 새 메서드를 호출
        List<Vector2Int> entryDirections = new List<Vector2Int> { entryDirection };
        GenerateMaze(roomSize, cellSize, entryDirection, entryDirections, parent);
    }
    
    /// <summary>
    /// 함정방 미로를 생성합니다. (여러 개의 입출구 방향 지원)
    /// primaryEntryDirection: 메인 입구 방향 (시작 방과 연결되는 쪽)
    /// allEntryDirections: 열려 있는 모든 문 방향 (Up/Down/Left/Right 중 여러 개)
    /// </summary>
    public void GenerateMaze(float roomSize, float cellSize, Vector2Int primaryEntryDirection, List<Vector2Int> allEntryDirections, Transform parent)
    {
        // cellSize, roomSize 저장 (기본값)
        this.cellSize = cellSize;
        this.roomSize = roomSize;
        
        // 1) BaseRoom 컴포넌트가 있으면 그것을 기준으로 방/미로 크기를 결정 (가장 신뢰할 수 있는 값)
        BaseRoom baseRoom = parent != null ? parent.GetComponent<BaseRoom>() : null;
        if (baseRoom != null && baseRoom.TileSize > 0f)
        {
            this.cellSize = baseRoom.TileSize;
            // RoomWidth와 RoomHeight는 이미 Unity unit으로 반환되므로 그대로 사용
            float roomWidth = baseRoom.RoomWidth;
            float roomHeight = baseRoom.RoomHeight;
            this.roomSize = Mathf.Max(roomWidth, roomHeight);
            
            // 방의 셀 개수 계산 (가로와 세로 각각)
            int widthInCells = Mathf.Max(5, Mathf.RoundToInt(roomWidth / baseRoom.TileSize));
            int heightInCells = Mathf.Max(5, Mathf.RoundToInt(roomHeight / baseRoom.TileSize));
            mazeWidth = widthInCells;
            mazeHeight = heightInCells;
        }
        // 2) BaseRoom 정보가 없으면, 함정방의 실제 크기를 측정하여 미로 크기 결정
        else if (parent != null)
        {
            // 방 내부의 Tilemap과 Grid 찾기
            Tilemap roomTilemap = parent.GetComponentInChildren<Tilemap>();
            Grid roomGrid = null;
            
            if (roomTilemap != null)
            {
                roomGrid = roomTilemap.GetComponentInParent<Grid>();
            }
            
            if (roomGrid != null)
            {
                // 방의 실제 중심과 크기 계산
                Vector3 roomCenter = GetRoomActualCenter(parent);
                Vector3Int roomCenterCell = roomGrid.WorldToCell(roomCenter);
                
                // 방의 bounds를 계산하여 실제 크기 측정
                BoundsInt roomBounds = CalculateRoomBounds(parent, roomGrid, roomCenterCell);
                
                // 방 크기를 셀 단위로 계산
                int roomWidthInCells = roomBounds.size.x;
                int roomHeightInCells = roomBounds.size.y;
                
                // 요구사항: 방 크기와 미로 크기는 같아야 함
                // 따라서 미로 너비/높이를 방의 셀 크기와 동일하게 설정
                mazeWidth = Mathf.Max(5, roomWidthInCells);
                mazeHeight = Mathf.Max(5, roomHeightInCells);
            }
            else
            {
                // Grid를 찾지 못하면 roomSize 기반으로 방/미로 동일 크기 계산
                int sizeInCells = Mathf.Max(5, Mathf.RoundToInt(roomSize / this.cellSize));
                mazeWidth = sizeInCells;
                mazeHeight = sizeInCells;
            }
        }
        else
        {
            // parent가 없으면 기본값 사용
            mazeWidth = 15;
            mazeHeight = 15;
            if (mazeWidth % 2 == 0) mazeWidth--;
            if (mazeHeight % 2 == 0) mazeHeight--;
        }
        
        // 미로 그리드 초기화 (모두 벽으로 시작, false = 벽, true = 통로)
        mazeGrid = new bool[mazeWidth, mazeHeight];
        trapPositions = new List<Vector2Int>();
        trapObjects = new List<GameObject>();
        
        // 1단계: 벽으로 통로 만들기 (미로 생성)
        GenerateMazePaths();
        
        // 2단계: 메인 입구 위치 결정 (문이 있는 방향의 가장자리)
        entryPosition = DetermineEntryPosition(primaryEntryDirection);
        
        // 입구 위치 리스트 초기화 (메인 입구 포함)
        entryPositions.Clear();
        entryPositions.Add(entryPosition);
        
        // 추가 입출구가 있다면, 각 방향에 대해 가장자리 통로를 열어주고 리스트에 추가
        OpenAdditionalEntryPositions(primaryEntryDirection, allEntryDirections);
        
        // 3단계: 레버 위치 결정 (모서리에 최대한 가깝게)
        leverPosition = DetermineLeverPosition();
        
        // 4단계: 입구에서 레버까지 경로 보장
        EnsurePathToLever();
        
        // 5단계: 통로 안에 함정 배치
        PlaceTrapsInPaths();
        
        // 6단계: 실제 오브젝트 생성 (벽 먼저, 그 다음 함정)
        CreateMazeObjects(parent);
        CreateTraps(parent);
        CreateLever(parent);
    }
    
    /// <summary>
    /// Recursive Backtracking 기반 미로 생성 (개선된 버전)
    /// 홀수 크기 그리드에서 2칸씩 이동하여 완벽한 미로 생성
    /// </summary>
    private void GenerateMazePaths()
    {
        // 미로 크기를 홀수로 보정 (2칸씩 이동하기 위해)
        int actualWidth = (mazeWidth % 2 == 0) ? mazeWidth - 1 : mazeWidth;
        int actualHeight = (mazeHeight % 2 == 0) ? mazeHeight - 1 : mazeHeight;
        
        // 모든 셀을 벽으로 초기화
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                mazeGrid[x, y] = false;
            }
        }
        
        // DFS 스택
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        // 시작점 (중앙 근처의 홀수 좌표)
        int startX = (actualWidth / 2) * 2 + 1;
        int startY = (actualHeight / 2) * 2 + 1;
        startX = Mathf.Clamp(startX, 1, actualWidth - 2);
        startY = Mathf.Clamp(startY, 1, actualHeight - 2);
        
        Vector2Int start = new Vector2Int(startX, startY);
        stack.Push(start);
        visited.Add(start);
        mazeGrid[start.x, start.y] = true;
        
        // 4방향 (위, 아래, 오른쪽, 왼쪽)
        Vector2Int[] directions = {
            new Vector2Int(0, 2),   // 위 (2칸)
            new Vector2Int(0, -2),  // 아래 (2칸)
            new Vector2Int(2, 0),   // 오른쪽 (2칸)
            new Vector2Int(-2, 0)   // 왼쪽 (2칸)
        };
        
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> unvisitedNeighbors = new List<Vector2Int>();
            
            // 방문하지 않은 인접 셀 찾기 (2칸 떨어진 홀수 좌표)
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                
                // 유효한 범위 내이고, 방문하지 않았으며, 홀수 좌표인지 확인
                if (IsValidCell(next) && 
                    next.x >= 1 && next.x < actualWidth - 1 &&
                    next.y >= 1 && next.y < actualHeight - 1 &&
                    !visited.Contains(next))
                {
                    unvisitedNeighbors.Add(next);
                }
            }
            
            if (unvisitedNeighbors.Count > 0)
            {
                // 랜덤하게 선택
                Vector2Int chosen = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                Vector2Int wall = current + (chosen - current) / 2; // 중간 벽 위치
                
                // 벽 제거 (통로 만들기)
                mazeGrid[wall.x, wall.y] = true;
                mazeGrid[chosen.x, chosen.y] = true;
                
                visited.Add(chosen);
                stack.Push(chosen);
            }
            else
            {
                // 더 이상 갈 곳이 없으면 백트래킹
                stack.Pop();
            }
        }
        
        // 가장자리 벽 보정 (경계를 벽으로 확실히 설정)
        for (int x = 0; x < mazeWidth; x++)
        {
            mazeGrid[x, 0] = false; // 아래쪽 벽
            mazeGrid[x, mazeHeight - 1] = false; // 위쪽 벽
        }
        for (int y = 0; y < mazeHeight; y++)
        {
            mazeGrid[0, y] = false; // 왼쪽 벽
            mazeGrid[mazeWidth - 1, y] = false; // 오른쪽 벽
        }
        
    }
    
    /// <summary>
    /// 셀이 유효한 범위 내에 있는지 확인
    /// </summary>
    private bool IsValidCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < mazeWidth && cell.y >= 0 && cell.y < mazeHeight;
    }
    
    /// <summary>
    /// 입구 위치 결정 (문이 있는 방향의 가장자리)
    /// </summary>
    private Vector2Int DetermineEntryPosition(Vector2Int entryDirection)
    {
        Vector2Int entry = Vector2Int.zero;
        
        if (entryDirection == Direction.Up)
        {
            // 위쪽 벽의 중앙
            entry = new Vector2Int(mazeWidth / 2, mazeHeight - 1);
        }
        else if (entryDirection == Direction.Down)
        {
            // 아래쪽 벽의 중앙
            entry = new Vector2Int(mazeWidth / 2, 0);
        }
        else if (entryDirection == Direction.Left)
        {
            // 왼쪽 벽의 중앙
            entry = new Vector2Int(0, mazeHeight / 2);
        }
        else if (entryDirection == Direction.Right)
        {
            // 오른쪽 벽의 중앙
            entry = new Vector2Int(mazeWidth - 1, mazeHeight / 2);
        }
        
        // 입구를 통로로 만들기
        mazeGrid[entry.x, entry.y] = true;
        
        // 입구 주변도 통로로 만들기 (안전 구역)
        for (int x = Mathf.Max(0, entry.x - 1); x <= Mathf.Min(mazeWidth - 1, entry.x + 1); x++)
        {
            for (int y = Mathf.Max(0, entry.y - 1); y <= Mathf.Min(mazeHeight - 1, entry.y + 1); y++)
            {
                mazeGrid[x, y] = true;
            }
        }
        
        return entry;
    }
    
    /// <summary>
    /// 방향에 따른 가장자리 셀 위치를 계산합니다. (실제 그리드는 변경하지 않음)
    /// </summary>
    private Vector2Int GetBorderPositionForDirection(Vector2Int direction)
    {
        if (direction == Direction.Up)
        {
            return new Vector2Int(mazeWidth / 2, mazeHeight - 1);
        }
        if (direction == Direction.Down)
        {
            return new Vector2Int(mazeWidth / 2, 0);
        }
        if (direction == Direction.Left)
        {
            return new Vector2Int(0, mazeHeight / 2);
        }
        if (direction == Direction.Right)
        {
            return new Vector2Int(mazeWidth - 1, mazeHeight / 2);
        }
        // 기본값: 아래쪽 중앙
        return new Vector2Int(mazeWidth / 2, 0);
    }
    
    /// <summary>
    /// 메인 입구 외에, 열려 있는 다른 문 방향에 대해서도 가장자리 통로를 엽니다.
    /// </summary>
    private void OpenAdditionalEntryPositions(Vector2Int primaryDirection, List<Vector2Int> allDirections)
    {
        if (allDirections == null || allDirections.Count == 0) return;
        
        foreach (var dir in allDirections)
        {
            // 메인 입구 방향은 이미 처리했으므로 건너뜀
            if (dir == primaryDirection) continue;
            
            Vector2Int entry = GetBorderPositionForDirection(dir);
            
            // 경계 체크
            if (!IsValidCell(entry)) continue;
            
            // 입구 셀을 통로로 설정
            mazeGrid[entry.x, entry.y] = true;
            
            // 입구 위치 리스트에도 추가 (함정 안전 구역 계산용)
            entryPositions.Add(entry);
            
            // 주변 1칸(3x3)을 통로로 만들어, 방과 자연스럽게 연결되도록 함
            for (int x = Mathf.Max(0, entry.x - 1); x <= Mathf.Min(mazeWidth - 1, entry.x + 1); x++)
            {
                for (int y = Mathf.Max(0, entry.y - 1); y <= Mathf.Min(mazeHeight - 1, entry.y + 1); y++)
                {
                    mazeGrid[x, y] = true;
                }
            }
            
        }
    }
    
    /// <summary>
    /// 레버 위치 결정
    /// - 여러 입구가 있을 때, 어떤 입구에서 출발하더라도
    ///   레버까지의 "최단 경로"가 최대가 되도록(가능한 한 멀도록) 선택합니다.
    /// - 구현: 모든 입구를 동시에 시작점으로 하는 BFS로 각 셀까지의 최소 거리 계산 →
    ///   그 중 거리가 가장 큰 통로 셀을 레버 위치로 사용.
    /// </summary>
    private Vector2Int DetermineLeverPosition()
    {
        // 1. 입구 리스트 준비 (없으면 기존 단일 entryPosition 사용)
        List<Vector2Int> sources = new List<Vector2Int>();
        if (entryPositions != null && entryPositions.Count > 0)
        {
            sources.AddRange(entryPositions);
        }
        else
        {
            sources.Add(entryPosition);
        }
        
        // 2. BFS를 위한 거리 배열 초기화 (-1 = 방문 안 함)
        int[,] dist = new int[mazeWidth, mazeHeight];
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                dist[x, y] = -1;
            }
        }
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        // 3. 모든 입구를 시작점(거리 0)으로 큐에 추가 (멀티 소스 BFS)
        foreach (var src in sources)
        {
            if (!IsValidCell(src)) continue;
            dist[src.x, src.y] = 0;
            queue.Enqueue(src);
        }
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        // 4. BFS로 각 통로 셀까지의 "가장 가까운 입구"로부터의 거리 계산
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                
                if (!IsValidCell(next)) continue;
                if (!mazeGrid[next.x, next.y]) continue; // 통로가 아니면 패스
                if (dist[next.x, next.y] != -1) continue; // 이미 방문
                
                dist[next.x, next.y] = dist[current.x, current.y] + 1;
                queue.Enqueue(next);
            }
        }
        
        // 5. 가장 먼 통로 셀을 레버 위치 후보로 선택
        Vector2Int bestPos = entryPosition;
        int bestDist = -1;
        
        for (int x = 1; x < mazeWidth - 1; x++)
        {
            for (int y = 1; y < mazeHeight - 1; y++)
            {
                if (!mazeGrid[x, y]) continue;      // 통로만 대상
                if (dist[x, y] < 0) continue;       // 도달 불가
                
                // 입구 안전 구역(safeZoneRadius) 안쪽은 제외 (레버가 입구 바로 옆에 생기지 않도록)
                bool nearAnyEntry = false;
                foreach (var src in sources)
                {
                    int dEntry = Mathf.Abs(x - src.x) + Mathf.Abs(y - src.y);
                    if (dEntry <= safeZoneRadius)
                    {
                        nearAnyEntry = true;
                        break;
                    }
                }
                if (nearAnyEntry) continue;
                
                // 현재 셀이 기존 최선보다 더 멀다면 갱신
                if (dist[x, y] > bestDist)
                {
                    bestDist = dist[x, y];
                    bestPos = new Vector2Int(x, y);
                }
            }
        }
        
        // 6. 적절한 후보를 못 찾았으면, 기존 모서리 기반 로직으로 fallback
        if (bestDist < 0)
        {
            Debug.LogWarning("[TrapRoomMazeGenerator] 레버 위치 후보를 찾지 못해 기존 모서리 기반 로직을 사용합니다.");
            
        List<Vector2Int> cornerCandidates = new List<Vector2Int>
        {
                new Vector2Int(1, 1),
                new Vector2Int(mazeWidth - 2, 1),
                new Vector2Int(1, mazeHeight - 2),
                new Vector2Int(mazeWidth - 2, mazeHeight - 2)
            };
            
        Vector2Int farthestCorner = cornerCandidates[0];
        float maxDistance = 0f;
        
        foreach (var corner in cornerCandidates)
        {
                float sumDist = 0f;
                foreach (var src in sources)
                {
                    sumDist += Vector2Int.Distance(src, corner);
                }
                
                if (sumDist > maxDistance)
                {
                    maxDistance = sumDist;
                farthestCorner = corner;
            }
        }
        
        // 모서리 근처에서 통로가 있는 위치 찾기
        Vector2Int leverPos = farthestCorner;
        for (int radius = 0; radius < 3; radius++)
        {
            for (int x = Mathf.Max(1, farthestCorner.x - radius); x <= Mathf.Min(mazeWidth - 2, farthestCorner.x + radius); x++)
            {
                for (int y = Mathf.Max(1, farthestCorner.y - radius); y <= Mathf.Min(mazeHeight - 2, farthestCorner.y + radius); y++)
                {
                    if (mazeGrid[x, y])
                    {
                        leverPos = new Vector2Int(x, y);
                            return leverPos;
                        }
                    }
                }
            }
            
            return leverPos;
        }
        
        return bestPos;
    }
    
    /// <summary>
    /// 입구에서 레버까지 경로 보장 (BFS로 경로 확인 후 필요시 경로 생성)
    /// </summary>
    private void EnsurePathToLever()
    {
        // BFS로 경로 확인
        if (HasPath(entryPosition, leverPosition))
        {
            return; // 이미 경로가 있음
        }
        
        // 경로가 없으면 최단 경로 생성
        List<Vector2Int> path = FindShortestPath(entryPosition, leverPosition);
        
        foreach (var cell in path)
        {
            mazeGrid[cell.x, cell.y] = true;
        }
    }
    
    /// <summary>
    /// 두 위치 사이에 경로가 있는지 확인 (BFS)
    /// </summary>
    private bool HasPath(Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(start);
        visited.Add(start);
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            if (current == end)
            {
                return true;
            }
            
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                
                if (IsValidCell(next) && mazeGrid[next.x, next.y] && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 최단 경로 찾기 (A* 스타일, 단순화)
    /// </summary>
    private List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int end)
    {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(start);
        visited.Add(start);
        cameFrom[start] = start;
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            if (current == end)
            {
                // 경로 재구성
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int node = end;
                
                while (node != start)
                {
                    path.Add(node);
                    node = cameFrom[node];
                }
                path.Add(start);
                path.Reverse();
                
                return path;
            }
            
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                
                if (IsValidCell(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }
        }
        
        // 경로를 찾지 못한 경우 직선 경로 반환
        List<Vector2Int> straightPath = new List<Vector2Int>();
        Vector2Int currentPos = start;
        
        while (currentPos != end)
        {
            straightPath.Add(currentPos);
            Vector2Int diff = end - currentPos;
            
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                currentPos += new Vector2Int((int)Mathf.Sign(diff.x), 0);
            }
            else
            {
                currentPos += new Vector2Int(0, (int)Mathf.Sign(diff.y));
            }
        }
        straightPath.Add(end);
        
        return straightPath;
    }
    
    /// <summary>
    /// 통로 안에 함정 배치 (벽으로 만든 통로 내부에만 배치)
    /// </summary>
    private void PlaceTrapsInPaths()
    {
        // 1. 통로 위치 수집 (mazeGrid[x,y] == true인 위치만)
        List<Vector2Int> pathPositions = new List<Vector2Int>();
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                // 통로인 위치만 수집 (벽이 아닌 곳)
                if (mazeGrid[x, y])
                {
                    pathPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        if (pathPositions.Count == 0)
        {
            Debug.LogWarning($"[TrapRoomMazeGenerator] 배치할 통로가 없습니다.");
            return;
        }
        
        // 2. 최대 함정 개수 계산 (통로 타일의 30%)
        int maxTraps = Mathf.RoundToInt(pathPositions.Count * maxTrapRatio);
        maxTraps = Mathf.Max(1, maxTraps); // 최소 1개
        
        // 3. 함정 배치 가능한 위치 필터링
        List<Vector2Int> validTrapPositions = new List<Vector2Int>();
        
        foreach (var pos in pathPositions)
        {
            // 입구/출구 안전 구역 제외 (2칸 안에는 생성 안됨)
            // 여러 입구가 있을 수 있으므로, 모든 입구에 대해 최소 맨해튼 거리를 사용
            int minDistToAnyEntry = int.MaxValue;
            if (entryPositions != null && entryPositions.Count > 0)
            {
                foreach (var ep in entryPositions)
                {
                    int d = Mathf.Abs(pos.x - ep.x) + Mathf.Abs(pos.y - ep.y);
                    if (d < minDistToAnyEntry) minDistToAnyEntry = d;
                }
            }
            else
            {
                // fallback: 기존 단일 입구 로직
                minDistToAnyEntry = Mathf.Abs(pos.x - entryPosition.x) + Mathf.Abs(pos.y - entryPosition.y);
            }
            
            int distToLever = Mathf.Abs(pos.x - leverPosition.x) + Mathf.Abs(pos.y - leverPosition.y);
            
            if (minDistToAnyEntry <= safeZoneRadius || distToLever <= safeZoneRadius)
            {
                continue;
            }
            
            // 레버나 입구 위치 제외
            if (pos == entryPosition || pos == leverPosition)
            {
                continue;
            }
            
            validTrapPositions.Add(pos);
        }
        
        if (validTrapPositions.Count == 0)
        {
            Debug.LogWarning($"[TrapRoomMazeGenerator] 함정을 배치할 수 있는 유효한 위치가 없습니다.");
            return;
        }
        
        // 4. 함정 배치
        // - maxTrapRatio 비율로 통로 전체에 고르게 분포하도록 배치
        // - 입구/레버 안전 구역은 이미 제외된 상태
        // - 인접 셀에는 함정이 붙지 않도록 유지해 밀집을 방지
        HashSet<Vector2Int> placedTraps = new HashSet<Vector2Int>();
        int trapCount = 0;

        Vector2Int[] directions = {
            new Vector2Int(0, 1),   // 위
            new Vector2Int(0, -1),  // 아래
            new Vector2Int(1, 0),   // 오른쪽
            new Vector2Int(-1, 0)   // 왼쪽
        };

        // 후보를 무작위로 섞어서 시작
        List<Vector2Int> shuffledCandidates = validTrapPositions.OrderBy(_ => Random.value).ToList();

        // 균등 분포를 위해 매번 "이미 배치된 함정과의 최소 거리"가 가장 큰 지점을 선택
        while (trapCount < maxTraps && shuffledCandidates.Count > 0)
        {
            int bestIndex = -1;
            float bestMinDist = -1f;

            for (int i = 0; i < shuffledCandidates.Count; i++)
            {
                Vector2Int candidate = shuffledCandidates[i];

                // 인접 셀에 이미 함정이 있으면 스킵
                bool hasAdjacentTrap = false;
                foreach (var dir in directions)
                {
                    if (placedTraps.Contains(candidate + dir))
                    {
                        hasAdjacentTrap = true;
                        break;
                    }
                }
                if (hasAdjacentTrap) continue;

                // 현재까지 배치된 함정과의 최소 거리 계산
                float minDist = float.MaxValue;
                if (placedTraps.Count == 0)
                {
                    minDist = float.MaxValue;
                }
                else
                {
                    foreach (var placed in placedTraps)
                    {
                        float d = Vector2Int.Distance(candidate, placed);
                        if (d < minDist) minDist = d;
                    }
                }

                if (minDist > bestMinDist)
                {
                    bestMinDist = minDist;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                break; // 더 이상 배치할 수 있는 위치가 없음
            }

            Vector2Int selected = shuffledCandidates[bestIndex];
            shuffledCandidates.RemoveAt(bestIndex);

            placedTraps.Add(selected);
            trapPositions.Add(selected);
            trapCount++;
        }
    }
    
    /// <summary>
    /// 함정을 고려한 경로 확인 (BFS) - 최소 1개 이상의 경로가 존재하는지 확인
    /// </summary>
    private bool HasPathWithTraps(Vector2Int start, Vector2Int end, HashSet<Vector2Int> traps)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(start);
        visited.Add(start);
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            if (current == end)
            {
                return true; // 최소 1개 이상의 경로가 존재
            }
            
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                
                if (IsValidCell(next) && 
                    mazeGrid[next.x, next.y] && 
                    !traps.Contains(next) && // 함정 위치는 통과 불가
                    !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        
        return false; // 경로가 없음
    }
    
    /// <summary>
    /// 시작점에서 끝점까지의 모든 경로를 찾습니다. (DFS 기반)
    /// </summary>
    private List<List<Vector2Int>> FindAllPaths(Vector2Int start, Vector2Int end)
    {
        List<List<Vector2Int>> allPaths = new List<List<Vector2Int>>();
        List<Vector2Int> currentPath = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        FindAllPathsDFS(start, end, currentPath, visited, allPaths);
        
        return allPaths;
    }
    
    /// <summary>
    /// DFS로 모든 경로를 찾는 재귀 함수
    /// </summary>
    private void FindAllPathsDFS(Vector2Int current, Vector2Int end, List<Vector2Int> currentPath, 
        HashSet<Vector2Int> visited, List<List<Vector2Int>> allPaths)
    {
        // 최대 경로 개수 제한 (성능을 위해)
        if (allPaths.Count >= 10) return;
        
        currentPath.Add(current);
        visited.Add(current);
        
        if (current == end)
        {
            // 경로를 찾았으면 복사본 저장
            allPaths.Add(new List<Vector2Int>(currentPath));
            currentPath.RemoveAt(currentPath.Count - 1);
            visited.Remove(current);
            return;
        }
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        foreach (var dir in directions)
        {
            Vector2Int next = current + dir;
            
            if (IsValidCell(next) && 
                mazeGrid[next.x, next.y] && 
                !visited.Contains(next))
            {
                FindAllPathsDFS(next, end, currentPath, visited, allPaths);
            }
        }
        
        currentPath.RemoveAt(currentPath.Count - 1);
        visited.Remove(current);
    }
    
    /// <summary>
    /// 미로 벽 생성 (벽만 생성하여 통로 만들기)
    /// 컨테이너 기반 구조로 생성: Maze > Walls
    /// </summary>
    private void CreateMazeObjects(Transform parent)
    {
        if (wallTilePrefab == null)
        {
            Debug.LogWarning($"[TrapRoomMazeGenerator] 벽 프리펩이 설정되지 않았습니다. wallTilePrefab: {wallTilePrefab}");
            return;
        }
        
        // Maze 컨테이너 찾기 또는 생성
        Transform mazeContainer = FindOrCreateContainer(parent, "Maze");
        Transform wallContainer = FindOrCreateContainer(mazeContainer, "Walls");
        
        // 방 내부의 Tilemap과 Grid 찾기
        Tilemap roomTilemap = parent != null ? parent.GetComponentInChildren<Tilemap>() : null;
        Grid roomGrid = null;
        
        if (roomTilemap != null)
        {
            roomGrid = roomTilemap.GetComponentInParent<Grid>();
        }
        
        // 방의 실제 중심 위치 계산 (RoomCenterMarker 기준)
        Vector3 roomCenter = GetRoomActualCenter(parent);
        
        int wallCount = 0;
        
        // 1순위: Tilemap bounds가 유효할 때, 타일맵 좌표에 정확히 맞춰 배치
        if (roomGrid != null && roomTilemap != null)
        {
            roomTilemap.CompressBounds();
            BoundsInt tilemapBounds = roomTilemap.cellBounds;
            
            if (tilemapBounds.size.x > 0 && tilemapBounds.size.y > 0)
            {
                // 미로가 방보다 위로 3칸 올라가 있으므로, 원점을 y 방향으로 3칸 내려서 맞춘다.
                Vector3Int mazeOriginCell = new Vector3Int(tilemapBounds.xMin, tilemapBounds.yMin - 3, 0);
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                        // 벽만 생성 (mazeGrid[x, y] == false인 경우)
                        if (!mazeGrid[x, y])
                        {
                // 미로 그리드 (x, y) -> 타일맵 셀 좌표
                Vector3Int tilemapCell = mazeOriginCell + new Vector3Int(x, y, 0);
                
                // 셀 중앙 위치 (RoomCenterMarker 기준 roomCenter와 정렬)
                Vector3 worldPos = roomGrid.GetCellCenterWorld(tilemapCell);
                
                GameObject wall = Instantiate(wallTilePrefab, worldPos, Quaternion.identity, wallContainer);
                            // PlayerMoveController.wallTag에서 사용하는 "Wall" 태그를 자동으로 설정
                            wall.tag = "Wall";
                            wallCount++;
                        }
                    }
                }
                
            }
            else
            {
                // 타일맵 bounds가 비어있으면 월드 좌표 방식으로 배치
                float mazeActualWidth = mazeWidth * cellSize;
                float mazeActualHeight = mazeHeight * cellSize;
                Vector3 offset = new Vector3(-mazeActualWidth * 0.5f, -mazeActualHeight * 0.5f, 0f);
                Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
                
                for (int x = 0; x < mazeWidth; x++)
                {
                    for (int y = 0; y < mazeHeight; y++)
                    {
                        if (!mazeGrid[x, y])
                        {
                            Vector3 worldPos = roomCenter + offset + new Vector3(x * cellSize, y * cellSize, 0f) + cellCenterOffset;
                            GameObject wall = Instantiate(wallTilePrefab, worldPos, Quaternion.identity, wallContainer);
                            wall.tag = "Wall";
                            wallCount++;
                        }
                    }
                }
                
            }
        }
        // 2순위: Grid나 Tilemap이 없으면 월드 좌표 방식으로 배치
                else
                {
            float mazeActualWidth = mazeWidth * cellSize;
            float mazeActualHeight = mazeHeight * cellSize;
            Vector3 offset = new Vector3(-mazeActualWidth * 0.5f, -mazeActualHeight * 0.5f, 0f);
            Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
            
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    if (!mazeGrid[x, y])
                    {
                        Vector3 worldPos = roomCenter + offset + new Vector3(x * cellSize, y * cellSize, 0f) + cellCenterOffset;
                        GameObject wall = Instantiate(wallTilePrefab, worldPos, Quaternion.identity, wallContainer);
                        wall.tag = "Wall";
                        wallCount++;
                    }
                }
            }
            
            Debug.Log($"[TrapRoomMazeGenerator] 벽 생성: Grid/Tilemap 없음, 월드 기준 배치 (roomCenter={roomCenter}, mazeSize={mazeWidth}x{mazeHeight})");
        }
        
    }
    
    /// <summary>
    /// 컨테이너를 찾거나 생성합니다.
    /// </summary>
    private Transform FindOrCreateContainer(Transform parent, string containerName)
    {
        if (parent == null) return null;
        
        // 기존 컨테이너 찾기
        foreach (Transform child in parent)
        {
            if (child.name == containerName)
            {
                return child;
            }
        }
        
        // 컨테이너가 없으면 생성
        GameObject container = new GameObject(containerName);
        container.transform.SetParent(parent);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        
        return container.transform;
    }
    
    /// <summary>
    /// 방의 bounds를 계산합니다. (Grid 셀 단위)
    /// </summary>
    private BoundsInt CalculateRoomBounds(Transform parent, Grid grid, Vector3Int centerCell)
    {
        // Tilemap을 우선적으로 사용하여 정확한 크기 측정
        Tilemap roomTilemap = parent.GetComponentInChildren<Tilemap>();
        if (roomTilemap != null)
        {
            // Tilemap의 실제 사용된 셀 범위 계산
            roomTilemap.CompressBounds();
            BoundsInt tilemapBounds = roomTilemap.cellBounds;
            
            // Tilemap bounds를 사용 (더 정확함)
            if (tilemapBounds.size.x > 0 && tilemapBounds.size.y > 0)
            {
                return tilemapBounds;
            }
        }
        
        // Tilemap이 없거나 bounds가 없으면 Renderer/Collider bounds 사용
        Bounds? bounds = null;
        foreach (var r in parent.GetComponentsInChildren<Renderer>())
        {
            if (r.name.Contains("Door") || r.name.Contains("Maze")) continue;
            bounds = bounds.HasValue ? EncapsulateBounds(bounds.Value, r.bounds) : r.bounds;
        }
        foreach (var c in parent.GetComponentsInChildren<Collider2D>())
        {
            if (c.name.Contains("Door") || c.name.Contains("Maze")) continue;
            bounds = bounds.HasValue ? EncapsulateBounds(bounds.Value, c.bounds) : c.bounds;
        }
        
        if (bounds.HasValue)
        {
            // bounds를 Grid 셀 좌표로 변환
            Vector3Int minCell = grid.WorldToCell(bounds.Value.min);
            Vector3Int maxCell = grid.WorldToCell(bounds.Value.max);
            
            BoundsInt result = new BoundsInt(minCell, maxCell - minCell);
            return result;
        }
        
        // bounds를 찾지 못하면 BaseRoom의 RoomWidth/RoomHeight 사용
        BaseRoom baseRoom = parent.GetComponent<BaseRoom>();
        if (baseRoom != null)
        {
            float roomWidth = baseRoom.RoomWidth;
            float roomHeight = baseRoom.RoomHeight;
            int widthInCells = Mathf.RoundToInt(roomWidth / grid.cellSize.x);
            int heightInCells = Mathf.RoundToInt(roomHeight / grid.cellSize.y);
            int halfWidth = widthInCells / 2;
            int halfHeight = heightInCells / 2;
            BoundsInt result = new BoundsInt(
                centerCell - new Vector3Int(halfWidth, halfHeight, 0),
                new Vector3Int(widthInCells, heightInCells, 0)
            );
            return result;
        }
        
        // 최후의 수단: 기본값 반환
        Debug.LogWarning($"[TrapRoomMazeGenerator] 방 크기를 측정할 수 없어 기본값 사용");
        return new BoundsInt(centerCell - new Vector3Int(10, 10, 0), new Vector3Int(20, 20, 0));
    }
    
    /// <summary>
    /// 방의 실제 중심 위치를 계산합니다.
    /// 우선순위:
    /// 1) RoomCenterMarker 태그가 붙은 자식 오브젝트 위치
    /// 2) 방 Tilemap + Grid 의 cellBounds 중심 (시각적으로 보이는 방 중심)
    /// 3) Renderer/Collider bounds 중심
    /// 4) BaseRoom/Transform 위치
    /// </summary>
    private Vector3 GetRoomActualCenter(Transform parent)
    {
        if (parent == null) return transform.position;

        // 1) RoomCenterMarker 우선 사용
        Transform centerMarker = FindRoomCenterMarker(parent);
        if (centerMarker != null)
        {
            return centerMarker.position;
        }
        
        // 1) 방 Tilemap + Grid 기준 중심 계산
        Tilemap roomTilemap = parent.GetComponentInChildren<Tilemap>();
        if (roomTilemap != null)
        {
            Grid roomGrid = roomTilemap.GetComponentInParent<Grid>();
            if (roomGrid != null)
            {
                // 실제 타일이 깔린 영역의 중앙 셀을 사용
                roomTilemap.CompressBounds();
                BoundsInt tilemapBounds = roomTilemap.cellBounds;
                if (tilemapBounds.size.x > 0 && tilemapBounds.size.y > 0)
                {
                    int centerX = tilemapBounds.xMin + tilemapBounds.size.x / 2;
                    int centerY = tilemapBounds.yMin + tilemapBounds.size.y / 2;
                    Vector3Int centerCell = new Vector3Int(centerX, centerY, 0);
                    
                    Vector3 worldCenter = roomGrid.GetCellCenterWorld(centerCell);
                    return worldCenter;
                }
            }
        }
        
        // 2) Renderer/Collider bounds로 계산
        Bounds? bounds = null;
        foreach (var r in parent.GetComponentsInChildren<Renderer>())
        {
            if (r.name.Contains("Door") || r.name.Contains("Maze")) continue;
            bounds = bounds.HasValue ? EncapsulateBounds(bounds.Value, r.bounds) : r.bounds;
        }
        foreach (var c in parent.GetComponentsInChildren<Collider2D>())
        {
            if (c.name.Contains("Door") || c.name.Contains("Maze")) continue;
            bounds = bounds.HasValue ? EncapsulateBounds(bounds.Value, c.bounds) : c.bounds;
        }
        
        if (bounds.HasValue)
        {
            return bounds.Value.center;
        }
        
        // 4) 최후의 수단: BaseRoom/Transform 위치 사용
        return parent.position;
    }
    
    private Bounds EncapsulateBounds(Bounds a, Bounds b)
    {
        a.Encapsulate(b);
        return a;
    }

    /// <summary>
    /// 방 오브젝트 하위에서 RoomCenterMarker 태그를 가진 Transform을 찾습니다.
    /// </summary>
    private Transform FindRoomCenterMarker(Transform parent)
    {
        if (parent == null) return null;

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (var t in children)
        {
            if (t.CompareTag("RoomCenterMarker"))
            {
                return t;
            }
        }

        return null;
    }
    
    /// <summary>
    /// 함정 오브젝트 생성
    /// Traps 컨테이너에 배치
    /// </summary>
    private void CreateTraps(Transform parent)
    {
        if (trapPrefab == null && trapOffPrefab == null)
        {
            Debug.LogWarning($"[TrapRoomMazeGenerator] 함정 프리펩이 설정되지 않았습니다.");
            return;
        }
        
        if (trapPositions == null || trapPositions.Count == 0)
        {
            Debug.LogWarning($"[TrapRoomMazeGenerator] 배치할 함정이 없습니다.");
            return;
        }

        // 이전에 생성된 함정 오브젝트 목록 초기화
        if (trapObjects == null)
            trapObjects = new List<GameObject>();
        else
            trapObjects.Clear();
        
        // Traps 컨테이너 찾기 또는 생성 (Maze > Traps 또는 직접 Interactive > Traps)
        Transform trapsContainer = null;
        
        // 먼저 Maze > Traps 찾기
        Transform mazeContainer = FindOrCreateContainer(parent, "Maze");
        if (mazeContainer != null)
        {
            trapsContainer = FindOrCreateContainer(mazeContainer, "Traps");
        }
        
        // Maze > Traps가 없으면 Interactive > Traps 찾기
        if (trapsContainer == null)
        {
            Transform interactiveContainer = FindOrCreateContainer(parent, "Interactive");
            if (interactiveContainer != null)
            {
                trapsContainer = FindOrCreateContainer(interactiveContainer, "Traps");
            }
        }
        
        // 둘 다 없으면 Maze > Traps 생성
        if (trapsContainer == null)
        {
            trapsContainer = FindOrCreateContainer(mazeContainer, "Traps");
        }
        
        // 방 내부의 Tilemap과 Grid 찾기
        Tilemap roomTilemap = parent != null ? parent.GetComponentInChildren<Tilemap>() : null;
        Grid roomGrid = null;
        
        if (roomTilemap != null)
        {
            roomGrid = roomTilemap.GetComponentInParent<Grid>();
        }
        
        if (roomGrid == null)
        {
            // Grid를 찾지 못하면 기존 방식 사용
            Debug.LogWarning($"[TrapRoomMazeGenerator] 방의 Grid를 찾을 수 없어 기본 방식으로 배치합니다.");
            CreateTrapsWithWorldPosition(parent, trapsContainer);
            return;
        }
        
        // 방의 실제 중심 위치 계산 (RoomCenterMarker 기준)
        Vector3 roomCenter = GetRoomActualCenter(parent);
        
        // 1순위: Tilemap bounds가 유효할 때, 타일맵 좌표에 맞춰 배치
        roomTilemap.CompressBounds();
        BoundsInt tilemapBounds = roomTilemap.cellBounds;
        
        int trapCount = 0;
        
        if (tilemapBounds.size.x > 0 && tilemapBounds.size.y > 0)
        {
            // 미로가 방보다 위로 3칸 올라가 있으므로, 원점을 y 방향으로 3칸 내려서 맞춘다.
            Vector3Int mazeOriginCell = new Vector3Int(tilemapBounds.xMin, tilemapBounds.yMin - 3, 0);
        
        foreach (var trapPos in trapPositions)
        {
                // 미로 그리드 좌표를 타일맵 Grid 셀 좌표로 변환
                Vector3Int tilemapCell = mazeOriginCell + new Vector3Int(trapPos.x, trapPos.y, 0);
                
                // 셀 중앙 위치
                Vector3 worldPos = roomGrid.GetCellCenterWorld(tilemapCell);
                
                // 기본 상태는 OFF로 시작: trapOffPrefab이 있으면 OFF 프리팹, 없으면 ON 프리팹 사용
                GameObject initialTrapPrefab = trapOffPrefab != null ? trapOffPrefab : trapPrefab;
                GameObject trap = Instantiate(initialTrapPrefab, worldPos, Quaternion.identity, trapsContainer);
                
                trapObjects.Add(trap);
                trapCount++;
            }
            
        }
        // 2순위: Tilemap bounds가 비어있으면 월드 좌표 방식으로 배치
        else
        {
            float mazeActualWidth = mazeWidth * cellSize;
            float mazeActualHeight = mazeHeight * cellSize;
            Vector3 offset = new Vector3(-mazeActualWidth * 0.5f, -mazeActualHeight * 0.5f, 0f);
            Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, 0.5f * cellSize, 0f);
            
            foreach (var trapPos in trapPositions)
            {
                Vector3 worldPos = roomCenter + offset + new Vector3(trapPos.x * cellSize, trapPos.y * cellSize, 0f) + cellCenterOffset;

                GameObject initialTrapPrefab = trapOffPrefab != null ? trapOffPrefab : trapPrefab;
                GameObject trap = Object.Instantiate(initialTrapPrefab, worldPos, Quaternion.identity, trapsContainer);
                
                trapObjects.Add(trap);
                trapCount++;
            }
            
        }
    }
    
    /// <summary>
    /// 월드 좌표로 함정 생성 (Grid를 찾지 못한 경우)
    /// </summary>
    private void CreateTrapsWithWorldPosition(Transform parent, Transform trapsContainer)
    {
        if (trapsContainer == null)
        {
            trapsContainer = FindOrCreateContainer(parent, "Traps");
        }
        
        // 방의 실제 중심 위치 계산
        Vector3 roomCenter = GetRoomActualCenter(parent);
        
        // 미로의 실제 크기 (cellSize = 1)
        float mazeActualWidth = mazeWidth * cellSize;
        float mazeActualHeight = mazeHeight * cellSize;
        
        // 미로를 방 중앙에 맞추기 위한 오프셋
        // 미로 그리드 좌표 (0,0)이 방 중앙에 오도록 설정
        Vector3 offset = new Vector3(-mazeActualWidth * 0.5f, -mazeActualHeight * 0.5f, 0f);
        
        // 셀 중심으로 배치하기 위한 오프셋 추가 (cellSize = 1이므로 0.5)
        Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
        
        int trapCount = 0;
        foreach (var trapPos in trapPositions)
        {
            // 셀의 중심 위치 계산
            // trapPos는 미로 그리드 좌표 (0 ~ mazeWidth-1, 0 ~ mazeHeight-1)
            Vector3 worldPos = roomCenter + offset + new Vector3(trapPos.x * cellSize, trapPos.y * cellSize, 0f) + cellCenterOffset;

            GameObject initialTrapPrefab = trapOffPrefab != null ? trapOffPrefab : trapPrefab;
            GameObject trap = Instantiate(initialTrapPrefab, worldPos, Quaternion.identity, trapsContainer);
            
            trapObjects.Add(trap);
            trapCount++;
        }
        
    }
    
    /// <summary>
    /// 레버 오브젝트 생성
    /// Interactive 컨테이너에 배치
    /// </summary>
    private void CreateLever(Transform parent)
    {
        if (leverPrefab == null)
        {
            Debug.LogWarning($"[TrapRoomMazeGenerator] 레버 프리펩이 설정되지 않았습니다.");
            return;
        }
        
        // Interactive 컨테이너 찾기 또는 생성
        Transform interactiveContainer = FindOrCreateContainer(parent, "Interactive");
        
        // 방 내부의 Tilemap과 Grid 찾기
        Tilemap roomTilemap = parent != null ? parent.GetComponentInChildren<Tilemap>() : null;
        Grid roomGrid = null;
        
        if (roomTilemap != null)
        {
            roomGrid = roomTilemap.GetComponentInParent<Grid>();
        }
        
        Vector3 worldPos;
        
        if (roomGrid != null && roomTilemap != null)
        {
            // 타일맵 Grid 좌표 사용
            roomTilemap.CompressBounds();
            BoundsInt tilemapBounds = roomTilemap.cellBounds;
            
            if (tilemapBounds.size.x > 0 && tilemapBounds.size.y > 0)
            {
                // 미로가 방보다 위로 3칸 올라가 있으므로, 원점을 y 방향으로 3칸 내려서 맞춘다.
                Vector3Int mazeOriginCell = new Vector3Int(tilemapBounds.xMin, tilemapBounds.yMin - 3, 0);
                
                // 미로 그리드 좌표를 타일맵 Grid 셀 좌표로 변환
                Vector3Int tilemapCell = mazeOriginCell + new Vector3Int(leverPosition.x, leverPosition.y, 0);
                
                // 셀 중앙 위치
                worldPos = roomGrid.GetCellCenterWorld(tilemapCell);
                
            }
            else
            {
                // 타일맵 bounds가 비어있으면 월드 좌표 방식 사용
                Vector3 roomCenter = GetRoomActualCenter(parent);
                float mazeActualWidth = mazeWidth * cellSize;
                float mazeActualHeight = mazeHeight * cellSize;
                Vector3 offset = new Vector3(-mazeActualWidth * 0.5f, -mazeActualHeight * 0.5f, 0f);
                Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
                worldPos = roomCenter + offset + new Vector3(leverPosition.x * cellSize, leverPosition.y * cellSize, 0f) + cellCenterOffset;
                
            }
        }
        else
        {
            // Grid를 찾지 못하면 기존 방식 사용
            Vector3 roomCenter = GetRoomActualCenter(parent);
            float mazeActualWidth = mazeWidth * cellSize;
            float mazeActualHeight = mazeHeight * cellSize;
            Vector3 offset = new Vector3(-mazeActualWidth * 0.5f, -mazeActualHeight * 0.5f, 0f);
            Vector3 cellCenterOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
            worldPos = roomCenter + offset + new Vector3(leverPosition.x * cellSize, leverPosition.y * cellSize, 0f) + cellCenterOffset;
            
        }
        
        GameObject lever = Instantiate(leverPrefab, worldPos, Quaternion.identity, interactiveContainer);

        // TrapRoomController와 연동되는 레버 스크립트 추가/초기화
        TrapRoomController trapRoomController = parent != null ? parent.GetComponent<TrapRoomController>() : null;
        if (trapRoomController == null && parent != null)
        {
            trapRoomController = parent.GetComponentInParent<TrapRoomController>();
        }

        if (trapRoomController != null)
        {
            TrapRoomLever leverLogic = lever.GetComponent<TrapRoomLever>();
            if (leverLogic == null)
            {
                leverLogic = lever.AddComponent<TrapRoomLever>();
            }
            // 레버에 보물상자 프리팹과 확률을 전달 (상호작용 시 사용)
            leverLogic.Initialize(trapRoomController, treasureChestPrefab, treasureChance);
        }

    }
    
    /// <summary>
    /// 레버 위치 반환 (외부 접근용)
    /// </summary>
    public Vector2Int GetLeverPosition()
    {
        return leverPosition;
    }
    
    /// <summary>
    /// 함정 위치 목록 반환 (외부 접근용)
    /// </summary>
    public List<Vector2Int> GetTrapPositions()
    {
        return trapPositions;
    }

    /// <summary>
    /// 함정을 ON/OFF 상태로 전환합니다.
    /// trapPrefab / trapOffPrefab이 모두 설정되어 있으면 프리팹을 갈아끼우고,
    /// 그렇지 않으면 기존 함정 오브젝트의 활성/비활성만 전환합니다.
    /// </summary>
    public void SetTrapsActive(bool isOn)
    {
        if (trapObjects == null || trapObjects.Count == 0)
        {
            Debug.LogWarning($"[TrapRoomMazeGenerator] SetTrapsActive({isOn}) 호출됐지만 trapObjects가 비어 있습니다. 방:{name}");
            return;
        }

        // ON/OFF 프리팹이 모두 설정된 경우: 프리팹 갈아끼우기
        if (trapPrefab != null && trapOffPrefab != null)
        {
            GameObject targetPrefab = isOn ? trapPrefab : trapOffPrefab;
            Debug.Log($"[TrapRoomMazeGenerator] SetTrapsActive({isOn}) - 프리팹 교체, target:{targetPrefab.name}, count:{trapObjects.Count}");
            for (int i = 0; i < trapObjects.Count; i++)
            {
                GameObject oldTrap = trapObjects[i];
                if (oldTrap == null) continue;

                Transform parent = oldTrap.transform.parent;
                Vector3 pos = oldTrap.transform.position;
                Quaternion rot = oldTrap.transform.rotation;

                Object.Destroy(oldTrap);

                GameObject newTrap = Object.Instantiate(targetPrefab, pos, rot, parent);
                trapObjects[i] = newTrap;
            }
        }
        else
        {
            Debug.Log($"[TrapRoomMazeGenerator] SetTrapsActive({isOn}) - 프리팹 한 종류만 있어 active 토글, count:{trapObjects.Count}");
            // 한 종류의 프리팹만 있는 경우: 활성/비활성 토글만
            foreach (var trap in trapObjects)
            {
                if (trap != null)
                {
                    trap.SetActive(isOn);
                }
            }
        }
    }
}
