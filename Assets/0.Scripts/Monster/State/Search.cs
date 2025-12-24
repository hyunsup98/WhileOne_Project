using System.Collections;
using System.Threading;
using UnityEngine;


public class Search : IState
{
    private MonsterPresenter _monster;
    private Transform _myTransform;
    private float _sight;
    private float _searchTime;
    private float _timer;
    private MonsterView _view;
    private Transform _searchImage;



    public Search(MonsterPresenter monster)
    {
        _monster = monster;
        _myTransform = monster.View.MyTransform;
        _sight = monster.Model.Sight;
        _searchTime = monster.Model.SearchTime;
        _view = monster.View;
        _searchImage = monster.View.MyTransform.Find("Search");
    }


    public void Enter()
    {
        _view.OnIdleAni();

        if(_searchImage != null)
            _searchImage.gameObject.SetActive(true);
    }

    public void Exit()
    {
        _timer = 0f;
        _view.OnDisIdleAni();

        if(_searchImage != null )
            _searchImage.gameObject.SetActive(false);
    }

    public void Update() 
    {
        OnSearch();
    }


    private void OnSearch()
    {
        _timer += Time.unscaledDeltaTime;

        Vector2 dir = _monster.Model.ChaseTarget.position - _myTransform.position;
        Debug.DrawRay(_myTransform.position, dir.normalized * _sight, Color.brown);
        if(_timer < _searchTime)
        {
            if(_monster.OnSight())
                _monster.Model.SetState(MonsterState.Chase);
            return;
        }

        _monster.Model.SetState(MonsterState.BackReturn);
    }
}
