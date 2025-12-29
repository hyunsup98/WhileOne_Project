using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 이벤트 방 컨셉별 프리팹을 저장하는 클래스.
/// </summary>
[System.Serializable]
public class EventRoomPrefabs
{
    [Tooltip("도굴 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] diggingRoomPrefabs;
    [Tooltip("버려진 대장간 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] abandonedForgeRoomPrefabs;
    [Tooltip("도박의 샘 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] gamblingWellRoomPrefabs;
    [Tooltip("상자방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] chestRoomPrefabs;
    [Tooltip("체력 회복 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] healingRoomPrefabs;
}

/// <summary>
/// 한 층에서 사용할 방 프리팹들을 저장하는 클래스.
/// Unity Inspector에서 각 RoomType별 프리팹 리스트를 설정할 수 있습니다.
/// 각 방 타입마다 여러 개의 프리팹을 설정하면, 생성 시 랜덤하게 선택됩니다.
/// </summary>
[System.Serializable]
public class FloorRoomPrefabs
{
    [Tooltip("시작 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] startRoomPrefabs;
    [Tooltip("일반 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] normalRoomPrefabs;
    [Tooltip("출구 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] exitRoomPrefabs;
    [Tooltip("함정 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] trapRoomPrefabs;
    [Tooltip("보물 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] treasureRoomPrefabs;
    [Tooltip("보스 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] bossRoomPrefabs;
    [Tooltip("포탈 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택, 3층에서 보스 방으로 이동)")]
    public GameObject[] portalRoomPrefabs;
    [Tooltip("이벤트 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택, EventRoomType별 프리팹이 없을 때 fallback으로 사용)")]
    public GameObject[] eventRoomPrefabs;
    [Tooltip("이벤트 방 컨셉별 프리팹 설정")]
    public EventRoomPrefabs eventRoomTypePrefabs;
}

/// <summary>
/// 층별 방 타입 구성을 정의하고, 층 정보를 편하게 조회할 수 있도록 제공하는 유틸리티.
/// MonoBehaviour 싱글톤으로 구현하여 Unity Inspector에서 프리팹을 설정할 수 있습니다.
/// </summary>
public class FloorRoomSetManager : MonoBehaviour
{
    private static FloorRoomSetManager instance;

    [Header("층별 방 프리팹들")]
    [SerializeField] [Tooltip("1층에서 사용할 방 프리팹들")]
    private FloorRoomPrefabs floor1Prefabs;
    [SerializeField] [Tooltip("2층에서 사용할 방 프리팹들")]
    private FloorRoomPrefabs floor2Prefabs;
    [SerializeField] [Tooltip("3층에서 사용할 방 프리팹들")]
    private FloorRoomPrefabs floor3Prefabs;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// FloorRoomSetManager 인스턴스를 가져옵니다.
    /// </summary>
    public static FloorRoomSetManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("FloorRoomSetManager");
                instance = go.AddComponent<FloorRoomSetManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    /// <summary>
    /// 한 층에 대한 정보(방 타입 배열 + 편의 메서드)를 담는 클래스.
    /// </summary>
    public class FloorInfo
    {
        /// <summary>층 번호 (1, 2, 3, ...)</summary>
        public int FloorNumber { get; }

        /// <summary>이 층에 포함된 방 타입 배열 (순서는 자유, 필요시 사용)</summary>
        public RoomType[] Rooms { get; }

        /// <summary>이 층에서 사용할 맵 프리팹들 (RoomType별로 매핑된 딕셔너리, 각 타입마다 프리팹 배열)</summary>
        public Dictionary<RoomType, GameObject[]> FloorRoomPrefabs { get; }
        
        /// <summary>이 층에서 사용할 이벤트 방 컨셉별 프리팹들</summary>
        public Dictionary<EventRoomType, GameObject[]> EventRoomTypePrefabs { get; }

        public FloorInfo(int floorNumber, RoomType[] rooms, Dictionary<RoomType, GameObject[]> floorRoomPrefabs, Dictionary<EventRoomType, GameObject[]> eventRoomTypePrefabs)
        {
            FloorNumber = floorNumber;
            Rooms = rooms;
            FloorRoomPrefabs = floorRoomPrefabs;
            EventRoomTypePrefabs = eventRoomTypePrefabs;
        }

        /// <summary>
        /// 이 층에서 특정 RoomType이 몇 개 있는지 반환.
        /// </summary>
        public int GetRoomCountWithType(RoomType roomType)
        {
            if (Rooms == null || Rooms.Length == 0) return 0;

            int count = 0;
            foreach (RoomType type in Rooms)
            {
                if (type == roomType)
                {
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// 현재 씬의 FloorRoomSetManager 인스턴스를 찾아 반환합니다.
    /// </summary>
    private static FloorRoomSetManager FindCurrentInstance()
    {
        return Object.FindFirstObjectByType<FloorRoomSetManager>();
    }

    /// <summary>
    /// 층 번호 → FloorInfo 매핑 테이블.
    /// 런타임에 동적으로 생성됩니다.
    /// </summary>
    private Dictionary<int, FloorInfo> floorInfos;

    /// <summary>
    /// 층별 방 타입 구성을 반환합니다.
    /// </summary>
    private static Dictionary<int, RoomType[]> GetFloorRoomTypes()
    {
        return new Dictionary<int, RoomType[]>
        {
            {
                1,
                new[]
                {
                    RoomType.Start,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Event,
                    RoomType.Treasure,
                    RoomType.Trap,
                    RoomType.Exit
                }
            },
            {
                2,
                new[]
                {
                    RoomType.Start,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Event,
                    RoomType.Event,
                    RoomType.Treasure,
                    RoomType.Trap,
                    RoomType.Trap,
                    RoomType.Exit
                }
            },
            {
                3,
                new[]
                {
                    RoomType.Start,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Normal,
                    RoomType.Event,
                    RoomType.Event,
                    RoomType.Trap,
                    RoomType.Trap,
                    RoomType.Portal
                }
            }
        };
    }

    /// <summary>
    /// 이벤트 방 컨셉별 프리팹을 딕셔너리로 변환합니다.
    /// </summary>
    private Dictionary<EventRoomType, GameObject[]> ConvertEventRoomPrefabsToDictionary(EventRoomPrefabs eventPrefabs)
    {
        Dictionary<EventRoomType, GameObject[]> dict = new Dictionary<EventRoomType, GameObject[]>();
        
        if (eventPrefabs == null)
        {
            Debug.LogWarning("[FloorRoomSetManager] ConvertEventRoomPrefabsToDictionary: eventPrefabs가 null입니다.");
            return dict;
        }
        
        Debug.Log($"[FloorRoomSetManager] ConvertEventRoomPrefabsToDictionary 시작 - eventPrefabs: {(eventPrefabs != null ? "not null" : "null")}");
        
        if (eventPrefabs.diggingRoomPrefabs != null && eventPrefabs.diggingRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(eventPrefabs.diggingRoomPrefabs, p => p != null);
            Debug.Log($"[FloorRoomSetManager] Digging 프리팹 - 원본: {eventPrefabs.diggingRoomPrefabs.Length}개, 유효: {validPrefabs.Length}개");
            if (validPrefabs.Length > 0) dict[EventRoomType.Digging] = validPrefabs;
        }
        else
        {
            Debug.Log($"[FloorRoomSetManager] Digging 프리팹 - 배열이 null이거나 비어있음 (null: {eventPrefabs.diggingRoomPrefabs == null}, Length: {(eventPrefabs.diggingRoomPrefabs != null ? eventPrefabs.diggingRoomPrefabs.Length : 0)})");
        }
        
        if (eventPrefabs.abandonedForgeRoomPrefabs != null && eventPrefabs.abandonedForgeRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(eventPrefabs.abandonedForgeRoomPrefabs, p => p != null);
            Debug.Log($"[FloorRoomSetManager] AbandonedForge 프리팹 - 원본: {eventPrefabs.abandonedForgeRoomPrefabs.Length}개, 유효: {validPrefabs.Length}개");
            if (validPrefabs.Length > 0) dict[EventRoomType.AbandonedForge] = validPrefabs;
        }
        else
        {
            Debug.Log($"[FloorRoomSetManager] AbandonedForge 프리팹 - 배열이 null이거나 비어있음 (null: {eventPrefabs.abandonedForgeRoomPrefabs == null}, Length: {(eventPrefabs.abandonedForgeRoomPrefabs != null ? eventPrefabs.abandonedForgeRoomPrefabs.Length : 0)})");
        }
        
        if (eventPrefabs.gamblingWellRoomPrefabs != null && eventPrefabs.gamblingWellRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(eventPrefabs.gamblingWellRoomPrefabs, p => p != null);
            Debug.Log($"[FloorRoomSetManager] GamblingWell 프리팹 - 원본: {eventPrefabs.gamblingWellRoomPrefabs.Length}개, 유효: {validPrefabs.Length}개");
            if (validPrefabs.Length > 0) dict[EventRoomType.GamblingWell] = validPrefabs;
        }
        else
        {
            Debug.Log($"[FloorRoomSetManager] GamblingWell 프리팹 - 배열이 null이거나 비어있음 (null: {eventPrefabs.gamblingWellRoomPrefabs == null}, Length: {(eventPrefabs.gamblingWellRoomPrefabs != null ? eventPrefabs.gamblingWellRoomPrefabs.Length : 0)})");
        }
        
        if (eventPrefabs.chestRoomPrefabs != null && eventPrefabs.chestRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(eventPrefabs.chestRoomPrefabs, p => p != null);
            Debug.Log($"[FloorRoomSetManager] ChestRoom 프리팹 - 원본: {eventPrefabs.chestRoomPrefabs.Length}개, 유효: {validPrefabs.Length}개");
            if (validPrefabs.Length > 0) dict[EventRoomType.ChestRoom] = validPrefabs;
        }
        else
        {
            Debug.Log($"[FloorRoomSetManager] ChestRoom 프리팹 - 배열이 null이거나 비어있음 (null: {eventPrefabs.chestRoomPrefabs == null}, Length: {(eventPrefabs.chestRoomPrefabs != null ? eventPrefabs.chestRoomPrefabs.Length : 0)})");
        }
        
        if (eventPrefabs.healingRoomPrefabs != null && eventPrefabs.healingRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(eventPrefabs.healingRoomPrefabs, p => p != null);
            Debug.Log($"[FloorRoomSetManager] Healing 프리팹 - 원본: {eventPrefabs.healingRoomPrefabs.Length}개, 유효: {validPrefabs.Length}개");
            if (validPrefabs.Length > 0) dict[EventRoomType.Healing] = validPrefabs;
        }
        else
        {
            Debug.Log($"[FloorRoomSetManager] Healing 프리팹 - 배열이 null이거나 비어있음 (null: {eventPrefabs.healingRoomPrefabs == null}, Length: {(eventPrefabs.healingRoomPrefabs != null ? eventPrefabs.healingRoomPrefabs.Length : 0)})");
        }
        
        Debug.Log($"[FloorRoomSetManager] ConvertEventRoomPrefabsToDictionary 완료 - 총 {dict.Count}개 EventRoomType 등록됨");
        return dict;
    }
    
    /// <summary>
    /// 층별 프리팹을 RoomType별 딕셔너리로 변환합니다.
    /// 각 RoomType마다 프리팹 배열을 저장합니다.
    /// </summary>
    private Dictionary<RoomType, GameObject[]> ConvertPrefabsToDictionary(FloorRoomPrefabs floorPrefabs)
    {
        Dictionary<RoomType, GameObject[]> dict = new Dictionary<RoomType, GameObject[]>();
        
        if (floorPrefabs == null)
        {
            Debug.LogWarning("[FloorRoomSetManager] 층별 프리팹이 설정되지 않았습니다. 기본 프리팹을 사용하세요.");
            return dict;
        }

        // null이 아닌 프리팹만 필터링하여 배열로 저장
        if (floorPrefabs.startRoomPrefabs != null && floorPrefabs.startRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.startRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Start] = validPrefabs;
        }
        
        if (floorPrefabs.normalRoomPrefabs != null && floorPrefabs.normalRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.normalRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Normal] = validPrefabs;
        }
        
        if (floorPrefabs.exitRoomPrefabs != null && floorPrefabs.exitRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.exitRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Exit] = validPrefabs;
        }
        
        if (floorPrefabs.eventRoomPrefabs != null && floorPrefabs.eventRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.eventRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Event] = validPrefabs;
        }
        
        if (floorPrefabs.trapRoomPrefabs != null && floorPrefabs.trapRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.trapRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Trap] = validPrefabs;
        }
        
        if (floorPrefabs.treasureRoomPrefabs != null && floorPrefabs.treasureRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.treasureRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Treasure] = validPrefabs;
        }
        
        if (floorPrefabs.bossRoomPrefabs != null && floorPrefabs.bossRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.bossRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Boss] = validPrefabs;
        }
        
        if (floorPrefabs.portalRoomPrefabs != null && floorPrefabs.portalRoomPrefabs.Length > 0)
        {
            var validPrefabs = System.Array.FindAll(floorPrefabs.portalRoomPrefabs, p => p != null);
            if (validPrefabs.Length > 0) dict[RoomType.Portal] = validPrefabs;
        }

        return dict;
    }

    /// <summary>
    /// FloorInfo 딕셔너리를 초기화합니다.
    /// </summary>
    private void InitializeFloorInfos()
    {
        if (floorInfos != null)
        {
            Debug.Log($"[FloorRoomSetManager] InitializeFloorInfos: 이미 초기화되어 있습니다. (floorInfos.Count: {floorInfos.Count})");
            return;
        }

        floorInfos = new Dictionary<int, FloorInfo>();
        Dictionary<int, RoomType[]> roomTypes = GetFloorRoomTypes();
        FloorRoomSetManager instance = Instance;

        foreach (var kvp in roomTypes)
        {
            int floorNumber = kvp.Key;
            RoomType[] rooms = kvp.Value;
            FloorRoomPrefabs floorPrefabs = instance.GetFloorPrefabs(floorNumber);
            
            Debug.Log($"[FloorRoomSetManager] {floorNumber}층 초기화 시작 - floorPrefabs: {(floorPrefabs != null ? "not null" : "null")}, " +
                $"eventRoomTypePrefabs: {(floorPrefabs != null && floorPrefabs.eventRoomTypePrefabs != null ? "not null" : "null")}");
            
            Dictionary<RoomType, GameObject[]> prefabDict = instance.ConvertPrefabsToDictionary(floorPrefabs);
            Dictionary<EventRoomType, GameObject[]> eventPrefabDict = instance.ConvertEventRoomPrefabsToDictionary(
                floorPrefabs != null ? floorPrefabs.eventRoomTypePrefabs : null);

            Debug.Log($"[FloorRoomSetManager] {floorNumber}층 초기화 완료 - eventPrefabDict.Count: {eventPrefabDict.Count}");
            floorInfos[floorNumber] = new FloorInfo(floorNumber, rooms, prefabDict, eventPrefabDict);
        }
    }

    /// <summary>
    /// 층 번호에 해당하는 프리팹 설정을 반환합니다.
    /// </summary>
    private FloorRoomPrefabs GetFloorPrefabs(int floorNumber)
    {
        switch (floorNumber)
        {
            case 1: return floor1Prefabs;
            case 2: return floor2Prefabs;
            case 3: return floor3Prefabs;
            default: return null;
        }
    }

    /// <summary>
    /// 층 번호를 넣으면 해당 층의 정보를 반환합니다.
    /// - 정의되지 않은 층이면 null 을 반환하고 경고 로그를 남깁니다.
    /// </summary>
    public static FloorInfo GetFloorInfo(int floorNumber, bool forceReinitialize = false)
    {
        FloorRoomSetManager instance = FindCurrentInstance();
        if (instance == null)
        {
            Debug.LogError($"[FloorRoomSetManager] 현재 씬에 FloorRoomSetManager가 없습니다.");
            return null;
        }
        
        // 강제 재초기화가 요청되면 floorInfos를 null로 설정
        if (forceReinitialize)
        {
            Debug.Log($"[FloorRoomSetManager] GetFloorInfo: floorInfos 강제 재초기화");
            instance.floorInfos = null;
        }
        
        instance.InitializeFloorInfos();

        if (instance.floorInfos.TryGetValue(floorNumber, out FloorInfo info))
        {
            return info;
        }

        Debug.LogWarning($"[FloorRoomSetManager] 정의되지 않은 층 번호입니다: {floorNumber}.");
        return null;
    }

    /// <summary>
    /// 층 번호를 넣으면 해당 층의 방 타입 배열을 바로 반환합니다.
    /// - 정의되지 않은 층이면 null 반환.
    /// </summary>
    public static RoomType[] GetRooms(int floorNumber)
    {
        return GetFloorInfo(floorNumber)?.Rooms;
    }

    /// <summary>
    /// 층 번호와 RoomType을 넣으면, 그 층에 해당 타입 방이 몇 개 있는지 반환합니다.
    /// - 정의되지 않은 층이면 0 반환.
    /// </summary>
    public static int GetRoomCountWithType(int floorNumber, RoomType roomType)
    {
        FloorInfo info = GetFloorInfo(floorNumber);
        if (info == null) return 0;
        return info.GetRoomCountWithType(roomType);
    }

    /// <summary>
    /// 층 번호와 RoomType을 넣으면, 해당 층에서 사용할 프리팹 리스트를 반환합니다.
    /// - 정의되지 않은 층이거나 프리팹이 없으면 null 반환.
    /// </summary>
    public static GameObject[] GetRoomPrefabs(int floorNumber, RoomType roomType)
    {
        FloorInfo info = GetFloorInfo(floorNumber);
        if (info == null || info.FloorRoomPrefabs == null) return null;
        
        info.FloorRoomPrefabs.TryGetValue(roomType, out GameObject[] prefabs);
        return prefabs;
    }

    /// <summary>
    /// 층 번호와 RoomType을 넣으면, 해당 층에서 사용할 프리팹을 랜덤하게 선택하여 반환합니다.
    /// - 정의되지 않은 층이거나 프리팹이 없으면 null 반환.
    /// - 프리팹 리스트가 여러 개인 경우 랜덤하게 하나를 선택합니다.
    /// </summary>
    public static GameObject GetRoomPrefabRandom(int floorNumber, RoomType roomType)
    {
        GameObject[] prefabs = GetRoomPrefabs(floorNumber, roomType);
        if (prefabs == null || prefabs.Length == 0) return null;
        
        // 프리팹 리스트에서 랜덤하게 선택
        return prefabs[Random.Range(0, prefabs.Length)];
    }
    
    /// <summary>
    /// 층 번호와 EventRoomType을 넣으면, 해당 층에서 사용할 이벤트 방 프리팹 리스트를 반환합니다.
    /// - 정의되지 않은 층이거나 프리팹이 없으면 null 반환.
    /// </summary>
    public static GameObject[] GetEventRoomPrefabs(int floorNumber, EventRoomType eventType)
    {
        FloorInfo info = GetFloorInfo(floorNumber);
        if (info == null)
        {
            Debug.LogWarning($"[FloorRoomSetManager] GetEventRoomPrefabs({floorNumber}, {eventType}): floorInfo가 null입니다.");
            return null;
        }
        
        if (info.EventRoomTypePrefabs == null)
        {
            Debug.LogWarning($"[FloorRoomSetManager] GetEventRoomPrefabs({floorNumber}, {eventType}): EventRoomTypePrefabs가 null입니다.");
            return null;
        }
        
        bool hasKey = info.EventRoomTypePrefabs.TryGetValue(eventType, out GameObject[] prefabs);
        if (!hasKey)
        {
            Debug.LogWarning($"[FloorRoomSetManager] GetEventRoomPrefabs({floorNumber}, {eventType}): EventRoomTypePrefabs에 {eventType} 키가 없습니다. (총 {info.EventRoomTypePrefabs.Count}개 키 존재)");
            return null;
        }
        
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning($"[FloorRoomSetManager] GetEventRoomPrefabs({floorNumber}, {eventType}): 프리팹 배열이 null이거나 비어있습니다.");
            return null;
        }
        
        Debug.Log($"[FloorRoomSetManager] GetEventRoomPrefabs({floorNumber}, {eventType}): {prefabs.Length}개 프리팹 반환");
        return prefabs;
    }
    
    /// <summary>
    /// 층 번호와 EventRoomType을 넣으면, 해당 층에서 사용할 이벤트 방 프리팹을 랜덤하게 선택하여 반환합니다.
    /// - 정의되지 않은 층이거나 프리팹이 없으면 null 반환.
    /// - 프리팹 리스트가 여러 개인 경우 랜덤하게 하나를 선택합니다.
    /// </summary>
    public static GameObject GetEventRoomPrefabRandom(int floorNumber, EventRoomType eventType)
    {
        GameObject[] prefabs = GetEventRoomPrefabs(floorNumber, eventType);
        if (prefabs == null || prefabs.Length == 0) return null;
        
        // 프리팹 리스트에서 랜덤하게 선택
        return prefabs[Random.Range(0, prefabs.Length)];
    }
}