using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 포탈 방 프리팹
/// BaseRoom을 상속받아 포탈 영역 진입 시 보스 방 씬으로 이동하는 기능을 추가합니다.
/// </summary>
public class PortalRoom : BaseRoom
{
    [Header("Portal Room Settings")]
    [SerializeField] [Tooltip("포탈 영역 감지 반경 (Unity unit, 0이면 자동 계산)")]
    private float portalZoneRadius = 0f;
    [SerializeField] [Tooltip("보스 방 씬 이름 (없으면 자동으로 찾음)")]
    private string bossSceneName = "BossRoom"; // 보스 방 씬 이름
    
    private GameObject portalZoneObject; // 포탈 영역 오브젝트 (PortalZone 태그 또는 이름으로 찾음)
    private Vector3 portalCenterPosition; // 포탈 중앙 위치
    
    /// <summary>
    /// 방을 초기화합니다.
    /// </summary>
    public override void InitializeRoom(Room room)
    {
        base.InitializeRoom(room);
        
        // 포탈 영역 설정
        SetupPortalZone();
    }
    
    /// <summary>
    /// 포탈 방의 포탈 영역을 설정합니다.
    /// PortalZone 오브젝트를 찾거나, RoomCenterMarker를 사용하여 중앙 위치를 계산합니다.
    /// </summary>
    private void SetupPortalZone()
    {
        // PortalZone 태그로 오브젝트 찾기
        portalZoneObject = FindPortalZoneObject();
        
        if (portalZoneObject != null)
        {
            portalCenterPosition = portalZoneObject.transform.position;
            
            Debug.Log($"[{name}] 포탈 영역 설정 완료 - PortalZone 오브젝트 사용: {portalZoneObject.name}, 위치: {portalCenterPosition}, 반경: {portalZoneRadius}");
        }
        else
        {
            Debug.LogWarning($"[{name}] PortalZone 오브젝트를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// PortalZone 오브젝트를 찾습니다. (태그로 검색)
    /// </summary>
    private GameObject FindPortalZoneObject()
    {
        // 태그로 찾기
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("PortalZone");
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
    /// PortalZoneTrigger에서 호출됩니다. 플레이어가 포탈 영역에 진입했을 때 처리.
    /// </summary>
    public void OnPlayerEnterPortalZone(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[{name}] 플레이어가 포탈 영역에 진입했습니다.");

        // PortalZone 오브젝트인 경우에만 처리
        if (portalZoneObject != null)
        {
            LoadBossScene();
        }
    }
    
    /// <summary>
    /// 보스 방 씬으로 이동합니다.
    /// </summary>
    private void LoadBossScene()
    {
        Debug.Log($"[{name}] 포탈 방 진입 감지! 보스 방 씬으로 이동합니다.");
        
        // GameManager를 통해 씬 전환
        if (GameManager.Instance != null)
        {
            // 보스 방 씬 이름이 설정되어 있으면 해당 씬으로 이동
            if (!string.IsNullOrEmpty(bossSceneName))
            {
                // 씬이 빌드 설정에 있는지 확인
                bool sceneExists = false;
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    if (sceneName == bossSceneName)
                    {
                        sceneExists = true;
                        LoadingManager.nextSceneIndex = i;
                        SceneManager.LoadScene("Loading");
                        break;
                    }
                }
                
                if (!sceneExists)
                {
                    Debug.LogError($"[{name}] 보스 방 씬 '{bossSceneName}'을 찾을 수 없습니다. 빌드 설정에 추가되어 있는지 확인하세요.");
                }
            }
            else
            {
                Debug.LogWarning($"[{name}] 보스 방 씬 이름이 설정되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogError($"[{name}] GameManager.Instance가 null입니다.");
        }
    }
}

