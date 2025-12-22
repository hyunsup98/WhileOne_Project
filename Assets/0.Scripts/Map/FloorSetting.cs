using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 층별 방 타입 구성을 정의하고, 층 정보를 편하게 조회할 수 있도록 제공하는 유틸리티.
/// </summary>
public static class FloorSetting
{
    /// <summary>
    /// 한 층에 대한 정보(방 타입 배열 + 편의 메서드)를 담는 클래스.
    /// </summary>
    public class FloorInfo
    {
        /// <summary>층 번호 (1, 2, 3, ...)</summary>
        public int FloorNumber { get; }

        /// <summary>이 층에 포함된 방 타입 배열 (순서는 자유, 필요시 사용)</summary>
        public RoomType[] Rooms { get; }

        public FloorInfo(int floorNumber, RoomType[] rooms)
        {
            FloorNumber = floorNumber;
            Rooms = rooms;
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
    /// </summary>
    private static readonly Dictionary<int, FloorInfo> floorInfos = new Dictionary<int, FloorInfo>
    {
        {
            1,
            new FloorInfo(
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
                })
        },
        {
            2,
            new FloorInfo(
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
                })
        },
        {
            3,
            new FloorInfo(
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
                })
        }
    };

    /// <summary>
    /// 층 번호를 넣으면 해당 층의 정보를 반환합니다.
    /// - 정의되지 않은 층이면 null 을 반환하고 경고 로그를 남깁니다.
    /// </summary>
    public static FloorInfo GetFloorInfo(int floorNumber)
    {
        if (floorInfos.TryGetValue(floorNumber, out FloorInfo info))
        {
            return info;
        }

        Debug.LogWarning($"[FloorSetting] 정의되지 않은 층 번호입니다: {floorNumber}.");
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
}