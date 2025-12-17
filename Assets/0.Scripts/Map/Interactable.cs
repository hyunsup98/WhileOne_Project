using UnityEngine;

/// <summary>
/// 상호작용 가능한 오브젝트의 기본 클래스
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] [Tooltip("플레이어가 상호작용 가능한 범위 (Unity 단위)")]
    protected float interactionRange = 1.5f;
    [SerializeField] [Tooltip("플레이어가 속한 레이어 (충돌 체크용)")]
    protected LayerMask playerLayer;
    [SerializeField] [Tooltip("현재 상호작용 가능한지 여부")]
    protected bool canInteract = true;
    
    protected bool isPlayerNearby = false;
    
    private void Update()
    {
        CheckPlayerDistance();
    }
    
    /// <summary>
    /// 플레이어와의 거리를 확인합니다.
    /// </summary>
    private void CheckPlayerDistance()
    {
        if (!canInteract) return;
        
        Collider2D player = Physics2D.OverlapCircle(transform.position, interactionRange, playerLayer);
        bool wasNearby = isPlayerNearby;
        isPlayerNearby = player != null;
        
        if (!wasNearby && isPlayerNearby)
        {
            OnPlayerEnter();
        }
        else if (wasNearby && !isPlayerNearby)
        {
            OnPlayerExit();
        }
    }
    
    /// <summary>
    /// 상호작용을 수행합니다.
    /// </summary>
    public virtual void Interact(GameObject player)
    {
        if (!canInteract) return;
        OnInteract(player);
    }
    
    /// <summary>
    /// 플레이어가 범위에 들어왔을 때
    /// </summary>
    protected virtual void OnPlayerEnter()
    {
        // 오버라이드하여 사용
    }
    
    /// <summary>
    /// 플레이어가 범위를 벗어났을 때
    /// </summary>
    protected virtual void OnPlayerExit()
    {
        // 오버라이드하여 사용
    }
    
    /// <summary>
    /// 상호작용 실행
    /// </summary>
    protected abstract void OnInteract(GameObject player);
    
    void OnDrawGizmos()
    {
        Gizmos.color = isPlayerNearby ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

