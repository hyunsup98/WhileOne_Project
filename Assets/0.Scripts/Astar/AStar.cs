using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Astar : MonoBehaviour
{

    public Transform Target;


    [SerializeField] private Tilemap _wallTilemap;

    private TilemapGrid _map;

    // 좌표에 따른 노드들을 기억해 놓는 Dictionary
    private Dictionary<Vector2Int, Node> nodeMap = new();

    private List<Node> _openList = new();
    private List<Vector2Int> _closedList = new();
    private Node current;

    private void Awake()
    {
        _map = new TilemapGrid(_wallTilemap);
    }

    private void Start()
    {
        Vector2Int start = (Vector2Int)_wallTilemap.WorldToCell(transform.position);
        Vector2Int target = (Vector2Int)_wallTilemap.WorldToCell(Target.transform.position);
        Debug.Log("시작: " + start);
        Debug.Log("목표: " + target);
        
        Pathfinder(start, target);
    }



    public List<Vector2Int> Pathfinder(Vector2Int start, Vector2Int target)
    {
        //currentPos = (Vector2Int)tilemap.WorldToCell((Vector3Int)start);

        Node current = GetNode(start, 0, Heuristic(start, target));
            
        _openList.Add(current);

        while (_openList.Count > 0)
        {
            current = GetLowestFNode(_openList);

            if (current.Pos == target)
                return BuildPath(current);

            _openList.Remove(current);
            _closedList.Add(current.Pos);


            foreach (var neighborPos in _map.GetNeighbors(current.Pos))
            {
                if (_closedList.Contains(neighborPos))
                    continue;

                int sumG = current.G + _map.GetMoveCost(current.Pos);

                Node neighborNode = GetNode
                    (
                    neighborPos, 
                    sumG, 
                    Heuristic(neighborPos, target), 
                    current
                    );

                if (!_openList.Contains(neighborNode))
                {
                    _openList.Add(neighborNode);
                }
            }

            _map.Init();
        }

        Debug.Log("경로 탐색 실패");
        return null;
        
    }


    List<Vector2Int> BuildPath(Node targetNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        Node current = targetNode;

        while (current != null)
        {
            path.Add(current.Pos);
            current = current.Parent;
        }

        path.Reverse();

        foreach (var a in path)
            Debug.Log("최적 경로 노드: " + a);

        return path;
    }


    // 셀 좌표에 해당하는 노드 생성 메서드
    private Node GetNode(Vector2Int cellPos, int g, int h, Node parent = null)
    {
        if (!nodeMap.TryGetValue(cellPos, out var node))
        {
            // 셀좌표에 해당하는 노드가 없다면 새로 생성
            node = new Node(cellPos, g, h, parent);
            nodeMap[cellPos] = node;
        }
        return node;
    }

    // 오픈리스트에서 F 비용이 가장 적은 노드를 반환
    private Node GetLowestFNode(List<Node> openList)
    {
        Node best = null;
        int bestF = int.MaxValue;

        foreach (var node in openList)
        {
            if (node.F < bestF)
            {
                bestF = node.F;
                best = node;
            }
        }
        return best;
    }

    private int Heuristic(Vector2Int current, Vector2Int target) =>
        (Mathf.Abs(target.x - current.x) + Mathf.Abs(target.y - current.y)) * 10;




}