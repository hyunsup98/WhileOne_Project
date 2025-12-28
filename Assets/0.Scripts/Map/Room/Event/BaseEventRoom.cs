using UnityEngine;

/// <summary>
/// 모든 이벤트 방의 기본 클래스
/// </summary>
public abstract class BaseEventRoom : BaseRoom
{
    protected EventRoomType eventRoomType;
    
    /// <summary>
    /// 이벤트 방을 초기화합니다.
    /// </summary>
    public override void InitializeRoom(Room room)
    {
        base.InitializeRoom(room);
        
        // 이벤트 방 타입 설정
        if (room.eventRoomType.HasValue)
        {
            eventRoomType = room.eventRoomType.Value;
        }
        
        // 이벤트 방 특화 초기화
        InitializeEventRoom();
    }
    
    /// <summary>
    /// 각 이벤트 방 컨셉별 초기화 로직
    /// </summary>
    protected abstract void InitializeEventRoom();
    
    #region 플레이어 정보 접근 헬퍼 메서드
    
    /// <summary>
    /// 현재 플레이어 인스턴스를 가져옵니다.
    /// </summary>
    protected Player GetPlayer()
    {
        return GameManager.Instance?.player;
    }
    
    /// <summary>
    /// 플레이어의 현재 체력을 가져옵니다.
    /// </summary>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected float GetPlayerHealth(Player player = null)
    {
        if (player == null) player = GetPlayer();
        return player != null ? player.Hp : 0f;
    }
    
    /// <summary>
    /// 플레이어의 최대 체력을 가져옵니다.
    /// </summary>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected float GetPlayerMaxHealth(Player player = null)
    {
        if (player == null) player = GetPlayer();
        return player != null ? player.MaxHp : 0f;
    }
    
    /// <summary>
    /// 플레이어의 체력 비율을 가져옵니다. (0.0 ~ 1.0)
    /// </summary>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected float GetPlayerHealthRatio(Player player = null)
    {
        if (player == null) player = GetPlayer();
        if (player == null || player.MaxHp <= 0f) return 0f;
        return player.Hp / player.MaxHp;
    }
    
    /// <summary>
    /// 플레이어가 도굴 중인지 확인합니다.
    /// </summary>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected bool IsPlayerDigging(Player player = null)
    {
        if (player == null) player = GetPlayer();
        return player != null && player.PlayerDig != null && player.PlayerDig.IsDigging;
    }
    
    /// <summary>
    /// 플레이어의 체력을 변경합니다.
    /// </summary>
    /// <param name="amount">변경할 체력량 (양수면 회복, 음수면 피해)</param>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected void ChangePlayerHealth(float amount, Player player = null)
    {
        if (player == null) player = GetPlayer();
        if (player != null)
        {
            player.ChangedHealth += amount;
        }
    }
    
    /// <summary>
    /// 플레이어의 체력을 비율로 변경합니다.
    /// </summary>
    /// <param name="ratio">변경할 체력 비율 (0.1 = 10%, 양수면 회복, 음수면 피해)</param>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected void ChangePlayerHealthByRatio(float ratio, Player player = null)
    {
        if (player == null) player = GetPlayer();
        if (player != null)
        {
            float amount = player.MaxHp * ratio;
            player.ChangedHealth += amount;
        }
    }
    
    /// <summary>
    /// 플레이어에게 피해를 입힙니다.
    /// </summary>
    /// <param name="damage">입힐 피해량</param>
    /// <param name="sourcePosition">피해 원인 위치 (넉백용, Vector2.zero면 넉백 없음)</param>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected void DamagePlayer(float damage, Vector2 sourcePosition = default, Player player = null)
    {
        if (player == null) player = GetPlayer();
        if (player != null && player.GetDamage != null)
        {
            player.GetDamage.TakenDamage(damage, sourcePosition);
        }
    }
    
    /// <summary>
    /// 플레이어의 체력을 비율로 회복합니다.
    /// </summary>
    /// <param name="ratio">회복할 체력 비율 (0.1 = 10%, 최대 1.0 = 100%)</param>
    /// <param name="player">대상 플레이어 (null이면 현재 플레이어)</param>
    protected void HealPlayer(float ratio, Player player = null)
    {
        if (player == null) player = GetPlayer();
        if (player != null)
        {
            ratio = Mathf.Clamp01(ratio);
            float healAmount = player.MaxHp * ratio;
            player.ChangedHealth = Mathf.Min(player.ChangedHealth + healAmount, player.MaxHp);
        }
    }
    
    #endregion
}

