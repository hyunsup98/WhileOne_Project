using UnityEngine;

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
