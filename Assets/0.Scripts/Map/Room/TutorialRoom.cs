using UnityEngine;

public class TutorialRoom : BaseRoom
{
    void Start()
    {
        GameManager.Instance.CurrentDungeon.CurrentRoom = this.GetComponent<RoomController>();
    }
}
