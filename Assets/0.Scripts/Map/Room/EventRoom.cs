using UnityEngine;

/// <summary>
/// 이벤트 방 프리팹
/// BaseRoom을 상속받아 기본 방 구조를 가집니다.
/// 이벤트 방 컨셉에 따라 적절한 이벤트 방 컴포넌트를 추가합니다.
/// </summary>
public class EventRoom : BaseRoom
{
    private BaseEventRoom eventRoomComponent;
    
    public override void InitializeRoom(Room room)
    {
        base.InitializeRoom(room);
        
        // 이벤트 방 컨셉이 설정되어 있으면 해당 컴포넌트 추가
        if (room.eventRoomType.HasValue)
        {
            //AddEventRoomComponent(room.eventRoomType.Value);
            //수동 설정으로 변경
        }
        else
        {
            Debug.LogWarning("[EventRoom] 이벤트 방 컨셉이 설정되지 않았습니다.");
        }
        
        // 수동으로 추가된 BaseEventRoom 컴포넌트 찾아서 초기화
        BaseEventRoom[] eventRoomComponents = GetComponents<BaseEventRoom>();
        foreach (var eventRoomComp in eventRoomComponents)
        {
            if (eventRoomComp != null && eventRoomComp != this)
            {
                eventRoomComp.InitializeRoom(room);
            }
        }
    }
    
    /// <summary>
    /// 이벤트 방 컨셉에 따라 적절한 컴포넌트를 추가합니다.
    /// </summary>
    private void AddEventRoomComponent(EventRoomType eventType)
    {
        // 기존 컴포넌트 제거
        if (eventRoomComponent != null)
        {
            DestroyImmediate(eventRoomComponent);
        }
        
        // 이벤트 방 컨셉에 따라 컴포넌트 추가
        switch (eventType)
        {
            case EventRoomType.Digging:
                eventRoomComponent = gameObject.AddComponent<DiggingRoom>();
                break;
            case EventRoomType.AbandonedForge:
                eventRoomComponent = gameObject.AddComponent<AbandonedForgeRoom>();
                break;
            case EventRoomType.GamblingWell:
                eventRoomComponent = gameObject.AddComponent<GamblingWellRoom>();
                break;
            case EventRoomType.ChestRoom:
                eventRoomComponent = gameObject.AddComponent<ChestRoom>();
                break;
            case EventRoomType.Healing:
                eventRoomComponent = gameObject.AddComponent<HealingRoom>();
                break;
            default:
                Debug.LogWarning($"[EventRoom] 알 수 없는 이벤트 방 컨셉: {eventType}");
                break;
        }
        
        // 컴포넌트 초기화
        if (eventRoomComponent != null)
        {
            eventRoomComponent.InitializeRoom(roomData);
        }
    }
}

