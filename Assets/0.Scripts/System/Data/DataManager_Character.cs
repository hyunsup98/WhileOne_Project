using UnityEngine;
using System.Collections.Generic;

public class DataManager_Character : MonoBehaviour
{
    [field: SerializeField] public CharacterDatabaseSO CharacterDatabase { get; private set; }

    public float _playerHp;
    public float _playerStamina;

    public List<Treasure> _treasureList = new List<Treasure>();
    public Weapon _subWeapon;
}
