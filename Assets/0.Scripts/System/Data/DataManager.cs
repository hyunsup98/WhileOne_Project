using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    #region SO 데이터베이스
    [field: SerializeField] public DataManager_BGM BGMData { get; private set; }
    [field: SerializeField] public DataManager_Character CharacterData { get; private set; }
    [field: SerializeField] public DataManager_Monster MonsterData { get; private set; }
    [field: SerializeField] public DataManager_Trap TrapData { get; private set; }
    [field: SerializeField] public DataManager_Treasure TreasureData { get; private set; }
    [field: SerializeField] public DataManager_UI UIData {  get; private set; }
    [field: SerializeField] public DataManager_Weapon WeaponData { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }
}
