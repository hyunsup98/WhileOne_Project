using UnityEngine;

public interface IInteractable
{
    public Vector3 Pos { get; }
    public float YOffset { get; set; }
    public void OnInteract();
}
