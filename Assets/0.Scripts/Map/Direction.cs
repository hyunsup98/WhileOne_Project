using UnityEngine;

/// <summary>
/// 방향을 나타내는 유틸리티 클래스
/// </summary>
public static class Direction
{
    public static readonly Vector2Int Up = new Vector2Int(0, 1);
    public static readonly Vector2Int Down = new Vector2Int(0, -1);
    public static readonly Vector2Int Left = new Vector2Int(-1, 0);
    public static readonly Vector2Int Right = new Vector2Int(1, 0);
    
    public static readonly Vector2Int[] All = { Up, Down, Left, Right };
    
    /// <summary>
    /// 랜덤 방향을 반환합니다.
    /// </summary>
    public static Vector2Int Random()
    {
        return All[UnityEngine.Random.Range(0, All.Length)];
    }
    
    /// <summary>
    /// 방향을 반대 방향으로 변환합니다.
    /// </summary>
    public static Vector2Int Opposite(Vector2Int dir)
    {
        return -dir;
    }
    
    /// <summary>
    /// 방향을 문자열로 변환합니다.
    /// </summary>
    public static string ToString(Vector2Int dir)
    {
        if (dir == Up) return "Up";
        if (dir == Down) return "Down";
        if (dir == Left) return "Left";
        if (dir == Right) return "Right";
        return "Unknown";
    }
}

