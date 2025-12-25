using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MonsterDatabaseSO", menuName = "Scriptable Objects/DataBase/MonsterDatabaseSO")]
public class MonsterDatabaseSO : TableDatabase<int, MonsterDataSO>
{

}
