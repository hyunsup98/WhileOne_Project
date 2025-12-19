using UnityEngine;

/// <summary>
/// 층당 방 설정 정보를 저장합니다.
/// </summary>
public class FloorSetting
{
    // 1층
    // 방 9개
    // 시작 방 1개, 전투 방 4개, 이벤트 방 1개, 보물 방 1개, 함정 방 1개, 탈출 방 1개
    // 방 타입(RoomType enum)과 개수를 층마다 저장
    public class Floor1
    {
        // RoomType enum 배열로 방 타입과 개수를 저장
        public RoomType[] rooms = new RoomType[]
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
        };

        // 파라미터로 RoomType을 받아 해당 타입의 방 개수를 반환하는 메서드
        public int GetRoomCountWithType(RoomType roomType)
        {
            int count = 0;
            foreach (RoomType type in rooms)
            {
                if (type == roomType)
                {
                    count++;
                }
            }
            return count;
        }
    }

    // 2층
    // 방 12개
    // 시작 방 1개, 전투 방 4개, 이벤트 방 2개, 보물 방 1개, 함정 방 3개, 탈출 방 1개
    public class Floor2
    {
        public RoomType[] rooms = new RoomType[]
        {
            RoomType.Start,
            RoomType.Normal,
            RoomType.Normal,
            RoomType.Normal,
            RoomType.Normal,
            RoomType.Event,
            RoomType.Event,
            RoomType.Treasure,
            RoomType.Trap,
            RoomType.Trap,
            RoomType.Trap,
            RoomType.Exit
        };

        public int GetRoomCountWithType(RoomType roomType)
        {
            int count = 0;
            foreach (RoomType type in rooms)
            {
                if (type == roomType)
                {
                    count++;
                }
            }
            return count;
        }
    }

    // 3층
    // 방 10개
    // 시작 방 1개, 전투 방 5개, 이벤트 방 1개, 보물 방 1개, 함정 방 1개, 보스 방 1개
    public class Floor3
    {
        public RoomType[] rooms = new RoomType[]
        {
            RoomType.Start,
            RoomType.Normal,
            RoomType.Normal,
            RoomType.Normal,
            RoomType.Normal,
            RoomType.Normal,
            RoomType.Event,
            RoomType.Treasure,
            RoomType.Trap,
            RoomType.Boss
        };

        public int GetRoomCountWithType(RoomType roomType)
        {
            int count = 0;
            foreach (RoomType type in rooms)
            {
                if (type == roomType)
                {
                    count++;
                }
            }
            return count;
        }
    }

    // 파라미터로 층 정보를 받아서 GetRoomCountWithType
    //public static int GetRoomCountWithType(int floorNumber, RoomType roomType)
    //{
    //    switch (floorNumber)
    //    {
    //        case 1:
    //            Floor1 floor1 = new Floor1();
    //            return floor1.GetRoomCountWithType(roomType);
    //        case 2:
    //            Floor2 floor2 = new Floor2();
    //            return floor2.GetRoomCountWithType(roomType);
    //        case 3:
    //            Floor3 floor3 = new Floor3();
    //            return floor3.GetRoomCountWithType(roomType);
    //        default:
    //            Debug.LogError("Invalid floor number");
    //            return 0;
    //    }
    //}
}