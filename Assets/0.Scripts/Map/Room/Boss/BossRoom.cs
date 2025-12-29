using UnityEngine;
using System;

/// <summary>
/// 보스방 씬을 관리하는 클래스
/// - 플레이어 진입 시 입구 차단
/// - 보스 사망 시 위쪽 문 열기 및 OrbRoom 활성화
/// </summary>
public class BossRoom : MonoBehaviour
{
    [Header("입구 차단 설정")]
    [SerializeField] [Tooltip("플레이어 진입 시 차단할 입구 오브젝트들 (Collider2D가 있어야 함)")]
    private GameObject[] entranceBlockers; // 입구 차단 오브젝트들
    
    [SerializeField] [Tooltip("플레이어 진입 감지 영역 (Trigger Collider2D가 있어야 함, 이 오브젝트에 BossRoomEntryZone 컴포넌트를 붙여야 함)")]
    private GameObject entryZone; // 플레이어 진입 감지 영역
    
    [Header("보스 설정")]
    [SerializeField] [Tooltip("보스 프리팹 (EntryZone 중앙에 소환됨)")]
    private GameObject bossPrefab; // 보스 프리팹
    
    [SerializeField] [Tooltip("보스 오브젝트 (소환된 보스 인스턴스, 자동 설정됨)")]
    private GameObject bossObject; // 보스 오브젝트 (소환 후 설정)
    
    [Header("보스 사망 후 설정")]
    [SerializeField] [Tooltip("보스 사망 시 열릴 위쪽 문 오브젝트들")]
    private GameObject[] upperDoors; // 위쪽 문 오브젝트들
    
    [SerializeField] [Tooltip("보스 사망 시 활성화할 OrbRoom 오브젝트")]
    private GameObject orbRoom; // OrbRoom 오브젝트
    
    private bool hasPlayerEntered = false; // 플레이어 진입 여부
    private bool isBossDead = false; // 보스 사망 여부
    private IDead bossDeadInterface; // 보스의 IDead 인터페이스
    
    private void Awake()
    {
        InitializeBossRoom();
    }
    
    private void Start()
    {
        // EntryZone에 트리거 스크립트 설정
        SetupEntryZone();
        
        // 초기에는 보스가 소환되지 않았으므로 구독하지 않음
        // 보스 소환 후 SubscribeToBossDeath() 호출
    }
    
    /// <summary>
    /// EntryZone에 트리거 스크립트를 설정합니다.
    /// </summary>
    private void SetupEntryZone()
    {
        if (entryZone == null)
        {
            Debug.LogWarning("[BossRoom] EntryZone이 설정되지 않았습니다.");
            return;
        }
        
        // BossRoomEntryZone 컴포넌트 추가 또는 가져오기
        BossRoomEntryZone entryZoneScript = entryZone.GetComponent<BossRoomEntryZone>();
        if (entryZoneScript == null)
        {
            entryZoneScript = entryZone.AddComponent<BossRoomEntryZone>();
        }
        
        // BossRoom 참조 설정
        entryZoneScript.SetBossRoom(this);
        
        Debug.Log($"[BossRoom] EntryZone 설정 완료: {entryZone.name}");
    }
    
    /// <summary>
    /// 보스방 초기화
    /// </summary>
    private void InitializeBossRoom()
    {
        // 입구 차단 오브젝트 초기 상태 확인
        if (entranceBlockers != null && entranceBlockers.Length > 0)
        {
            foreach (var blocker in entranceBlockers)
            {
                if (blocker != null)
                {
                    // 초기에는 입구가 열려있어야 함 (비활성화)
                    blocker.SetActive(false);
                }
            }
        }
        
        // 위쪽 문 초기 상태 확인
        if (upperDoors != null && upperDoors.Length > 0)
        {
            foreach (var door in upperDoors)
            {
                if (door != null)
                {
                    // 초기에는 위쪽 문이 닫혀있어야 함 (활성화)
                    door.SetActive(true);
                }
            }
        }
        
        // OrbRoom 초기 상태 확인
        if (orbRoom != null)
        {
            // 초기에는 OrbRoom이 비활성화되어 있어야 함
            orbRoom.SetActive(false);
        }
        
        Debug.Log("[BossRoom] 보스방 초기화 완료");
    }
    
