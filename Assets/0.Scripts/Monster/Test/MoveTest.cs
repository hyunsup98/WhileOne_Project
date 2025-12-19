using UnityEngine;
using UnityEngine.InputSystem;

public class MoveTest : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Vector2 moveInput;

    // Player Input (Invoke Unity Events)에서
    // Move 액션에 이 함수 연결
    public void OnMove(InputAction.CallbackContext context)
    {
        // Value / Vector2 액션 기준
        moveInput = context.ReadValue<Vector2>().normalized;
        
    }

    private void Update()
    {
        transform.Translate(moveInput * moveSpeed * Time.deltaTime);
    }
}