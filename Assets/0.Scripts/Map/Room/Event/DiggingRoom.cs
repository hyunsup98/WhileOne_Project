using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 도굴 방 이벤트
/// 15x15 크기의 방, 중앙에 경고 표지판이 있고 5개 중 진짜 2개 가짜 3개
/// </summary>
public class DiggingRoom : BaseEventRoom
{
    [Header("Digging Room Settings")]
    [SerializeField] [Tooltip("경고 표지판 오브젝트 (중앙에 배치)")]
    private GameObject warningSign;
    [SerializeField] [Tooltip("Dig Spot 타일")]
    private Tile digSpotTile;
    [SerializeField] [Tooltip("진짜 Dig Spot 개수")]
    private int realDigSpotCount = 2;
    [SerializeField] [Tooltip("가짜 Dig Spot 개수")]
    private int fakeDigSpotCount = 3;
    [SerializeField] [Tooltip("가짜를 파면 나오는 적대 몬스터 프리팹")]
    private GameObject enemyPrefab;
    [SerializeField] [Tooltip("가짜를 파면 받는 HP 피해 비율 (0.1 = 10%)")]
    private float fakeDigDamageRatio = 0.1f;
    
    private Tilemap tilemap;
    private RoomController roomController;
    private int totalDigSpots = 0;
    private int realDigSpotsFound = 0;
    private int fakeDigSpotsFound = 0;
    
    protected override void InitializeEventRoom()
    {
        // RoomController 찾기 및 이벤트 구독
        roomController = GetComponent<RoomController>();
        if (roomController != null)
        {
            // RoomController의 DigSpotTileMap 사용
            tilemap = roomController.DigSpotTileMap;
            roomController.OnDigSpotRemoved += HandleDigSpotRemoved;
        }
        else
        {
            Debug.LogWarning("[DiggingRoom] RoomController를 찾을 수 없습니다. 자식 타일맵을 찾습니다.");
            // RoomController가 없으면 자식 타일맵 사용
            tilemap = GetComponentInChildren<Tilemap>();
        }
        
        if (tilemap == null)
        {
            Debug.LogError("[DiggingRoom] 타일맵을 찾을 수 없습니다.");
            return;
        }
        
        // 경고 표지판 배치 (방 중앙)
        if (warningSign != null)
        {
            Vector3 center = GetRoomCenter();
            warningSign.transform.position = center;
            warningSign.SetActive(true);
        }
        
        // Dig Spot 배치
        PlaceDigSpots();
        
        // 규칙 안내 (경고 표지판에 표시)
        ShowRules();
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (roomController != null)
        {
            roomController.OnDigSpotRemoved -= HandleDigSpotRemoved;
        }
    }
    
    /// <summary>
    /// RoomController의 Dig 이벤트 핸들러
    /// </summary>
    private void HandleDigSpotRemoved(Vector3Int cellPosition, Tilemap sourceTilemap, TileBase removedTile)
    {
        // DiggingRoom의 타일맵에서 발생한 이벤트인지 확인
        if (sourceTilemap != tilemap) return;
        
        // Dig Spot 타일인지 확인
        if (removedTile != digSpotTile) return;
        
        // 플레이어 찾기 (헬퍼 메서드 사용)
        Player player = GetPlayer();
        if (player == null) return;
        
        // Dig Spot 처리
        OnDigSpotDug(cellPosition, player);
    }
    
