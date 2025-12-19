using UnityEngine;
using UnityEngine.UI;

public class StatBarView : MonoBehaviour
{
    [SerializeField] private Slider _staminaSlider;
    [SerializeField] private Slider _hpSlider;

    private StatBarPresenter _presenter;

    private void Start()
    {
        if(GameManager.Instance.player != null)
            _presenter = new StatBarPresenter(GameManager.Instance.player, this);
    }

    public void SetStaminaSlider(float maxValue, float currentValue)
    {
        SetSliderValue(_staminaSlider, maxValue, currentValue);
    }

    public void SetHpSlider(float maxValue, float currentValue)
    {
        SetSliderValue(_hpSlider, maxValue, currentValue);
    }

    private void SetSliderValue(Slider slider, float maxValue, float currentValue)
    {
        slider.value = currentValue / maxValue;
        Debug.Log(currentValue / maxValue);
    }
}
