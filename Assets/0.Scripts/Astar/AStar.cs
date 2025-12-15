using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Astar : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    private TilemapGrid map;

    // 좌표에 따른 노드들을 기억해 놓는 Dictionary
    private Dictionary<Vector2Int, Node> nodeMap = new();

    private List<Node> _openList = new();
    private List<Vector2Int> _closedList = new();
    private Vector2Int currentPos;
    private Node current;

    private void Awake()
    {
        current = new Node(currentPos);
        map = new TilemapGrid(tilemap);
    }


    
    public List<Vector2Int> Pathfinder(Vector2Int start, Vector2Int target)
    {
        currentPos = (Vector2Int)tilemap.WorldToCell((Vector3Int)start);

        Node current = GetNode(start);
        current.G = 0;
        current.H = Heuristic(start, target);
            
        _openList.Add(current);

        while (_openList.Count > 0)
        {
            current = GetLowestFNode(_openList);

            if (current.Pos == target)
                return BuildPath(current);

            _openList.Remove(current);
            _closedList.Add(current.Pos);


            foreach (var neighborPos in map.GetNeighbors(current.Pos))
            {
                if (_closedList.Contains(neighborPos))
                    continue;

                int sumG = current.G + map.GetMoveCost(this.current.Pos);

                Node neighborNode = GetNode(neighborPos);

                if (!_openList.Contains(neighborNode))
                {
                    neighborNode.SetNode
                        (
                        sumG,
                        Heuristic(this.current.Pos, target),
                        current
                        );

                    _openList.Add(neighborNode);
                }
                else if (sumG < neighborNode.G)
                {
                    neighborNode.G = sumG;
                    neighborNode.Parent = current;
                }
            }
        }
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
        return path;
    }


    // 셀 좌표에 해당하는 노드 생성 메서드
    private Node GetNode(Vector2Int cellPos)
    {
        if (!nodeMap.TryGetValue(cellPos, out var node))
        {
            // 셀좌표에 해당하는 노드가 없다면 새로 생성
            node = new Node(cellPos);
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