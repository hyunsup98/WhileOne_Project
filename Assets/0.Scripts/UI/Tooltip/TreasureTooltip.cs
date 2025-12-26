using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureTooltip : MonoBehaviour
{
    [SerializeField] private GameObject _treasureTooltip;

    [SerializeField] private Image _treasureIcon;
    [SerializeField] private TMP_Text _treasureName;
    [SerializeField] private TMP_Text _treasureStat;
    [SerializeField] private TMP_Text _treasureDesc;

    public void HideTooltip() => _treasureTooltip.SetActive(false);

    public void ShowTooltip(Treasure treasure)
    {
        if (_treasureTooltip == null || treasure == null) return;

        _treasureTooltip.SetActive(true);

        _treasureIcon.sprite = treasure.TreasureData.treasureIconPath_Sprite;
        _treasureName.text = treasure.TreasureData.treasure_Name;
        
        if(treasure.TreasureData.damageBoost > 0)
        {
            _treasureStat.text = $"°ø°Ý·Â + {treasure.TreasureData.damageBoost}";
        }
        else
        {
            _treasureStat.text = string.Empty;
        }

        _treasureDesc.text = $"{treasure.TreasureData.treasure_Desc}";
    }
}
