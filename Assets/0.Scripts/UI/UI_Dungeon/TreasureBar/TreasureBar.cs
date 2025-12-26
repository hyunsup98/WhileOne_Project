using UnityEngine;
using UnityEngine.InputSystem;

public class TreasureBar : MonoBehaviour
{
    [SerializeField] private Transform _contentTrans;       // º¸¹° ½½·ÔÀÌ »ý¼ºµÉ Æ®·£½ºÆû
    [SerializeField] private TreasureSlot _treasureSlot;    //º¸¹° ½½·Ô ÇÁ¸®ÆÕ


    public void AddTreasure(Treasure treasure)
    {
        var slot = Instantiate(_treasureSlot, _contentTrans);
        slot.SetIcon(treasure);
        slot.transform.SetAsFirstSibling();
    }
}
