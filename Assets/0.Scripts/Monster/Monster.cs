using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [field: SerializeField] public string Hp {  get; private set; }
    [field: SerializeField] public float Att {  get; private set; }
    [field: SerializeField] public float Vision { get; private set; }
    [field: SerializeField] public float Speed {  get; private set; }
    [field: SerializeField] public int Tier {  get; private set; }
    [field: SerializeField] public List<Transform> PatrolPoint { get; private set; }


    private Dictionary<MonsterState, IMonsterState> _monsterState;
    private IMonsterState _currentState;


    private void Awake()
    {
        _monsterState = new Dictionary<MonsterState, IMonsterState>();
        _monsterState.Add(MonsterState.Patrol, new Patrol(this));
        _monsterState.Add(MonsterState.Chase, new Chase(this));
        _monsterState.Add(MonsterState.Search, new Search(this));
        _currentState = _monsterState[MonsterState.Patrol];
    }
    private void Update()
    {
        _currentState.Update();
    }

    public void SetState(MonsterState state)
    {
        _currentState?.Exit();
        _currentState = _monsterState[state];
        _currentState.Enter();
    }

}


public enum MonsterState
{
    Patrol, Chase, Search
}