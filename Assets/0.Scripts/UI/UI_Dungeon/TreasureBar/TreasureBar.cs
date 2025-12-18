using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TreasureBar : MonoBehaviour
{
    [SerializeField] private Transform _contentTrans;       // º¸¹° ½½·ÔÀÌ »ý¼ºµÉ Æ®·£½ºÆû
    [SerializeField] private TreasureSlot _treasureSlot;    //º¸¹° ½½·Ô ÇÁ¸®ÆÕ

    [SerializeField] private Treasure testPrefab;

    private void Update()
    {
        if(Keyboard.current.jKey.wasPressedThisFrame)
        {
            AddTreasure(testPrefab);
        }
    }

    public void AddTreasure(Treasure treasure)
    {
        var slot = Instantiate(_treasureSlot, _contentTrans);
        slot.SetIcon(treasure.icon);
        slot.transform.SetAsFirstSibling();
    }
}
