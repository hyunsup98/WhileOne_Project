using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Astar
{
    private Tilemap _wallTilemap;
    private NodeManager _nodeManager;

    private List<Node> _openList;
    private Dictionary<Vector2Int, Node> _openDict;
    private HashSet<Vector2Int> _closedSet;

    public Astar(Tilemap wallTilemap)
    {
        _wallTilemap = wallTilemap;
        _nodeManager = new NodeManager(wallTilemap);

        _openList = new List<Node>();
        _openDict = new Dictionary<Vector2Int, Node>();
        _closedSet = new HashSet<Vector2Int>();
    }


    // 경로 탐색 알고리즘
    public List<Vector2> Pathfinder(Vector3 start, Vector3 target)
    {
        Vector2Int startCellPos = (Vector2Int)_wallTilemap.WorldToCell(start);
        Vector2Int targetCellPos = (Vector2Int)_wallTilemap.WorldToCell(target);

        Node current = _nodeManager.GetNode(startCellPos, 0, Heuristic(startCellPos, targetCellPos));

        _openList.Add(current);
        _openDict.Add(startCellPos, current);


        while (_openList.Count > 0)
        {
            current = GetLowestFNode(_openList);

            // 타겟에 도달시, BuildPath로 경로 출력
            if (current.Pos == targetCellPos)
                return BuildPath(current);

            _openList.Remove(current);
            _openDict.Remove(current.Pos);
            _closedSet.Add(current.Pos);

            // 갈 수 있는 이웃 셀좌표를 GetNeighbors로 생성 후 순회하며 이웃 노드 추가
            foreach (var neighborPos in _nodeManager.GetNeighbors(current.Pos))
            {
                // 방문 기록이 있다면 다음 이웃 노드 확인
                if (_closedSet.Contains(neighborPos))
                    continue;

                int sumG = current.G + _nodeManager.GetMoveCost(current.Pos);

                // _openDict에 이웃 노드 정보가 없으면 이웃 노드 생성 및 추가
                if (!_openDict.TryGetValue(neighborPos, out Node neighborNode))
                {
                    neighborNode = _nodeManager.GetNode
                    (
                    neighborPos,
                    sumG,
                    Heuristic(neighborPos, targetCellPos),
                    current
                    );

                    _openList.Add(neighborNode);
                    _openDict.Add(neighborPos, neighborNode);

                }
                else if (sumG < neighborNode.G)
                {
                    neighborNode.G = sumG;
                    neighborNode.Parent = current;
                }
            }
        }

        Init();
        Debug.Log("경로 탐색 실패");
        return null;
    }


    // 최종 최적경로 출력 메서드
    List<Vector2> BuildPath(Node targetNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = targetNode;

        while (current != null)
        {
            // 타일 중앙 좌표로 변환
            path.Add(GetCellPos(current.Pos));
            current = current.Parent;
        }

        path.Reverse();

        //foreach (var a in path)
        //    Debug.Log("최적 경로 노드: " + a);

        Init();
        return path;
    }

    // 오픈리스트에서 F 비용이 가장 적은 노드를 반환
    private Node GetLowestFNode(List<Node> openList)
    {
        Node best = openList[0];

        foreach (var node in openList)
        {
            if (node.F < best.F || (node.F == best.F && node.H < best.H))
                best = node;
        }

        return best;
    }

    // 휴리스틱 예측치 계산
    private int Heuristic(Vector2Int current, Vector2Int target) =>
        (Mathf.Abs(target.x - current.x) + Mathf.Abs(target.y - current.y)) * 10;


    // 타일의 중앙으로 벡터값을 변경하는 오버로딩
    public Vector2 GetCellPos(Vector3 pos) => CellPos(pos);
    public Vector2 GetCellPos(Vector2 pos) => CellPos(pos);
    public Vector2 GetCellPos(Vector2Int pos) => CellPos(pos);

    private Vector2 CellPos(Vector2 pos) => 
        _wallTilemap.GetCellCenterWorld(Vector3Int.FloorToInt(pos));

    private void Init()
    {
        _openList.Clear();
        _openDict.Clear();
        _closedSet.Clear();
        _nodeManager.Init();
    }

}