using UnityEngine;
using System.Collections;

/// <summary>
/// 함정 방의 런타임 동작을 관리하는 컨트롤러.
/// - 플레이어가 방에 진입하면 문을 잠그고 함정을 활성화
/// - 레버가 작동되면 문을 다시 열고 함정을 비활성화
/// </summary>
public class TrapRoomController : MonoBehaviour
{
    [Header("Door Lock Settings")]
    [SerializeField] [Tooltip("문 닫기 전 딜레이 시간 (초)")]
    private float doorLockDelay = 0.3f;
    [SerializeField] [Tooltip("문 근처로 판단할 거리 (Unity unit)")]
    private float doorProximityDistance = 2.0f;
    [SerializeField] [Tooltip("플레이어를 밀어낼 거리 (Unity unit)")]
    private float pushDistance = 1.5f;

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

        //Debug.Log($"[TrapRoomController] Initialize - room:{room?.name}, hasMazeGenerator:{mazeGenerator != null}");
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
            //Debug.Log($"[TrapRoomController] EnsureTriggerCollider - using existing Collider2D:{col.GetType().Name}, isTrigger:{col.isTrigger}, room:{name}");
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

        // 함정 방 진입: 플레이어를 밀어낸 후 문 잠그고 함정 ON
        isLocked = true;
        Debug.Log($"[TrapRoomController] Player entered trap room, preparing to lock doors. room:{name}");

        // 코루틴으로 플레이어 밀어내기 및 문 잠그기
        StartCoroutine(LockDoorsWithDelay(other));
    }

    /// <summary>
    /// 플레이어를 문에서 밀어낸 후 문을 잠그는 코루틴
    /// </summary>
    private IEnumerator LockDoorsWithDelay(Collider2D playerCollider)
    {
        // 플레이어 찾기
        Player player = playerCollider?.GetComponent<Player>();
        if (player == null)
        {
            player = GameManager.Instance?.player;
        }

        // 플레이어가 문 근처에 있으면 밀어내기
        if (player != null && baseRoom != null)
        {
            PushPlayerAwayFromDoors(player);
        }

        // 딜레이 후 문 잠그기
        yield return new WaitForSeconds(doorLockDelay);

        if (baseRoom != null)
        {
            baseRoom.LockAllDoors();
            Debug.Log($"[TrapRoomController] Doors locked after delay. room:{name}");
        }

        // 함정 ON (프리팹 갈아끼우기 또는 활성화)
        if (mazeGenerator != null)
        {
            mazeGenerator.SetTrapsActive(true);
        }
    }

    /// <summary>
    /// 플레이어를 문에서 멀리 밀어냅니다.
    /// </summary>
    private void PushPlayerAwayFromDoors(Player player)
    {
        if (player == null || baseRoom == null) return;

        Vector3 playerPos = player.transform.position;
        var doorPositions = baseRoom.GetConnectedDoorPositions();
        Vector3 roomCenter = transform.position;

        // 가장 가까운 문 찾기
        float minDistance = float.MaxValue;
        Vector3 nearestDoorPos = Vector3.zero;
        bool foundNearDoor = false;

        foreach (var doorPos in doorPositions)
        {
            float distance = Vector3.Distance(playerPos, doorPos);
            if (distance < doorProximityDistance && distance < minDistance)
            {
                minDistance = distance;
                nearestDoorPos = doorPos;
                foundNearDoor = true;
            }
        }

        // 문 근처에 있으면 방 안쪽으로 밀어내기
        if (foundNearDoor)
        {
            // 문에서 방 중심 방향으로 벡터 계산
            Vector3 pushDirection = (roomCenter - nearestDoorPos).normalized;
            
            // 플레이어를 밀어낼 위치 계산
            Vector3 targetPosition = playerPos + pushDirection * pushDistance;
            
            // 플레이어 위치 직접 변경 (Rigidbody2D가 있으면 velocity 사용)
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Rigidbody2D가 있으면 velocity로 밀어내기
                rb.velocity = Vector2.zero; // 기존 속도 초기화
                rb.MovePosition(targetPosition);
            }
            else
            {
                // Rigidbody2D가 없으면 transform 직접 변경
                player.transform.position = targetPosition;
            }

            Debug.Log($"[TrapRoomController] Pushed player away from door. From: {playerPos}, To: {targetPosition}");
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

        return false;
    }
}


