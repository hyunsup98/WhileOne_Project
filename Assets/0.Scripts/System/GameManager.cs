using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//게임 상태
public enum GameState
{
    Playing,    // 시간이 흐르고 있는 상태
    Pause,      // 시간이 멈춰있는 상태
    Dead,       // 플레이어가 죽었을 때(데드 엔딩 씬으로 이동)
}

/// <summary>
/// 게임의 전체 상태 및 데이터를 관리하는 클래스
/// </summary>
public class GameManager : Singleton<GameManager>
{
    #region 게임 상태
    public event Action onGameStateInGame;
    public event Action onGameStatePause;
    public event Action onGameStateDead;

    private GameState _currentGameState;
    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        set
        {
            if (_currentGameState == value) return;

            _currentGameState = value;

            if (_currentGameState == GameState.Playing)
            {
                Time.timeScale = 1f;
                onGameStateInGame?.Invoke();
            }
            else if (_currentGameState == GameState.Pause)
            {
                Time.timeScale = 0f;
                onGameStatePause?.Invoke();
            }
            else if (_currentGameState == GameState.Dead)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("GameOver");
                onGameStateDead?.Invoke();
            }
        }
    }
    #endregion

    [field: SerializeField] public Player player { get; set; }                      // [던전 씬에서만 값이 존재]현재 씬에 존재하는 플레이어를 참조하는 변수
    [field: SerializeField] public DungeonManager CurrentDungeon { get; set; }      // [던전 씬에서만 값이 존재]현재 씬에 존재하는 던전을 관리하는 매니저

    private IInteractable _interactObj;
    public IInteractable InteractObj
    {
        get { return _interactObj; }
        set
        {
            _interactObj = value;

            if(CurrentDungeon != null && CurrentDungeon.InteractImg != null)
            {
                // _interactObj에 값이 있으면 상호작용 ui 키고, 값이 없으면 (null이면) 끄기
                if(_interactObj != null)
                    CurrentDungeon.SetPosInteractImg(InteractObj.Pos + new Vector3(0f, InteractObj.YOffset, 0f), _interactObj.InteractText);
                else
                    CurrentDungeon.InteractImg.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if(Keyboard.current.yKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("BossScene");
        }

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene("Dungeon_Floor1");
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene("Dungeon_Floor2");
        }

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene("Dungeon_Floor3");
        }
    }

    public void InitToSceneChanged(Scene scene, LoadSceneMode mode) => InteractObj = null;

    //게임 상태를 변경하는 메서드
    public void SetGameState(GameState state) => CurrentGameState = state;

    protected override void Awake()
    {
        base.Awake();

        CurrentGameState = GameState.Playing;

        SceneManager.sceneLoaded += InitToSceneChanged;

        #region Input System 연결
        OnInteractKey();
        #endregion
    }

    #region Input System 정의
    private void OnInteractKey()
    {
        InputSystem.actions["Interactable"].started += ctx =>
        {
            if (InteractObj != null)
            {
                InteractObj.OnInteract();
            }
        };
    }
    #endregion
}
