using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도굴( Dig ) 이벤트 방 컨트롤러.
/// - 자식들 중 태그가 "DigSpot" 인 오브젝트를 찾아 5개를 관리합니다.
/// - 그 중 2개는 진짜, 3개는 가짜로 랜덤 지정합니다.
/// - 가짜 DigSpot 상호작용 시 사용할 몬스터 프리팹 / HP 패널티 퍼센트를 제공하는 역할을 합니다.
/// </summary>
public class DigRoomController : MonoBehaviour
{
    [Header("Fake DigSpot Settings")]
    [SerializeField] [Tooltip("가짜 DigSpot에서 소환할 적 몬스터 프리팹 (옵션)")]
    private GameObject monsterPrefab;

    [SerializeField] [Tooltip("가짜 DigSpot일 때 플레이어에게 줄 체력 패널티 (퍼센트 단위, 예: 10 = 10%)")]
    private float fakeDamagePercent = 10f;

    [SerializeField] [Tooltip("진짜 DigSpot 개수 (기본 2개)")]
    private int realSpotCount = 2;

    private readonly List<DigSpot> digSpots = new List<DigSpot>();

    private void Awake()
    {
        InitializeDigSpots();
    }

    /// <summary>
    /// 방 안의 DigSpot들을 찾아 초기화하고, 진짜/가짜를 랜덤으로 지정합니다.
    /// </summary>
    private void InitializeDigSpots()
    {
        digSpots.Clear();

        // 자식 트랜스폼 중에서 태그가 "DigSpot" 인 오브젝트 모두 찾기
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (var t in children)
        {
            if (!t.CompareTag("DigSpot")) continue;

            DigSpot spot = t.GetComponent<DigSpot>();
            if (spot == null)
            {
                spot = t.gameObject.AddComponent<DigSpot>();
            }

            spot.Initialize(this);
            digSpots.Add(spot);
        }

        if (digSpots.Count == 0)
        {
            Debug.LogWarning($"[DigRoomController] 방 '{name}' 에서 DigSpot(태그)이 하나도 발견되지 않았습니다.");
            return;
        }

        // 진짜 개수 보정 (DigSpot 개수보다 많지 않도록)
        int realCount = Mathf.Clamp(realSpotCount, 0, digSpots.Count);

        // 랜덤 셔플 후 앞에서부터 realCount 개를 진짜로 지정
        List<DigSpot> shuffled = new List<DigSpot>(digSpots);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int j = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        for (int i = 0; i < shuffled.Count; i++)
        {
            bool isReal = i < realCount;
            shuffled[i].SetIsReal(isReal);
        }

        Debug.Log($"[DigRoomController] 방 '{name}' 에서 DigSpot {digSpots.Count}개 중 진짜 {realCount}개, 가짜 {digSpots.Count - realCount}개로 설정됨.");
    }

    /// <summary>
    /// 가짜 DigSpot이 플레이어에게 줄 데미지 퍼센트(양수, 예: 10 = 10%)를 반환합니다.
    /// </summary>
    public float GetFakeDamagePercent()
    {
        return fakeDamagePercent;
    }

    /// <summary>
    /// 가짜 DigSpot에서 소환할 몬스터 프리팹을 반환합니다. (null일 수 있음)
    /// </summary>
    public GameObject GetMonsterPrefab()
    {
        return monsterPrefab;
    }
}


