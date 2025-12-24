using UnityEngine;
using System.Collections.Generic;

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
    [Tooltip("이벤트 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] eventRoomPrefabs;
    [Tooltip("함정 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] trapRoomPrefabs;
    [Tooltip("보물 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] treasureRoomPrefabs;
    [Tooltip("보스 방 프리팹 리스트 (여러 개 설정 시 랜덤 선택)")]
    public GameObject[] bossRoomPrefabs;
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

        public FloorInfo(int floorNumber, RoomType[] rooms, Dictionary<RoomType, GameObject[]> floorRoomPrefabs)
        {
            FloorNumber = floorNumber;
            Rooms = rooms;
            FloorRoomPrefabs = floorRoomPrefabs;
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
    /// 층 번호 → FloorInfo 매핑 테이블.
    /// 런타임에 동적으로 생성됩니다.
    /// </summary>
    private static Dictionary<int, FloorInfo> floorInfos;

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
                    RoomType.Boss
                }
            }
        };
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

        return dict;
    }

    /// <summary>
    /// FloorInfo 딕셔너리를 초기화합니다.
    /// </summary>
    private void InitializeFloorInfos()
    {
        if (floorInfos != null) return;

        floorInfos = new Dictionary<int, FloorInfo>();
        Dictionary<int, RoomType[]> roomTypes = GetFloorRoomTypes();
        FloorRoomSetManager instance = Instance;

        foreach (var kvp in roomTypes)
        {
            int floorNumber = kvp.Key;
            RoomType[] rooms = kvp.Value;
            FloorRoomPrefabs floorPrefabs = instance.GetFloorPrefabs(floorNumber);
            Dictionary<RoomType, GameObject[]> prefabDict = instance.ConvertPrefabsToDictionary(floorPrefabs);

            floorInfos[floorNumber] = new FloorInfo(floorNumber, rooms, prefabDict);
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
    public static FloorInfo GetFloorInfo(int floorNumber)
    {
        if (floorInfos == null)
        {
            Instance.InitializeFloorInfos();
        }

        if (floorInfos.TryGetValue(floorNumber, out FloorInfo info))
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
}