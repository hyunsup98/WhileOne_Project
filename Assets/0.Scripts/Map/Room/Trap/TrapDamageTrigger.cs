using System.Collections;
using UnityEngine;

/// <summary>
/// 함정의 피격 판정을 처리하는 컴포넌트
/// 플레이어가 특정 애니메이션 구간에 지나가면 체력을 감소시킵니다.
/// </summary>
public class TrapDamageTrigger : MonoBehaviour
{
    [SerializeField] [Tooltip("데미지를 주는 간격 (초 단위, 같은 함정에 연속으로 데미지를 받지 않도록)")]
    private float damageCooldown = 0.5f;
    
    private const float INVINCIBILITY_DURATION = 0.7f; // 무적 지속 시간 (PlayerDamage의 _finsihTime과 동일)
    
    [Header("Animation Settings")]
    [SerializeField] [Tooltip("애니메이션에서 데미지를 주는 구간의 애니메이션 이름 (비어있으면 항상 데미지)")]
    private string damageAnimationName = "";
    
    [SerializeField] [Tooltip("애니메이션에서 데미지를 주는 구간의 시작 시간 (초 단위, 실제 애니메이션 시간)")]
    private float damageStartTime = 0.6f;
    
    [SerializeField] [Tooltip("애니메이션에서 데미지를 주는 구간의 끝 시간 (초 단위, 실제 애니메이션 시간)")]
    private float damageEndTime = 1.1f;
    
    [Header("Collider Settings")]
    [SerializeField] [Tooltip("플레이어 레이어")]
    private LayerMask playerLayer;
    
    private Animator animator;
    private float lastDamageTime = -999f; // 초기값을 매우 작게 설정하여 첫 데미지 가능하도록
    private bool isPlayerInTrigger = false;
    private Player currentPlayer = null;
    private bool hasDamagedInCurrentAnimationCycle = false; // 현재 애니메이션 사이클에서 데미지를 줬는지
    private float lastAnimationNormalizedTime = -1f; // 마지막 체크한 애니메이션 정규화 시간

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // 플레이어 레이어가 설정되지 않았으면 자동으로 찾기
        if (playerLayer.value == 0)
        {
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            if (playerLayerIndex != -1)
            {
                playerLayer = 1 << playerLayerIndex;
                Debug.Log($"[TrapDamageTrigger] 플레이어 레이어 자동 설정: {playerLayerIndex}");
            }
            else
            {
                Debug.LogWarning($"[TrapDamageTrigger] 'Player' 레이어를 찾을 수 없습니다. 함정: {gameObject.name}");
            }
        }
        
