using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeManager
{
    private Tilemap _wallTilemap;
    private List<Vector2Int> _neighborDic;

    // 좌표에 따른 노드들을 기억해 놓는 Dictionary
    private Dictionary<Vector2Int, Node> _nodeMap;

    public NodeManager(Tilemap wall)
    {
        _wallTilemap = wall;
        _neighborDic = new List<Vector2Int>();
        _nodeMap = new Dictionary<Vector2Int, Node>();
    }

    // 이동이 가능한 cell인지를 검사
    public bool IsWalkable(Vector2Int cellPos) => !_wallTilemap.HasTile((Vector3Int)cellPos);

    // 해당 셀의 코스트(G값)를 반환
    public int GetMoveCost(Vector2Int cellPos) => 10;


    // 해당 셀의 이웃한 셀(상하좌우)을 출력
    public Vector2Int[] GetNeighbors(Vector2Int cellPos)
    {
        // 이웃 노드를 임시로 담는 _neighborDic이 항상 초기화가 되어야 함
        _neighborDic.Clear();

        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
        };

        // 이웃한 타일이 갈 수 있는 타일인지 검사
        foreach (var dir in directions)
        {
            Vector2Int next = cellPos + dir;

            if (!IsWalkable(next))
                continue;

            _neighborDic.Add(next);
        }
        return _neighborDic.ToArray();
    }

    // 셀좌표에 해당하는 노드 생성
    public Node GetNode(Vector2Int cellPos, int g, int h, Node parent = null)
    {
        if (!_nodeMap.TryGetValue(cellPos, out var node))
        {
            // 셀좌표에 해당하는 노드가 없다면 새로 생성
            node = new Node(cellPos, g, h, parent);
            _nodeMap[cellPos] = node;
        }
        else if (g < node.G)
        {
            node.G = g;
            node.H = h;
            node.Parent = parent;
        }
        return node;
    }

    public void Init()
    {
        _nodeMap.Clear();
        _neighborDic.Clear();
    }
}