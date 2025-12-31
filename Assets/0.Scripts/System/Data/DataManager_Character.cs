using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class DataManager_Character : MonoBehaviour
{
    [field: SerializeField] public CharacterDatabaseSO CharacterDatabase { get; private set; }

    public float _playerHp;         // 플레이어 체력
    public float _playerStamina;    // 플레이어 스태미나
    public float _bonusAtk;         // 보물로 인한 추가 공격력

    public List<Treasure> _treasureList = new List<Treasure>();     // 플레이어가 획득했던 보물들
    public Weapon _subWeapon;                                       // 플레이어가 장착중인 서브 무기

    private Coroutine _getTreasure;

    public void AddTreasureData(Treasure treasure)
    {
        _treasureList.Add(treasure);
        _bonusAtk += treasure.TreasureData.damageBoost;
        PlayGetTreasureSound();
    }

    // 데이터 초기화
    public void InitPlayerData()
    {
        _playerHp = 100;
        _playerStamina = 100;
        _bonusAtk = 0;

        _treasureList.Clear();
        _subWeapon = null;

        DataManager.Instance.TreasureData.InitTreasureList();
    }

    public void PlayGetTreasureSound()
    {
        if (_getTreasure != null)
            StopCoroutine(_getTreasure);

        _getTreasure = StartCoroutine(GetTreasure());
    }

    private IEnumerator GetTreasure()
    {
        yield return CoroutineManager.waitForSeconds(0.45f);

        SoundManager.Instance.PlaySoundEffect("Shovel_TreasureGet_FX_001");
    }
}
