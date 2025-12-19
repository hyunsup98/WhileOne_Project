using UnityEngine;

public class StatBarPresenter
{
    private Player _model;
    private StatBarView _view;

    public StatBarPresenter(Player model, StatBarView view)
    {
        _model = model;
        _view = view;

        _model._onStaminaChanged += _view.SetStaminaSlider;
        _model._onHpChanged += _view.SetHpSlider;
    }
}