    /// <summary>
    /// Dig Spot을 배치합니다.
    /// </summary>
    private void PlaceDigSpots()
    {
        if (tilemap == null || digSpotTile == null) return;
        
        // 방 크기 계산 (15x15 타일)
        int roomSize = 15;
        int centerX = roomSize / 2;
        int centerY = roomSize / 2;
        
        // Dig Spot 배치 가능한 위치 리스트 생성 (중앙 제외)
        List<Vector3Int> availablePositions = new List<Vector3Int>();
        for (int x = 1; x < roomSize - 1; x++)
        {
            for (int y = 1; y < roomSize - 1; y++)
            {
                // 중앙 영역 제외 (경고 표지판 위치)
                if (Mathf.Abs(x - centerX) > 2 || Mathf.Abs(y - centerY) > 2)
                {
                    availablePositions.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        
        // 랜덤하게 섞기
        for (int i = 0; i < availablePositions.Count; i++)
        {
            Vector3Int temp = availablePositions[i];
            int randomIndex = Random.Range(i, availablePositions.Count);
            availablePositions[i] = availablePositions[randomIndex];
            availablePositions[randomIndex] = temp;
        }
        
        // 진짜 Dig Spot 배치
        for (int i = 0; i < realDigSpotCount && i < availablePositions.Count; i++)
        {
            Vector3Int pos = availablePositions[i];
            tilemap.SetTile(pos, digSpotTile);
            totalDigSpots++;
        }
        
        // 가짜 Dig Spot 배치
        int startIndex = realDigSpotCount;
        for (int i = 0; i < fakeDigSpotCount && startIndex + i < availablePositions.Count; i++)
        {
            Vector3Int pos = availablePositions[startIndex + i];
            tilemap.SetTile(pos, digSpotTile);
            totalDigSpots++;
        }
    }
    
    /// <summary>
    /// 규칙을 안내합니다.
    /// </summary>
    private void ShowRules()
    {
        string rules = "도굴 방 규칙:\n" +
                      "5개 중 진짜는 2개, 가짜는 3개\n" +
                      "가짜를 파면 적대 몬스터가 나오거나 HP가 10% 감소합니다.";
        
        Debug.Log($"[DiggingRoom] {rules}");
        // TODO: UI에 규칙 표시
    }
    
    /// <summary>
    /// Dig Spot을 파는 이벤트 처리
    /// (타일은 이미 RoomController에서 제거되었으므로 여기서는 로직만 처리)
    /// </summary>
    public void OnDigSpotDug(Vector3Int position, Player player)
    {
        if (player == null) return;
        
        // 진짜인지 가짜인지 판단 (확률 기반)
        bool isReal = DetermineIfReal();
        
        if (isReal)
        {
            realDigSpotsFound++;
            OnRealDigSpotFound(player);
        }
        else
        {
            fakeDigSpotsFound++;
            OnFakeDigSpotFound(player);
        }
    }
    
    /// <summary>
    /// 진짜인지 가짜인지 판단합니다.
    /// </summary>
    private bool DetermineIfReal()
    {
        // 남은 진짜 개수와 가짜 개수에 따라 확률 계산
        int remainingReal = realDigSpotCount - realDigSpotsFound;
        int remainingFake = fakeDigSpotCount - fakeDigSpotsFound;
        int totalRemaining = remainingReal + remainingFake;
        
        if (totalRemaining == 0) return false;
        
        float realProbability = (float)remainingReal / totalRemaining;
        return Random.Range(0f, 1f) < realProbability;
    }
    
    /// <summary>
    /// 진짜 Dig Spot을 찾았을 때
    /// </summary>
    private void OnRealDigSpotFound(Player player)
    {
        Debug.Log("[DiggingRoom] 진짜 Dig Spot을 찾았습니다!");
        
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
    /// 가짜 Dig Spot을 찾았을 때
    /// </summary>
    private void OnFakeDigSpotFound(Player player)
    {
        Debug.Log("[DiggingRoom] 가짜 Dig Spot을 찾았습니다!");
        
        // 50% 확률로 적대 몬스터 소환 또는 HP 피해
        if (Random.Range(0f, 1f) < 0.5f)
        {
            // 적대 몬스터 소환
            // TODO: 몬스터 스폰 시스템이 있다면 해당 시스템 사용 (예: MonsterSpawnManager.SpawnEnemy())
            // 현재는 프리팹 직접 인스턴스화 사용
            if (enemyPrefab != null)
            {
                Vector3 spawnPos = GetRoomCenter() + (Vector3)(Random.insideUnitCircle * 3f);
                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            // HP 피해 (헬퍼 메서드 사용)
            player.ChangedHealth += -player.MaxHp * fakeDigDamageRatio;
            //ChangePlayerHealthByRatio(-fakeDigDamageRatio, player);
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

