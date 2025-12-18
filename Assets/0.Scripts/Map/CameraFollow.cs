using UnityEngine;

/// <summary>
/// 메인 카메라가 대상(플레이어)을 따라다니도록 합니다.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] [Tooltip("카메라가 따라갈 대상 (보통 플레이어 Transform)")]
    private Transform target;   // 따라갈 대상 (플레이어)
    [SerializeField] [Tooltip("대상으로부터의 카메라 오프셋 (상대 위치)")]
    private Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라 오프셋
    [SerializeField] [Tooltip("카메라 이동의 부드러움 정도 (초 단위). 값이 작을수록 더 빠르게 따라감")]
    private float smoothTime = 0.1f; // 부드럽게 이동할 시간

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// 런타임 중에 따라갈 대상을 설정합니다.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
