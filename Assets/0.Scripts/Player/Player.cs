using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("플레이어 수치 조절 컴포넌트")]
    [SerializeField] private float _hp;
    [SerializeField] private float _stamina;
    [SerializeField] private float _moveSpeed = 5f;

    //외부에서 사용할 프로퍼티
    public float Hp => _hp;
    public float Stamina => _stamina;
    public float MoveSpeed => _moveSpeed;

    void Start()
    {
        
    }

   
    void Update()
    {
        
    }
}
