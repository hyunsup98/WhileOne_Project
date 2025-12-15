using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Player : MonoBehaviour
{
    [Header("플레이어 수치 조절 컴포넌트")]
    [SerializeField] private int _hp = 100;
    [SerializeField] private int _stamina = 100;
    [SerializeField] private int _moveSpeed = 3;
    [SerializeField] private int _attack = 5;
    [SerializeField] private int _attackSpeed = 4;

    //현재 상태를 담을 인터페이스 변수
    private IPlayerState currentState;



    //외부에서 사용할 프로퍼티
    public int Hp => _hp;
    public int Stamina => _stamina;
    public int MoveSpeed => _moveSpeed;
    public int Attack => _attack;
    public int AttackSpeed => _attackSpeed;

    void Start()
    {
        
    }

   
    void Update()
    {
        
    }

    public void SetState(IPlayerState state)
    {
        currentState?.OnExit();
        currentState = state;
        currentState.OnEnter();
    }
}
