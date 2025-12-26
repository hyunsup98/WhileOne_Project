using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponDatabaseSO", menuName = "Scriptable Objects/DataBase/WeaponDatabaseSO")]
public class WeaponDatabaseSO : TableDatabase<int, WeaponDataSO>
{

}
