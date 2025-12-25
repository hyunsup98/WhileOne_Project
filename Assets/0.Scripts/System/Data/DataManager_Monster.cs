using UnityEngine;

public class DataManager_Monster : MonoBehaviour
{
    [field: SerializeField] public MonsterDatabaseSO MonsterDatabase { get; private set; }
    [field: SerializeField] public Monster_Action1DatabaseSO MonsterAction1Database { get; private set; }
    [field: SerializeField] public Monster_Action2DatabaseSO MonsterAction2Database { get; private set; }
}