    /// <summary>
    /// 보스의 IDead 인터페이스를 찾아서 OnDeath 이벤트를 구독합니다.
    /// </summary>
    private void SubscribeToBossDeath()
    {
        if (bossObject == null)
        {
            Debug.LogWarning("[BossRoom] 보스 오브젝트가 설정되지 않았습니다.");
            return;
        }
        
        // MonsterView 컴포넌트를 통해 MonsterPresenter에 접근
        MonsterView monsterView = bossObject.GetComponent<MonsterView>();
        if (monsterView != null && monsterView.Presenter != null)
        {
            MonsterPresenter presenter = monsterView.Presenter;
            if (presenter is IDead deadInterface)
            {
                bossDeadInterface = deadInterface;
                bossDeadInterface.OnDeath += OnBossDeath;
                Debug.Log($"[BossRoom] 보스의 OnDeath 이벤트를 구독했습니다: {bossObject.name}");
                return;
            }
        }
        
        // 자식 오브젝트에서도 MonsterView 찾기
        monsterView = bossObject.GetComponentInChildren<MonsterView>();
        if (monsterView != null && monsterView.Presenter != null)
        {
            MonsterPresenter presenter = monsterView.Presenter;
            if (presenter is IDead deadInterface)
            {
                bossDeadInterface = deadInterface;
                bossDeadInterface.OnDeath += OnBossDeath;
                Debug.Log($"[BossRoom] 보스의 OnDeath 이벤트를 구독했습니다 (자식에서 찾음): {bossObject.name}");
                return;
            }
        }
        
        Debug.LogWarning($"[BossRoom] 보스 오브젝트 '{bossObject.name}'에서 MonsterView 또는 IDead 인터페이스를 찾을 수 없습니다.");
    }
    
    /// <summary>
    /// 플레이어가 진입 영역에 들어왔을 때 호출됩니다.
    /// BossRoomEntryZone에서 호출합니다.
    /// </summary>
    public void OnPlayerEnterEntryZone()
    {
        // 이미 진입했거나 보스가 죽었으면 무시
        if (hasPlayerEntered || isBossDead) return;
        
        // 입구 차단
        BlockEntrance();
        
        // 보스 소환 (EntryZone 중앙)
        //SpawnBoss();
        
        hasPlayerEntered = true;
        
        Debug.Log("[BossRoom] 플레이어가 진입했습니다. 입구를 차단하고 보스를 소환합니다.");
    }
    
    /// <summary>
    /// EntryZone 중앙에 보스를 소환합니다.
    /// </summary>
    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("[BossRoom] 보스 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        if (entryZone == null)
        {
            Debug.LogWarning("[BossRoom] EntryZone이 설정되지 않아 보스를 소환할 수 없습니다.");
            return;
        }
        
        // EntryZone의 중앙 위치 계산
        Vector3 spawnPosition = entryZone.transform.position;
        
        // EntryZone의 Collider2D가 있으면 그 중심 사용
        Collider2D entryCollider = entryZone.GetComponent<Collider2D>();
        if (entryCollider != null)
        {
            spawnPosition = entryCollider.bounds.center;
        }
        
        // 보스 소환
        bossObject = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        
        Debug.Log($"[BossRoom] 보스를 소환했습니다: {bossObject.name}, 위치: {spawnPosition}");
        
        // 보스 소환 후 IDead 인터페이스 구독
        SubscribeToBossDeath();
    }
    
    /// <summary>
    /// 입구를 차단합니다.
    /// </summary>
    private void BlockEntrance()
    {
        if (entranceBlockers == null || entranceBlockers.Length == 0)
        {
            Debug.LogWarning("[BossRoom] 입구 차단 오브젝트가 설정되지 않았습니다.");
            return;
        }
        
        foreach (var blocker in entranceBlockers)
        {
            if (blocker != null)
            {
                blocker.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// 보스가 죽었을 때 호출됩니다.
    /// </summary>
    private void OnBossDeath()
    {
        if (isBossDead) return; // 중복 호출 방지
        
        isBossDead = true;
        
        Debug.Log("[BossRoom] 보스가 사망했습니다. 위쪽 문을 열고 OrbRoom을 활성화합니다.");
        
        // 위쪽 문 열기
        OpenUpperDoors();
        
        // OrbRoom 활성화
        ActivateOrbRoom();
    }
    
    /// <summary>
    /// 위쪽 문을 엽니다.
    /// </summary>
    private void OpenUpperDoors()
    {
        if (upperDoors == null || upperDoors.Length == 0)
        {
            Debug.LogWarning("[BossRoom] 위쪽 문 오브젝트가 설정되지 않았습니다.");
            return;
        }
        
        foreach (var door in upperDoors)
        {
            if (door != null)
            {
                door.SetActive(false); // 문 오브젝트를 비활성화하여 열기
            }
        }
    }
    
    /// <summary>
    /// OrbRoom을 활성화합니다.
    /// </summary>
    private void ActivateOrbRoom()
    {
        if (orbRoom == null)
        {
            Debug.LogWarning("[BossRoom] OrbRoom 오브젝트가 설정되지 않았습니다.");
            return;
        }
        
        orbRoom.SetActive(true);
    }
    
    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    private void OnDestroy()
    {
        if (bossDeadInterface != null)
        {
            bossDeadInterface.OnDeath -= OnBossDeath;
        }
    }
}

