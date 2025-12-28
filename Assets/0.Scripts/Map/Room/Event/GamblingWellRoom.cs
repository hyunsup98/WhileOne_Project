using UnityEngine;

/// <summary>
/// 도박의 샘 이벤트
/// 15x15 크기의 방, 중앙에 샘이 있고 시체(백골) 옆에서 규칙 확인
/// </summary>
public class GamblingWellRoom : BaseEventRoom
{
    [Header("Gambling Well Settings")]
    [SerializeField] [Tooltip("샘 오브젝트 (중앙에 배치)")]
    private GameObject well;
    [SerializeField] [Tooltip("시체(백골) 오브젝트 (샘 옆에 배치)")]
    private GameObject skeleton;
    
    private int interactionCount = 0;
    private const int maxInteractions = 2;
    
    protected override void InitializeEventRoom()
    {
        Vector3 center = GetRoomCenter();
        
        // 샘 배치 (방 중앙)
        if (well != null)
        {
            well.transform.position = center;
            well.SetActive(true);
            
            // 샘에 상호작용 컴포넌트 추가
            WellInteractable wellInteractable = well.GetComponent<WellInteractable>();
            if (wellInteractable == null)
            {
                wellInteractable = well.AddComponent<WellInteractable>();
            }
            wellInteractable.Initialize(this);
        }
        
        // 시체 배치 (샘 옆)
        if (skeleton != null)
        {
            Vector3 skeletonPos = center + Vector3.right * 2f;
            skeleton.transform.position = skeletonPos;
            skeleton.SetActive(true);
            
            // 시체에 상호작용 컴포넌트 추가 (규칙 확인용)
            SkeletonInteractable skeletonInteractable = skeleton.GetComponent<SkeletonInteractable>();
            if (skeletonInteractable == null)
            {
                skeletonInteractable = skeleton.AddComponent<SkeletonInteractable>();
            }
            skeletonInteractable.Initialize(this);
        }
    }
    
    /// <summary>
    /// 샘 상호작용 처리
    /// </summary>
    public void OnWellInteracted(Player player)
    {
        if (player == null) return;
        
        // 최대 상호작용 횟수 체크
        if (interactionCount >= maxInteractions)
        {
            Debug.Log("[GamblingWellRoom] 샘은 더 이상 상호작용할 수 없습니다.");
            return;
        }
        
        interactionCount++;
        
        // HP 20% 소모 (헬퍼 메서드 사용)
        ChangePlayerHealthByRatio(-0.2f, player);
        
        // 50% 확률로 보상 지급
        bool success = Random.Range(0f, 1f) < 0.5f;
        
        if (success)
        {
            OnGamblingSuccess(player);
        }
        else
        {
            OnGamblingFailure();
        }
        
        // 최대 횟수 도달 시 상호작용 비활성화
        if (interactionCount >= maxInteractions)
        {
            DisableWellInteraction();
        }
    }
    
    /// <summary>
    /// 시체 상호작용 처리 (규칙 확인)
    /// </summary>
    public void OnSkeletonInteracted(Player player)
    {
        ShowRules();
    }
    
    /// <summary>
    /// 규칙을 안내합니다.
    /// </summary>
    private void ShowRules()
    {
        string rules = "도박의 샘 규칙:\n" +
                      "HP를 20% 바치면 50% 확률로 보물을 받습니다.\n" +
                      "실패하면 아무 일도 일어나지 않습니다.\n" +
                      "샘은 총 2번만 상호작용 가능합니다.";
        
        Debug.Log($"[GamblingWellRoom] {rules}");
        // TODO: UI에 규칙 표시
    }
    
    /// <summary>
    /// 도박 성공
    /// </summary>
    private void OnGamblingSuccess(Player player)
    {
        Debug.Log("[GamblingWellRoom] 도박 성공! 보물을 받았습니다!");
        
        // 보물 지급 (DataManager의 PickTreasure 함수 사용)
        if (GameManager.Instance?.CurrentDungeon?.TreasureBarUI != null && DataManager.Instance?.TreasureData != null)
        {
            var treasure = DataManager.Instance.TreasureData.PickTreasure();
            if (treasure != null)
            {
                GameManager.Instance.CurrentDungeon.TreasureBarUI.AddTreasure(treasure);
            }
        }
    }
    
    /// <summary>
    /// 도박 실패
    /// </summary>
    private void OnGamblingFailure()
    {
        Debug.Log("[GamblingWellRoom] 도박 실패. 아무 일도 일어나지 않았습니다.");
    }
    
    /// <summary>
    /// 샘 상호작용 비활성화
    /// </summary>
    private void DisableWellInteraction()
    {
        WellInteractable wellInteractable = well?.GetComponent<WellInteractable>();
        if (wellInteractable != null)
        {
            wellInteractable.SetCanInteract(false);
        }
    }
    
    /// <summary>
    /// 방의 중심 위치를 반환합니다.
    /// </summary>
    private Vector3 GetRoomCenter()
    {
        return transform.position;
    }
}

/// <summary>
/// 샘 상호작용 컴포넌트
/// </summary>
public class WellInteractable : Interactable
{
    private GamblingWellRoom wellRoom;
    
    public void Initialize(GamblingWellRoom room)
    {
        wellRoom = room;
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
    
    protected override void OnInteract(Player player)
    {
        if (wellRoom != null)
        {
            wellRoom.OnWellInteracted(player);
        }
    }
}

/// <summary>
/// 시체 상호작용 컴포넌트
/// </summary>
public class SkeletonInteractable : Interactable
{
    private GamblingWellRoom wellRoom;
    
    public void Initialize(GamblingWellRoom room)
    {
        wellRoom = room;
    }
    
    protected override void OnInteract(Player player)
    {
        if (wellRoom != null)
        {
            wellRoom.OnSkeletonInteracted(player);
        }
    }
}

