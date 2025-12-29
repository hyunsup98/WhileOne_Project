using UnityEngine;

/// <summary>
/// 보스방 진입 영역을 감지하는 트리거 스크립트
/// BossRoom의 entryZone에 붙여서 사용합니다.
/// </summary>
public class BossRoomEntryZone : MonoBehaviour
{
    private BossRoom bossRoom;
    
    /// <summary>
    /// BossRoom 참조를 설정합니다.
    /// </summary>
    public void SetBossRoom(BossRoom room)
    {
        bossRoom = room;
    }
    
    /// <summary>
    /// 플레이어가 진입 영역에 들어왔을 때 호출됩니다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어만 처리
        if (!other.CompareTag("Player")) return;
        
        // BossRoom에 알림
        if (bossRoom != null)
        {
            bossRoom.OnPlayerEnterEntryZone();
        }
    }
}

