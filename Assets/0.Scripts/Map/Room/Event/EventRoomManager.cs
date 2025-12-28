using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 이벤트 방 컨셉을 관리하는 클래스
/// 한 층에 이벤트 방 컨셉이 겹치지 않도록 스택 방식으로 관리합니다.
/// </summary>
public static class EventRoomManager
{
    private static List<EventRoomType> availableEventTypes = new List<EventRoomType>();
    
    /// <summary>
    /// 층이 시작될 때 사용 가능한 이벤트 방 컨셉을 초기화합니다.
    /// </summary>
    public static void InitializeForFloor()
    {
        // 모든 이벤트 방 컨셉을 리스트에 추가
        availableEventTypes = new List<EventRoomType>
        {
            EventRoomType.Digging,
            EventRoomType.AbandonedForge,
            EventRoomType.GamblingWell,
            EventRoomType.ChestRoom,
            EventRoomType.Healing
        };
    }
    
    /// <summary>
    /// 사용 가능한 이벤트 방 컨셉을 랜덤하게 하나 선택하고 리스트에서 제거합니다.
    /// </summary>
    /// <returns>선택된 이벤트 방 컨셉, 없으면 null</returns>
    public static EventRoomType? GetRandomEventType()
    {
        if (availableEventTypes == null || availableEventTypes.Count == 0)
        {
            Debug.LogWarning("[EventRoomManager] 사용 가능한 이벤트 방 컨셉이 없습니다.");
            return null;
        }
        
        // 랜덤하게 하나 선택
        int randomIndex = Random.Range(0, availableEventTypes.Count);
        EventRoomType selectedType = availableEventTypes[randomIndex];
        
        // 리스트에서 제거 (스택 방식)
        availableEventTypes.RemoveAt(randomIndex);
        
        return selectedType;
    }
    
    /// <summary>
    /// 현재 사용 가능한 이벤트 방 컨셉 개수를 반환합니다.
    /// </summary>
    public static int GetAvailableCount()
    {
        return availableEventTypes != null ? availableEventTypes.Count : 0;
    }
    
    /// <summary>
    /// 특정 이벤트 방 컨셉이 사용 가능한지 확인합니다.
    /// </summary>
    public static bool IsAvailable(EventRoomType eventType)
    {
        return availableEventTypes != null && availableEventTypes.Contains(eventType);
    }
}

