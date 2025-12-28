using UnityEngine;
using System.Collections.Generic;

public class DataManager_Character : MonoBehaviour
{
    [field: SerializeField] public CharacterDatabaseSO CharacterDatabase { get; private set; }

    public float _playerHp;         // 플레이어 체력
    public float _playerStamina;    // 플레이어 스태미나
    public float _bonusAtk;         // 보물로 인한 추가 공격력

    public List<Treasure> _treasureList = new List<Treasure>();     // 플레이어가 획득했던 보물들
    public Weapon _subWeapon;                                       // 플레이어가 장착중인 서브 무기

    public void AddTreasureData(Treasure treasure)
    {
        _treasureList.Add(treasure);
        _bonusAtk += treasure.TreasureData.damageBoost;
        Debug.Log($"현재 보너스 공격력: {_bonusAtk}");
    }
}
