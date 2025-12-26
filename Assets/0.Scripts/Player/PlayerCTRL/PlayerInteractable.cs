using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractable : MonoBehaviour
{
    Player _player;
    PlayerInput _input;
    InputActionMap _actionMap;
    InputAction _interaction;

    IInteratable _interact;
    private void Awake()
    {
        _player = GetComponent<Player>(); 
    }
    private void Start()
    {
        _input = _player.Playerinput;
        _actionMap = _input.actions.FindActionMap("Player");
        _interaction = _actionMap.FindAction("Interact");

        _interaction.performed += InteractionAble;
    }

    private void InteractionAble(InputAction.CallbackContext ctx)
    {
        //상호작용 키 누르면 할 수 있는것들
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Interactable"))
        {
            //UI띄워놓기
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Interactable"))
        {
            //UI사라지기
        }
    }
}
