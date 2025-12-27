using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponChange : MonoBehaviour
{
    [SerializeField] Transform _weaponHands; //무기 들 손 위치
    private GameObject _currentWeapon; //현재 활성화 되어 있는 오브젝트
    AttackDamage _decision;

    //private SO _currentWeaponData; //SO 들어갈 자리
    //Dictionary<SO, GameObject> weaponList = new(); //SO 키 값
    //public SO slotWeapon1
    //private SO slotWeapon2


    public Weapon currentweapon;
    private Dictionary<Weapon, GameObject> weaponList = new();

    private Weapon _slotWeapon1;        // 메인 무기 → 삽(변경될 일 없음)
    public Weapon _slotWeapon2;         // 서브 무기 → 다양한 무기(변경됨)
    private float _durability;
    private float _weaponDamage;
    private bool _isAlreadyHit;


    PlayerInput _input;
    InputActionMap _inputActionMap;
    InputAction _switchWeapon1;
    InputAction _switchWeapon2;
    Player _player;

    public event Action<int> onSwapWeapon;          // 무기 스왑 이벤트
    public event Action<Weapon> onWeaponChanged;    // 무기 변경 이벤트


    private void Start()
    {
        _player = GetComponent<Player>();

        _input = _player.Playerinput;
        _inputActionMap = _input.actions.FindActionMap("Player");
        _switchWeapon1 = _inputActionMap.FindAction("Previous");
        _switchWeapon2 = _inputActionMap.FindAction("Next");

        _switchWeapon1.performed += WeaponSwitch1;
        _switchWeapon2.performed += WeaponSwitch2;

        _decision.OnHit += HitAble;
    }

    private void WeaponSwitch1(InputAction.CallbackContext ctx)
    {
        SwitchSlot(1);
    }

    private void WeaponSwitch2(InputAction.CallbackContext ctx)
    {
        // 서브 무기에 무기가 없을 경우 무시
        if (_slotWeapon2 == null) return;

        SwitchSlot(2);
    }

    private void SwitchSlot(int slotNum)
    {
        //SO targetData = (slotNum == 1) ? slotWeapon1 : slotWeapon2;
        //EquipWeapon(targetData);

        onSwapWeapon?.Invoke(slotNum);
    }

    public void ChangeWeapon(Weapon weapon)//스크립터블 오브젝트 넣는 곳)
    {
        //if(_currentWeaponData == 대충 매개변수로 받아온 변수) //스크립터블과 비교하는 곳
        //return;

        _slotWeapon2 = weapon;

        if(_currentWeapon != null) //만약 기존에 데이터가 있다면
        {
            _currentWeapon.SetActive(false); //해당 데이터는 비활성화
        }

        onWeaponChanged?.Invoke(_slotWeapon2);

        //if(!weaponeList.Contains(대충 매개변수로 받아온 변수))
        //{
        //    GameObject newWp = Instantiate(매개변수.weapon, _weaponHands);
        //    weaponList.Add(매개변수, newWp);
        //}

        //_currentWeapon = _weaponList[매개변수];
        //_currentWeapon.SetActive(true);
        //_currentWeaponData = 매개변수;

        //_currentWeapon.GetComponent<Weapon>
    }

    //void EquipWeapon(SO data)
    //{
    //    if(_currentWeapon != null)
    //    {
    //        _currentWeapon.SetActive(false);
    //    }
    //    if (!weaponList.ContainsKey(data))
    //    {
    //        GameObject newWp = Instantiate(data.weapon, _weaponHands);
    //        weaponList.Add(data, newWp);
    //    }
    //    _currentWeapon = weaponList[data];
    //    _currentWeapon.SetActive(true);
    //}
    public void ResetAttack()
    {
        _isAlreadyHit = false;
    }
    private void HitAble(GameObject enemy)
    {
        if (enemy.TryGetComponent<MonsterView>(out var monster))
        {
            monster.Presenter.OnHit(_weaponDamage);
        }

        if (!_isAlreadyHit)
        {
            _durability--;
            _isAlreadyHit = true;
            if (_durability <= 0)
            {
                WeaponBreak();
            }
        }
    }
    private void WeaponBreak()
    {
        //무기 뿌사짐
    }
    private void OnDisable()
    {
        _switchWeapon1.performed -= WeaponSwitch1;
        _switchWeapon2.performed -= WeaponSwitch2;
    }
}