        // Collider2D가 없으면 자동으로 추가
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            // BoxCollider2D 추가 (함정 크기에 맞게 조정 필요)
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector2(0.8f, 0.8f); // 기본 크기 (필요시 조정)
            Debug.Log($"[TrapDamageTrigger] Collider2D 자동 추가 - 함정: {gameObject.name}");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.Log($"[TrapDamageTrigger] Collider2D를 Trigger로 설정 - 함정: {gameObject.name}");
        }
        
        Debug.Log($"[TrapDamageTrigger] 초기화 완료 - 함정: {gameObject.name}, 활성화: {gameObject.activeSelf}, Animator: {animator != null}, Collider: {GetComponent<Collider2D>() != null}");
    }
    
    private void Start()
    {
        // Start에서도 한 번 더 확인
        if (animator == null)
        {
            Debug.LogWarning($"[TrapDamageTrigger] Animator를 찾을 수 없습니다. 함정: {gameObject.name}");
        }
        
        if (string.IsNullOrEmpty(damageAnimationName))
        {
            Debug.Log($"[TrapDamageTrigger] 애니메이션 이름이 설정되지 않아 항상 데미지 적용 모드입니다. 함정: {gameObject.name}");
        }
        else
        {
            //Debug.Log($"[TrapDamageTrigger] 데미지 애니메이션: {damageAnimationName}, 구간: {damageStartTime}초 ~ {damageEndTime}초, 함정: {gameObject.name}");
        }
    }
    
    private void Update()
    {
        // 함정이 비활성화되어 있으면 동작하지 않음
        if (!gameObject.activeSelf)
        {
            return;
        }
        
        // 플레이어가 트리거 안에 있고, 쿨다운이 지났으면 데미지 적용을 시도
        if (isPlayerInTrigger && currentPlayer != null)
        {
            // 무적 상태가 너무 오래 지속되고 있다면 초기화 (다른 함정에서 받은 무적 상태가 해제되지 않은 경우)
            if (currentPlayer.GetDamage.IsDamaged)
            {
                // 무적 상태가지만 실제로는 무적 시간이 지났을 수 있으므로
                // PlayerDamage의 Blink 코루틴이 끝났는지 확인하기 위해
                // 여기서는 일단 무적 상태 체크만 하고, ApplyDamage에서 처리
            }
            
            // 쿨다운 체크
            float timeSinceLastDamage = Time.time - lastDamageTime;
            if (timeSinceLastDamage >= damageCooldown)
            {
                //// 1) damageAnimationName 이 비어 있으면 애니메이션 구간 상관없이 쿨다운마다 데미지 적용
                //if (string.IsNullOrEmpty(damageAnimationName))
                //{
                //    ApplyDamage();
                //    // 애니메이션 구간 플래그는 사용하지 않음
                //    lastAnimationNormalizedTime = -1f;
                //    hasDamagedInCurrentAnimationCycle = false;
                //    return;
                //}

                // 2) 특정 애니메이션 구간에서만 데미지를 주는 모드
                bool isInDamageRange = IsInDamageAnimationRange();
                float currentNormalizedTime = GetCurrentNormalizedTime();
                
                if (isInDamageRange)
                {
                    // 애니메이션 구간에 진입했는지 확인 (이전 프레임에는 없었고 지금은 있음)
                    // 이전 시간이 데미지 구간 밖에 있고, 현재 시간이 데미지 구간 안에 있으면 진입
                    bool wasOutsideRange = lastAnimationNormalizedTime < 0 || 
                                         lastAnimationNormalizedTime < damageStartTime || 
                                         lastAnimationNormalizedTime > damageEndTime;
                    bool isNowInRange = currentNormalizedTime >= damageStartTime && currentNormalizedTime <= damageEndTime;
                    bool isEnteringDamageRange = !hasDamagedInCurrentAnimationCycle && wasOutsideRange && isNowInRange;
                    
                    if (isEnteringDamageRange)
                    {
                        ApplyDamage();
                        hasDamagedInCurrentAnimationCycle = true;
                    }
                }
                else
                {
                    // 데미지 구간을 벗어나면 플래그 리셋
                    if (hasDamagedInCurrentAnimationCycle)
                    {
                        hasDamagedInCurrentAnimationCycle = false;
                    }
                }
                
                lastAnimationNormalizedTime = currentNormalizedTime;
            }
        }
    }
    
    /// <summary>
    /// 현재 애니메이션이 데미지를 주는 구간인지 확인합니다.
    /// </summary>
    private bool IsInDamageAnimationRange()
    {
        // 애니메이션 이름이 설정되지 않았으면 항상 데미지 적용
        if (string.IsNullOrEmpty(damageAnimationName))
        {
            return true;
        }
        
        if (animator == null)
        {
            return false;
        }
        
        // Animator가 비활성화되어 있으면 false
        if (!animator.enabled)
        {
            return false;
        }
        
        // 현재 재생 중인 애니메이션 상태 확인
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 애니메이션 이름이 일치하는지 확인
        if (stateInfo.IsName(damageAnimationName))
        {
            // 실제 애니메이션 시간 계산 (초 단위)
            // normalizedTime은 반복 횟수를 포함하므로, 실제 시간 = normalizedTime * 애니메이션 길이
            float animationLength = stateInfo.length; // 애니메이션 길이 (초)
            float normalizedTime = stateInfo.normalizedTime % 1f; // 현재 사이클의 정규화된 시간 (0~1)
            float actualTime = normalizedTime * animationLength; // 현재 사이클의 실제 시간
            
            // 데미지 구간 안에 있는지 확인
            bool inRange = actualTime >= damageStartTime && actualTime <= damageEndTime;
            
            // 디버그 로그 (주석 처리 가능)
            // if (inRange && isPlayerInTrigger)
            // {
            //     Debug.Log($"[TrapDamageTrigger] 데미지 구간 내 - 실제 시간: {actualTime:F2}초, 구간: {damageStartTime}~{damageEndTime}초");
            // }
            
            return inRange;
        }
        
        return false;
    }
    
    /// <summary>
    /// 현재 애니메이션의 실제 시간(초)을 가져옵니다.
    /// </summary>
    private float GetCurrentNormalizedTime()
    {
        if (animator == null)
        {
            return -1f;
        }
        
        // 애니메이션 이름이 설정되지 않았으면 0 반환
        if (string.IsNullOrEmpty(damageAnimationName))
        {
            return 0f;
        }
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(damageAnimationName))
        {
            // 실제 시간(초) 반환
            float animationLength = stateInfo.length;
            return (stateInfo.normalizedTime % 1f) * animationLength;
        }
        
        return -1f;
    }
    
    /// <summary>
    /// 플레이어에게 데미지를 적용합니다.
    /// </summary>
    private void ApplyDamage()
    {
        if (currentPlayer == null)
        {
            return;
        }
        
        // 플레이어가 무적 상태면 데미지 주지 않음
        if (currentPlayer.GetDamage.IsDamaged)
        {
            return;
        }

        // Player.TakenDamage 호출
        // 플레이어의 최대 체력의 10% 데미지 적용
        float damage = Mathf.Max(1f, currentPlayer.MaxHp * 0.1f);
        currentPlayer.GetDamage.TakenDamage(damage, transform.position);
        lastDamageTime = Time.time;
        
        Debug.Log($"[TrapDamageTrigger] 플레이어에게 데미지 {damage} 적용 - 함정: {gameObject.name}, 시간: {Time.time:F2}");

        // 무적 시간 후 자동으로 무적 상태 초기화하는 코루틴 시작
        StartCoroutine(ResetInvincibilityAfterDelay(currentPlayer));

        // 데미지 후에는 플레이어가 트리거 안에 있더라도 플래그 초기화
        isPlayerInTrigger = false;
        currentPlayer = null;
    }
    
    /// <summary>
    /// 무적 시간이 지난 후 플레이어의 무적 상태를 초기화하는 코루틴
    /// </summary>
    private IEnumerator ResetInvincibilityAfterDelay(Player player)
    {
        yield return new WaitForSeconds(INVINCIBILITY_DURATION);
        
        if (player != null && player.GetDamage != null)
        {
            // 무적 시간이 지났으므로 무적 상태 해제
            player.GetDamage.IsDamaged = false;
            Debug.Log($"[TrapDamageTrigger] 플레이어 무적 상태 초기화 - 함정: {gameObject.name}");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[TrapDamageTrigger] OnTriggerEnter2D - other: {other.name}, layer: {other.gameObject.layer}, playerLayer: {playerLayer.value}");
        
        // 플레이어 레이어 확인
        if(other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // 무적 상태가 너무 오래 지속되고 있다면 초기화
                // (다른 함정에서 받은 무적 상태가 정상적으로 해제되지 않은 경우 대비)
                if (player.GetDamage != null && player.GetDamage.IsDamaged)
                {
                    // 무적 시간이 지났을 가능성이 있으므로 강제로 초기화
                    // 실제로는 코루틴에서 처리되지만, 혹시 모를 경우를 대비
                    StartCoroutine(ResetInvincibilityAfterDelay(player));
                }
                
                isPlayerInTrigger = true;
                currentPlayer = player;
                //Debug.Log($"[TrapDamageTrigger] 플레이어가 함정 범위에 진입 - 함정: {gameObject.name}, 플레이어: {player.name}");
            }
            else
            {
                Debug.LogWarning($"[TrapDamageTrigger] Player 컴포넌트를 찾을 수 없습니다. 오브젝트: {other.name}");
            }
        }
        else
        {
            Debug.Log($"[TrapDamageTrigger] 플레이어 레이어가 아닙니다. 레이어: {other.gameObject.layer}, 레이어 이름: {LayerMask.LayerToName(other.gameObject.layer)}");
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        // 플레이어 레이어 확인
        if(other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                isPlayerInTrigger = true;
                currentPlayer = player;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // 플레이어 레이어 확인
        if(other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player == currentPlayer)
            {
                isPlayerInTrigger = false;
                currentPlayer = null;
                // 플레이어가 나가면 상태 리셋
                hasDamagedInCurrentAnimationCycle = false;
                lastAnimationNormalizedTime = -1f;
                //Debug.Log($"[TrapDamageTrigger] 플레이어가 함정 범위에서 벗어남 - 함정: {gameObject.name}");
            }
        }
    }
}

