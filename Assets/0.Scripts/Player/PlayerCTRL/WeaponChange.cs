using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponChange : MonoBehaviour
{
    [SerializeField] Transform _weaponHands;
    private GameObject _currentWeapon;
    //private SO _currentWeaponData; //SO 들어갈 자리
    //Dictionary<SO, GameObject> weaponList = new(); //SO 키 값

    PlayerInput _input;

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
}
