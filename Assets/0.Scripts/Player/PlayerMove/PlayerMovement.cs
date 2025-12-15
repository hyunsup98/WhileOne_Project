using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    PlayerInput playerInput;
    InputActionMap actionMap;
    InputAction moveAction;

    Rigidbody playerRigid;
    Vector2 dir;
    Vector3 move;

    [SerializeField] float moveSpeed = 5f;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        //playerRigid = GetComponent<Rigidbody>();
        
    }
    void Start()
    {
        actionMap = playerInput.actions.FindActionMap("Player");

        moveAction = actionMap.FindAction("Move");

        moveAction.performed += ctx
        =>
        {
            dir = ctx.ReadValue<Vector2>();
            move = new Vector3(dir.x,dir.y,0).normalized;
            Debug.Log(ctx.ReadValue<Vector2>());
        };

        moveAction.canceled += ctx => dir = Vector2.zero;
    }
    void Update()
    {
        //이동시 방향 전환 버그있음
        
        //if (dir.x < 0)
        //{
        //    transform.localEulerAngles = new Vector3(0, 0, 0);
        //    Debug.Log($"{dir} 방향");
        //}
        //if (dir.x > 0)
        //{
        //    transform.localEulerAngles = new Vector3(0, 180, 0);
        //    Debug.Log($"{dir} 방향");
        //}
        transform.Translate(move * Time.deltaTime * moveSpeed);

    }

}
