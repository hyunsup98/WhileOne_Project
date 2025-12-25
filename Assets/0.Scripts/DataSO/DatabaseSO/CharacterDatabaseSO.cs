using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterDatabaseSO", menuName = "Scriptable Objects/DataBase/CharacterDatabaseSO")]
public class CharacterDatabaseSO : TableDatabase<string, CharacterDataSO>
{

}
