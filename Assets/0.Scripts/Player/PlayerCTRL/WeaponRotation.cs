using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class WeaponRotation : MonoBehaviour
{
    Vector3 _mousePosition;

    [Header("최대 최소값 조절")]
    [SerializeField] private float max;
    [SerializeField] private float min;
    [SerializeField] private float angleSpeed = 5f;


    //회전값을 저장
    private float currentAngle;

    void Update()
    {

        //마우스 좌표값을 월드 좌표값으로 변환
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //플레이어와 마우스 사이 좌표 거리 계산
        Vector3 angle = _mousePosition - transform.position;

        //좌표 거리값을 바탕으로 각도(aTan) 계산

        currentAngle = Mathf.Atan2(angle.y, angle.x) * Mathf.Rad2Deg; //라디안을 도(°)로 단위 변환



        transform.rotation = Quaternion.Euler(0, 0, currentAngle - 90); //위의 값을 바탕으로 무기 회전
        
        // 트랜스폼 회전 값을 최대, 최소 값을 둬서 제한을 넘어가지 못하게 만들어 줌
        if (transform.root.localScale.x == 1)
        {
            if (transform.rotation.eulerAngles.z >= max)
            {
                transform.rotation = Quaternion.Euler(0, 0, max);
            }
            else if (transform.rotation.eulerAngles.z <= min)
            {
                transform.rotation = Quaternion.Euler(0, 0, min);
            }
        }
        else if (transform.root.localScale.x == -1)
        {
            if (transform.rotation.eulerAngles.z >= 350)
            {
                transform.rotation = Quaternion.Euler(0, 0, 350);
            }
            else if (transform.rotation.eulerAngles.z <= 280)
            {
                transform.rotation = Quaternion.Euler(0, 0, 280);
            }
        }

    }
}
