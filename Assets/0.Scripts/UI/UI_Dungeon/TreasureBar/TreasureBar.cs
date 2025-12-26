using UnityEngine;

public class TreasureBar : MonoBehaviour
{
    [SerializeField] private Transform _contentTrans;       // º¸¹° ½½·ÔÀÌ »ý¼ºµÉ Æ®·£½ºÆû
    [SerializeField] private TreasureSlot _treasureSlot;    // º¸¹° ½½·Ô ÇÁ¸®ÆÕ

    public void AddTreasure(TreasureDataSO data)
    {
        if (data == null) return;

        var slot = Instantiate(_treasureSlot, _contentTrans);
        slot.SetIcon(data);
        slot.transform.SetAsFirstSibling();
    }
}
