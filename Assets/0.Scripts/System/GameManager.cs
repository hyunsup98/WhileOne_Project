using System;
using UnityEngine;

//게임 상태
public enum GameState
{
    Title,      //타이틀 화면 상태
    InGame,     //인게임(플레이중인 상태)
    Pause,      //인게임(팝업 UI등에 의해서 일시중지인 상태)
}

/// <summary>
/// 게임의 전체 상태 및 데이터를 관리하는 클래스
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public event Action<GameState> StateChanged;

    private GameState _currentGameState;
    public GameState _CurrentGameState
    {
        get { return _currentGameState; }
        set
        {
            if (_currentGameState == value) return;

            _currentGameState = value;

            if(_currentGameState == GameState.Title)
            {

            }
            else if(_currentGameState == GameState.InGame)
            {

            }
            else if(_currentGameState == GameState.Pause)
            {

            }
        }
    }

    //게임 상태를 변경하는 메서드
    public void SetGameState(GameState state) => _currentGameState = state;

    protected override void Awake()
    {
        base.Awake();

        _CurrentGameState = GameState.Title;
    }
}
