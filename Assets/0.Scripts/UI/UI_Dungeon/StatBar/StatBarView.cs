using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatBarView : MonoBehaviour
{
    [SerializeField] private Slider _staminaSlider;
    [SerializeField] private TMP_Text _staminaText;
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TMP_Text _hpText;

    private StatBarPresenter _presenter;

    private void Start()
    {
        if(GameManager.Instance.player != null)
            _presenter = new StatBarPresenter(GameManager.Instance.player, this);
    }

    public void SetStaminaSlider(float maxValue, float currentValue)
    {
        SetSliderValue(_staminaSlider, maxValue, currentValue);
        _staminaText.text = $"{currentValue} / {maxValue}";
    }

    public void SetHpSlider(float maxValue, float currentValue)
    {
        SetSliderValue(_hpSlider, maxValue, currentValue);
        _hpText.text = $"{currentValue} / {maxValue}";
    }

    private void SetSliderValue(Slider slider, float maxValue, float currentValue)
    {
        slider.value = currentValue / maxValue;
    }
}
