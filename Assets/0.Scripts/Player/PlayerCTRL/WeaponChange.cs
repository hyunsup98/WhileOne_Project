using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponChange : MonoBehaviour
{
    [SerializeField] Transform _weaponHands; //무기 들 손 위치
    private GameObject _currentWeapon; //현재 활성화 되어 있는 오브젝트

    //private SO _currentWeaponData; //SO 들어갈 자리
    //Dictionary<SO, GameObject> weaponList = new(); //SO 키 값
    //private SO slotWeapon1
    //private SO slotWeapon2

    PlayerInput _input;
    InputActionMap _inputActionMap;
    InputAction _switchWeapon1;
    InputAction _switchWeapon2;
    Player _player;


    private void Start()
    {
        _input = _player.Playerinput;
        _inputActionMap = _input.actions.FindActionMap("Player");
        _switchWeapon1 = _inputActionMap.FindAction("Previous");
        _switchWeapon2 = _inputActionMap.FindAction("Next");

        _switchWeapon1.performed += WeaponSwitch1;
        _switchWeapon2.performed += WeaponSwitch2;
    }

    private void WeaponSwitch1(InputAction.CallbackContext ctx)
    {
        SwitchSlot(1);
    }
    private void WeaponSwitch2(InputAction.CallbackContext ctx)
    {
        SwitchSlot(2);
    }

    private void SwitchSlot(int slotNum)
    {
        //SO targetData = (slotNum == 1) ? slotWeapon1 : slotWeapon2;
        //EquipWeapon(targetData);
    }


    public void ChangeWeapon()//스크립터블 오브젝트 넣는 곳)
    {
        //if(_currentWeaponData == 대충 매개변수로 받아온 변수) //스크립터블과 비교하는 곳
        //return;

        if(_currentWeapon != null) //만약 기존에 데이터가 있다면 
        {
            _currentWeapon.SetActive(false); //해당 데이터는 비활성화
        }

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
    private void OnDisable()
    {
        _switchWeapon1.performed -= WeaponSwitch1;
        _switchWeapon2.performed -= WeaponSwitch2;
    }
}
