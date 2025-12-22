using UnityEngine;
/// <summary>
/// 이벤트용 함수
/// 발굴 중 플레이어 이동 막기
/// </summary>
public class MoveStopAnimation : MonoBehaviour
{
    private bool _action;
    public bool Action => _action;

    public void StopMove()
    {
        _action = true;
    }
    public void StartMove()
    {
        _action = false;
    }
}
