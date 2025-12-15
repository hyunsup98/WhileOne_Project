using UnityEngine;


// 경로 탐색 시에 탐색할(or탐색한) 셀 좌표 구간을 Node로 지정한다.
// 현재 타일에서 계산을 통해 다음 노드를 찾아나간다.
public class Node
{
    public Vector2Int Pos;   // 타일맵 셀 좌표
    public int G;            // 시작 → 현재의 거리적 비용
    public int H;            // 현재 → 목표의 휴리스틱
    public int F => G + H;   // 총 비용
    public Node Parent;      // 경로 추적용 부모 노드

    public Node(Vector2Int pos)
    {
        Pos = pos;
    }

    public void SetNode(int g, int h, Node parent)
    {
        G = g;
        H = h;
        Parent = parent;
    }
}
