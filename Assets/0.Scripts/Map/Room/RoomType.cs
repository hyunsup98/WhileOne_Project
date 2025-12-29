/// <summary>
/// 방의 타입을 정의하는 열거형
/// </summary>
public enum RoomType
{
    Start,      // 시작 방
    Normal,     // 일반 방
    Exit,       // 출구 방 (다음층 입구)
    Event,      // 이벤트 방
    Trap,        // 함정 방
    Treasure,   // 보물 방
    Boss,       // 보스 방
    Portal      // 포탈 방 (3층에서 보스 방으로 이동)
    // 새로운 방 타입은 BaseRoom을 상속받아 구성
}

