using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 간단한 플레이어 이동 스크립트
/// WASD 입력으로 8방향 자유 이동합니다.
/// 스프라이트 기반 충돌 체크를 사용합니다.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] [Tooltip("플레이어의 이동 속도 (단위/초)")]
    private float moveSpeed = 5f; // 이동 속도
    
    [Header("Collision Check")]
    [SerializeField] [Tooltip("충돌 체크를 수행할 반경 (Unity unit)")]
    private float collisionCheckRadius = 0.3f; // 충돌 체크 반경
    [SerializeField] [Tooltip("충돌 체크할 장애물 레이어 (벽 등 포함)")]
    private LayerMask obstacleLayer; // 장애물 레이어 (벽 포함)
    [SerializeField] [Tooltip("벽으로 인식할 태그 이름 (선택사항, 빈 문자열이면 사용 안 함)")]
    private string wallTag = "Wall"; // 벽 태그 (선택사항)
    
    [Header("Interaction")]
    [SerializeField] [Tooltip("상호작용에 사용할 키 (Input System Key 타입)")]
    private Key interactKey = Key.E; // 상호작용 키
    [SerializeField] [Tooltip("상호작용 가능한 오브젝트를 탐지할 범위 (Unity unit)")]
    private float interactRange = 1.5f; // 상호작용 범위

    [Header("Status")]
    [SerializeField] [Tooltip("플레이어의 최대 체력")]
    private int maxHp = 100;
    [SerializeField] [Tooltip("시작 시 현재 체력 (최대 체력을 넘지 않도록 자동 보정)")]
    private int currentHp = 100;
    [SerializeField] [Tooltip("체력 텍스트가 표시될 위치 오프셋 (플레이어 기준)")]
    private Vector3 hpTextOffset = new Vector3(0f, 1.0f, 0f);
    [SerializeField] [Tooltip("체력 텍스트 색상")]
    private Color hpTextColor = Color.red;
    [SerializeField] [Tooltip("체력 텍스트 폰트 크기")]
    private int hpTextFontSize = 24;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Keyboard keyboard;

    // 머리 위에 표시할 체력 텍스트
    private TextMesh hpTextMesh;

    // 플레이어가 현재 속해 있는 방 (RoomController)
    private RoomController currentRoom;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Rigidbody2D가 없으면 추가
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Rigidbody2D 설정
        rb.gravityScale = 0; // 중력 비활성화
        rb.linearDamping = 0; // 드래그 없음
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 고정
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 연속 충돌 감지
        rb.bodyType = RigidbodyType2D.Dynamic; // Dynamic으로 설정
        
        // 키보드 입력 초기화
        keyboard = Keyboard.current;

        // HP 값 보정 및 텍스트 생성
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        CreateHpText();
        UpdateHpText();
    }
    
    private void Update()
    {
        // 키보드가 없으면 리턴
        if (keyboard == null)
        {
            keyboard = Keyboard.current;
            return;
        }
        
        // WASD 입력 읽기 (Input System)
        moveInput = Vector2.zero;
        
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            moveInput.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            moveInput.y -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveInput.x += 1f;
        
        // 상호작용 키 입력
        if (keyboard[interactKey].wasPressedThisFrame)
        {
            TryInteract();
        }

        // 마우스 우클릭으로 Dig 시도
        var mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
        {
            TryRightClickDig(mouse);
        }
    }

    private void LateUpdate()
    {
        // HP 텍스트가 플레이어 머리 위를 따라가도록 위치 갱신
        if (hpTextMesh != null)
        {
            hpTextMesh.transform.position = transform.position + hpTextOffset;
        }
    }
    
    /// <summary>
    /// 상호작용을 시도합니다.
    /// </summary>
    private void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);
        foreach (var hit in hits)
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject);
                break; // 첫 번째 상호작용 가능한 오브젝트만
            }
        }
    }

    /// <summary>
    /// 마우스 우클릭 지점에 대해 DigSpot 타일이 있으면 DugSpot으로 변경을 시도합니다.
    /// (현재 플레이어가 속한 RoomController 기준)
    /// </summary>
    private void TryRightClickDig(Mouse mouse)
    {
        if (currentRoom == null) return;

        // 마우스 화면 좌표 → 월드 좌표
        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 worldPos = Camera.main != null
            ? Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f))
            : (Vector3)screenPos;
        worldPos.z = 0f;

        bool dug = currentRoom.TryDigAtWorldPosition(worldPos);

        // 필요하면 디버그 로그
        // Debug.Log($"[Player] Right click dig result: {dug}, pos:{worldPos}");
    }

    /// <summary>
    /// RoomController에서 호출: 플레이어가 이 방에 들어왔을 때 현재 방 설정
    /// </summary>
    public void SetCurrentRoom(RoomController room)
    {
        currentRoom = room;
    }

    /// <summary>
    /// RoomController에서 호출: 플레이어가 이 방에서 나갔을 때 현재 방 해제
    /// </summary>
    public void ClearCurrentRoom(RoomController room)
    {
        if (currentRoom == room)
        {
            currentRoom = null;
        }
    }

    #region HP 관리 및 표시

    /// <summary>
    /// 머리 위에 표시할 HP 텍스트 오브젝트 생성
    /// </summary>
    private void CreateHpText()
    {
        if (hpTextMesh != null) return;

        GameObject hpObj = new GameObject("PlayerHPText");
        hpObj.transform.SetParent(transform);
        hpObj.transform.position = transform.position + hpTextOffset;

        hpTextMesh = hpObj.AddComponent<TextMesh>();
        hpTextMesh.alignment = TextAlignment.Center;
        hpTextMesh.anchor = TextAnchor.LowerCenter;
        hpTextMesh.color = hpTextColor;
        hpTextMesh.fontSize = hpTextFontSize;
        // order in layer 설정 (필요시 조정)
        hpTextMesh.GetComponent<MeshRenderer>().sortingOrder = 999;
    }

    /// <summary>
    /// HP 텍스트 내용 갱신
    /// </summary>
    private void UpdateHpText()
    {
        if (hpTextMesh != null)
        {
            hpTextMesh.text = currentHp.ToString();
        }
    }

    /// <summary>
    /// 외부에서 HP를 설정할 때 사용 (0~maxHp로 자동 보정)
    /// </summary>
    public void SetHp(int value)
    {
        currentHp = Mathf.Clamp(value, 0, maxHp);
        UpdateHpText();
    }

    /// <summary>
    /// HP 증감 (음수 = 데미지, 양수 = 회복)
    /// </summary>
    public void AddHp(int delta)
    {
        SetHp(currentHp + delta);
    }

    /// <summary>
    /// HP를 퍼센트(최대 HP 기준)로 증감합니다. (예: -10 = -10%, 10 = +10%)
    /// </summary>
    public void AddHpPercent(float percent)
    {
        // 1보다 큰 값은 "퍼센트" 로 간주 (10 -> 0.1), 0~1 사이는 곧바로 비율로 사용
        float ratio = Mathf.Abs(percent) > 1f ? percent / 100f : percent;

        int delta = Mathf.RoundToInt(maxHp * ratio);
        AddHp(delta);
    }

    public int GetCurrentHp() => currentHp;
    public int GetMaxHp() => maxHp;

    #endregion
    
    private void FixedUpdate()
    {
        // 입력이 없으면 이동하지 않음
        if (moveInput.magnitude < 0.1f)
        {
            return;
        }
        
        // 입력 정규화 (대각선 이동 시 속도 일정하게)
        Vector2 moveDirection = moveInput.normalized;
        
        // 이동할 위치 계산
        Vector2 targetPosition = (Vector2)transform.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        
        // 충돌 체크
        if (CanMoveTo(targetPosition))
        {
            // 이동 가능하면 이동
            rb.MovePosition(targetPosition);
        }
        else
        {
            // 충돌이 있으면 X축 또는 Y축만 이동 시도
            Vector2 xOnlyMove = new Vector2(moveDirection.x * moveSpeed * Time.fixedDeltaTime, 0);
            Vector2 yOnlyMove = new Vector2(0, moveDirection.y * moveSpeed * Time.fixedDeltaTime);
            
            if (CanMoveTo((Vector2)transform.position + xOnlyMove))
            {
                rb.MovePosition((Vector2)transform.position + xOnlyMove);
            }
            else if (CanMoveTo((Vector2)transform.position + yOnlyMove))
            {
                rb.MovePosition((Vector2)transform.position + yOnlyMove);
            }
        }
    }
    
    /// <summary>
    /// 해당 위치로 이동할 수 있는지 확인합니다.
    /// </summary>
    private bool CanMoveTo(Vector2 worldPosition)
    {
        // 레이어 기반 충돌 체크 (벽 및 장애물)
        Collider2D hit = Physics2D.OverlapCircle(worldPosition, collisionCheckRadius, obstacleLayer);
        if (hit != null)
        {
            return false; // 장애물이 있으면 이동 불가
        }
        
        // 태그 기반 벽 체크 (선택사항)
        if (!string.IsNullOrEmpty(wallTag))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, collisionCheckRadius);
            foreach (var h in hits)
            {
                if (h.CompareTag(wallTag))
                {
                    return false; // 벽 태그가 있으면 이동 불가
                }
            }
        }
        
        return true;
    }
    
    void OnDrawGizmos()
    {
        // 충돌 체크 반경 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
        
        // 상호작용 범위 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}

