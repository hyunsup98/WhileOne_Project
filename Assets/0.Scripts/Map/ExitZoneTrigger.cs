using UnityEngine;

/// <summary>
/// ExitZone에 붙어서 플레이어 트리거를 감지하고, 상위 ExitRoom에 전달하는 중계 스크립트.
/// </summary>
public class ExitZoneTrigger : MonoBehaviour
{
    [SerializeField] [Tooltip("이 ExitZone이 속한 ExitRoom")]
    private ExitRoom exitRoom;

    private void Awake()
    {
        // 인스펙터에서 지정하지 않았다면 부모에서 자동으로 찾기
        if (exitRoom == null)
        {
            exitRoom = GetComponentInParent<ExitRoom>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exitRoom == null) return;

        // ExitRoom 측에서 처리하도록 전달
        exitRoom.OnPlayerEnterExitZone(other);
    }
}


