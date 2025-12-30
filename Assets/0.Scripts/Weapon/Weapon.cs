using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int _keyId;                     // 무기 데이터베이스 SO에서 데이터를 가져오기 위해 필요한 ID
    [SerializeField] private SpriteRenderer _renderer;

    public WeaponDataSO WeaponData { get; private set; }    // 실제 무기 데이터

    #region 변하는 무기 데이터 변수들
    public int Durability { get; private set; }     // 무기 내구도
    #endregion

    private void Awake()
    {
        if (_renderer == null)
            TryGetComponent(out _renderer);

        InitData(DataManager.Instance.WeaponData.WeaponDatabase[_keyId]);
    }

    /// <summary>
    /// 무기 데이터 실체화
    /// </summary>
    /// <param name="data"> 적용할 무기 데이터 </param>
    public void InitData(WeaponDataSO data)
    {
        if (data == null) return;

        WeaponData = data;
        _keyId = WeaponData.weaponID;
        Durability = WeaponData.weaponDurability;
        _renderer.sprite = WeaponData.weaponResourcePath_Sprite;
    }

    // 무기 데이터 세팅
    public void SetWeaponData(WeaponDataSO weaponData)
    {
        WeaponData = weaponData;
        _keyId = WeaponData.weaponID;
        Durability = weaponData.weaponDurability;

        InitData(WeaponData);
    }

    /// <summary>
    /// 내구도 감소
    /// </summary>
    /// <param name="amount"> 내구도 감소 수치 </param>
    public void ReduceDurability(int amount)
    {
        Durability -= amount;

        if (Durability <= 0)
        {
            // 내구도가 다 떨어지면 무기 제거
            transform.SetParent(DataManager.Instance.WeaponData.transform, false);
            WeaponPool.Instance.TakeObject(this);
        }
    }
}
