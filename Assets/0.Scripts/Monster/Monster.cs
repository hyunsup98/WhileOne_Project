using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
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


}


public enum MonsterState
{
    Patrol, Chase, Search
}