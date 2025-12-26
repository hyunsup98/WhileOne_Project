using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public abstract void OnPointerEnter(PointerEventData eventData);

    public abstract void OnPointerExit(PointerEventData eventData);
}
