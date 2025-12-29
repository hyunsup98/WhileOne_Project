using UnityEngine;

/// <summary>
/// PortalZone에 붙어서 플레이어 트리거를 감지하고, 상위 PortalRoom에 전달하는 중계 스크립트.
/// </summary>
public class PortalZoneTrigger : MonoBehaviour
{
    [SerializeField] [Tooltip("이 PortalZone이 속한 PortalRoom")]
    private PortalRoom portalRoom;

    private void Awake()
    {
        // 인스펙터에서 지정하지 않았다면 부모에서 자동으로 찾기
        if (portalRoom == null)
        {
            portalRoom = GetComponentInParent<PortalRoom>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (portalRoom == null) return;

        // PortalRoom 측에서 처리하도록 전달
        portalRoom.OnPlayerEnterPortalZone(other);
    }
}

