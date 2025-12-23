using UnityEngine;

/// <summary>
/// 함정 방의 런타임 동작을 관리하는 컨트롤러.
/// - 플레이어가 방에 진입하면 문을 잠그고 함정을 활성화(예정)
/// - 레버가 작동되면 문을 다시 열고 함정을 비활성화(예정)
/// </summary>
public class TrapRoomController : MonoBehaviour
{
    private BaseRoom baseRoom;
    private TrapRoomMazeGenerator mazeGenerator;

    private bool isLocked = false;   // 문이 잠겼는지 여부 (플레이어가 들어왔는지)
    private bool isCleared = false;  // 레버를 당겨서 방을 클리어했는지 여부

    /// <summary>
    /// BaseRoom에서 함정 방 생성 직후 호출.
    /// </summary>
    public void Initialize(BaseRoom room, TrapRoomMazeGenerator generator)
    {
        baseRoom = room;
        mazeGenerator = generator;
        EnsureTriggerCollider();

        Debug.Log($"[TrapRoomController] Initialize - room:{room?.name}, hasMazeGenerator:{mazeGenerator != null}");
    }

    /// <summary>
    /// 방 전체 영역을 커버하는 Trigger Collider가 있는지 확인합니다.
    /// Collider2D는 방 프리팹에서 수동으로 설정하는 것을 전제로 하며,
    /// 여기서는 존재 여부와 isTrigger 설정만 검증합니다.
    /// </summary>
    private void EnsureTriggerCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning($"[TrapRoomController] Collider2D가 없습니다. 방 프리팹에 Trigger Collider를 수동으로 추가해야 합니다. room:{name}");
            return;
        }

        if (!col.isTrigger)
        {
            Debug.LogWarning($"[TrapRoomController] Collider2D가 Trigger로 설정되어 있지 않습니다. isTrigger=true로 설정해야 플레이어 입장을 감지합니다. room:{name}");
        }
        else
        {
            Debug.Log($"[TrapRoomController] EnsureTriggerCollider - using existing Collider2D:{col.GetType().Name}, isTrigger:{col.isTrigger}, room:{name}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = IsPlayer(other);
        Debug.Log($"[TrapRoomController] OnTriggerEnter2D - other:{other.name}, tag:{other.tag}, isPlayer:{isPlayer}, isLocked:{isLocked}, isCleared:{isCleared}");

        if (isLocked || isCleared)
            return;

        // 플레이어가 아닌 경우 무시
        if (!isPlayer)
            return;

        // 함정 방 진입: 문 잠그고 함정 ON
        isLocked = true;
        Debug.Log($"[TrapRoomController] Player entered trap room, locking doors. room:{name}");

        if (baseRoom != null)
        {
            baseRoom.LockAllDoors();
        }

        // 함정 ON (프리팹 갈아끼우기 또는 활성화)
        if (mazeGenerator != null)
        {
            mazeGenerator.SetTrapsActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool isPlayer = IsPlayer(other);
        Debug.Log($"[TrapRoomController] OnTriggerExit2D - other:{other.name}, tag:{other.tag}, isPlayer:{isPlayer}, isLocked:{isLocked}, isCleared:{isCleared}");
        // 방을 나갈 때 추가 동작은 현재 없음 (디버그용 로그만)
    }

    /// <summary>
    /// 레버에서 호출: 함정 방 클리어.
    /// </summary>
    public void OnLeverActivated()
    {
        if (!isLocked || isCleared)
            return;

        isCleared = true;
        isLocked = false;

        if (baseRoom != null)
        {
            baseRoom.UnlockAllDoors();
        }

        // 함정 OFF (프리팹 갈아끼우기 또는 비활성화)
        if (mazeGenerator != null)
        {
            mazeGenerator.SetTrapsActive(false);
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        // 1순위: 태그로 빠르게 판별
        if (other.CompareTag("Player"))
        {
            return true;
        }

        // 2순위: 자신 또는 부모 중에 PlayerController가 있는지 확인
        if (other.GetComponent<PlayerController>() != null)
        {
            return true;
        }

        if (other.GetComponentInParent<PlayerController>() != null)
        {
            return true;
        }

        return false;
    }
}


