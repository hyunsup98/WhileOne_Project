using UnityEngine;

public class StatBarPresenter
{
    private Player _model;
    private StatBarView _view;

    public StatBarPresenter(Player model,  StatBarView view)
    {
        _model = model;
        _view = view;

        // todo: model과 view 결합 코드가 들어갈 곳
    }
}
