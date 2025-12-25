using UnityEngine;

public class DataManager_Character : MonoBehaviour
{
    [field: SerializeField] public CharacterDatabaseSO CharacterDatabase { get; private set; }
}
