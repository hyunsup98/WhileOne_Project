using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    Player _player;

    //플레이어 공격관련 (프로퍼티로 받아 올것들)
    private int _attack;
    private int _attackSpeed;


    private void Awake()
    {
        _player = GetComponent<Player>();
    }
    void Start()
    {

    }
    void Update()
    {

    }
}
