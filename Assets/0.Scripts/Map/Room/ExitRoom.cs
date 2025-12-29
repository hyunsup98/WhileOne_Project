using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 출구 방 프리팹
/// BaseRoom을 상속받아 출구 영역 진입 시 다음 씬으로 이동하는 기능을 추가합니다.
/// </summary>
public class ExitRoom : BaseRoom
{
    [Header("Exit Room Settings")]
    [SerializeField] [Tooltip("출구 영역 감지 반경 (Unity unit, 0이면 자동 계산)")]
    private float exitZoneRadius = 0f;
    
    private GameObject exitZoneObject; // 출구 영역 오브젝트 (ExitZone 태그 또는 이름으로 찾음)
    private Vector3 exitCenterPosition; // 출구 중앙 위치
    
    /// <summary>
    /// 방을 초기화합니다.
    /// </summary>
    public override void InitializeRoom(Room room)
    {
        base.InitializeRoom(room);
        
        // 출구 영역 설정
        SetupExitZone();
    }
    
    /// <summary>
    /// 출구 방의 출구 영역을 설정합니다.
    /// ExitZone 오브젝트를 찾거나, RoomCenterMarker를 사용하여 중앙 위치를 계산합니다.
    /// </summary>
    private void SetupExitZone()
    {
        // ExitZone 태그로 오브젝트 찾기
        exitZoneObject = FindExitZoneObject();
        
        if (exitZoneObject != null)
        {
            exitCenterPosition = exitZoneObject.transform.position;
            
            //Debug.Log($"[{name}] 출구 영역 설정 완료 - ExitZone 오브젝트 사용: {exitZoneObject.name}, 위치: {exitCenterPosition}, 반경: {exitZoneRadius}");
        }
    }
    
    /// <summary>
    /// ExitZone 오브젝트를 찾습니다. (태그로 검색)
    /// </summary>
    private GameObject FindExitZoneObject()
    {
        // 태그로 찾기
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("ExitZone");
        foreach (GameObject obj in taggedObjects)
        {
            if (obj.transform.IsChildOf(transform) || obj.transform == transform)
            {
                return obj;
            }
        }

        return null;
    }
    
    /// <summary>
    /// ExitZoneTrigger에서 호출됩니다. 플레이어가 출구 영역에 진입했을 때 처리.
    /// </summary>
    public void OnPlayerEnterExitZone(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[{name}] 플레이어가 출구 영역에 진입했습니다.");

        // ExitZone 오브젝트인 경우에만 처리
        if (exitZoneObject != null)
        {
            // 임시로 DungeonGenerator 층 값 증가
            // TODO: 나중에 DungeonManager나 DataManager에서 층 정보 관리하도록 수정 필요
            DungeonGenerator.currentFloor++;
            LoadNextScene();
        }
    }
    
    /// <summary>
    /// 다음 씬으로 이동합니다.
    /// GameManager나 SceneManager를 사용하여 씬을 전환합니다.
    /// </summary>
    private void LoadNextScene()
    {
        Debug.Log($"[{name}] 출구 방 진입 감지! 다음 씬으로 이동합니다.");
        
        // GameManager를 통해 씬 전환 (GameManager는 수정하지 않고 사용만)
        if (GameManager.Instance != null)
        {
            // 임시: 현재 씬 인덱스 + 1로 다음 씬 로드
            // 나중에 GameManager나 DataManager로 씬 전환 및 데이터 관리를 구현해야 함
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                LoadingManager.nextSceneIndex = nextSceneIndex;
                SceneManager.LoadScene("Loading");
            }
            else
            {
                Debug.LogWarning($"[{name}] 다음 씬이 없습니다. (현재 씬 인덱스: {currentSceneIndex})");
                // TODO: 게임 클리어 처리 또는 메인 메뉴로 이동
            }
        }
    }
}

