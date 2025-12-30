using TMPro;
using UnityEngine;

public interface IInteractable
{
    public Vector3 Pos { get; }                 // 상호작용 오브젝트의 현재 위치
    public float YOffset { get; set; }          // 상호작용 이미지가 오브젝트 기준 얼마나 위에 배치할지
    public string InteractText { get; set; }    // 상호작용 이미지 옆에 띄울 문구
    public void OnInteract();

}
