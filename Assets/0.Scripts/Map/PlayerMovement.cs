using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 간단한 플레이어 이동 스크립트
/// WASD 입력으로 8방향 자유 이동합니다.
/// 스프라이트 기반 충돌 체크를 사용합니다.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f; // 이동 속도
    
    [Header("Collision Check")]
    [SerializeField] private float collisionCheckRadius = 0.3f; // 충돌 체크 반경
    [SerializeField] private LayerMask obstacleLayer; // 장애물 레이어 (벽 포함)
    [SerializeField] private string wallTag = "Wall"; // 벽 태그 (선택사항)
    
    [Header("Interaction")]
    [SerializeField] private Key interactKey = Key.E; // 상호작용 키
    [SerializeField] private float interactRange = 1.5f; // 상호작용 범위
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Keyboard keyboard;
    
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

