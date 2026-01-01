using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponChange : MonoBehaviour
{
    [SerializeField] Transform _weaponHands; //무기 들 손 위치
    private GameObject _currentWeapon; //현재 활성화 되어 있는 오브젝트
    public AttackDamage _decision;

    //private SO _currentWeaponData; //SO 들어갈 자리
    //Dictionary<SO, GameObject> weaponList = new(); //SO 키 값
    //public SO slotWeapon1
    //private SO slotWeapon2


    public Weapon currentweapon;
    private Dictionary<Weapon, GameObject> weaponList = new();

    private Weapon _slotWeapon1;        // 메인 무기 → 삽(변경될 일 없음)
    public Weapon _slotWeapon2;         // 서브 무기 → 다양한 무기(변경됨)
    private float _weaponDamage;
    private bool _isAlreadyHit;

    [SerializeField] private Weapon shovel;

    PlayerInput _input;
    InputActionMap _inputActionMap;
    InputAction _switchWeapon1;
    InputAction _switchWeapon2;
    Player _player;

    public event Action<float> Weaponchanged;

    private void Awake()
    {
        _slotWeapon1 = shovel;
        currentweapon = _slotWeapon1;
    }

    private void Start()
    {
        _player = GetComponent<Player>();

        _input = _player.Playerinput;
        _inputActionMap = _input.actions.FindActionMap("Player");
        _switchWeapon1 = _inputActionMap.FindAction("Previous");
        _switchWeapon2 = _inputActionMap.FindAction("Next");

        _switchWeapon1.performed += WeaponSwitch1;
        _switchWeapon2.performed += WeaponSwitch2;

        TrakingPlayer trakingPlayer;
        _weaponHands.TryGetComponent(out trakingPlayer);
        _decision = trakingPlayer.AttackFxInstance;
        _decision.OnHit += HitAble;

        GameManager.Instance.CurrentDungeon.MainWeaponSlot.ChangeIcon(_slotWeapon1);
    }

    private void Update()
    {
        if(Keyboard.current.bKey.wasPressedThisFrame)
        {
            HitAble(gameObject);
        }
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
        switch (slotNum)
        {
            case 1:
                currentweapon = _slotWeapon1;
                _slotWeapon1.gameObject.SetActive(true);
                if (_slotWeapon2 != null)
                    _slotWeapon2.gameObject.SetActive(false);
                Weaponchanged?.Invoke(currentweapon.WeaponData.weaponAttack1Speed);
                break;

            case 2:
                currentweapon = _slotWeapon2;
                _slotWeapon2.gameObject.SetActive(true);
                _slotWeapon1.gameObject.SetActive(false);
                Weaponchanged?.Invoke(currentweapon.WeaponData.weaponAttack1Speed);
                break;
        }
        GameManager.Instance.CurrentDungeon.EquipSlotController.EquipWeapon(slotNum);
    }

    public void ChangeWeapon(Weapon weapon)//스크립터블 오브젝트 넣는 곳)
    {
        //if(_currentWeaponData == 대충 매개변수로 받아온 변수) //스크립터블과 비교하는 곳
        //return;

        if (_slotWeapon2 != null)
            WeaponPool.Instance.TakeObject(_slotWeapon2);

        _slotWeapon2 = weapon;
        _slotWeapon2.transform.SetParent(_weaponHands, false);
        _slotWeapon2.transform.localPosition = Vector3.zero;

        if (!_slotWeapon2.gameObject.activeSelf)
            _slotWeapon2.gameObject.SetActive(true);

        currentweapon.gameObject.SetActive(false);
        currentweapon = _slotWeapon2;

        GameManager.Instance.CurrentDungeon.EquipSlotController.ChangeSubWeapon(_slotWeapon2);
        GameManager.Instance.CurrentDungeon.EquipSlotController.ChangeSubWeaponDurability(_slotWeapon2.Durability, _slotWeapon2.WeaponData.weaponDurability);
        GameManager.Instance.CurrentDungeon.EquipSlotController.EquipWeapon(2);
        Weaponchanged?.Invoke(currentweapon.WeaponData.weaponAttack1Speed);
    }


    public void ResetAttack()
    {
        _isAlreadyHit = false;
    }

    private void HitAble(GameObject enemy)
    {
        if (enemy.TryGetComponent<MonsterView>(out var monster))
        {
            monster.Presenter.OnHit(currentweapon.WeaponData.weaponAttack1Damage + DataManager.Instance.CharacterData._bonusAtk);
        }

        if (!_isAlreadyHit)
       {
            if (currentweapon == _slotWeapon2)
            {
                _slotWeapon2.ReduceDurability(1);
                GameManager.Instance.CurrentDungeon.EquipSlotController.ChangeSubWeaponDurability(_slotWeapon2.Durability, _slotWeapon2.WeaponData.weaponDurability);

                _isAlreadyHit = true;
                if (_slotWeapon2.Durability <= 0)
                {
                    WeaponBreak();
                }
            }
        }
    }
    public void WeaponBreak()
    {
        WeaponPool.Instance.TakeObject(currentweapon);
        _slotWeapon2 = null;
        GameManager.Instance.CurrentDungeon.EquipSlotController.ChangeSubWeapon(_slotWeapon2);
        currentweapon = _slotWeapon1;
        Weaponchanged?.Invoke(currentweapon.WeaponData.weaponAttack1Speed);
        _slotWeapon1.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        _switchWeapon1.performed -= WeaponSwitch1;
        _switchWeapon2.performed -= WeaponSwitch2;

        if (_slotWeapon2 != null)
        {
            _slotWeapon2.transform.SetParent(DataManager.Instance.CharacterData.transform, false);
            DataManager.Instance.CharacterData._subWeapon = _slotWeapon2;
        }
    }
}
