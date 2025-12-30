using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 보물 방 프리팹
/// BaseRoom을 상속받아 기본 방 구조를 가집니다.
/// </summary>
public class TreasureRoom : BaseRoom
{
    /// <summary>
    /// 보물 방에 Dig Spot을 배치합니다.
    /// 1개는 100% 배치, 다른 1개는 20% 확률로 추가 배치
    /// </summary>
    public void PlaceDigSpots(Grid unityGrid)
    {
        // 첫 번째 Dig Spot 배치 (100%)
        DungeonItemPlacer.PlaceDigSpotInRoom(gameObject, unityGrid);
        
        // 두 번째 Dig Spot 배치 (20% 확률)
        if (Random.Range(0f, 100f) < 20f)
        {
            DungeonItemPlacer.PlaceDigSpotInRoom(gameObject, unityGrid);
        }
    }
}

