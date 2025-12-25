using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TreasureDatabaseSO", menuName = "Scriptable Objects/DataBase/TreasureDatabaseSO")]
public class TreasureDatabaseSO : TableDatabase<int, TreasureDataSO>
{

}
