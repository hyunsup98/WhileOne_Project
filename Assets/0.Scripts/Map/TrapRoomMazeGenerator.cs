using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 함정방 미로 생성기
/// 기획서 요구사항에 따라 미로, 함정, 레버를 배치합니다.
/// </summary>
public class TrapRoomMazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] private int mazeWidth = 15; // 미로 가로 크기 (타일 수)
    [SerializeField] private int mazeHeight = 15; // 미로 세로 크기 (타일 수)
    
    [Header("Trap Settings")]
    [SerializeField] private GameObject trapPrefab; // 함정 프리펩 (1x1)
    [SerializeField] private float trapAttackInterval = 2f; // 함정 공격 간격 (초)
    [SerializeField] private float maxTrapRatio = 0.3f; // 함정 최대 비율 (30%)
    [SerializeField] private int safeZoneRadius = 2; // 입구/출구 주변 안전 구역 (칸)
    
    [Header("Lever Settings")]
    [SerializeField] private GameObject leverPrefab; // 레버 프리펩
    [SerializeField] private GameObject treasureChestPrefab; // 보물상자 프리펩 (10% 확률)
    [SerializeField] private float treasureChance = 0.1f; // 보물상자 등장 확률 (10%)
    
    [Header("Maze Prefabs")]
    [SerializeField] private GameObject floorTilePrefab; // 바닥 타일 프리펩
    [SerializeField] private GameObject wallTilePrefab; // 벽 타일 프리펩
    
    // 미로 그리드 (true = 통로, false = 벽)
    private bool[,] mazeGrid;
    
    // 함정 위치
    private List<Vector2Int> trapPositions;
    
    // 레버 위치
    private Vector2Int leverPosition;
    
    // 입구 위치 (문이 있는 방향)
    private Vector2Int entryPosition;
    
    // 방 크기 (월드 단위)
    private float roomSize;
    private float cellSize;
    
    /// <summary>
    /// 함정방 미로를 생성합니다.
    /// </summary>
    public void GenerateMaze(float roomSize, float cellSize, Vector2Int entryDirection, Transform parent)
    {
        this.roomSize = roomSize;
        this.cellSize = cellSize;
        
        // 미로 크기 계산 (방 크기를 타일 수로 변환)
        mazeWidth = Mathf.RoundToInt(roomSize / cellSize);
        mazeHeight = Mathf.RoundToInt(roomSize / cellSize);
        
        // 미로 그리드 초기화 (모두 벽으로 시작)
        mazeGrid = new bool[mazeWidth, mazeHeight];
        trapPositions = new List<Vector2Int>();
        
        // 1. 미로 생성 (DFS 기반)
        GenerateMazePaths();
        
        // 2. 입구 위치 결정 (문이 있는 방향의 가장자리)
        entryPosition = DetermineEntryPosition(entryDirection);
        
        // 3. 레버 위치 결정 (모서리에 최대한 가깝게)
        leverPosition = DetermineLeverPosition();
        
        // 4. 입구에서 레버까지 경로 보장
        EnsurePathToLever();
        
        // 5. 함정 배치
        PlaceTraps();
        
        // 6. 실제 오브젝트 생성
        CreateMazeObjects(parent);
        CreateTraps(parent);
        CreateLever(parent);
    }
    
    /// <summary>
    /// DFS 기반 미로 생성
    /// </summary>
    private void GenerateMazePaths()
    {
        // DFS 스택
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        // 시작점 (중앙 근처)
        Vector2Int start = new Vector2Int(mazeWidth / 2, mazeHeight / 2);
        stack.Push(start);
        visited.Add(start);
        mazeGrid[start.x, start.y] = true;
        
        // DFS로 미로 생성
        Vector2Int[] directions = {
            new Vector2Int(0, 1),  // 위
            new Vector2Int(0, -1), // 아래
            new Vector2Int(1, 0),  // 오른쪽
            new Vector2Int(-1, 0)  // 왼쪽
        };
        
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = new List<Vector2Int>();
            
            // 방문하지 않은 인접 셀 찾기
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir * 2; // 2칸씩 이동 (벽 하나 건너뛰기)
                
                if (IsValidCell(next) && !visited.Contains(next))
                {
                    neighbors.Add(next);
                }
            }
            
            if (neighbors.Count > 0)
            {
                // 랜덤하게 선택
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                Vector2Int wall = current + (chosen - current) / 2;
                
                // 벽 제거 (통로 만들기)
                mazeGrid[wall.x, wall.y] = true;
                mazeGrid[chosen.x, chosen.y] = true;
                
                visited.Add(chosen);
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
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
    /// 레버 위치 결정 (모서리에 최대한 가깝게)
    /// </summary>
    private Vector2Int DetermineLeverPosition()
    {
        // 4개 모서리 후보
        List<Vector2Int> cornerCandidates = new List<Vector2Int>
        {
            new Vector2Int(1, 1),                    // 왼쪽 아래 모서리
            new Vector2Int(mazeWidth - 2, 1),        // 오른쪽 아래 모서리
            new Vector2Int(1, mazeHeight - 2),      // 왼쪽 위 모서리
            new Vector2Int(mazeWidth - 2, mazeHeight - 2) // 오른쪽 위 모서리
        };
        
        // 입구에서 가장 먼 모서리 선택
        Vector2Int farthestCorner = cornerCandidates[0];
        float maxDistance = 0f;
        
        foreach (var corner in cornerCandidates)
        {
            float dist = Vector2Int.Distance(entryPosition, corner);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                farthestCorner = corner;
            }
        }
        
        // 모서리 근처에서 통로가 있는 위치 찾기
        Vector2Int leverPos = farthestCorner;
        
        // 주변에서 통로 찾기
        for (int radius = 0; radius < 3; radius++)
        {
            for (int x = Mathf.Max(1, farthestCorner.x - radius); x <= Mathf.Min(mazeWidth - 2, farthestCorner.x + radius); x++)
            {
                for (int y = Mathf.Max(1, farthestCorner.y - radius); y <= Mathf.Min(mazeHeight - 2, farthestCorner.y + radius); y++)
                {
                    if (mazeGrid[x, y])
                    {
                        leverPos = new Vector2Int(x, y);
                        goto FoundLever;
                    }
                }
            }
        }
        
        FoundLever:
        // 레버 주변도 통로로 만들기 (안전 구역)
        for (int x = Mathf.Max(0, leverPos.x - 1); x <= Mathf.Min(mazeWidth - 1, leverPos.x + 1); x++)
        {
            for (int y = Mathf.Max(0, leverPos.y - 1); y <= Mathf.Min(mazeHeight - 1, leverPos.y + 1); y++)
            {
                mazeGrid[x, y] = true;
            }
        }
        
        return leverPos;
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
    /// 함정 배치
    /// </summary>
    private void PlaceTraps()
    {
        // 이동 가능한 타일 수 계산
        int walkableTiles = 0;
        List<Vector2Int> walkablePositions = new List<Vector2Int>();
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (mazeGrid[x, y])
                {
                    walkableTiles++;
                    walkablePositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        // 최대 함정 개수 (30%)
        int maxTraps = Mathf.RoundToInt(walkableTiles * maxTrapRatio);
        
        // 함정 배치 가능한 위치 필터링
        List<Vector2Int> validTrapPositions = new List<Vector2Int>();
        
        foreach (var pos in walkablePositions)
        {
            // 입구/출구 안전 구역 제외 (맨해튼 거리)
            int distToEntry = Mathf.Abs(pos.x - entryPosition.x) + Mathf.Abs(pos.y - entryPosition.y);
            int distToLever = Mathf.Abs(pos.x - leverPosition.x) + Mathf.Abs(pos.y - leverPosition.y);
            
            if (distToEntry <= safeZoneRadius || distToLever <= safeZoneRadius)
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
        
        // 함정 배치 (연속되지 않게, 경로 유지)
        HashSet<Vector2Int> placedTraps = new HashSet<Vector2Int>();
        int trapCount = 0;
        
        // 위치를 랜덤하게 섞기
        List<Vector2Int> shuffled = validTrapPositions.OrderBy(x => Random.value).ToList();
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        foreach (var pos in shuffled)
        {
            if (trapCount >= maxTraps)
                break;
            
            // 인접한 함정이 있는지 확인
            bool hasAdjacentTrap = false;
            foreach (var dir in directions)
            {
                Vector2Int neighbor = pos + dir;
                if (placedTraps.Contains(neighbor))
                {
                    hasAdjacentTrap = true;
                    break;
                }
            }
            
            // 인접한 함정이 없으면 배치 시도
            if (!hasAdjacentTrap)
            {
                // 임시로 함정 배치하고 경로 확인
                placedTraps.Add(pos);
                
                // 경로가 여전히 존재하는지 확인 (함정은 통과 불가능하므로)
                if (HasPathWithTraps(entryPosition, leverPosition, placedTraps))
                {
                    // 경로가 유지되면 함정 배치
                    trapPositions.Add(pos);
                    trapCount++;
                }
                else
                {
                    // 경로가 끊어지면 함정 제거
                    placedTraps.Remove(pos);
                }
            }
        }
        
        Debug.Log($"[TrapRoomMazeGenerator] 함정 배치 완료: {trapCount}개 (최대: {maxTraps}개)");
    }
    
    /// <summary>
    /// 함정을 고려한 경로 확인 (BFS)
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
                return true;
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
        
        return false;
    }
    
    /// <summary>
    /// 미로 오브젝트 생성 (바닥/벽)
    /// </summary>
    private void CreateMazeObjects(Transform parent)
    {
        if (floorTilePrefab == null || wallTilePrefab == null)
        {
            Debug.LogWarning("바닥/벽 프리펩이 설정되지 않았습니다.");
            return;
        }
        
        Vector3 roomCenter = transform.position;
        Vector3 offset = new Vector3(-roomSize * 0.5f, -roomSize * 0.5f, 0f);
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 worldPos = roomCenter + offset + new Vector3(x * cellSize, y * cellSize, 0f);
                
                if (mazeGrid[x, y])
                {
                    // 통로 = 바닥
                    Instantiate(floorTilePrefab, worldPos, Quaternion.identity, parent);
                }
                else
                {
                    // 벽
                    Instantiate(wallTilePrefab, worldPos, Quaternion.identity, parent);
                }
            }
        }
    }
    
    /// <summary>
    /// 함정 오브젝트 생성
    /// </summary>
    private void CreateTraps(Transform parent)
    {
        if (trapPrefab == null)
        {
            Debug.LogWarning("함정 프리펩이 설정되지 않았습니다.");
            return;
        }
        
        Vector3 roomCenter = transform.position;
        Vector3 offset = new Vector3(-roomSize * 0.5f, -roomSize * 0.5f, 0f);
        
        foreach (var trapPos in trapPositions)
        {
            Vector3 worldPos = roomCenter + offset + new Vector3(trapPos.x * cellSize, trapPos.y * cellSize, 0f);
            GameObject trap = Instantiate(trapPrefab, worldPos, Quaternion.identity, parent);
            
            // 함정 공격 간격 설정 (컴포넌트가 있다면)
            // Trap 컴포넌트가 있다면 설정
        }
    }
    
    /// <summary>
    /// 레버 오브젝트 생성
    /// </summary>
    private void CreateLever(Transform parent)
    {
        if (leverPrefab == null)
        {
            Debug.LogWarning("레버 프리펩이 설정되지 않았습니다.");
            return;
        }
        
        Vector3 roomCenter = transform.position;
        Vector3 offset = new Vector3(-roomSize * 0.5f, -roomSize * 0.5f, 0f);
        Vector3 worldPos = roomCenter + offset + new Vector3(leverPosition.x * cellSize, leverPosition.y * cellSize, 0f);
        
        GameObject lever = Instantiate(leverPrefab, worldPos, Quaternion.identity, parent);
        
        // 10% 확률로 보물상자 생성
        if (Random.value < treasureChance && treasureChestPrefab != null)
        {
            Instantiate(treasureChestPrefab, worldPos, Quaternion.identity, parent);
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
}
